using System.Text;
using Microsoft.Extensions.Options;
using PaymentMock.Configuration;
using PaymentMock.Enums;
using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;

namespace PaymentMock.Services.Background;

public class WebhookDeliveryBackgroundService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
    private const string HttpClientName = "webhooks";

    private readonly IWebhookDeliveryRepository _webhookDeliveryRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WebhookSettings _webhookSettings;
    private readonly ILogger<WebhookDeliveryBackgroundService> _logger;

    public WebhookDeliveryBackgroundService(
        IWebhookDeliveryRepository webhookDeliveryRepository,
        IHttpClientFactory httpClientFactory,
        IOptions<AppSettings> options,
        ILogger<WebhookDeliveryBackgroundService> logger)
    {
        _webhookDeliveryRepository = webhookDeliveryRepository;
        _httpClientFactory = httpClientFactory;
        _webhookSettings = options.Value.Webhooks;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);
        do
        {
            try
            {
                var pending = await _webhookDeliveryRepository.GetPendingDeliveriesAsync(50);
                foreach (var delivery in pending)
                    _ = DeliverAsync(delivery, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling webhook delivery queue");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task DeliverAsync(WebhookDelivery delivery, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);

        try
        {
            using var content = new StringContent(delivery.Payload, Encoding.UTF8, "application/json");
            if (!string.IsNullOrEmpty(delivery.Signature))
                content.Headers.Add("X-Webhook-Signature", delivery.Signature);
            content.Headers.Add("X-Webhook-Attempt", (delivery.AttemptCount + 1).ToString());

            var response = await client.PostAsync(delivery.TargetUrl, content, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            delivery.AttemptCount++;
            delivery.HttpStatusCode = (int)response.StatusCode;
            delivery.ResponseBody = Truncate(responseBody, 2000);

            if (response.IsSuccessStatusCode)
            {
                delivery.Status = WebhookDeliveryStatus.Sent;
                delivery.DeliveredAt = DateTime.UtcNow;
            }
            else
            {
                ApplyRetryOrFail(delivery, $"Received HTTP {(int)response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            delivery.AttemptCount++;
            ApplyRetryOrFail(delivery, ex.Message);
        }

        await _webhookDeliveryRepository.UpdateAsync(delivery);
    }

    private void ApplyRetryOrFail(WebhookDelivery delivery, string error)
    {
        delivery.ErrorMessage = Truncate(error, 500);

        if (delivery.AttemptCount >= _webhookSettings.MaxRetryAttempts)
        {
            delivery.Status = WebhookDeliveryStatus.Failed;
        }
        else
        {
            delivery.Status = WebhookDeliveryStatus.Pending;
            delivery.ScheduledAt = DateTime.UtcNow.AddMilliseconds(_webhookSettings.RetryDelayMs * delivery.AttemptCount);
        }
    }

    private static string Truncate(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];
}
