using PaymentMock.Enums;

namespace PaymentMock.Models;

public class WebhookDelivery
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? TransactionId { get; set; }
    public string? PayoutId { get; set; }
    public string TargetUrl { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string? Signature { get; set; }
    public WebhookDeliveryStatus Status { get; set; } = WebhookDeliveryStatus.Pending;
    public int AttemptCount { get; set; }
    public int? HttpStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
}
