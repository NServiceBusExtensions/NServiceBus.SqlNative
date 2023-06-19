namespace NServiceBus.Transport.SqlServerNative;

public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
    where TIncoming : class, IIncomingMessage
{
    public virtual async Task<IncomingResult> Read(int size, long startRowVersion, Func<TIncoming, Cancel, Task> func, Cancel cancel = default)
    {
        Guard.AgainstNegativeAndZero(size);
        Guard.AgainstNegativeAndZero(startRowVersion);
        using var command = BuildReadCommand(size, startRowVersion);
        return await ReadMultiple(command, func, cancel);
    }
}