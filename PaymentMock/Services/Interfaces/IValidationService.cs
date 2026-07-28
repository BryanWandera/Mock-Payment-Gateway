using PaymentMock.DTOs.Generic;

namespace PaymentMock.Services.Interfaces;

public interface IValidationService
{
    void ValidateMobileMoneyPayment(MobileMoneyPaymentRequest request);
    void ValidateCardPayment(CardPaymentRequest request);
    void ValidateHostedCheckout(HostedCheckoutRequest request);
    void ValidatePayout(PayoutRequest request);
}
