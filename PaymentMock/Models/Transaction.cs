using PaymentMock.Enums;

namespace PaymentMock.Models;

public class Transaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string GatewayTransactionId { get; set; } = string.Empty;
    public string MerchantReference { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
    public string? ReceiptNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public PaymentProvider Provider { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Created;
    public string? FailureReason { get; set; }
    public string? Scenario { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public string? CardLastFour { get; set; }
    public string? CallbackUrl { get; set; }
    public string? NotificationId { get; set; }
    public string? Description { get; set; }
    public string? CheckoutToken { get; set; }
    public DateTime? CheckoutCompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
