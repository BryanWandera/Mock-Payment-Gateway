using PaymentMock.Enums;
using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;

namespace PaymentMock.Repositories.Implementations;

public class PayoutRepository : BaseRepository<Payout>, IPayoutRepository
{
    public PayoutRepository(string connectionString) : base(connectionString, "payouts") { }

    public async Task<Payout?> GetByGatewayPayoutIdAsync(string gatewayPayoutId) =>
        await QueryFirstOrDefaultAsync<Payout>(
            "SELECT * FROM payouts WHERE GatewayPayoutId = @GatewayPayoutId",
            new { GatewayPayoutId = gatewayPayoutId });

    public async Task<List<Payout>> SearchAsync(PayoutStatus? status, int page, int pageSize)
    {
        var sql = status.HasValue
            ? "SELECT * FROM payouts WHERE Status = @Status ORDER BY CreatedAt DESC LIMIT @PageSize OFFSET @Offset"
            : "SELECT * FROM payouts ORDER BY CreatedAt DESC LIMIT @PageSize OFFSET @Offset";
        var result = await QueryAsync<Payout>(sql, new
        {
            Status = status?.ToString(),
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });
        return result.ToList();
    }

    public async Task<List<Payout>> GetPendingProcessingAsync(int limit)
    {
        var result = await QueryAsync<Payout>(
            "SELECT * FROM payouts WHERE Status IN ('Created', 'Pending', 'Processing') ORDER BY CreatedAt ASC LIMIT @Limit",
            new { Limit = limit });
        return result.ToList();
    }

    public async Task<int> CountAsync(PayoutStatus? status)
    {
        var sql = status.HasValue
            ? "SELECT COUNT(*) FROM payouts WHERE Status = @Status"
            : "SELECT COUNT(*) FROM payouts";
        return await QueryFirstOrDefaultAsync<int>(sql, new { Status = status?.ToString() });
    }
}
