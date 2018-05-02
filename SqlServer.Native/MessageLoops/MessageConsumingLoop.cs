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
        int batchSize;

        public MessageConsumingLoop(
            string table,
            Func<CancellationToken, Task<SqlConnection>> connectionBuilder,
            Func<IncomingBytesMessage, CancellationToken, Task> callback,
            Action<Exception> errorCallback,
            int batchSize = 10,
            TimeSpan? delay = null) :
            base(callback, errorCallback, delay)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(connectionBuilder, nameof(connectionBuilder));
            Guard.AgainstNegativeAndZero(batchSize, nameof(batchSize));
            this.table = table;
            this.connectionBuilder = connectionBuilder;
            this.batchSize = batchSize;
        }

        protected override async Task RunBatch(Func<IncomingBytesMessage, CancellationToken, Task> callback, CancellationToken cancellation)
        {
            using (var connection = await connectionBuilder(cancellation).ConfigureAwait(false))
            {
                await RunBatch(callback, cancellation, connection).ConfigureAwait(false);
            }
        }

        async Task RunBatch(Func<IncomingBytesMessage, CancellationToken, Task> callback, CancellationToken cancellation, SqlConnection connection)
        {
            var consumer = new QueueManager(table, connection);
            while (true)
            {
                var result = await consumer.ConsumeBytes(batchSize, message => callback(message, cancellation), cancellation)
                    .ConfigureAwait(false);
                if (result.Count < batchSize)
                {
                    break;
                }
            }
        }
    }
}