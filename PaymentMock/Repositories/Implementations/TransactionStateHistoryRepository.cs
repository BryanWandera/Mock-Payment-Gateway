using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;

namespace PaymentMock.Repositories.Implementations;

public class TransactionStateHistoryRepository : BaseRepository<TransactionStateHistory>, ITransactionStateHistoryRepository
{
    public TransactionStateHistoryRepository(string connectionString) : base(connectionString, "transaction_state_history") { }

    public async Task<List<TransactionStateHistory>> GetByTransactionIdAsync(string transactionId)
    {
        var result = await QueryAsync<TransactionStateHistory>(
            "SELECT * FROM transaction_state_history WHERE TransactionId = @TransactionId ORDER BY TransitionedAt ASC",
            new { TransactionId = transactionId });
        return result.ToList();
    }
}
