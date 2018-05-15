using System;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        public virtual Task<IncomingResult> Consume(int size, Action<IncomingMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(action, nameof(action));
            return Consume(size, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> Consume(int size, Func<IncomingMessage, Task> func, CancellationToken cancellation = default)
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