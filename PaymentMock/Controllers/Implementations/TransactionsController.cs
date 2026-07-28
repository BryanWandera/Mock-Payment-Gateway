using Microsoft.AspNetCore.Mvc;
using PaymentMock.Controllers.Interfaces;
using PaymentMock.Enums;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Controllers.Implementations;

[ApiController]
[Route("api/v1/transactions")]
[Produces("application/json")]
public class TransactionsController : ControllerBase, ITransactionsController
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var transaction = await _transactionService.GetByIdAsync(id);
        return Ok(transaction);
    }

    [HttpGet("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(string id)
    {
        var status = await _transactionService.GetStatusAsync(id);
        return Ok(status);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] TransactionStatus? status,
        [FromQuery] PaymentProvider? provider,
        [FromQuery] PaymentMethod? paymentMethod,
        [FromQuery] string? merchantReference,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _transactionService.SearchAsync(status, provider, paymentMethod, merchantReference, fromDate, toDate, page, pageSize);
        return Ok(result);
    }
}
