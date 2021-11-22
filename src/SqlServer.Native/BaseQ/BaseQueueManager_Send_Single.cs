using Microsoft.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative;

public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
    where TIncoming : class, IIncomingMessage
{
    public virtual Task<long> Send(TOutgoing message, CancellationToken cancellation = default)
    {
        return InnerSend(message, cancellation);
    }

    async Task<long> InnerSend(TOutgoing message, CancellationToken cancellation)
    {
        await using var command = CreateSendCommand(message);
        var rowVersion = await command.RunScalar(cancellation);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (rowVersion == null)
        {
            return 0;
        }

        return (long) rowVersion;
    }

    protected abstract SqlCommand CreateSendCommand(TOutgoing message);
}