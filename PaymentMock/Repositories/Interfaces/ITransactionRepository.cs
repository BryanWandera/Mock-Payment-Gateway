using PaymentMock.Enums;
using PaymentMock.Models;

namespace PaymentMock.Repositories.Interfaces;

public interface ITransactionRepository : IBaseRepository<Transaction>
{
    Task<Transaction?> GetByGatewayTransactionIdAsync(string gatewayTransactionId);
    Task<Transaction?> GetByMerchantReferenceAsync(string merchantReference);
    Task<Transaction?> GetByCheckoutTokenAsync(string checkoutToken);
    Task<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey);
    Task<List<Transaction>> SearchAsync(
        TransactionStatus? status,
        PaymentProvider? provider,
        PaymentMethod? paymentMethod,
        string? merchantReference,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize);
    Task<List<Transaction>> GetPendingProcessingAsync(int limit);
    Task<int> CountAsync(
        TransactionStatus? status,
        PaymentProvider? provider,
        PaymentMethod? paymentMethod,
        string? merchantReference,
        DateTime? fromDate,
        DateTime? toDate);
}
