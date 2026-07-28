using PaymentMock.Models;

namespace PaymentMock.Repositories.Interfaces;

public interface IAuditLogRepository : IBaseRepository<AuditLog>
{
    Task<List<AuditLog>> SearchAsync(string? logType, int page, int pageSize);
    Task<int> CountAsync(string? logType);
}
