using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;

namespace PaymentMock.Repositories.Implementations;

public class IdempotencyRepository : BaseRepository<IdempotencyRecord>, IIdempotencyRepository
{
    public IdempotencyRepository(string connectionString) : base(connectionString, "idempotency_keys") { }

    public async Task<IdempotencyRecord?> GetByKeyAndPathAsync(string idempotencyKey, string requestPath) =>
        await QueryFirstOrDefaultAsync<IdempotencyRecord>(
            "SELECT * FROM idempotency_keys WHERE IdempotencyKey = @IdempotencyKey AND RequestPath = @RequestPath AND ExpiresAt > UTC_TIMESTAMP()",
            new { IdempotencyKey = idempotencyKey, RequestPath = requestPath });
}
