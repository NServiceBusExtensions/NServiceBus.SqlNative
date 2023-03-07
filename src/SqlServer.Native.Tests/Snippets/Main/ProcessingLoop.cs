using Microsoft.Data.SqlClient;
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

        static async Task Callback(
            SqlTransaction transaction,
            IncomingMessage message,
            Cancellation cancellation)
        {
            if (message.Body == null)
            {
                return;
            }

            using var reader = new StreamReader(message.Body);
            var bodyText = await reader.ReadToEndAsync(cancellation);
            Console.WriteLine($"Message received in error message:\r\n{bodyText}");
        }

        static void ErrorCallback(Exception exception)
        {
            Environment.FailFast("Message processing loop failed", exception);
        }

        Task<SqlTransaction> BuildTransaction(Cancellation cancellation)
        {
            return ConnectionHelpers.BeginTransaction(connectionString, cancellation);
        }

        Task PersistRowVersion(
            SqlTransaction transaction,
            long rowVersion,
            Cancellation cancellation)
        {
            return rowVersionTracker.Save(sqlConnection, rowVersion, cancellation);
        }

        var processingLoop = new MessageProcessingLoop(
            table: "error",
            delay: TimeSpan.FromSeconds(1),
            transactionBuilder: BuildTransaction,
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