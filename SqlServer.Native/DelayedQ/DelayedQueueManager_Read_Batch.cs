using System;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        public virtual Task<IncomingResult> Read(int size, long startRowVersion, Action<IncomingDelayedMessage> action, CancellationToken cancellation = default)
        {
            return Read(size, startRowVersion, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> Read(int size, long startRowVersion, Func<IncomingDelayedMessage, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNull(func, nameof(func));
            using (var command = BuildReadCommand(size, startRowVersion))
            {
                return await command.ReadDelayedMultipleStream(func, cancellation).ConfigureAwait(false);
            }
        }
    }
}