using PaymentMock.Models;

namespace PaymentMock.Repositories.Interfaces;

public interface IGatewayAccountRepository : IBaseRepository<GatewayAccount>
{
    Task<GatewayAccount?> GetByCurrencyAsync(string currency);
    Task IncrementTransactionCountAsync(string currency, decimal amount);
    Task IncrementPayoutCountAsync(string currency, decimal amount);
}
