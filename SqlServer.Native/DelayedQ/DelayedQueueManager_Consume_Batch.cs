using System;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        public virtual Task<IncomingResult> Consume(int size, Action<IncomingDelayedMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(action, nameof(action));
            return Consume(size, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> Consume(int size, Func<IncomingDelayedMessage, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(func, nameof(func));
            using (var command = BuildConsumeCommand(size))
            {
                return await command.ReadDelayedMultipleStream(func, cancellation).ConfigureAwait(false);
            }
        }
    }
}