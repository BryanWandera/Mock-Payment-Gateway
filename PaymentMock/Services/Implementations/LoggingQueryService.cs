using PaymentMock.DTOs.Generic;
using PaymentMock.Enums;
using PaymentMock.Repositories.Interfaces;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Implementations;

public class LoggingQueryService : ILoggingQueryService
{
    private readonly IWebhookDeliveryRepository _webhookDeliveryRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IScenarioExecutionRepository _scenarioExecutionRepository;

    public LoggingQueryService(
        IWebhookDeliveryRepository webhookDeliveryRepository,
        IAuditLogRepository auditLogRepository,
        IScenarioExecutionRepository scenarioExecutionRepository)
    {
        _webhookDeliveryRepository = webhookDeliveryRepository;
        _auditLogRepository = auditLogRepository;
        _scenarioExecutionRepository = scenarioExecutionRepository;
    }

    public async Task<PagedResponse<WebhookLogDto>> GetWebhookDeliveriesAsync(WebhookDeliveryStatus? status, int page, int pageSize)
    {
        (page, pageSize) = Normalize(page, pageSize);
        var deliveries = await _webhookDeliveryRepository.SearchAsync(status, page, pageSize);
        var total = await _webhookDeliveryRepository.CountAsync(status);

        return new PagedResponse<WebhookLogDto>
        {
            Items = deliveries.Select(d => new WebhookLogDto
            {
                Id = d.Id,
                TransactionId = d.TransactionId,
                PayoutId = d.PayoutId,
                TargetUrl = d.TargetUrl,
                Status = d.Status.ToString(),
                AttemptCount = d.AttemptCount,
                HttpStatusCode = d.HttpStatusCode,
                ErrorMessage = d.ErrorMessage,
                CreatedAt = d.CreatedAt,
                ScheduledAt = d.ScheduledAt,
                DeliveredAt = d.DeliveredAt
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<PagedResponse<ErrorLogDto>> GetErrorsAsync(int page, int pageSize)
    {
        (page, pageSize) = Normalize(page, pageSize);
        var errors = await _auditLogRepository.SearchAsync("Error", page, pageSize);
        var total = await _auditLogRepository.CountAsync("Error");

        return new PagedResponse<ErrorLogDto>
        {
            Items = errors.Select(e => new ErrorLogDto
            {
                Id = e.Id,
                Message = e.Message,
                Details = e.Details,
                CorrelationId = e.CorrelationId,
                CreatedAt = e.CreatedAt
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<PagedResponse<ScenarioLogDto>> GetScenarioExecutionsAsync(string? scenarioName, int page, int pageSize)
    {
        (page, pageSize) = Normalize(page, pageSize);
        var executions = await _scenarioExecutionRepository.SearchAsync(scenarioName, page, pageSize);
        var total = await _scenarioExecutionRepository.CountAsync(scenarioName);

        return new PagedResponse<ScenarioLogDto>
        {
            Items = executions.Select(e => new ScenarioLogDto
            {
                Id = e.Id,
                ScenarioName = e.ScenarioName,
                TransactionId = e.TransactionId,
                PayoutId = e.PayoutId,
                Details = e.Details,
                ExecutedAt = e.ExecutedAt
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    private static (int Page, int PageSize) Normalize(int page, int pageSize) =>
        (page < 1 ? 1 : page, pageSize is < 1 or > 200 ? 20 : pageSize);
}
