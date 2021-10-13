using System.Data.Common;

namespace NServiceBus.Transport.SqlServerNative;

/// <summary>
/// Base class for <see cref="DelayedQueueManager"/> and <see cref="QueueManager"/>.
/// Not to be used as an extension point.
/// </summary>
public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
    where TIncoming : class, IIncomingMessage
{
    protected abstract DbCommand BuildConsumeCommand(int batchSize);

#if NETSTANDARD2_1
        protected abstract TIncoming ReadMessage(DbDataReader dataReader, params IAsyncDisposable[] cleanups);
#else
    protected abstract TIncoming ReadMessage(DbDataReader dataReader, params IDisposable[] cleanups);
#endif
    public virtual Task<IncomingResult> Consume(int size, Action<TIncoming> action, CancellationToken cancellation = default)
    {
        return Consume(size, action.ToTaskFunc(), cancellation);
    }

    public virtual async Task<IncomingResult> Consume(int size, Func<TIncoming, Task> func, CancellationToken cancellation = default)
    {
        Guard.AgainstNegativeAndZero(size, nameof(size));
        using var command = BuildConsumeCommand(size);
        return await ReadMultiple(command, func, cancellation);
    }
}