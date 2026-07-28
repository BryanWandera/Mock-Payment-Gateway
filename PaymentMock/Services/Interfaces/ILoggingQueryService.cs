using PaymentMock.DTOs.Generic;
using PaymentMock.Enums;

namespace PaymentMock.Services.Interfaces;

public interface ILoggingQueryService
{
    Task<PagedResponse<WebhookLogDto>> GetWebhookDeliveriesAsync(WebhookDeliveryStatus? status, int page, int pageSize);
    Task<PagedResponse<ErrorLogDto>> GetErrorsAsync(int page, int pageSize);
    Task<PagedResponse<ScenarioLogDto>> GetScenarioExecutionsAsync(string? scenarioName, int page, int pageSize);
}
