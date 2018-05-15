using System;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        public virtual Task<IncomingResult> ConsumeStream(int size, Action<IncomingStreamMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(action, nameof(action));
            return ConsumeStream(size, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> ConsumeStream(int size, Func<IncomingStreamMessage, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(func, nameof(func));
            using (var command = BuildConsumeCommand(size))
            {
                return await command.ReadMultipleStream(func, cancellation).ConfigureAwait(false);
            }
        }
    }
}