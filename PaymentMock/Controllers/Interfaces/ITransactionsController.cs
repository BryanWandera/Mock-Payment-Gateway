using Microsoft.AspNetCore.Mvc;
using PaymentMock.Enums;

namespace PaymentMock.Controllers.Interfaces;

public interface ITransactionsController
{
    Task<IActionResult> GetById(string id);
    Task<IActionResult> GetStatus(string id);
    Task<IActionResult> Search(
        TransactionStatus? status,
        PaymentProvider? provider,
        PaymentMethod? paymentMethod,
        string? merchantReference,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize);
}
