using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
        where TIncoming : IIncomingMessage
    {
        public virtual Task Send(IEnumerable<TOutgoing> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(messages, nameof(messages));
            var tasks = messages.Select(message => InnerSend(message, cancellation));
            return Task.WhenAll(tasks);
        }
    }
}