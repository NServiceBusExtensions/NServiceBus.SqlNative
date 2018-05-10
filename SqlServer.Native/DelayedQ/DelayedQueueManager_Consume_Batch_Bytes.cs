using System;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        public virtual Task<IncomingResult> ConsumeBytes(int size, Action<IncomingDelayedBytesMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(action, nameof(action));
            return ConsumeBytes(size, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> ConsumeBytes(int size, Func<IncomingDelayedBytesMessage, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(func, nameof(func));
            using (var command = BuildConsumeCommand(size))
            {
                return await command.ReadDelayedMultipleBytes(func, cancellation);
            }
        }
    }
}