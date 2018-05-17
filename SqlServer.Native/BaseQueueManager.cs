using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
        where TIncoming : IIncomingMessage
    {
        protected string fullTableName;
        protected SqlConnection connection;
        protected SqlTransaction transaction;

        protected BaseQueueManager(string table, SqlConnection connection, string schema) :
            this(table, connection, schema, true)
        {
        }

        protected BaseQueueManager(string table, SqlConnection connection, string schema, bool sanitize)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNullOrEmpty(schema, nameof(schema));
            Guard.AgainstNull(connection, nameof(connection));
            this.connection = connection;

            if (sanitize)
            {
                table = SqlSanitizer.Sanitize(table);
                schema = SqlSanitizer.Sanitize(schema);
            }

            fullTableName = $"{schema}.{table}";
        }

        protected BaseQueueManager(string table, SqlTransaction transaction, string schema = "dbo") :
            this(table, transaction, schema, true)
        {
        }

        protected BaseQueueManager(string table, SqlTransaction transaction, string schema, bool sanitize)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNullOrEmpty(schema, nameof(schema));
            Guard.AgainstNull(transaction, nameof(transaction));
            this.transaction = transaction;
            connection = transaction.Connection;

            if (sanitize)
            {
                table = SqlSanitizer.Sanitize(table);
                schema = SqlSanitizer.Sanitize(schema);
            }

            fullTableName = $"{schema}.{table}";
        }

        async Task<IncomingResult> ReadMultiple(SqlCommand command, Func<TIncoming, Task> func, CancellationToken cancellation)
        {
            var count = 0;
            long? lastRowVersion = null;
            using (var reader = await command.ExecuteSequentialReader(cancellation).ConfigureAwait(false))
            {
                while (await reader.ReadAsync(cancellation).ConfigureAwait(false))
                {
                    count++;
                    cancellation.ThrowIfCancellationRequested();
                    using (var message = ReadMessage(reader))
                    {
                        lastRowVersion = message.RowVersion;
                        await func(message).ConfigureAwait(false);
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