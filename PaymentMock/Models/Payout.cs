using PaymentMock.Enums;

namespace PaymentMock.Models;

public class Payout
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string GatewayPayoutId { get; set; } = string.Empty;
    public string MerchantReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentProvider Provider { get; set; }
    public PayoutStatus Status { get; set; } = PayoutStatus.Created;
    public string? FailureReason { get; set; }
    public string? Scenario { get; set; }
    public string? RecipientName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
