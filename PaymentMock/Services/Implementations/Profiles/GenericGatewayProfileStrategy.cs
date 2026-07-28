using PaymentMock.DTOs.Generic;
using PaymentMock.Enums;
using PaymentMock.Models;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Implementations.Profiles;

public class GenericGatewayProfileStrategy : IGatewayProfileStrategy
{
    public GatewayProfileType Profile => GatewayProfileType.Generic;

    public string GenerateGatewayTransactionId() =>
        $"GTW-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}";

    public string GenerateGatewayPayoutId() =>
        $"PYT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}";

    public object BuildPaymentResponse(Transaction transaction, string? message = null) => new PaymentResponse
    {
        TransactionId = transaction.Id,
        GatewayTransactionId = transaction.GatewayTransactionId,
        MerchantReference = transaction.MerchantReference,
        Amount = transaction.Amount,
        Currency = transaction.Currency,
        Status = transaction.Status.ToString(),
        Message = message
    };

    public object BuildHostedCheckoutResponse(Transaction transaction, string redirectUrl, string? message = null) => new HostedCheckoutResponse
    {
        TransactionId = transaction.Id,
        GatewayTransactionId = transaction.GatewayTransactionId,
        MerchantReference = transaction.MerchantReference,
        Amount = transaction.Amount,
        Currency = transaction.Currency,
        Status = transaction.Status.ToString(),
        RedirectUrl = redirectUrl,
        Message = message
    };

    public object BuildTransactionDto(Transaction transaction) => MapTransactionDto(transaction);

    public object BuildTransactionStatusDto(Transaction transaction) => new TransactionStatusDto
    {
        TransactionId = transaction.Id,
        Status = transaction.Status.ToString(),
        FailureReason = transaction.FailureReason,
        UpdatedAt = transaction.UpdatedAt
    };

    public object BuildTransactionListResponse(List<Transaction> transactions, int page, int pageSize, int total) => new TransactionListResponse
    {
        Items = transactions.Select(MapTransactionDto).ToList(),
        Page = page,
        PageSize = pageSize,
        Total = total
    };

    public object BuildPayoutResponse(Payout payout, string? message = null) => new PayoutResponse
    {
        PayoutId = payout.Id,
        GatewayPayoutId = payout.GatewayPayoutId,
        MerchantReference = payout.MerchantReference,
        Amount = payout.Amount,
        Currency = payout.Currency,
        Status = payout.Status.ToString(),
        Message = message
    };

    public object BuildPayoutDto(Payout payout) => MapPayoutDto(payout);

    public object BuildPayoutListResponse(List<Payout> payouts, int page, int pageSize, int total) => new PayoutListResponse
    {
        Items = payouts.Select(MapPayoutDto).ToList(),
        Page = page,
        PageSize = pageSize,
        Total = total
    };

    public object BuildTransactionWebhookPayload(Transaction transaction, string eventType) => new WebhookPayload
    {
        Event = eventType,
        GatewayTransactionId = transaction.GatewayTransactionId,
        MerchantReference = transaction.MerchantReference,
        ExternalReference = transaction.ExternalReference,
        ReceiptNumber = transaction.ReceiptNumber,
        Amount = transaction.Amount,
        Currency = transaction.Currency,
        Provider = transaction.Provider.ToString(),
        PaymentMethod = transaction.PaymentMethod.ToString(),
        Status = transaction.Status.ToString(),
        FailureReason = transaction.FailureReason
    };

    public object BuildPayoutWebhookPayload(Payout payout, string eventType) => new PayoutWebhookPayload
    {
        Event = eventType,
        GatewayPayoutId = payout.GatewayPayoutId,
        MerchantReference = payout.MerchantReference,
        Amount = payout.Amount,
        Currency = payout.Currency,
        PaymentMethod = payout.PaymentMethod.ToString(),
        Provider = payout.Provider.ToString(),
        Status = payout.Status.ToString(),
        FailureReason = payout.FailureReason
    };

    private static TransactionDto MapTransactionDto(Transaction transaction) => new()
    {
        TransactionId = transaction.Id,
        GatewayTransactionId = transaction.GatewayTransactionId,
        MerchantReference = transaction.MerchantReference,
        ExternalReference = transaction.ExternalReference,
        ReceiptNumber = transaction.ReceiptNumber,
        Amount = transaction.Amount,
        Currency = transaction.Currency,
        Provider = transaction.Provider.ToString(),
        PaymentMethod = transaction.PaymentMethod.ToString(),
        Status = transaction.Status.ToString(),
        FailureReason = transaction.FailureReason,
        Scenario = transaction.Scenario,
        CreatedAt = transaction.CreatedAt,
        UpdatedAt = transaction.UpdatedAt,
        CompletedAt = transaction.CompletedAt
    };

    private static PayoutDto MapPayoutDto(Payout payout) => new()
    {
        PayoutId = payout.Id,
        GatewayPayoutId = payout.GatewayPayoutId,
        MerchantReference = payout.MerchantReference,
        Amount = payout.Amount,
        Currency = payout.Currency,
        PaymentMethod = payout.PaymentMethod.ToString(),
        Provider = payout.Provider.ToString(),
        Status = payout.Status.ToString(),
        FailureReason = payout.FailureReason,
        CreatedAt = payout.CreatedAt,
        UpdatedAt = payout.UpdatedAt,
        CompletedAt = payout.CompletedAt
    };
}
