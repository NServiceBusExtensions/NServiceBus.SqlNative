using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public class MessageConsumingLoop : MessageLoop
    {
        string table;
        Func<CancellationToken, Task<SqlConnection>> connectionBuilder;
        Func<CancellationToken, Task<SqlTransaction>> transactionBuilder;
        Func<SqlTransaction, IncomingMessage, CancellationToken, Task> transactionCallback;
        Func<SqlConnection, IncomingMessage, CancellationToken, Task> connectionCallback;
        int batchSize;

        public MessageConsumingLoop(
            string table,
            Func<CancellationToken, Task<SqlTransaction>> transactionBuilder,
            Func<SqlTransaction, IncomingMessage, CancellationToken, Task> callback,
            Action<Exception> errorCallback,
            int batchSize = 10,
            TimeSpan? delay = null) :
            base(errorCallback, delay)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(transactionBuilder, nameof(transactionBuilder));
            Guard.AgainstNull(callback, nameof(callback));
            Guard.AgainstNegativeAndZero(batchSize, nameof(batchSize));
            this.table = table;
            transactionCallback = callback.WrapFunc(nameof(transactionCallback));
            this.transactionBuilder = transactionBuilder.WrapFunc(nameof(transactionBuilder));
            this.batchSize = batchSize;
        }

        public MessageConsumingLoop(
            string table,
            Func<CancellationToken, Task<SqlConnection>> connectionBuilder,
            Func<SqlConnection, IncomingMessage, CancellationToken, Task> callback,
            Action<Exception> errorCallback,
            int batchSize = 10,
            TimeSpan? delay = null) :
            base(errorCallback, delay)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(connectionBuilder, nameof(connectionBuilder));
            Guard.AgainstNegativeAndZero(batchSize, nameof(batchSize));
            Guard.AgainstNull(callback, nameof(callback));
            connectionCallback = callback.WrapFunc(nameof(connectionCallback));
            this.table = table;
            this.connectionBuilder = connectionBuilder.WrapFunc(nameof(this.connectionBuilder));
            this.batchSize = batchSize;
        }

        protected override async Task RunBatch(CancellationToken cancellation)
        {
            SqlConnection connection = null;
            if (connectionBuilder != null)
            {
                using (connection = await connectionBuilder(cancellation).ConfigureAwait(false))
                {
                    var consumer = new QueueManager(table, connection);
                    await RunBatch(consumer, message => connectionCallback(connection, message, cancellation), cancellation).ConfigureAwait(false);
                }

                return;
            }
            SqlTransaction transaction = null;
            try
            {
                transaction = await transactionBuilder(cancellation).ConfigureAwait(false);
                connection = transaction.Connection;
                var consumer = new QueueManager(table, transaction);
                try
                {
                    await RunBatch(consumer, message => transactionCallback(transaction, message, cancellation), cancellation).ConfigureAwait(false);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            finally
            {
                transaction?.Dispose();
                connection?.Dispose();
            }
        }

        async Task RunBatch(QueueManager consumer, Func<IncomingMessage, Task> action, CancellationToken cancellation)
        {
            while (true)
            {
                var result = await consumer.Consume(batchSize, action, cancellation)
                    .ConfigureAwait(false);
                if (result.Count < batchSize)
                {
                    break;
                }
            }
        }
    }
}