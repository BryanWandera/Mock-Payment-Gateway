namespace PaymentMock.Models;

public class ConfigHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConfigSection { get; set; } = string.Empty;
    public string Snapshot { get; set; } = string.Empty;
    public string? ChangedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
