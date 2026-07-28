using Microsoft.AspNetCore.Mvc;
using PaymentMock.Controllers.Interfaces;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Controllers.Implementations;

[ApiController]
[Route("api/v1/account")]
[Produces("application/json")]
public class AccountController : ControllerBase, IAccountController
{
    private readonly IGatewayAccountService _gatewayAccountService;

    public AccountController(IGatewayAccountService gatewayAccountService)
    {
        _gatewayAccountService = gatewayAccountService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountSummary()
    {
        var summary = await _gatewayAccountService.GetAccountSummaryAsync();
        return Ok(summary);
    }
}
