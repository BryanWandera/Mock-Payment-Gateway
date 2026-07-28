using PaymentMock.Models;

namespace PaymentMock.Repositories.Interfaces;

public interface ITransactionStateHistoryRepository : IBaseRepository<TransactionStateHistory>
{
    Task<List<TransactionStateHistory>> GetByTransactionIdAsync(string transactionId);
}
