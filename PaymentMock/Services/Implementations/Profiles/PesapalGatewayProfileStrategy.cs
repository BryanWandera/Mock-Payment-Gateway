using PaymentMock.DTOs.Pesapal;
using PaymentMock.Enums;
using PaymentMock.Models;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Implementations.Profiles;

public class PesapalGatewayProfileStrategy : IGatewayProfileStrategy
{
    public GatewayProfileType Profile => GatewayProfileType.Pesapal;

    public string GenerateGatewayTransactionId() => Guid.NewGuid().ToString();

    public string GenerateGatewayPayoutId() => Guid.NewGuid().ToString();

    public object BuildPaymentResponse(Transaction transaction, string? message = null) => new PesapalPaymentResponse
    {
        OrderTrackingId = transaction.GatewayTransactionId,
        MerchantReference = transaction.MerchantReference,
        Status = "200",
        RedirectUrl = string.Empty,
        Message = message ?? "Request processed"
    };

    public object BuildHostedCheckoutResponse(Transaction transaction, string redirectUrl, string? message = null) => new PesapalPaymentResponse
    {
        OrderTrackingId = transaction.GatewayTransactionId,
        MerchantReference = transaction.MerchantReference,
        Status = "200",
        RedirectUrl = redirectUrl,
        Message = message ?? "Request processed"
    };

    public object BuildTransactionDto(Transaction transaction) => MapTransactionStatusDto(transaction);

    public object BuildTransactionStatusDto(Transaction transaction) => MapTransactionStatusDto(transaction);

    public object BuildTransactionListResponse(List<Transaction> transactions, int page, int pageSize, int total) => new PesapalTransactionListResponse
    {
        Items = transactions.Select(MapTransactionStatusDto).ToList(),
        Page = page,
        PageSize = pageSize,
        Total = total
    };

    public object BuildPayoutResponse(Payout payout, string? message = null) => new PesapalPayoutResponse
    {
        PayoutTrackingId = payout.GatewayPayoutId,
        MerchantReference = payout.MerchantReference,
        Status = "200",
        Message = message ?? "Payout request processed"
    };

    public object BuildPayoutDto(Payout payout) => MapPayoutDto(payout);

    public object BuildPayoutListResponse(List<Payout> payouts, int page, int pageSize, int total) => new PesapalPayoutListResponse
    {
        Items = payouts.Select(MapPayoutDto).ToList(),
        Page = page,
        PageSize = pageSize,
        Total = total
    };

    public object BuildTransactionWebhookPayload(Transaction transaction, string eventType) => new PesapalWebhookPayload
    {
        OrderTrackingId = transaction.GatewayTransactionId,
        OrderMerchantReference = transaction.MerchantReference,
        OrderNotificationType = "IPNCHANGE",
        PaymentStatusDescription = MapStatusDescription(transaction.Status)
    };

    public object BuildPayoutWebhookPayload(Payout payout, string eventType) => new PesapalWebhookPayload
    {
        OrderTrackingId = payout.GatewayPayoutId,
        OrderMerchantReference = payout.MerchantReference,
        OrderNotificationType = "PAYOUTCHANGE",
        PaymentStatusDescription = MapPayoutStatusDescription(payout.Status)
    };

    private static PesapalTransactionStatusDto MapTransactionStatusDto(Transaction transaction) => new()
    {
        OrderTrackingId = transaction.GatewayTransactionId,
        MerchantReference = transaction.MerchantReference,
        PaymentMethod = transaction.PaymentMethod.ToString(),
        Amount = transaction.Amount,
        Currency = transaction.Currency,
        CreatedDate = transaction.CreatedAt.ToString("O"),
        ConfirmationCode = transaction.ReceiptNumber,
        PaymentStatusDescription = MapStatusDescription(transaction.Status),
        PaymentStatusCode = MapStatusCode(transaction.Status),
        Description = transaction.Description,
        Message = transaction.FailureReason,
        CallBackUrl = transaction.CallbackUrl,
        Error = transaction.Status == TransactionStatus.Failed ? transaction.FailureReason : null
    };

    private static PesapalPayoutDto MapPayoutDto(Payout payout) => new()
    {
        PayoutTrackingId = payout.GatewayPayoutId,
        MerchantReference = payout.MerchantReference,
        Amount = payout.Amount,
        Currency = payout.Currency,
        PaymentMethod = payout.PaymentMethod.ToString(),
        PaymentStatusDescription = MapPayoutStatusDescription(payout.Status),
        Description = payout.FailureReason,
        CreatedDate = payout.CreatedAt.ToString("O")
    };

    private static string MapStatusDescription(TransactionStatus status) => status switch
    {
        TransactionStatus.Completed => "COMPLETED",
        TransactionStatus.Failed => "FAILED",
        TransactionStatus.Cancelled => "FAILED",
        TransactionStatus.Expired => "INVALID",
        _ => "PENDING"
    };

    private static int MapStatusCode(TransactionStatus status) => status switch
    {
        TransactionStatus.Completed => 1,
        TransactionStatus.Failed => 2,
        TransactionStatus.Cancelled => 2,
        TransactionStatus.Expired => 0,
        _ => 3
    };

    private static string MapPayoutStatusDescription(PayoutStatus status) => status switch
    {
        PayoutStatus.Completed => "COMPLETED",
        PayoutStatus.Failed => "FAILED",
        PayoutStatus.Cancelled => "FAILED",
        _ => "PENDING"
    };
}
