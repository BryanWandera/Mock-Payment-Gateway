using System.Text.Json;
using Microsoft.Extensions.Options;
using PaymentMock.Configuration;
using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Implementations;

public class ScenarioEngine : IScenarioEngine
{
    private static readonly ScenarioSettings FallbackHappyPath = new() { FinalStatus = "Completed" };

    private readonly IScenarioExecutionRepository _scenarioExecutionRepository;
    private readonly AppSettings _appSettings;
    private readonly Dictionary<string, ScenarioSettings> _scenarios;

    public ScenarioEngine(IScenarioExecutionRepository scenarioExecutionRepository, IOptions<AppSettings> options)
    {
        _scenarioExecutionRepository = scenarioExecutionRepository;
        _appSettings = options.Value;
        _scenarios = new Dictionary<string, ScenarioSettings>(_appSettings.Scenarios, StringComparer.OrdinalIgnoreCase);
    }

    public (string ScenarioName, ScenarioSettings Settings) ResolveSettings(string? requestedScenario)
    {
        var candidate = !string.IsNullOrWhiteSpace(requestedScenario) ? requestedScenario : _appSettings.Gateway.DefaultScenario;

        if (_scenarios.TryGetValue(candidate, out var settings))
            return (candidate, settings);

        if (_scenarios.TryGetValue(_appSettings.Gateway.DefaultScenario, out var defaultSettings))
            return (_appSettings.Gateway.DefaultScenario, defaultSettings);

        return ("HappyPath", FallbackHappyPath);
    }

    public async Task<ScenarioOutcome> ResolveAndRecordAsync(string? requestedScenario, string? transactionId = null, string? payoutId = null)
    {
        var (name, settings) = ResolveSettings(requestedScenario);

        bool shouldSucceed;
        if (!string.IsNullOrWhiteSpace(settings.FinalStatus))
            shouldSucceed = settings.FinalStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase);
        else
            shouldSucceed = Random.Shared.Next(1, 101) <= settings.SuccessRatePercent;

        var failureReason = shouldSucceed ? null : settings.FailureReason ?? "Transaction declined";

        var outcome = new ScenarioOutcome
        {
            ScenarioName = name,
            ShouldSucceed = shouldSucceed,
            FailureReason = failureReason,
            StuckAtStatus = settings.StuckAtStatus,
            ProcessingDelayMultiplier = settings.ProcessingDelayMultiplier <= 0 ? 1 : settings.ProcessingDelayMultiplier,
            FailFast = settings.FailFast,
            SimulateTimeout = settings.SimulateTimeout,
            SkipWebhook = settings.SkipWebhook,
            WebhookDelayMs = settings.WebhookDelayMs,
            WebhookDuplicateCount = Math.Max(1, settings.WebhookDuplicateCount),
            SimulateInvalidSignature = settings.SimulateInvalidSignature,
            SimulateOutOfOrderWebhook = settings.SimulateOutOfOrderWebhook
        };

        await _scenarioExecutionRepository.CreateAsync(new ScenarioExecution
        {
            ScenarioName = name,
            TransactionId = transactionId,
            PayoutId = payoutId,
            Details = JsonSerializer.Serialize(outcome)
        });

        return outcome;
    }
}
