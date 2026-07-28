using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;

namespace PaymentMock.Repositories.Implementations;

public class ConfigHistoryRepository : BaseRepository<ConfigHistory>, IConfigHistoryRepository
{
    public ConfigHistoryRepository(string connectionString) : base(connectionString, "config_history") { }

    public async Task<List<ConfigHistory>> GetRecentAsync(int limit)
    {
        var result = await QueryAsync<ConfigHistory>(
            "SELECT * FROM config_history ORDER BY CreatedAt DESC LIMIT @Limit",
            new { Limit = limit });
        return result.ToList();
    }
}
