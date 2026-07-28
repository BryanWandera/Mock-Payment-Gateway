using PaymentMock.Enums;
using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;

namespace PaymentMock.Repositories.Implementations;

public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(string connectionString) : base(connectionString, "transactions") { }

    public async Task<Transaction?> GetByGatewayTransactionIdAsync(string gatewayTransactionId) =>
        await QueryFirstOrDefaultAsync<Transaction>(
            "SELECT * FROM transactions WHERE GatewayTransactionId = @GatewayTransactionId",
            new { GatewayTransactionId = gatewayTransactionId });

    public async Task<Transaction?> GetByMerchantReferenceAsync(string merchantReference) =>
        await QueryFirstOrDefaultAsync<Transaction>(
            "SELECT * FROM transactions WHERE MerchantReference = @MerchantReference ORDER BY CreatedAt DESC LIMIT 1",
            new { MerchantReference = merchantReference });

    public async Task<Transaction?> GetByCheckoutTokenAsync(string checkoutToken) =>
        await QueryFirstOrDefaultAsync<Transaction>(
            "SELECT * FROM transactions WHERE CheckoutToken = @CheckoutToken",
            new { CheckoutToken = checkoutToken });

    public async Task<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey) =>
        await QueryFirstOrDefaultAsync<Transaction>(
            "SELECT * FROM transactions WHERE IdempotencyKey = @IdempotencyKey ORDER BY CreatedAt DESC LIMIT 1",
            new { IdempotencyKey = idempotencyKey });

    public async Task<List<Transaction>> SearchAsync(
        TransactionStatus? status,
        PaymentProvider? provider,
        PaymentMethod? paymentMethod,
        string? merchantReference,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize)
    {
        var conditions = new List<string> { "1=1" };
        var parameters = new Dictionary<string, object?>();

        if (status.HasValue) { conditions.Add("Status = @Status"); parameters["Status"] = status.Value.ToString(); }
        if (provider.HasValue) { conditions.Add("Provider = @Provider"); parameters["Provider"] = provider.Value.ToString(); }
        if (paymentMethod.HasValue) { conditions.Add("PaymentMethod = @PaymentMethod"); parameters["PaymentMethod"] = paymentMethod.Value.ToString(); }
        if (!string.IsNullOrWhiteSpace(merchantReference)) { conditions.Add("MerchantReference = @MerchantReference"); parameters["MerchantReference"] = merchantReference; }
        if (fromDate.HasValue) { conditions.Add("CreatedAt >= @FromDate"); parameters["FromDate"] = fromDate.Value; }
        if (toDate.HasValue) { conditions.Add("CreatedAt <= @ToDate"); parameters["ToDate"] = toDate.Value; }

        parameters["Offset"] = (page - 1) * pageSize;
        parameters["PageSize"] = pageSize;

        var sql = $@"SELECT * FROM transactions WHERE {string.Join(" AND ", conditions)}
                     ORDER BY CreatedAt DESC LIMIT @PageSize OFFSET @Offset";
        var result = await QueryAsync<Transaction>(sql, parameters);
        return result.ToList();
    }

    public async Task<List<Transaction>> GetPendingProcessingAsync(int limit)
    {
        var sql = @"SELECT * FROM transactions
                    WHERE Status IN ('Created', 'Pending', 'Processing')
                    ORDER BY CreatedAt ASC LIMIT @Limit";
        var result = await QueryAsync<Transaction>(sql, new { Limit = limit });
        return result.ToList();
    }

    public async Task<int> CountAsync(
        TransactionStatus? status,
        PaymentProvider? provider,
        PaymentMethod? paymentMethod,
        string? merchantReference,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var conditions = new List<string> { "1=1" };
        var parameters = new Dictionary<string, object?>();

        if (status.HasValue) { conditions.Add("Status = @Status"); parameters["Status"] = status.Value.ToString(); }
        if (provider.HasValue) { conditions.Add("Provider = @Provider"); parameters["Provider"] = provider.Value.ToString(); }
        if (paymentMethod.HasValue) { conditions.Add("PaymentMethod = @PaymentMethod"); parameters["PaymentMethod"] = paymentMethod.Value.ToString(); }
        if (!string.IsNullOrWhiteSpace(merchantReference)) { conditions.Add("MerchantReference = @MerchantReference"); parameters["MerchantReference"] = merchantReference; }
        if (fromDate.HasValue) { conditions.Add("CreatedAt >= @FromDate"); parameters["FromDate"] = fromDate.Value; }
        if (toDate.HasValue) { conditions.Add("CreatedAt <= @ToDate"); parameters["ToDate"] = toDate.Value; }

        var sql = $"SELECT COUNT(*) FROM transactions WHERE {string.Join(" AND ", conditions)}";
        return await QueryFirstOrDefaultAsync<int>(sql, parameters);
    }
}
