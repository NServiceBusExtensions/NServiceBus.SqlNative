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

    async Task<IncomingResult> ReadMultiple(SqlCommand command, Func<TIncoming, Cancel, Task> func, Cancel cancel)
    {
        var count = 0;
        long? lastRowVersion = null;
        using var reader = await command.RunSequentialReader(cancel);
        while (await reader.ReadAsync(cancel))
        {
            count++;
            cancel.ThrowIfCancellationRequested();
            await using var message = await ReadMessage(reader);
            lastRowVersion = message.RowVersion;
            await func(message, cancel);
        }
        return new()
        {
            Count = count,
            LastRowVersion = lastRowVersion
        };
    }
}