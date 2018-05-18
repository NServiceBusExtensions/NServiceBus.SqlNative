using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public abstract partial class BaseQueueManager<TIncoming,TOutgoing>
        where TIncoming: IIncomingMessage
    {
        protected abstract SqlCommand BuildConsumeCommand(int batchSize);

        protected abstract TIncoming ReadMessage(SqlDataReader dataReader, params IDisposable[] cleanups);

        public virtual Task<IncomingResult> Consume(int size, Action<TIncoming> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(action, nameof(action));
            return Consume(size, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> Consume(int size, Func<TIncoming, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(func, nameof(func));
            using (var command = BuildConsumeCommand(size))
            {
                return await ReadMultiple(command,func, cancellation).ConfigureAwait(false);
            }
        }
    }
}