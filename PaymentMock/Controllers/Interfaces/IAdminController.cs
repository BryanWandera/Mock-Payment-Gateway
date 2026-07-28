using Microsoft.AspNetCore.Mvc;
using PaymentMock.Enums;

namespace PaymentMock.Controllers.Interfaces;

public interface IAdminController
{
    Task<IActionResult> GetTransactions(TransactionStatus? status, PaymentProvider? provider, PaymentMethod? paymentMethod, string? merchantReference, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<IActionResult> GetPayouts(PayoutStatus? status, int page, int pageSize);
    Task<IActionResult> GetWebhookDeliveries(WebhookDeliveryStatus? status, int page, int pageSize);
    Task<IActionResult> GetErrors(int page, int pageSize);
    Task<IActionResult> GetScenarioExecutions(string? scenarioName, int page, int pageSize);
    IActionResult GetConfiguration();
    Task<IActionResult> GetConfigurationHistory(int limit);
}
