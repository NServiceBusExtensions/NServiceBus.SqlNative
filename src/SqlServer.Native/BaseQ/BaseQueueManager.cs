using Microsoft.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative;

public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
    where TIncoming : class, IIncomingMessage
{
    protected Table Table;
    protected SqlConnection Connection;
    protected SqlTransaction? Transaction;

    protected BaseQueueManager(Table table, SqlConnection connection)
    {
        Table = table;
        Connection = connection;
    }

    protected BaseQueueManager(Table table, SqlTransaction transaction)
    {
        Table = table;
        Transaction = transaction;
        Connection = transaction.Connection!;
    }

    async Task<IncomingResult> ReadMultiple(SqlCommand command, Func<TIncoming, Task> func, Cancellation cancellation)
    {
        var count = 0;
        long? lastRowVersion = null;
        using var reader = await command.RunSequentialReader(cancellation);
        while (await reader.ReadAsync(cancellation))
        {
            count++;
            cancellation.ThrowIfCancellationRequested();
            await using var message = await ReadMessage(reader);
            lastRowVersion = message.RowVersion;
            await func(message);
        }
        return new()
        {
            Count = count,
            LastRowVersion = lastRowVersion
        };
    }
}