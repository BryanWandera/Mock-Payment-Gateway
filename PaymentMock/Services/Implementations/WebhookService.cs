using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PaymentMock.Configuration;
using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Implementations;

public class WebhookService : IWebhookService
{
    private static readonly string[] FinalTransactionEvents = { "transaction.completed", "transaction.failed", "transaction.expired" };
    private static readonly string[] FinalPayoutEvents = { "payout.completed", "payout.failed" };
    private static readonly TimeSpan OutOfOrderDelay = TimeSpan.FromSeconds(6);
    private static readonly TimeSpan DuplicateStagger = TimeSpan.FromSeconds(1);

    private readonly IWebhookDeliveryRepository _webhookDeliveryRepository;
    private readonly IWebhookRegistrationRepository _webhookRegistrationRepository;
    private readonly IGatewayProfileStrategy _profileStrategy;
    private readonly WebhookSettings _webhookSettings;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public WebhookService(
        IWebhookDeliveryRepository webhookDeliveryRepository,
        IWebhookRegistrationRepository webhookRegistrationRepository,
        IGatewayProfileStrategy profileStrategy,
        IOptions<AppSettings> options)
    {
        _webhookDeliveryRepository = webhookDeliveryRepository;
        _webhookRegistrationRepository = webhookRegistrationRepository;
        _profileStrategy = profileStrategy;
        _webhookSettings = options.Value.Webhooks;
    }

    public async Task SendTransactionEventAsync(Transaction transaction, string eventType, ScenarioOutcome? outcome = null)
    {
        var payload = _profileStrategy.BuildTransactionWebhookPayload(transaction, eventType);
        var targetUrl = await ResolveTargetUrlAsync(transaction.CallbackUrl);
        var isFinal = FinalTransactionEvents.Contains(eventType);

        await QueueDeliveriesAsync(transaction.Id, null, targetUrl, payload, isFinal ? outcome : null, isIntermediate: !isFinal);
    }

    public async Task SendPayoutEventAsync(Payout payout, string eventType, ScenarioOutcome? outcome = null)
    {
        var payload = _profileStrategy.BuildPayoutWebhookPayload(payout, eventType);
        var targetUrl = await ResolveTargetUrlAsync(null);
        var isFinal = FinalPayoutEvents.Contains(eventType);

        await QueueDeliveriesAsync(null, payout.Id, targetUrl, payload, isFinal ? outcome : null, isIntermediate: !isFinal);
    }

    public async Task<WebhookRegistration> RegisterAsync(string url, string notificationType)
    {
        var registration = new WebhookRegistration
        {
            Url = url,
            NotificationType = notificationType,
            IpnId = Guid.NewGuid().ToString("N")
        };

        return await _webhookRegistrationRepository.CreateAsync(registration);
    }

    public async Task<List<WebhookRegistration>> ListRegistrationsAsync() =>
        await _webhookRegistrationRepository.GetAllAsync();

    private async Task QueueDeliveriesAsync(
        string? transactionId,
        string? payoutId,
        string? targetUrl,
        object payload,
        ScenarioOutcome? outcome,
        bool isIntermediate)
    {
        var payloadJson = JsonSerializer.Serialize(payload, JsonOptions);

        if (!_webhookSettings.Enabled || string.IsNullOrWhiteSpace(targetUrl))
        {
            await _webhookDeliveryRepository.CreateAsync(new WebhookDelivery
            {
                TransactionId = transactionId,
                PayoutId = payoutId,
                TargetUrl = targetUrl ?? string.Empty,
                Payload = payloadJson,
                Status = Enums.WebhookDeliveryStatus.Skipped,
                ErrorMessage = !_webhookSettings.Enabled ? "Webhooks are globally disabled" : "No callback URL configured"
            });
            return;
        }

        if (outcome?.SkipWebhook == true)
        {
            await _webhookDeliveryRepository.CreateAsync(new WebhookDelivery
            {
                TransactionId = transactionId,
                PayoutId = payoutId,
                TargetUrl = targetUrl,
                Payload = payloadJson,
                Status = Enums.WebhookDeliveryStatus.Skipped,
                ErrorMessage = "Scenario configured to lose this webhook"
            });
            return;
        }

        var scheduledAt = DateTime.UtcNow;

        if (isIntermediate && outcome?.SimulateOutOfOrderWebhook == true)
            scheduledAt = scheduledAt.Add(OutOfOrderDelay);
        else if (!isIntermediate && outcome?.WebhookDelayMs > 0)
            scheduledAt = scheduledAt.AddMilliseconds(outcome.WebhookDelayMs);

        var signature = ComputeSignature(payloadJson, outcome?.SimulateInvalidSignature == true);
        var deliveryCount = !isIntermediate ? Math.Max(1, outcome?.WebhookDuplicateCount ?? 1) : 1;

        for (var i = 0; i < deliveryCount; i++)
        {
            await _webhookDeliveryRepository.CreateAsync(new WebhookDelivery
            {
                TransactionId = transactionId,
                PayoutId = payoutId,
                TargetUrl = targetUrl,
                Payload = payloadJson,
                Signature = signature,
                Status = Enums.WebhookDeliveryStatus.Pending,
                ScheduledAt = scheduledAt.Add(TimeSpan.FromTicks(DuplicateStagger.Ticks * i))
            });
        }
    }

    private async Task<string?> ResolveTargetUrlAsync(string? callbackUrl)
    {
        if (!string.IsNullOrWhiteSpace(callbackUrl))
            return callbackUrl;

        var registrations = await _webhookRegistrationRepository.GetAllAsync();
        var active = registrations.Where(r => r.IsActive).OrderByDescending(r => r.CreatedAt).FirstOrDefault();
        if (active != null)
            return active.Url;

        return string.IsNullOrWhiteSpace(_webhookSettings.DefaultUrl) ? null : _webhookSettings.DefaultUrl;
    }

    private string ComputeSignature(string payload, bool useInvalidSignature)
    {
        var secret = useInvalidSignature ? _webhookSettings.SignatureSecret + "-tampered" : _webhookSettings.SignatureSecret;
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(keyBytes, payloadBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
