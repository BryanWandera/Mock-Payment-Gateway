namespace PaymentMock.Services.Interfaces;

public interface IPayoutProcessingQueue
{
    void Enqueue(string payoutId);
    IAsyncEnumerable<string> DequeueAllAsync(CancellationToken cancellationToken);
}
