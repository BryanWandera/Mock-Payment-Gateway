using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;

namespace PaymentMock.Repositories.Implementations;

public class ScenarioExecutionRepository : BaseRepository<ScenarioExecution>, IScenarioExecutionRepository
{
    public ScenarioExecutionRepository(string connectionString) : base(connectionString, "scenario_executions") { }

    public async Task<List<ScenarioExecution>> SearchAsync(string? scenarioName, int page, int pageSize)
    {
        var sql = string.IsNullOrWhiteSpace(scenarioName)
            ? "SELECT * FROM scenario_executions ORDER BY ExecutedAt DESC LIMIT @PageSize OFFSET @Offset"
            : "SELECT * FROM scenario_executions WHERE ScenarioName = @ScenarioName ORDER BY ExecutedAt DESC LIMIT @PageSize OFFSET @Offset";
        var result = await QueryAsync<ScenarioExecution>(sql, new
        {
            ScenarioName = scenarioName,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });
        return result.ToList();
    }

    public async Task<int> CountAsync(string? scenarioName)
    {
        var sql = string.IsNullOrWhiteSpace(scenarioName)
            ? "SELECT COUNT(*) FROM scenario_executions"
            : "SELECT COUNT(*) FROM scenario_executions WHERE ScenarioName = @ScenarioName";
        return await QueryFirstOrDefaultAsync<int>(sql, new { ScenarioName = scenarioName });
    }
}
