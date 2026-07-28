using PaymentMock.Enums;
using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;

namespace PaymentMock.Repositories.Implementations;

public class WebhookDeliveryRepository : BaseRepository<WebhookDelivery>, IWebhookDeliveryRepository
{
    public WebhookDeliveryRepository(string connectionString) : base(connectionString, "webhook_deliveries") { }

    public async Task<List<WebhookDelivery>> GetPendingDeliveriesAsync(int limit)
    {
        var result = await QueryAsync<WebhookDelivery>(
            @"SELECT * FROM webhook_deliveries
              WHERE Status = 'Pending' AND (ScheduledAt IS NULL OR ScheduledAt <= UTC_TIMESTAMP())
              ORDER BY CreatedAt ASC LIMIT @Limit",
            new { Limit = limit });
        return result.ToList();
    }

    public async Task<List<WebhookDelivery>> SearchAsync(WebhookDeliveryStatus? status, int page, int pageSize)
    {
        var sql = status.HasValue
            ? "SELECT * FROM webhook_deliveries WHERE Status = @Status ORDER BY CreatedAt DESC LIMIT @PageSize OFFSET @Offset"
            : "SELECT * FROM webhook_deliveries ORDER BY CreatedAt DESC LIMIT @PageSize OFFSET @Offset";
        var result = await QueryAsync<WebhookDelivery>(sql, new
        {
            Status = status?.ToString(),
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });
        return result.ToList();
    }

    public async Task<int> CountAsync(WebhookDeliveryStatus? status)
    {
        var sql = status.HasValue
            ? "SELECT COUNT(*) FROM webhook_deliveries WHERE Status = @Status"
            : "SELECT COUNT(*) FROM webhook_deliveries";
        return await QueryFirstOrDefaultAsync<int>(sql, new { Status = status?.ToString() });
    }
}
