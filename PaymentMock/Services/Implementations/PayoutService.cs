using PaymentMock.DTOs.Generic;
using PaymentMock.Enums;
using PaymentMock.Exceptions;
using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Implementations;

public class PayoutService : IPayoutService
{
    private readonly IValidationService _validationService;
    private readonly IScenarioEngine _scenarioEngine;
    private readonly IPayoutRepository _payoutRepository;
    private readonly IPayoutStateHistoryRepository _payoutStateHistoryRepository;
    private readonly IPayoutProcessingQueue _processingQueue;
    private readonly IGatewayProfileStrategy _profileStrategy;

    public PayoutService(
        IValidationService validationService,
        IScenarioEngine scenarioEngine,
        IPayoutRepository payoutRepository,
        IPayoutStateHistoryRepository payoutStateHistoryRepository,
        IPayoutProcessingQueue processingQueue,
        IGatewayProfileStrategy profileStrategy)
    {
        _validationService = validationService;
        _scenarioEngine = scenarioEngine;
        _payoutRepository = payoutRepository;
        _payoutStateHistoryRepository = payoutStateHistoryRepository;
        _processingQueue = processingQueue;
        _profileStrategy = profileStrategy;
    }

    public async Task<object> InitiatePayoutAsync(PayoutRequest request)
    {
        _validationService.ValidatePayout(request);

        var (scenarioName, _) = _scenarioEngine.ResolveSettings(request.Scenario);

        var payout = new Payout
        {
            GatewayPayoutId = _profileStrategy.GenerateGatewayPayoutId(),
            MerchantReference = request.MerchantReference,
            Amount = request.Amount,
            Currency = request.Currency.ToUpperInvariant(),
            PaymentMethod = request.PaymentMethod,
            Provider = request.Provider,
            Status = PayoutStatus.Created,
            Scenario = scenarioName,
            RecipientName = request.RecipientName,
            PhoneNumber = request.PhoneNumber,
            BankAccountNumber = request.BankAccountNumber,
            BankName = request.BankName,
            BankCode = request.BankCode
        };

        await _payoutRepository.CreateAsync(payout);
        await _payoutStateHistoryRepository.CreateAsync(new PayoutStateHistory
        {
            PayoutId = payout.Id,
            FromStatus = PayoutStatus.Created,
            ToStatus = PayoutStatus.Created,
            Notes = "Payout created"
        });

        _processingQueue.Enqueue(payout.Id);

        return _profileStrategy.BuildPayoutResponse(payout, "Payout initiated");
    }

    public async Task<object> GetByIdAsync(string payoutId)
    {
        var payout = await _payoutRepository.GetByIdAsync(payoutId)
            ?? throw new NotFoundException($"Payout '{payoutId}' was not found");

        return _profileStrategy.BuildPayoutDto(payout);
    }

    public async Task<object> SearchAsync(PayoutStatus? status, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;

        var payouts = await _payoutRepository.SearchAsync(status, page, pageSize);
        var total = await _payoutRepository.CountAsync(status);

        return _profileStrategy.BuildPayoutListResponse(payouts, page, pageSize, total);
    }
}
