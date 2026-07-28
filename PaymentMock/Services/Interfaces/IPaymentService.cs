using PaymentMock.DTOs.Generic;

namespace PaymentMock.Services.Interfaces;

public interface IPaymentService
{
    Task<object> InitiateMobileMoneyPaymentAsync(MobileMoneyPaymentRequest request);
    Task<object> InitiateCardPaymentAsync(CardPaymentRequest request);
    Task<object> InitiateHostedCheckoutAsync(HostedCheckoutRequest request);
}
