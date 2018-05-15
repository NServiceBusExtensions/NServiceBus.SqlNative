using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public abstract class BaseQueueManager<TIncoming,TOutgoing> where TIncoming: IIncomingMessage
    {
        protected string table;
        protected SqlConnection connection;
        protected SqlTransaction transaction;

        protected BaseQueueManager(string table, SqlConnection connection)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(connection, nameof(connection));
            this.table = table;
            this.connection = connection;
        }

        protected BaseQueueManager(string table, SqlTransaction transaction)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(transaction, nameof(transaction));
            this.table = table;
            this.transaction = transaction;
            connection = transaction.Connection;
        }
        protected abstract SqlCommand BuildConsumeCommand(int batchSize);


        protected abstract TIncoming ReadMessage(SqlDataReader dataReader, params IDisposable[] cleanups);


        public virtual Task<IncomingResult> Consume(int size, Action<TIncoming> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(action, nameof(action));
            return Consume(size, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> Consume(int size, Func<TIncoming, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(func, nameof(func));
            using (var command = BuildConsumeCommand(size))
            {
                return await ReadMultipleStream(command,func, cancellation).ConfigureAwait(false);
            }
        }

        async Task<IncomingResult> ReadMultipleStream(SqlCommand command, Func<TIncoming, Task> func, CancellationToken cancellation)
        {
            using (var reader = await command.ExecuteSequentialReader(cancellation).ConfigureAwait(false))
            {
                var count = 0;
                long? lastRowVersion = null;
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

                return new IncomingResult
                {
                    Count = count,
                    LastRowVersion = lastRowVersion
                };
            }
        }

    }
}