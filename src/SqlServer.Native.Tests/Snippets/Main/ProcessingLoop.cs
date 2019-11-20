using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
// ReSharper disable UnusedVariable

public class ProcessingLoop
{
    SqlConnection sqlConnection = null!;
    string connectionString = null!;

    async Task RowTracking()
    {

        long newRowVersion = 0;

        #region RowVersionTracker

        var versionTracker = new RowVersionTracker();

        // create table
        await versionTracker.CreateTable(sqlConnection);

        // save row version
        await versionTracker.Save(sqlConnection, newRowVersion);

        // get row version
        var startingRow = await versionTracker.Get(sqlConnection);

        #endregion
    }

    async Task ReadLoop()
    {
        #region ProcessingLoop

        var rowVersionTracker = new RowVersionTracker();

        var startingRow = await rowVersionTracker.Get(sqlConnection);

        async Task Callback(DbTransaction transaction, IncomingMessage message, CancellationToken cancellation)
        {
            if (message.Body == null)
            {
                return;
            }

            using var reader = new StreamReader(message.Body);
            var bodyText = await reader.ReadToEndAsync();
            Console.WriteLine($"Message received in error message:\r\n{bodyText}");
        }

        void ErrorCallback(Exception exception)
        {
            Environment.FailFast("Message processing loop failed", exception);
        }

        Task<DbTransaction> TransactionBuilder(CancellationToken cancellation)
        {
            return SnippetConnectionHelpers.BeginTransaction(connectionString, cancellation);
        }

        Task PersistRowVersion(DbTransaction transaction, long rowVersion, CancellationToken token)
        {
            return rowVersionTracker.Save(sqlConnection, rowVersion, token);
        }

        var processingLoop = new MessageProcessingLoop(
            table: "error",
            delay: TimeSpan.FromSeconds(1),
            transactionBuilder: TransactionBuilder,
            callback: Callback,
            errorCallback: ErrorCallback,
            startingRow: startingRow,
            persistRowVersion: PersistRowVersion);
        processingLoop.Start();

        Console.ReadKey();

        await processingLoop.Stop();

        #endregion
    }

}