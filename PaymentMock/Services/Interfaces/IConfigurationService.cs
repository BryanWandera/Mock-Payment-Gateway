using PaymentMock.DTOs.Generic;

namespace PaymentMock.Services.Interfaces;

public interface IConfigurationService
{
    ConfigSummaryDto GetEffectiveConfiguration();
    Task RecordSnapshotAsync(string section, object snapshot, string? changedBy = null);
    Task<List<ConfigHistoryDto>> GetHistoryAsync(int limit);
}
