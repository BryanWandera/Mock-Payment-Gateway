using System.ComponentModel.DataAnnotations;
using PaymentMock.Enums;

namespace PaymentMock.DTOs.Generic;

public class MobileMoneyPaymentRequest
{
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public PaymentProvider Provider { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "KES";

    [Required]
    public string MerchantReference { get; set; } = string.Empty;

    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? Scenario { get; set; }
    public string? CallbackUrl { get; set; }
}

public class CardPaymentRequest
{
    [Required]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    public PaymentProvider Provider { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "KES";

    [Required]
    public string MerchantReference { get; set; } = string.Empty;

    [Required]
    public string ExpiryMonth { get; set; } = string.Empty;

    [Required]
    public string ExpiryYear { get; set; } = string.Empty;

    [Required]
    public string Cvv { get; set; } = string.Empty;

    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? Scenario { get; set; }
    public string? CallbackUrl { get; set; }
}

public class HostedCheckoutRequest
{
    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [Required]
    public PaymentProvider Provider { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "KES";

    [Required]
    public string MerchantReference { get; set; } = string.Empty;

    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Description { get; set; }
    public string? Scenario { get; set; }
    public string? CallbackUrl { get; set; }
}

public class HostedCheckoutResponse
{
    public string TransactionId { get; set; } = string.Empty;
    public string GatewayTransactionId { get; set; } = string.Empty;
    public string MerchantReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
    public string? Message { get; set; }
}

public class PaymentResponse
{
    public string TransactionId { get; set; } = string.Empty;
    public string GatewayTransactionId { get; set; } = string.Empty;
    public string MerchantReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
}

public class TransactionDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string GatewayTransactionId { get; set; } = string.Empty;
    public string MerchantReference { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
    public string? ReceiptNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public string? Scenario { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class TransactionStatusDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TransactionListResponse
{
    public List<TransactionDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}

public class PayoutRequest
{
    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [Required]
    public PaymentProvider Provider { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "KES";

    [Required]
    public string MerchantReference { get; set; } = string.Empty;

    public string? RecipientName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public string? Scenario { get; set; }
}

public class PayoutDto
{
    public string PayoutId { get; set; } = string.Empty;
    public string GatewayPayoutId { get; set; } = string.Empty;
    public string MerchantReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class PayoutListResponse
{
    public List<PayoutDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}

public class PayoutResponse
{
    public string PayoutId { get; set; } = string.Empty;
    public string GatewayPayoutId { get; set; } = string.Empty;
    public string MerchantReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
}

public class WebhookPayload
{
    public string Event { get; set; } = string.Empty;
    public string GatewayTransactionId { get; set; } = string.Empty;
    public string MerchantReference { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
    public string? ReceiptNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class PayoutWebhookPayload
{
    public string Event { get; set; } = string.Empty;
    public string GatewayPayoutId { get; set; } = string.Empty;
    public string MerchantReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class AccountDto
{
    public decimal CurrentBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public List<CurrencyBalanceDto> CurrencyBalances { get; set; } = new();
    public int TotalTransactions { get; set; }
    public int TotalPayouts { get; set; }
}

public class CurrencyBalanceDto
{
    public string Currency { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal AvailableBalance { get; set; }
}

public class WebhookRegistrationRequest
{
    [Required]
    public string Url { get; set; } = string.Empty;
    public string NotificationType { get; set; } = "POST";
}

public class WebhookRegistrationResponse
{
    public string RegistrationId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class CheckoutPageResult
{
    public bool Found { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string CheckoutToken { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string MerchantReference { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsAwaitingAction { get; set; }
}

public class ErrorResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public object? Details { get; set; }
}
