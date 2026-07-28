using Microsoft.AspNetCore.Mvc;
using PaymentMock.DTOs.Generic;

namespace PaymentMock.Controllers.Interfaces;

public interface IPaymentsController
{
    Task<IActionResult> InitiateMobileMoneyPayment(MobileMoneyPaymentRequest request);
    Task<IActionResult> InitiateCardPayment(CardPaymentRequest request);
    Task<IActionResult> InitiateHostedCheckout(HostedCheckoutRequest request);
}
