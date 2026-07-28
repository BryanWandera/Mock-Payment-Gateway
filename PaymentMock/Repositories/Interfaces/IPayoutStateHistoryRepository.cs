using PaymentMock.Models;

namespace PaymentMock.Repositories.Interfaces;

public interface IPayoutStateHistoryRepository : IBaseRepository<PayoutStateHistory>
{
    Task<List<PayoutStateHistory>> GetByPayoutIdAsync(string payoutId);
}
