using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;

namespace PaymentMock.Repositories.Implementations;

public class WebhookRegistrationRepository : BaseRepository<WebhookRegistration>, IWebhookRegistrationRepository
{
    public WebhookRegistrationRepository(string connectionString) : base(connectionString, "webhook_registrations") { }

    public async Task<WebhookRegistration?> GetByIpnIdAsync(string ipnId) =>
        await QueryFirstOrDefaultAsync<WebhookRegistration>(
            "SELECT * FROM webhook_registrations WHERE IpnId = @IpnId AND IsActive = 1",
            new { IpnId = ipnId });
}
