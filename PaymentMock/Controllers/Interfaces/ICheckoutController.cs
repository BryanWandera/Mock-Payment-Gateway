using Microsoft.AspNetCore.Mvc;

namespace PaymentMock.Controllers.Interfaces;

public interface ICheckoutController
{
    Task<IActionResult> GetCheckoutPage(string token);
    Task<IActionResult> Approve(string token);
    Task<IActionResult> Decline(string token);
    Task<IActionResult> GetCompletePage(string token);
}
