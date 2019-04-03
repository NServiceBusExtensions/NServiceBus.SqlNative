using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
        where TIncoming : IIncomingMessage
    {
        protected Table Table;
        protected SqlConnection Connection;
        protected SqlTransaction Transaction;

        protected BaseQueueManager(Table table, SqlConnection connection)
        {
            Guard.AgainstNull(table, nameof(table));
            Guard.AgainstNull(connection, nameof(connection));
            Table = table;
            Connection = connection;
        }

        protected BaseQueueManager(Table table, SqlTransaction transaction)
        {
            Guard.AgainstNull(table, nameof(table));
            Guard.AgainstNull(transaction, nameof(transaction));
            Table = table;
            Transaction = transaction;
            Connection = transaction.Connection;
        }

        async Task<IncomingResult> ReadMultiple(SqlCommand command, Func<TIncoming, Task> func, CancellationToken cancellation)
        {
            var count = 0;
            long? lastRowVersion = null;
            using (var reader = await command.ExecuteSequentialReader(cancellation))
            {
                while (await reader.ReadAsync(cancellation))
                {
                    count++;
                    cancellation.ThrowIfCancellationRequested();
                    using (var message = ReadMessage(reader))
                    {
                        lastRowVersion = message.RowVersion;
                        await func(message);
                    }
                }
            }

            return new IncomingResult
            {
                Count = count,
                LastRowVersion = lastRowVersion
            };
        }
    }
}