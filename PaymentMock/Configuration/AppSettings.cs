namespace PaymentMock.Configuration;

public class AppSettings
{
    public string GatewayProfile { get; set; } = "Generic";
    public DatabaseSettings Database { get; set; } = new();
    public AuthenticationSettings Authentication { get; set; } = new();
    public GatewaySettings Gateway { get; set; } = new();
    public Dictionary<string, ScenarioSettings> Scenarios { get; set; } = new();
    public WebhookSettings Webhooks { get; set; } = new();
}

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}

public class AuthenticationSettings
{
    public List<ApiKeySettings> ApiKeys { get; set; } = new();
    public List<PesapalConsumerKeySettings> PesapalConsumerKeys { get; set; } = new();
}

public class ApiKeySettings
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class PesapalConsumerKeySettings
{
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
}

public class GatewaySettings
{
    public List<string> SupportedCurrencies { get; set; } = new() { "KES", "UGX", "TZS", "USD" };
    public List<string> SupportedProviders { get; set; } = new() { "MTN", "Airtel", "Visa", "Mastercard" };
    public string DefaultScenario { get; set; } = "HappyPath";
    public ProcessingDelaySettings ProcessingDelays { get; set; } = new();
    public int TimingVariancePercent { get; set; } = 20;

    /// <summary>
    /// Base URL this gateway is reachable at from a phone/browser/emulator opening the hosted
    /// checkout page — must not be "localhost" outside of same-machine testing.
    /// </summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:8080";
}

public class ProcessingDelaySettings
{
    public int StkPushMs { get; set; } = 2000;
    public int PinEntryMs { get; set; } = 5000;
    public int ProcessingMs { get; set; } = 3000;
    public int PayoutProcessingMs { get; set; } = 4000;
}

public class ScenarioSettings
{
    public string? FinalStatus { get; set; }
    public string? FailureReason { get; set; }
    public string? StuckAtStatus { get; set; }
    public double ProcessingDelayMultiplier { get; set; } = 1;
    public int SuccessRatePercent { get; set; } = 100;
    public bool FailFast { get; set; }
    public bool SkipWebhook { get; set; }
    public int WebhookDelayMs { get; set; }
    public int WebhookDuplicateCount { get; set; } = 1;
    public bool SimulateInvalidSignature { get; set; }
    public bool SimulateOutOfOrderWebhook { get; set; }
    public bool SimulateGatewayError { get; set; }
    public bool SimulateDuplicateRequest { get; set; }
    public bool SimulateTimeout { get; set; }
    public bool SimulateProviderUnavailable { get; set; }
}

public class WebhookSettings
{
    public bool Enabled { get; set; } = true;
    public string DefaultUrl { get; set; } = string.Empty;
    public string SignatureSecret { get; set; } = string.Empty;
    public bool SimulateInvalidSignature { get; set; }
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 5000;
}
