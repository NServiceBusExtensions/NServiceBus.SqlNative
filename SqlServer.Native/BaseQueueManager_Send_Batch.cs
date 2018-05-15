using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
        where TIncoming : IIncomingMessage
    {
        public virtual async Task<long> Send(IEnumerable<TOutgoing> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(messages, nameof(messages));
            long rowVersion = 0;
            foreach (var message in messages)
            {
                cancellation.ThrowIfCancellationRequested();
                rowVersion= await InnerSend(message, cancellation).ConfigureAwait(false);
            }

            return rowVersion;
        }
    }
}