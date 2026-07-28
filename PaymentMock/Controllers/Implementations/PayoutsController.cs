using Microsoft.AspNetCore.Mvc;
using PaymentMock.Controllers.Interfaces;
using PaymentMock.DTOs.Generic;
using PaymentMock.Enums;
using PaymentMock.Extensions;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Controllers.Implementations;

[ApiController]
[Route("api/v1/payouts")]
[Produces("application/json")]
public class PayoutsController : ControllerBase, IPayoutsController
{
    private readonly IPayoutService _payoutService;

    public PayoutsController(IPayoutService payoutService)
    {
        _payoutService = payoutService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitiatePayout([FromBody] PayoutRequest request)
    {
        ModelState.ThrowIfInvalid();
        var response = await _payoutService.InitiatePayoutAsync(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var payout = await _payoutService.GetByIdAsync(id);
        return Ok(payout);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] PayoutStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _payoutService.SearchAsync(status, page, pageSize);
        return Ok(result);
    }
}
