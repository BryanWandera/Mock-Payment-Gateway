using PaymentMock.DTOs.Generic;
using PaymentMock.Enums;

namespace PaymentMock.Services.Interfaces;

public interface IPayoutService
{
    Task<object> InitiatePayoutAsync(PayoutRequest request);
    Task<object> GetByIdAsync(string payoutId);
    Task<object> SearchAsync(PayoutStatus? status, int page, int pageSize);
}
