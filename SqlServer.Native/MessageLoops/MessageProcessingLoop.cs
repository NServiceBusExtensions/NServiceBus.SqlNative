using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public class MessageProcessingLoop : MessageLoop
    {
        string table;
        long startingRow;
        Func<CancellationToken, Task<SqlConnection>> connectionBuilder;
        Func<CancellationToken, Task<SqlTransaction>> transactionBuilder;
        Func<SqlTransaction, IncomingBytesMessage, CancellationToken, Task> transactionCallback;
        Func<SqlConnection, IncomingBytesMessage, CancellationToken, Task> connectionCallback;
        Func<long, CancellationToken, Task> persistRowVersion;
        int batchSize;

        public MessageProcessingLoop(
            string table,
            long startingRow,
            Func<CancellationToken, Task<SqlTransaction>> transactionBuilder,
            Func<SqlTransaction, IncomingBytesMessage, CancellationToken, Task> callback,
            Action<Exception> errorCallback,
            Func<long, CancellationToken, Task> persistRowVersion,
            int batchSize = 10,
            TimeSpan? delay = null)
            : base(errorCallback, delay)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNegativeAndZero(startingRow, nameof(startingRow));
            Guard.AgainstNull(transactionBuilder, nameof(transactionBuilder));
            Guard.AgainstNull(persistRowVersion, nameof(persistRowVersion));
            Guard.AgainstNull(callback, nameof(callback));
            Guard.AgainstNegativeAndZero(batchSize, nameof(batchSize));
            this.table = table;
            this.startingRow = startingRow;
            this.transactionBuilder = transactionBuilder;
            transactionCallback = callback;
            this.persistRowVersion = persistRowVersion;
            this.batchSize = batchSize;
        }

        public MessageProcessingLoop(
            string table,
            long startingRow,
            Func<CancellationToken, Task<SqlConnection>> connectionBuilder,
            Func<SqlConnection, IncomingBytesMessage, CancellationToken, Task> callback,
            Action<Exception> errorCallback,
            Func<long, CancellationToken, Task> persistRowVersion,
            int batchSize = 10,
            TimeSpan? delay = null)
            : base(errorCallback, delay)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNegativeAndZero(startingRow, nameof(startingRow));
            Guard.AgainstNull(connectionBuilder, nameof(connectionBuilder));
            Guard.AgainstNull(persistRowVersion, nameof(persistRowVersion));
            Guard.AgainstNull(callback, nameof(callback));
            Guard.AgainstNegativeAndZero(batchSize, nameof(batchSize));
            connectionCallback = callback;
            this.table = table;
            this.startingRow = startingRow;
            this.connectionBuilder = connectionBuilder;
            this.persistRowVersion = persistRowVersion;
            this.batchSize = batchSize;
        }

        protected override async Task RunBatch(CancellationToken cancellation)
        {
            if (connectionBuilder != null)
            {
                using (var connection = await connectionBuilder(cancellation).ConfigureAwait(false))
                {
                    var reader = new QueueManager(table, connection);
                    await RunBatch(reader, message => connectionCallback(connection, message, cancellation), cancellation)
                        .ConfigureAwait(false);
                }

                return;
            }

            using (var transaction = await transactionBuilder(cancellation).ConfigureAwait(false))
            {
                var reader = new QueueManager(table, transaction);
                try
                {
                    await RunBatch(reader, message => transactionCallback(transaction, message, cancellation), cancellation)
                        .ConfigureAwait(false);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        async Task RunBatch(QueueManager reader, Func<IncomingBytesMessage, Task> func, CancellationToken cancellation)
        {
            while (true)
            {
                var result = await reader.ReadBytes(batchSize, startingRow, func, cancellation)
                    .ConfigureAwait(false);
                if (result.Count == 0)
                {
                    break;
                }

                startingRow = result.LastRowVersion.Value + 1;
                await persistRowVersion(startingRow, cancellation).ConfigureAwait(false);
                if (result.Count < batchSize)
                {
                    break;
                }
            }
        }
    }
}