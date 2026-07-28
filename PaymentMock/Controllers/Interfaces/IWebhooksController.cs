using Microsoft.AspNetCore.Mvc;
using PaymentMock.DTOs.Generic;

namespace PaymentMock.Controllers.Interfaces;

public interface IWebhooksController
{
    Task<IActionResult> Register(WebhookRegistrationRequest request);
    Task<IActionResult> List();
}
