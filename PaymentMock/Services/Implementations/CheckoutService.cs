using PaymentMock.DTOs.Generic;
using PaymentMock.Enums;
using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Implementations;

public class CheckoutService : ICheckoutService
{
    private const string CustomerDeclinedReason = "Cancelled by customer on checkout page";

    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionStateHistoryRepository _transactionStateHistoryRepository;
    private readonly ITransactionProcessingQueue _processingQueue;
    private readonly IWebhookService _webhookService;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService(
        ITransactionRepository transactionRepository,
        ITransactionStateHistoryRepository transactionStateHistoryRepository,
        ITransactionProcessingQueue processingQueue,
        IWebhookService webhookService,
        ILogger<CheckoutService> logger)
    {
        _transactionRepository = transactionRepository;
        _transactionStateHistoryRepository = transactionStateHistoryRepository;
        _processingQueue = processingQueue;
        _webhookService = webhookService;
        _logger = logger;
    }

    public async Task<CheckoutPageResult> GetCheckoutPageAsync(string checkoutToken)
    {
        var transaction = await _transactionRepository.GetByCheckoutTokenAsync(checkoutToken);
        return MapResult(transaction);
    }

    public async Task<CheckoutPageResult> ApproveAsync(string checkoutToken)
    {
        var transaction = await _transactionRepository.GetByCheckoutTokenAsync(checkoutToken);
        if (transaction == null)
            return MapResult(null);

        // Idempotent: a second click (or a double form submission) just returns the current
        // state instead of re-transitioning an already-decided transaction.
        if (transaction.Status != TransactionStatus.AwaitingCheckout)
            return MapResult(transaction);

        var fromStatus = transaction.Status;
        transaction.Status = TransactionStatus.Created;
        transaction.CheckoutCompletedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;
        await _transactionRepository.UpdateAsync(transaction);
        await _transactionStateHistoryRepository.CreateAsync(new TransactionStateHistory
        {
            TransactionId = transaction.Id,
            FromStatus = fromStatus,
            ToStatus = TransactionStatus.Created,
            Notes = "Customer approved payment on hosted checkout page"
        });

        // Re-enter the normal processing pipeline so this reuses the existing scenario
        // resolution, delay simulation, and webhook delivery machinery for Card/BankTransfer.
        _processingQueue.Enqueue(transaction.Id);
        _logger.LogInformation("Checkout {CheckoutToken} approved, transaction {TransactionId} re-queued", checkoutToken, transaction.Id);

        return MapResult(transaction);
    }

    public async Task<CheckoutPageResult> DeclineAsync(string checkoutToken)
    {
        var transaction = await _transactionRepository.GetByCheckoutTokenAsync(checkoutToken);
        if (transaction == null)
            return MapResult(null);

        if (transaction.Status != TransactionStatus.AwaitingCheckout)
            return MapResult(transaction);

        var fromStatus = transaction.Status;
        transaction.Status = TransactionStatus.Failed;
        transaction.FailureReason = CustomerDeclinedReason;
        transaction.CheckoutCompletedAt = DateTime.UtcNow;
        transaction.CompletedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;
        await _transactionRepository.UpdateAsync(transaction);
        await _transactionStateHistoryRepository.CreateAsync(new TransactionStateHistory
        {
            TransactionId = transaction.Id,
            FromStatus = fromStatus,
            ToStatus = TransactionStatus.Failed,
            Notes = CustomerDeclinedReason
        });

        await _webhookService.SendTransactionEventAsync(transaction, "transaction.failed");
        _logger.LogInformation("Checkout {CheckoutToken} declined, transaction {TransactionId} failed", checkoutToken, transaction.Id);

        return MapResult(transaction);
    }

    private static CheckoutPageResult MapResult(Transaction? transaction)
    {
        if (transaction == null)
            return new CheckoutPageResult { Found = false };

        return new CheckoutPageResult
        {
            Found = true,
            TransactionId = transaction.Id,
            CheckoutToken = transaction.CheckoutToken ?? string.Empty,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            MerchantReference = transaction.MerchantReference,
            PaymentMethod = transaction.PaymentMethod.ToString(),
            CustomerName = transaction.CustomerName,
            CustomerEmail = transaction.CustomerEmail,
            Status = transaction.Status.ToString(),
            IsAwaitingAction = transaction.Status == TransactionStatus.AwaitingCheckout
        };
    }
}
