using Microsoft.AspNetCore.Mvc;
using PaymentMock.Controllers.Interfaces;
using PaymentMock.Enums;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Controllers.Implementations;

[ApiController]
[Route("api/v1/admin")]
[Produces("application/json")]
public class AdminController : ControllerBase, IAdminController
{
    private readonly ITransactionService _transactionService;
    private readonly IPayoutService _payoutService;
    private readonly ILoggingQueryService _loggingQueryService;
    private readonly IConfigurationService _configurationService;

    public AdminController(
        ITransactionService transactionService,
        IPayoutService payoutService,
        ILoggingQueryService loggingQueryService,
        IConfigurationService configurationService)
    {
        _transactionService = transactionService;
        _payoutService = payoutService;
        _loggingQueryService = loggingQueryService;
        _configurationService = configurationService;
    }

    [HttpGet("transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] TransactionStatus? status,
        [FromQuery] PaymentProvider? provider,
        [FromQuery] PaymentMethod? paymentMethod,
        [FromQuery] string? merchantReference,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20) =>
        Ok(await _transactionService.SearchAsync(status, provider, paymentMethod, merchantReference, fromDate, toDate, page, pageSize));

    [HttpGet("payouts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayouts([FromQuery] PayoutStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(await _payoutService.SearchAsync(status, page, pageSize));

    [HttpGet("webhooks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWebhookDeliveries([FromQuery] WebhookDeliveryStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(await _loggingQueryService.GetWebhookDeliveriesAsync(status, page, pageSize));

    [HttpGet("errors")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetErrors([FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(await _loggingQueryService.GetErrorsAsync(page, pageSize));

    [HttpGet("scenarios")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScenarioExecutions([FromQuery] string? scenarioName, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(await _loggingQueryService.GetScenarioExecutionsAsync(scenarioName, page, pageSize));

    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetConfiguration() => Ok(_configurationService.GetEffectiveConfiguration());

    [HttpGet("config-history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfigurationHistory([FromQuery] int limit = 20) =>
        Ok(await _configurationService.GetHistoryAsync(limit));
}
