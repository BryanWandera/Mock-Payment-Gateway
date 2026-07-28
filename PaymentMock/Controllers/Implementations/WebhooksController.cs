using Microsoft.AspNetCore.Mvc;
using PaymentMock.Controllers.Interfaces;
using PaymentMock.DTOs.Generic;
using PaymentMock.Extensions;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Controllers.Implementations;

[ApiController]
[Route("api/v1/webhooks")]
[Produces("application/json")]
public class WebhooksController : ControllerBase, IWebhooksController
{
    private readonly IWebhookService _webhookService;

    public WebhooksController(IWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] WebhookRegistrationRequest request)
    {
        ModelState.ThrowIfInvalid();
        var registration = await _webhookService.RegisterAsync(request.Url, request.NotificationType);

        return Ok(new WebhookRegistrationResponse
        {
            RegistrationId = registration.Id,
            Url = registration.Url,
            CreatedAt = registration.CreatedAt
        });
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List()
    {
        var registrations = await _webhookService.ListRegistrationsAsync();
        return Ok(registrations.Select(r => new WebhookRegistrationResponse
        {
            RegistrationId = r.Id,
            Url = r.Url,
            CreatedAt = r.CreatedAt
        }));
    }
}
