using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
        where TIncoming : class, IIncomingMessage
    {
        public virtual async Task Send(IEnumerable<TOutgoing> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(messages, nameof(messages));
            foreach (var message in messages)
            {
                await InnerSend(message, cancellation);
            }
        }
    }
}