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
            Cancel cancel)
        {
            if (message.Body == null)
            {
                return;
            }

            using var reader = new StreamReader(message.Body);
            var bodyText = await reader.ReadToEndAsync(cancel);
            Console.WriteLine($"Message received in error message:\r\n{bodyText}");
        }

        static void ErrorCallback(Exception exception)
        {
            Environment.FailFast("Message processing loop failed", exception);
        }

        Task<SqlTransaction> BuildTransaction(Cancel cancel)
        {
            return ConnectionHelpers.BeginTransaction(connectionString, cancel);
        }

        Task PersistRowVersion(
            SqlTransaction transaction,
            long rowVersion,
            Cancel cancel)
        {
            return rowVersionTracker.Save(sqlConnection, rowVersion, cancel);
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