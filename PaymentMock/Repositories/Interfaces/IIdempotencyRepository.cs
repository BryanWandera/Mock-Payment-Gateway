using PaymentMock.Models;

namespace PaymentMock.Repositories.Interfaces;

public interface IIdempotencyRepository : IBaseRepository<IdempotencyRecord>
{
    Task<IdempotencyRecord?> GetByKeyAndPathAsync(string idempotencyKey, string requestPath);
}
