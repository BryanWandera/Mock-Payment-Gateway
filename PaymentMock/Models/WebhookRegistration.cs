namespace PaymentMock.Models;

public class WebhookRegistration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Url { get; set; } = string.Empty;
    public string NotificationType { get; set; } = "POST";
    public string? IpnId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
