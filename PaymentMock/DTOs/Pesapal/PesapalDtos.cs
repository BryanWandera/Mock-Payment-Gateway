namespace PaymentMock.DTOs.Pesapal;

public class PesapalPaymentResponse
{
    public string OrderTrackingId { get; set; } = string.Empty;
    public string MerchantReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? Error { get; set; }
}

public class PesapalTransactionStatusDto
{
    public string OrderTrackingId { get; set; } = string.Empty;
    public string MerchantReference { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CreatedDate { get; set; } = string.Empty;
    public string? ConfirmationCode { get; set; }
    public string PaymentStatusDescription { get; set; } = string.Empty;
    public int PaymentStatusCode { get; set; }
    public string? Description { get; set; }
    public string? Message { get; set; }
    public string? CallBackUrl { get; set; }
    public string? Error { get; set; }
}

public class PesapalTransactionListResponse
{
    public List<PesapalTransactionStatusDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}

public class PesapalPayoutResponse
{
    public string PayoutTrackingId { get; set; } = string.Empty;
    public string MerchantReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
}

public class PesapalPayoutDto
{
    public string PayoutTrackingId { get; set; } = string.Empty;
    public string MerchantReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatusDescription { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CreatedDate { get; set; } = string.Empty;
}

public class PesapalPayoutListResponse
{
    public List<PesapalPayoutDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}

public class PesapalWebhookPayload
{
    public string OrderTrackingId { get; set; } = string.Empty;
    public string OrderMerchantReference { get; set; } = string.Empty;
    public string OrderNotificationType { get; set; } = "IPNCHANGE";
    public string PaymentStatusDescription { get; set; } = string.Empty;
}

public class PesapalAuthTokenRequest
{
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
}

public class PesapalAuthTokenResponse
{
    public string Token { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string? Error { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "200";
}
