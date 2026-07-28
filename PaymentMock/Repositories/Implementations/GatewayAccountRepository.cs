using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;

namespace PaymentMock.Repositories.Implementations;

public class GatewayAccountRepository : BaseRepository<GatewayAccount>, IGatewayAccountRepository
{
    public GatewayAccountRepository(string connectionString) : base(connectionString, "gateway_account") { }

    public async Task<GatewayAccount?> GetByCurrencyAsync(string currency) =>
        await QueryFirstOrDefaultAsync<GatewayAccount>(
            "SELECT * FROM gateway_account WHERE Currency = @Currency",
            new { Currency = currency });

    public async Task IncrementTransactionCountAsync(string currency, decimal amount)
    {
        await ExecuteAsync(
            @"UPDATE gateway_account SET
                TotalTransactions = TotalTransactions + 1,
                CurrentBalance = CurrentBalance + @Amount,
                AvailableBalance = AvailableBalance + @Amount,
                UpdatedAt = UTC_TIMESTAMP()
              WHERE Currency = @Currency",
            new { Currency = currency, Amount = amount });
    }

    public async Task IncrementPayoutCountAsync(string currency, decimal amount)
    {
        await ExecuteAsync(
            @"UPDATE gateway_account SET
                TotalPayouts = TotalPayouts + 1,
                CurrentBalance = CurrentBalance - @Amount,
                AvailableBalance = AvailableBalance - @Amount,
                UpdatedAt = UTC_TIMESTAMP()
              WHERE Currency = @Currency",
            new { Currency = currency, Amount = amount });
    }
}
