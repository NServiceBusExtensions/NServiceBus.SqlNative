using Microsoft.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative;

public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
    where TIncoming : class, IIncomingMessage
{
    protected abstract SqlCommand BuildReadCommand(int batchSize, long startRowVersion);

    public virtual async Task<TIncoming?> Read(long rowVersion, CancellationToken cancellation = default)
    {
        Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
        var shouldCleanup = false;
        SqlDataReader? reader = null;
        try
        {
            using var command = BuildReadCommand(1, rowVersion);
            reader = await command.RunSingleRowReader(cancellation);
            if (!await reader.ReadAsync(cancellation))
            {
                shouldCleanup = true;
                return default;
            }

            return await ReadMessage(reader, reader.DisposeAsync);
        }
        catch
        {
            shouldCleanup = true;
            throw;
        }
        finally
        {
            if (shouldCleanup && reader != null)
            {
                reader.Dispose();
            }
        }
    }
}