using Microsoft.AspNetCore.Mvc;
using PaymentMock.DTOs.Generic;
using PaymentMock.Enums;

namespace PaymentMock.Controllers.Interfaces;

public interface IPayoutsController
{
    Task<IActionResult> InitiatePayout(PayoutRequest request);
    Task<IActionResult> GetById(string id);
    Task<IActionResult> Search(PayoutStatus? status, int page, int pageSize);
}
