namespace PaymentMock.Models;

public class ScenarioExecution
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ScenarioName { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? PayoutId { get; set; }
    public string? Details { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}
