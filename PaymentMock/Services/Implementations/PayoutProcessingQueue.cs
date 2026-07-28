using System.Threading.Channels;
using PaymentMock.Services.Interfaces;

namespace PaymentMock.Services.Implementations;

public class PayoutProcessingQueue : IPayoutProcessingQueue
{
    private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();

    public void Enqueue(string payoutId) => _channel.Writer.TryWrite(payoutId);

    public IAsyncEnumerable<string> DequeueAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
