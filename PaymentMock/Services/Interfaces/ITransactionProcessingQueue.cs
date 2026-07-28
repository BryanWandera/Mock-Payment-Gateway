namespace PaymentMock.Services.Interfaces;

public interface ITransactionProcessingQueue
{
    void Enqueue(string transactionId);
    IAsyncEnumerable<string> DequeueAllAsync(CancellationToken cancellationToken);
}
