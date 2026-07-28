using PaymentMock.Models;

namespace PaymentMock.Repositories.Interfaces;

public interface IScenarioExecutionRepository : IBaseRepository<ScenarioExecution>
{
    Task<List<ScenarioExecution>> SearchAsync(string? scenarioName, int page, int pageSize);
    Task<int> CountAsync(string? scenarioName);
}
