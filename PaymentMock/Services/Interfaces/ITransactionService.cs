using PaymentMock.Enums;

namespace PaymentMock.Services.Interfaces;

public interface ITransactionService
{
    Task<object> GetByIdAsync(string transactionId);
    Task<object> GetStatusAsync(string transactionId);
    Task<object> SearchAsync(
        TransactionStatus? status,
        PaymentProvider? provider,
        PaymentMethod? paymentMethod,
        string? merchantReference,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize);
}
