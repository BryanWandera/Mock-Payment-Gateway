using Microsoft.AspNetCore.Mvc;
using PaymentMock.Controllers.Interfaces;
using PaymentMock.DTOs.Pesapal;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Controllers.Implementations;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase, IAuthController
{
    private readonly IAuthenticationService _authenticationService;

    public AuthController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Mints a bearer token from a consumer key/secret pair. Only meaningful when GatewayProfile is set to
    /// "Pesapal" — mirrors Pesapal's real RequestToken flow so client code can be swapped with minimal changes.
    /// </summary>
    [HttpPost("token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RequestToken([FromBody] PesapalAuthTokenRequest request)
    {
        var (token, expiresAt) = await _authenticationService.IssueTokenAsync(request.ConsumerKey, request.ConsumerSecret);

        return Ok(new PesapalAuthTokenResponse
        {
            Token = token,
            ExpiryDate = expiresAt.ToString("O"),
            Message = "Token issued successfully",
            Status = "200"
        });
    }
}
