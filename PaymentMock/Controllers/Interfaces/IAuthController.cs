using Microsoft.AspNetCore.Mvc;
using PaymentMock.DTOs.Pesapal;

namespace PaymentMock.Controllers.Interfaces;

public interface IAuthController
{
    Task<IActionResult> RequestToken(PesapalAuthTokenRequest request);
}
