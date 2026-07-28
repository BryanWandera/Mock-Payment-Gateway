using PaymentMock.Enums;
using PaymentMock.Exceptions;
using PaymentMock.Repositories.Interfaces;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Implementations;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IGatewayProfileStrategy _profileStrategy;

    public TransactionService(ITransactionRepository transactionRepository, IGatewayProfileStrategy profileStrategy)
    {
        _transactionRepository = transactionRepository;
        _profileStrategy = profileStrategy;
    }

    public async Task<object> GetByIdAsync(string transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId)
            ?? throw new NotFoundException($"Transaction '{transactionId}' was not found");

        return _profileStrategy.BuildTransactionDto(transaction);
    }

    public async Task<object> GetStatusAsync(string transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId)
            ?? throw new NotFoundException($"Transaction '{transactionId}' was not found");

        return _profileStrategy.BuildTransactionStatusDto(transaction);
    }

    public async Task<object> SearchAsync(
        TransactionStatus? status,
        PaymentProvider? provider,
        PaymentMethod? paymentMethod,
        string? merchantReference,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;

        var transactions = await _transactionRepository.SearchAsync(status, provider, paymentMethod, merchantReference, fromDate, toDate, page, pageSize);
        var total = await _transactionRepository.CountAsync(status, provider, paymentMethod, merchantReference, fromDate, toDate);

        return _profileStrategy.BuildTransactionListResponse(transactions, page, pageSize, total);
    }
}
