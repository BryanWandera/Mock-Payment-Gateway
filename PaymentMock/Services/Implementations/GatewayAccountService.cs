using Microsoft.Extensions.Options;
using PaymentMock.Configuration;
using PaymentMock.DTOs.Generic;
using PaymentMock.Repositories.Interfaces;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Implementations;

public class GatewayAccountService : IGatewayAccountService
{
    private readonly IGatewayAccountRepository _gatewayAccountRepository;
    private readonly GatewaySettings _gatewaySettings;

    public GatewayAccountService(IGatewayAccountRepository gatewayAccountRepository, IOptions<AppSettings> options)
    {
        _gatewayAccountRepository = gatewayAccountRepository;
        _gatewaySettings = options.Value.Gateway;
    }

    public async Task<AccountDto> GetAccountSummaryAsync()
    {
        var accounts = await _gatewayAccountRepository.GetAllAsync();
        var defaultCurrency = _gatewaySettings.SupportedCurrencies.FirstOrDefault() ?? "KES";
        var referenceAccount = accounts.FirstOrDefault(a => a.Currency == defaultCurrency) ?? accounts.FirstOrDefault();

        return new AccountDto
        {
            CurrentBalance = referenceAccount?.CurrentBalance ?? 0,
            AvailableBalance = referenceAccount?.AvailableBalance ?? 0,
            CurrencyBalances = accounts.Select(a => new CurrencyBalanceDto
            {
                Currency = a.Currency,
                CurrentBalance = a.CurrentBalance,
                AvailableBalance = a.AvailableBalance
            }).ToList(),
            TotalTransactions = accounts.Sum(a => a.TotalTransactions),
            TotalPayouts = accounts.Sum(a => a.TotalPayouts)
        };
    }
}
