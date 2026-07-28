using Microsoft.Extensions.Options;
using PaymentMock.Configuration;
using PaymentMock.Enums;
using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Background;

public class PayoutProcessingBackgroundService : BackgroundService
{
    private const int PendingAcknowledgementMs = 1000;

    private static readonly PayoutStatus[] TerminalStatuses =
    {
        PayoutStatus.Completed, PayoutStatus.Failed, PayoutStatus.Cancelled
    };

    private readonly IPayoutProcessingQueue _queue;
    private readonly IPayoutRepository _payoutRepository;
    private readonly IPayoutStateHistoryRepository _stateHistoryRepository;
    private readonly IGatewayAccountRepository _gatewayAccountRepository;
    private readonly IScenarioEngine _scenarioEngine;
    private readonly IWebhookService _webhookService;
    private readonly GatewaySettings _gatewaySettings;
    private readonly ILogger<PayoutProcessingBackgroundService> _logger;

    public PayoutProcessingBackgroundService(
        IPayoutProcessingQueue queue,
        IPayoutRepository payoutRepository,
        IPayoutStateHistoryRepository stateHistoryRepository,
        IGatewayAccountRepository gatewayAccountRepository,
        IScenarioEngine scenarioEngine,
        IWebhookService webhookService,
        IOptions<AppSettings> options,
        ILogger<PayoutProcessingBackgroundService> logger)
    {
        _queue = queue;
        _payoutRepository = payoutRepository;
        _stateHistoryRepository = stateHistoryRepository;
        _gatewayAccountRepository = gatewayAccountRepository;
        _scenarioEngine = scenarioEngine;
        _webhookService = webhookService;
        _gatewaySettings = options.Value.Gateway;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pending = await _payoutRepository.GetPendingProcessingAsync(500);
        foreach (var payout in pending)
            _queue.Enqueue(payout.Id);

        await foreach (var payoutId in _queue.DequeueAllAsync(stoppingToken))
        {
            _ = ProcessPayoutAsync(payoutId, stoppingToken);
        }
    }

    private async Task ProcessPayoutAsync(string payoutId, CancellationToken ct)
    {
        try
        {
            var payout = await _payoutRepository.GetByIdAsync(payoutId);
            if (payout == null || TerminalStatuses.Contains(payout.Status))
                return;

            var outcome = await _scenarioEngine.ResolveAndRecordAsync(payout.Scenario, payoutId: payout.Id);

            await TransitionAsync(payout, PayoutStatus.Pending, "Payout submitted to provider", ct);
            if (IsStuck(payout, outcome)) return;

            await DelayAsync(PendingAcknowledgementMs, outcome.ProcessingDelayMultiplier, ct);

            await TransitionAsync(payout, PayoutStatus.Processing, "Payout being disbursed", ct);
            if (IsStuck(payout, outcome)) return;

            await _webhookService.SendPayoutEventAsync(payout, "payout.processing", outcome);

            var delayMs = outcome.FailFast
                ? _gatewaySettings.ProcessingDelays.PayoutProcessingMs / 4
                : _gatewaySettings.ProcessingDelays.PayoutProcessingMs;
            await DelayAsync(delayMs, outcome.ProcessingDelayMultiplier, ct);

            if (outcome.ShouldSucceed)
            {
                await TransitionAsync(payout, PayoutStatus.Completed, "Payout disbursed successfully", ct, isCompletion: true);
                await _gatewayAccountRepository.IncrementPayoutCountAsync(payout.Currency, payout.Amount);
                await _webhookService.SendPayoutEventAsync(payout, "payout.completed", outcome);
            }
            else
            {
                payout.FailureReason = outcome.FailureReason ?? "Payout declined";
                await TransitionAsync(payout, PayoutStatus.Failed, payout.FailureReason, ct);
                await _webhookService.SendPayoutEventAsync(payout, "payout.failed", outcome);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error processing payout {PayoutId}", payoutId);
        }
    }

    private async Task TransitionAsync(Payout payout, PayoutStatus newStatus, string notes, CancellationToken ct, bool isCompletion = false)
    {
        var fromStatus = payout.Status;
        payout.Status = newStatus;
        payout.UpdatedAt = DateTime.UtcNow;
        if (isCompletion || TerminalStatuses.Contains(newStatus))
            payout.CompletedAt = DateTime.UtcNow;

        await _payoutRepository.UpdateAsync(payout);
        await _stateHistoryRepository.CreateAsync(new PayoutStateHistory
        {
            PayoutId = payout.Id,
            FromStatus = fromStatus,
            ToStatus = newStatus,
            Notes = notes
        });
    }

    private static bool IsStuck(Payout payout, ScenarioOutcome outcome) =>
        !string.IsNullOrWhiteSpace(outcome.StuckAtStatus) &&
        payout.Status.ToString().Equals(outcome.StuckAtStatus, StringComparison.OrdinalIgnoreCase);

    private async Task DelayAsync(int baseMs, double multiplier, CancellationToken ct)
    {
        var variancePercent = _gatewaySettings.TimingVariancePercent;
        var jitter = 1 + (Random.Shared.NextDouble() * 2 - 1) * variancePercent / 100.0;
        var delayMs = Math.Max(50, baseMs * multiplier * jitter);
        await Task.Delay(TimeSpan.FromMilliseconds(delayMs), ct);
    }
}
