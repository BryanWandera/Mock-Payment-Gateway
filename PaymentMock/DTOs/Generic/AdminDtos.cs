namespace PaymentMock.DTOs.Generic;

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}

public class WebhookLogDto
{
    public string Id { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? PayoutId { get; set; }
    public string TargetUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public int? HttpStatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

public class ErrorLogDto
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ScenarioLogDto
{
    public string Id { get; set; } = string.Empty;
    public string ScenarioName { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? PayoutId { get; set; }
    public string? Details { get; set; }
    public DateTime ExecutedAt { get; set; }
}

public class ConfigSummaryDto
{
    public string GatewayProfile { get; set; } = string.Empty;
    public List<string> SupportedCurrencies { get; set; } = new();
    public List<string> SupportedProviders { get; set; } = new();
    public string DefaultScenario { get; set; } = string.Empty;
    public List<string> ConfiguredScenarios { get; set; } = new();
    public int StkPushMs { get; set; }
    public int PinEntryMs { get; set; }
    public int ProcessingMs { get; set; }
    public int PayoutProcessingMs { get; set; }
    public int TimingVariancePercent { get; set; }
    public bool WebhooksEnabled { get; set; }
    public int WebhookMaxRetryAttempts { get; set; }
    public int ConfiguredApiKeyCount { get; set; }
}

public class ConfigHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string ConfigSection { get; set; } = string.Empty;
    public string Snapshot { get; set; } = string.Empty;
    public string? ChangedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
