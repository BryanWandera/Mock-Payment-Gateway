using PaymentMock.Enums;
using PaymentMock.Models;

namespace PaymentMock.Services.Interfaces;

public interface IGatewayProfileStrategy
{
    GatewayProfileType Profile { get; }

    string GenerateGatewayTransactionId();
    string GenerateGatewayPayoutId();

    object BuildPaymentResponse(Transaction transaction, string? message = null);
    object BuildHostedCheckoutResponse(Transaction transaction, string redirectUrl, string? message = null);
    object BuildTransactionDto(Transaction transaction);
    object BuildTransactionStatusDto(Transaction transaction);
    object BuildTransactionListResponse(List<Transaction> transactions, int page, int pageSize, int total);

    object BuildPayoutResponse(Payout payout, string? message = null);
    object BuildPayoutDto(Payout payout);
    object BuildPayoutListResponse(List<Payout> payouts, int page, int pageSize, int total);

    object BuildTransactionWebhookPayload(Transaction transaction, string eventType);
    object BuildPayoutWebhookPayload(Payout payout, string eventType);
}
