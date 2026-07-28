using Microsoft.Extensions.Options;
using PaymentMock.Configuration;
using PaymentMock.DTOs.Generic;
using PaymentMock.Enums;
using PaymentMock.Models;
using PaymentMock.Repositories.Interfaces;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Implementations;

public class PaymentService : IPaymentService
{
    private static readonly TransactionStatus[] DuplicateGuardStatuses =
    {
        TransactionStatus.Created, TransactionStatus.Pending, TransactionStatus.Processing, TransactionStatus.Completed,
        TransactionStatus.AwaitingCheckout
    };

    private readonly IValidationService _validationService;
    private readonly IScenarioEngine _scenarioEngine;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionStateHistoryRepository _transactionStateHistoryRepository;
    private readonly ITransactionProcessingQueue _processingQueue;
    private readonly IGatewayProfileStrategy _profileStrategy;
    private readonly GatewaySettings _gatewaySettings;

    public PaymentService(
        IValidationService validationService,
        IScenarioEngine scenarioEngine,
        ITransactionRepository transactionRepository,
        ITransactionStateHistoryRepository transactionStateHistoryRepository,
        ITransactionProcessingQueue processingQueue,
        IGatewayProfileStrategy profileStrategy,
        IOptions<AppSettings> options)
    {
        _validationService = validationService;
        _scenarioEngine = scenarioEngine;
        _transactionRepository = transactionRepository;
        _transactionStateHistoryRepository = transactionStateHistoryRepository;
        _processingQueue = processingQueue;
        _profileStrategy = profileStrategy;
        _gatewaySettings = options.Value.Gateway;
    }

    public async Task<object> InitiateMobileMoneyPaymentAsync(MobileMoneyPaymentRequest request)
    {
        _validationService.ValidateMobileMoneyPayment(request);

        var (scenarioName, settings) = _scenarioEngine.ResolveSettings(request.Scenario);

        if (settings.SimulateDuplicateRequest)
        {
            var existing = await FindDuplicateAsync(request.MerchantReference);
            if (existing != null)
                return _profileStrategy.BuildPaymentResponse(existing, "Duplicate request detected — returning the original transaction");
        }

        var transaction = new Transaction
        {
            GatewayTransactionId = _profileStrategy.GenerateGatewayTransactionId(),
            MerchantReference = request.MerchantReference,
            Amount = request.Amount,
            Currency = request.Currency.ToUpperInvariant(),
            Provider = request.Provider,
            PaymentMethod = PaymentMethod.MobileMoney,
            Status = TransactionStatus.Created,
            Scenario = scenarioName,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            PhoneNumber = request.PhoneNumber,
            CallbackUrl = request.CallbackUrl
        };

        await PersistAndEnqueueAsync(transaction);

        return _profileStrategy.BuildPaymentResponse(transaction, "Payment initiated. Awaiting customer confirmation.");
    }

    public async Task<object> InitiateCardPaymentAsync(CardPaymentRequest request)
    {
        _validationService.ValidateCardPayment(request);

        var (scenarioName, settings) = _scenarioEngine.ResolveSettings(request.Scenario);

        if (settings.SimulateDuplicateRequest)
        {
            var existing = await FindDuplicateAsync(request.MerchantReference);
            if (existing != null)
                return _profileStrategy.BuildPaymentResponse(existing, "Duplicate request detected — returning the original transaction");
        }

        var digitsOnly = new string(request.CardNumber.Where(char.IsDigit).ToArray());

        var transaction = new Transaction
        {
            GatewayTransactionId = _profileStrategy.GenerateGatewayTransactionId(),
            MerchantReference = request.MerchantReference,
            Amount = request.Amount,
            Currency = request.Currency.ToUpperInvariant(),
            Provider = request.Provider,
            PaymentMethod = PaymentMethod.Card,
            Status = TransactionStatus.Created,
            Scenario = scenarioName,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CardLastFour = digitsOnly.Length >= 4 ? digitsOnly[^4..] : digitsOnly,
            CallbackUrl = request.CallbackUrl
        };

        await PersistAndEnqueueAsync(transaction);

        return _profileStrategy.BuildPaymentResponse(transaction, "Payment initiated. Processing card transaction.");
    }

    public async Task<object> InitiateHostedCheckoutAsync(HostedCheckoutRequest request)
    {
        _validationService.ValidateHostedCheckout(request);

        var (scenarioName, settings) = _scenarioEngine.ResolveSettings(request.Scenario);

        if (settings.SimulateDuplicateRequest)
        {
            var existing = await FindDuplicateAsync(request.MerchantReference);
            if (existing != null && !string.IsNullOrEmpty(existing.CheckoutToken))
            {
                var existingRedirectUrl = BuildCheckoutRedirectUrl(existing.CheckoutToken);
                return _profileStrategy.BuildHostedCheckoutResponse(existing, existingRedirectUrl, "Duplicate request detected — returning the original transaction");
            }
        }

        var transaction = new Transaction
        {
            GatewayTransactionId = _profileStrategy.GenerateGatewayTransactionId(),
            MerchantReference = request.MerchantReference,
            Amount = request.Amount,
            Currency = request.Currency.ToUpperInvariant(),
            Provider = request.Provider,
            PaymentMethod = request.PaymentMethod,
            Status = TransactionStatus.AwaitingCheckout,
            Scenario = scenarioName,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            PhoneNumber = request.PhoneNumber,
            Description = request.Description,
            CallbackUrl = request.CallbackUrl,
            CheckoutToken = Guid.NewGuid().ToString("N")
        };

        // Intentionally not enqueued: the transaction stays parked in AwaitingCheckout until a
        // human hits Approve/Decline on the hosted checkout page (see CheckoutService).
        await PersistAsync(transaction, TransactionStatus.AwaitingCheckout, "Hosted checkout session created, awaiting customer action");

        var redirectUrl = BuildCheckoutRedirectUrl(transaction.CheckoutToken!);
        return _profileStrategy.BuildHostedCheckoutResponse(transaction, redirectUrl, "Checkout session created. Redirect the customer to the returned URL.");
    }

    private string BuildCheckoutRedirectUrl(string checkoutToken) =>
        $"{_gatewaySettings.PublicBaseUrl.TrimEnd('/')}/checkout/{checkoutToken}";

    private async Task<Transaction?> FindDuplicateAsync(string merchantReference)
    {
        var existing = await _transactionRepository.GetByMerchantReferenceAsync(merchantReference);
        return existing != null && DuplicateGuardStatuses.Contains(existing.Status) ? existing : null;
    }

    private async Task PersistAsync(Transaction transaction, TransactionStatus recordedStatus, string notes)
    {
        await _transactionRepository.CreateAsync(transaction);
        await _transactionStateHistoryRepository.CreateAsync(new TransactionStateHistory
        {
            TransactionId = transaction.Id,
            FromStatus = recordedStatus,
            ToStatus = recordedStatus,
            Notes = notes
        });
    }

    private async Task PersistAndEnqueueAsync(Transaction transaction)
    {
        await PersistAsync(transaction, TransactionStatus.Created, "Transaction created");
        _processingQueue.Enqueue(transaction.Id);
    }
}
