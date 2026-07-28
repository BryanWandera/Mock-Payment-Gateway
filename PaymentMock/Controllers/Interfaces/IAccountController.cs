using Microsoft.AspNetCore.Mvc;

namespace PaymentMock.Controllers.Interfaces;

public interface IAccountController
{
    Task<IActionResult> GetAccountSummary();
}
