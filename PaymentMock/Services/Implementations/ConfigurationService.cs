using System.Text.Json;
using Microsoft.Extensions.Options;
using PaymentMock.Configuration;
using PaymentMock.DTOs.Generic;
using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Implementations;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigHistoryRepository _configHistoryRepository;
    private readonly AppSettings _appSettings;

    public ConfigurationService(IConfigHistoryRepository configHistoryRepository, IOptions<AppSettings> options)
    {
        _configHistoryRepository = configHistoryRepository;
        _appSettings = options.Value;
    }

    public ConfigSummaryDto GetEffectiveConfiguration() => new()
    {
        GatewayProfile = _appSettings.GatewayProfile,
        SupportedCurrencies = _appSettings.Gateway.SupportedCurrencies,
        SupportedProviders = _appSettings.Gateway.SupportedProviders,
        DefaultScenario = _appSettings.Gateway.DefaultScenario,
        ConfiguredScenarios = _appSettings.Scenarios.Keys.ToList(),
        StkPushMs = _appSettings.Gateway.ProcessingDelays.StkPushMs,
        PinEntryMs = _appSettings.Gateway.ProcessingDelays.PinEntryMs,
        ProcessingMs = _appSettings.Gateway.ProcessingDelays.ProcessingMs,
        PayoutProcessingMs = _appSettings.Gateway.ProcessingDelays.PayoutProcessingMs,
        TimingVariancePercent = _appSettings.Gateway.TimingVariancePercent,
        WebhooksEnabled = _appSettings.Webhooks.Enabled,
        WebhookMaxRetryAttempts = _appSettings.Webhooks.MaxRetryAttempts,
        ConfiguredApiKeyCount = _appSettings.Authentication.ApiKeys.Count
    };

    public async Task RecordSnapshotAsync(string section, object snapshot, string? changedBy = null)
    {
        await _configHistoryRepository.CreateAsync(new ConfigHistory
        {
            ConfigSection = section,
            Snapshot = JsonSerializer.Serialize(snapshot),
            ChangedBy = changedBy
        });
    }

    public async Task<List<ConfigHistoryDto>> GetHistoryAsync(int limit)
    {
        var history = await _configHistoryRepository.GetRecentAsync(limit);
        return history.Select(h => new ConfigHistoryDto
        {
            Id = h.Id,
            ConfigSection = h.ConfigSection,
            Snapshot = h.Snapshot,
            ChangedBy = h.ChangedBy,
            CreatedAt = h.CreatedAt
        }).ToList();
    }
}
