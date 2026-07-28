using PaymentMock.Models;

namespace PaymentMock.Repositories.Interfaces;

public interface IWebhookRegistrationRepository : IBaseRepository<WebhookRegistration>
{
    Task<WebhookRegistration?> GetByIpnIdAsync(string ipnId);
}
