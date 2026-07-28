using PaymentMock.DTOs.Generic;

namespace PaymentMock.Services.Interfaces;

public interface ICheckoutService
{
    Task<CheckoutPageResult> GetCheckoutPageAsync(string checkoutToken);
    Task<CheckoutPageResult> ApproveAsync(string checkoutToken);
    Task<CheckoutPageResult> DeclineAsync(string checkoutToken);
}
