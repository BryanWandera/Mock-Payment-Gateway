using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;

namespace PaymentMock.Repositories.Implementations;

public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(string connectionString) : base(connectionString, "audit_logs") { }

    public async Task<List<AuditLog>> SearchAsync(string? logType, int page, int pageSize)
    {
        var sql = string.IsNullOrWhiteSpace(logType)
            ? "SELECT * FROM audit_logs ORDER BY CreatedAt DESC LIMIT @PageSize OFFSET @Offset"
            : "SELECT * FROM audit_logs WHERE LogType = @LogType ORDER BY CreatedAt DESC LIMIT @PageSize OFFSET @Offset";
        var result = await QueryAsync<AuditLog>(sql, new
        {
            LogType = logType,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });
        return result.ToList();
    }

    public async Task<int> CountAsync(string? logType)
    {
        var sql = string.IsNullOrWhiteSpace(logType)
            ? "SELECT COUNT(*) FROM audit_logs"
            : "SELECT COUNT(*) FROM audit_logs WHERE LogType = @LogType";
        return await QueryFirstOrDefaultAsync<int>(sql, new { LogType = logType });
    }
}
