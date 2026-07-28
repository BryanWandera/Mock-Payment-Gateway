using PaymentMock.Configuration;

namespace PaymentMock.Services.Interfaces;

public class ScenarioOutcome
{
    public string ScenarioName { get; set; } = "HappyPath";
    public bool ShouldSucceed { get; set; } = true;
    public string? FailureReason { get; set; }
    public string? StuckAtStatus { get; set; }
    public double ProcessingDelayMultiplier { get; set; } = 1;
    public bool FailFast { get; set; }
    public bool SimulateTimeout { get; set; }
    public bool SkipWebhook { get; set; }
    public int WebhookDelayMs { get; set; }
    public int WebhookDuplicateCount { get; set; } = 1;
    public bool SimulateInvalidSignature { get; set; }
    public bool SimulateOutOfOrderWebhook { get; set; }
}

public interface IScenarioEngine
{
    (string ScenarioName, ScenarioSettings Settings) ResolveSettings(string? requestedScenario);

    Task<ScenarioOutcome> ResolveAndRecordAsync(string? requestedScenario, string? transactionId = null, string? payoutId = null);
}
