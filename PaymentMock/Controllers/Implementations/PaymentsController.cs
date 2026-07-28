using Microsoft.AspNetCore.Mvc;
using PaymentMock.Controllers.Interfaces;
using PaymentMock.DTOs.Generic;
using PaymentMock.Extensions;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Controllers.Implementations;

[ApiController]
[Route("api/v1/payments")]
[Produces("application/json")]
public class PaymentsController : ControllerBase, IPaymentsController
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("mobile-money")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitiateMobileMoneyPayment([FromBody] MobileMoneyPaymentRequest request)
    {
        ModelState.ThrowIfInvalid();
        var response = await _paymentService.InitiateMobileMoneyPaymentAsync(request);
        return Ok(response);
    }

    [HttpPost("card")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitiateCardPayment([FromBody] CardPaymentRequest request)
    {
        ModelState.ThrowIfInvalid();
        var response = await _paymentService.InitiateCardPaymentAsync(request);
        return Ok(response);
    }

    [HttpPost("checkout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitiateHostedCheckout([FromBody] HostedCheckoutRequest request)
    {
        ModelState.ThrowIfInvalid();
        var response = await _paymentService.InitiateHostedCheckoutAsync(request);
        return Ok(response);
    }
}
