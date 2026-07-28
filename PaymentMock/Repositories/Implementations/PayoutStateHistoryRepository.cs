using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;

namespace PaymentMock.Repositories.Implementations;

public class PayoutStateHistoryRepository : BaseRepository<PayoutStateHistory>, IPayoutStateHistoryRepository
{
    public PayoutStateHistoryRepository(string connectionString) : base(connectionString, "payout_state_history") { }

    public async Task<List<PayoutStateHistory>> GetByPayoutIdAsync(string payoutId)
    {
        var result = await QueryAsync<PayoutStateHistory>(
            "SELECT * FROM payout_state_history WHERE PayoutId = @PayoutId ORDER BY TransitionedAt ASC",
            new { PayoutId = payoutId });
        return result.ToList();
    }
}
