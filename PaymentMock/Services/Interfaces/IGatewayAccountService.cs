using PaymentMock.DTOs.Generic;

namespace PaymentMock.Services.Interfaces;

public interface IGatewayAccountService
{
    Task<AccountDto> GetAccountSummaryAsync();
}
