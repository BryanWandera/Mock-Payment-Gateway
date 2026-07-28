namespace PaymentMock.Models;

public class GatewayAccount
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Currency { get; set; } = "KES";
    public decimal CurrentBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public int TotalTransactions { get; set; }
    public int TotalPayouts { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
