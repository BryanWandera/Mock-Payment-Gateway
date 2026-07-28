using PaymentMock.Enums;

namespace PaymentMock.Models;

public class PayoutStateHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PayoutId { get; set; } = string.Empty;
    public PayoutStatus FromStatus { get; set; }
    public PayoutStatus ToStatus { get; set; }
    public DateTime TransitionedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}
