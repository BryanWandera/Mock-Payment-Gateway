using Microsoft.Extensions.Options;
using PaymentMock.Configuration;
using PaymentMock.Enums;
using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Background;

public class TransactionProcessingBackgroundService : BackgroundService
{
    private static readonly TransactionStatus[] TerminalStatuses =
    {
        TransactionStatus.Completed, TransactionStatus.Failed, TransactionStatus.Cancelled, TransactionStatus.Expired
    };

    private readonly ITransactionProcessingQueue _queue;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionStateHistoryRepository _stateHistoryRepository;
    private readonly IGatewayAccountRepository _gatewayAccountRepository;
    private readonly IScenarioEngine _scenarioEngine;
    private readonly IWebhookService _webhookService;
    private readonly GatewaySettings _gatewaySettings;
    private readonly ILogger<TransactionProcessingBackgroundService> _logger;

    public TransactionProcessingBackgroundService(
        ITransactionProcessingQueue queue,
        ITransactionRepository transactionRepository,
        ITransactionStateHistoryRepository stateHistoryRepository,
        IGatewayAccountRepository gatewayAccountRepository,
        IScenarioEngine scenarioEngine,
        IWebhookService webhookService,
        IOptions<AppSettings> options,
        ILogger<TransactionProcessingBackgroundService> logger)
    {
        _queue = queue;
        _transactionRepository = transactionRepository;
        _stateHistoryRepository = stateHistoryRepository;
        _gatewayAccountRepository = gatewayAccountRepository;
        _scenarioEngine = scenarioEngine;
        _webhookService = webhookService;
        _gatewaySettings = options.Value.Gateway;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pending = await _transactionRepository.GetPendingProcessingAsync(500);
        foreach (var transaction in pending)
            _queue.Enqueue(transaction.Id);

        await foreach (var transactionId in _queue.DequeueAllAsync(stoppingToken))
        {
            _ = ProcessTransactionAsync(transactionId, stoppingToken);
        }
    }

    private async Task ProcessTransactionAsync(string transactionId, CancellationToken ct)
    {
        try
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null || TerminalStatuses.Contains(transaction.Status))
                return;

            var outcome = await _scenarioEngine.ResolveAndRecordAsync(transaction.Scenario, transactionId: transaction.Id);

            // Card and BankTransfer both arrive here via the hosted checkout page (the customer
            // already confirmed on that page), so neither needs the STK-push wording/flow that
            // MobileMoney uses.
            if (transaction.PaymentMethod is PaymentMethod.Card or PaymentMethod.BankTransfer)
                await ProcessCardAsync(transaction, outcome, ct);
            else
                await ProcessMobileMoneyAsync(transaction, outcome, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error processing transaction {TransactionId}", transactionId);
        }
    }

    private async Task ProcessMobileMoneyAsync(Transaction transaction, ScenarioOutcome outcome, CancellationToken ct)
    {
        await TransitionAsync(transaction, TransactionStatus.Pending, "STK push sent to customer device", outcome, ct);
        if (IsStuck(transaction, outcome)) return;

        await DelayAsync(_gatewaySettings.ProcessingDelays.StkPushMs, outcome.ProcessingDelayMultiplier, ct);

        if (outcome.SimulateTimeout)
        {
            await DelayAsync(_gatewaySettings.ProcessingDelays.PinEntryMs * 2, outcome.ProcessingDelayMultiplier, ct);
            await TransitionAsync(transaction, TransactionStatus.Expired, "Customer never confirmed the STK push", outcome, ct);
            await _webhookService.SendTransactionEventAsync(transaction, "transaction.expired", outcome);
            return;
        }

        await DelayAsync(_gatewaySettings.ProcessingDelays.PinEntryMs, outcome.ProcessingDelayMultiplier, ct);

        if (outcome.FailFast)
        {
            await FailAsync(transaction, outcome, ct, "Customer PIN entry rejected the transaction");
            return;
        }

        await TransitionAsync(transaction, TransactionStatus.Processing, "PIN accepted, processing with issuing bank", outcome, ct);
        if (IsStuck(transaction, outcome)) return;

        await _webhookService.SendTransactionEventAsync(transaction, "transaction.processing", outcome);

        await DelayAsync(_gatewaySettings.ProcessingDelays.ProcessingMs, outcome.ProcessingDelayMultiplier, ct);

        await FinalizeAsync(transaction, outcome, ct);
    }

    private async Task ProcessCardAsync(Transaction transaction, ScenarioOutcome outcome, CancellationToken ct)
    {
        await TransitionAsync(transaction, TransactionStatus.Processing, "Submitting card details to acquiring bank", outcome, ct);
        if (IsStuck(transaction, outcome)) return;

        await _webhookService.SendTransactionEventAsync(transaction, "transaction.processing", outcome);

        if (outcome.FailFast)
        {
            await DelayAsync(_gatewaySettings.ProcessingDelays.PinEntryMs, outcome.ProcessingDelayMultiplier, ct);
            await FailAsync(transaction, outcome, ct, "Card declined by issuer");
            return;
        }

        await DelayAsync(_gatewaySettings.ProcessingDelays.ProcessingMs, outcome.ProcessingDelayMultiplier, ct);

        await FinalizeAsync(transaction, outcome, ct);
    }

    private async Task FinalizeAsync(Transaction transaction, ScenarioOutcome outcome, CancellationToken ct)
    {
        if (outcome.ShouldSucceed)
        {
            transaction.ReceiptNumber = $"RCPT{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";
            await TransitionAsync(transaction, TransactionStatus.Completed, "Transaction settled successfully", outcome, ct, isCompletion: true);
            await _gatewayAccountRepository.IncrementTransactionCountAsync(transaction.Currency, transaction.Amount);
            await _webhookService.SendTransactionEventAsync(transaction, "transaction.completed", outcome);
        }
        else
        {
            await FailAsync(transaction, outcome, ct, outcome.FailureReason ?? "Transaction declined");
        }
    }

    private async Task FailAsync(Transaction transaction, ScenarioOutcome outcome, CancellationToken ct, string reason)
    {
        transaction.FailureReason = reason;
        await TransitionAsync(transaction, TransactionStatus.Failed, reason, outcome, ct);
        await _webhookService.SendTransactionEventAsync(transaction, "transaction.failed", outcome);
    }

    private async Task TransitionAsync(
        Transaction transaction,
        TransactionStatus newStatus,
        string notes,
        ScenarioOutcome outcome,
        CancellationToken ct,
        bool isCompletion = false)
    {
        var fromStatus = transaction.Status;
        transaction.Status = newStatus;
        transaction.UpdatedAt = DateTime.UtcNow;
        if (isCompletion || TerminalStatuses.Contains(newStatus))
            transaction.CompletedAt = DateTime.UtcNow;

        await _transactionRepository.UpdateAsync(transaction);
        await _stateHistoryRepository.CreateAsync(new TransactionStateHistory
        {
            TransactionId = transaction.Id,
            FromStatus = fromStatus,
            ToStatus = newStatus,
            Notes = notes
        });
    }

    private static bool IsStuck(Transaction transaction, ScenarioOutcome outcome) =>
        !string.IsNullOrWhiteSpace(outcome.StuckAtStatus) &&
        transaction.Status.ToString().Equals(outcome.StuckAtStatus, StringComparison.OrdinalIgnoreCase);

    private async Task DelayAsync(int baseMs, double multiplier, CancellationToken ct)
    {
        var variancePercent = _gatewaySettings.TimingVariancePercent;
        var jitter = 1 + (Random.Shared.NextDouble() * 2 - 1) * variancePercent / 100.0;
        var delayMs = Math.Max(50, baseMs * multiplier * jitter);
        await Task.Delay(TimeSpan.FromMilliseconds(delayMs), ct);
    }
}
