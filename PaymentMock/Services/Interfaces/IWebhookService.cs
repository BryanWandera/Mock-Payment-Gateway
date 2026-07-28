using PaymentMock.Models;

namespace PaymentMock.Services.Interfaces;

public interface IWebhookService
{
    Task SendTransactionEventAsync(Transaction transaction, string eventType, ScenarioOutcome? outcome = null);
    Task SendPayoutEventAsync(Payout payout, string eventType, ScenarioOutcome? outcome = null);
    Task<WebhookRegistration> RegisterAsync(string url, string notificationType);
    Task<List<WebhookRegistration>> ListRegistrationsAsync();
}
