using System.Threading.Channels;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Implementations;

public class TransactionProcessingQueue : ITransactionProcessingQueue
{
    private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();

    public void Enqueue(string transactionId) => _channel.Writer.TryWrite(transactionId);

    public IAsyncEnumerable<string> DequeueAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
