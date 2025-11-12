namespace NServiceBus.Transport.SqlServerNative;

public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
    where TIncoming : class, IIncomingMessage
{
    public virtual Task<long> Send(TOutgoing message, Cancel cancel = default) =>
        InnerSend(message, cancel);

    async Task<long> InnerSend(TOutgoing message, Cancel cancel)
    {
        using var command = CreateSendCommand(message);
        var rowVersion = await command.RunScalar(cancel);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (rowVersion == null)
        {
            return 0;
        }

        return (long) rowVersion;
    }

    protected abstract SqlCommand CreateSendCommand(TOutgoing message);
}