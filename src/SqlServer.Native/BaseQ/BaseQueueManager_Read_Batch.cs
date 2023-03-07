namespace NServiceBus.Transport.SqlServerNative;

public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
    where TIncoming : class, IIncomingMessage
{
    public virtual Task<IncomingResult> Read(int size, long startRowVersion, Action<TIncoming> action, Cancellation cancellation = default) =>
        Read(size, startRowVersion, action.ToTaskFunc(), cancellation);

    public virtual async Task<IncomingResult> Read(int size, long startRowVersion, Func<TIncoming, Task> func, Cancellation cancellation = default)
    {
        Guard.AgainstNegativeAndZero(size);
        Guard.AgainstNegativeAndZero(startRowVersion);
        using var command = BuildReadCommand(size, startRowVersion);
        return await ReadMultiple(command, func, cancellation);
    }
}