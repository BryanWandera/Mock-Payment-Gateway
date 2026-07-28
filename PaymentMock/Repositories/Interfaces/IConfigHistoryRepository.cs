using PaymentMock.Models;

namespace PaymentMock.Repositories.Interfaces;

public interface IConfigHistoryRepository : IBaseRepository<ConfigHistory>
{
    Task<List<ConfigHistory>> GetRecentAsync(int limit);
}
