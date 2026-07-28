using PaymentMock.Enums;
using PaymentMock.Models;

namespace PaymentMock.Repositories.Interfaces;

public interface IPayoutRepository : IBaseRepository<Payout>
{
    Task<Payout?> GetByGatewayPayoutIdAsync(string gatewayPayoutId);
    Task<List<Payout>> SearchAsync(PayoutStatus? status, int page, int pageSize);
    Task<List<Payout>> GetPendingProcessingAsync(int limit);
    Task<int> CountAsync(PayoutStatus? status);
}
