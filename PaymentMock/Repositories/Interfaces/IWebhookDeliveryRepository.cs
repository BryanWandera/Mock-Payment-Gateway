using PaymentMock.Enums;
using PaymentMock.Models;

namespace PaymentMock.Repositories.Interfaces;

public interface IWebhookDeliveryRepository : IBaseRepository<WebhookDelivery>
{
    Task<List<WebhookDelivery>> GetPendingDeliveriesAsync(int limit);
    Task<List<WebhookDelivery>> SearchAsync(WebhookDeliveryStatus? status, int page, int pageSize);
    Task<int> CountAsync(WebhookDeliveryStatus? status);
}
