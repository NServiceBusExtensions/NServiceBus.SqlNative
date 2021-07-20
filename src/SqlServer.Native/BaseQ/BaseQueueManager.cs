using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
        where TIncoming : class, IIncomingMessage
    {
        protected Table Table;
        protected DbConnection Connection;
        protected DbTransaction? Transaction;

        protected BaseQueueManager(Table table, DbConnection connection)
        {
            Table = table;
            Connection = connection;
        }

        protected BaseQueueManager(Table table, DbTransaction transaction)
        {
            Table = table;
            Transaction = transaction;
            Connection = transaction.Connection;
        }

        async Task<IncomingResult> ReadMultiple(DbCommand command, Func<TIncoming, Task> func, CancellationToken cancellation)
        {
            var count = 0;
            long? lastRowVersion = null;
            using var reader = await command.RunSequentialReader(cancellation);
            while (await reader.ReadAsync(cancellation))
            {
                count++;
                cancellation.ThrowIfCancellationRequested();
                await using var message = ReadMessage(reader);
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
}