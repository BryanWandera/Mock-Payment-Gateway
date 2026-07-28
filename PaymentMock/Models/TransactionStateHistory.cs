using PaymentMock.Enums;

namespace PaymentMock.Models;

public class TransactionStateHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TransactionId { get; set; } = string.Empty;
    public TransactionStatus FromStatus { get; set; }
    public TransactionStatus ToStatus { get; set; }
    public DateTime TransitionedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}
