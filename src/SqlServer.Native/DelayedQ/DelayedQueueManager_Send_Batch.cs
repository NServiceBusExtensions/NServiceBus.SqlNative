using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        public virtual async Task Send(IEnumerable<OutgoingDelayedMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(messages, nameof(messages));
            foreach (var message in messages)
            {
                await Send(message, cancellation);
            }
        }

        public virtual async Task Send(IAsyncEnumerable<OutgoingDelayedMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(messages, nameof(messages));
            await foreach (var message in messages.WithCancellation(cancellation))
            {
                await Send(message, cancellation);
            }
        }
    }
}