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
        Func<SqlTransaction, IncomingBytesMessage, CancellationToken, Task> transactionCallback;
        Func<SqlConnection, IncomingBytesMessage, CancellationToken, Task> connectionCallback;
        int batchSize;

        public MessageConsumingLoop(
            string table,
            Func<CancellationToken, Task<SqlTransaction>> transactionBuilder,
            Func<SqlTransaction, IncomingBytesMessage, CancellationToken, Task> callback,
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
            transactionCallback = callback;
            this.transactionBuilder = transactionBuilder;
            this.batchSize = batchSize;
        }

        public MessageConsumingLoop(
            string table,
            Func<CancellationToken, Task<SqlConnection>> connectionBuilder,
            Func<SqlConnection, IncomingBytesMessage, CancellationToken, Task> callback,
            Action<Exception> errorCallback,
            int batchSize = 10,
            TimeSpan? delay = null) :
            base(errorCallback, delay)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(connectionBuilder, nameof(connectionBuilder));
            Guard.AgainstNegativeAndZero(batchSize, nameof(batchSize));
            Guard.AgainstNull(callback, nameof(callback));
            connectionCallback = callback;
            this.table = table;
            this.connectionBuilder = connectionBuilder;
            this.batchSize = batchSize;
        }

        protected override async Task RunBatch(CancellationToken cancellation)
        {
            if (connectionBuilder != null)
            {
                using (var connection = await connectionBuilder(cancellation).ConfigureAwait(false))
                {
                    var consumer = new QueueManager(table, connection);
                    await RunBatch(consumer, message => connectionCallback(connection, message, cancellation), cancellation);
                }

                return;
            }

            using (var transaction = await transactionBuilder(cancellation).ConfigureAwait(false))
            {
                var consumer = new QueueManager(table, transaction);
                try
                {
                    await RunBatch(consumer, message => transactionCallback(transaction, message, cancellation), cancellation);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        async Task RunBatch(QueueManager consumer, Func<IncomingBytesMessage, Task> action, CancellationToken cancellation)
        {
            while (true)
            {
                var result = await consumer.ConsumeBytes(batchSize, action, cancellation)
                    .ConfigureAwait(false);
                if (result.Count < batchSize)
                {
                    break;
                }
            }
        }
    }
}