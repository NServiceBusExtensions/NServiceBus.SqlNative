using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{
    public class MessageProcessingLoop: MessageLoop
    {
        string table;
        long startingRow;
        Func<CancellationToken, Task<SqlConnection>> connectionBuilder;
        Func<long, CancellationToken, Task> persistRowVersion;
        int batchSize;

        public MessageProcessingLoop(
            string table,
            long startingRow,
            Func<CancellationToken, Task<SqlConnection>> connectionBuilder,
            Func<IncomingMessage, CancellationToken, Task> callback,
            Action<Exception> errorCallback,
            Func<long, CancellationToken, Task> persistRowVersion,
            int batchSize = 10,
            TimeSpan? delay = null):base(callback, errorCallback,delay)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNegativeAndZero(startingRow, nameof(startingRow));
            Guard.AgainstNull(connectionBuilder, nameof(connectionBuilder));
            Guard.AgainstNull(persistRowVersion, nameof(persistRowVersion));
            Guard.AgainstNegativeAndZero(batchSize, nameof(batchSize));
            this.table = table;
            this.startingRow = startingRow;
            this.connectionBuilder = connectionBuilder;
            this.persistRowVersion = persistRowVersion;
            this.batchSize = batchSize;
        }

        protected override async Task RunBatch(Func<IncomingMessage, CancellationToken, Task> callback, CancellationToken cancellation)
        {
            using (var connection = await connectionBuilder(cancellation).ConfigureAwait(false))
            {
                var finder = new Finder(table);
                while (true)
                {
                    var result = await finder.Find(connection, batchSize, startingRow, message => callback(message, cancellation), cancellation)
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
}