using System;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;

public class ConsumingLoop
{
    string connectionString = null!;

    async Task ConsumeLoop()
    {
        #region ConsumeLoop

        static async Task Callback(
            DbTransaction transaction,
            IncomingMessage message,
            CancellationToken cancellation)
        {
            if (message.Body != null)
            {
                using var reader = new StreamReader(message.Body);
                var bodyText = await reader.ReadToEndAsync();
                Console.WriteLine($"Reply received:\r\n{bodyText}");
            }
        }

        Task<DbTransaction> BuildTransaction(CancellationToken cancellation)
        {
            return ConnectionHelpers.BeginTransaction(connectionString, cancellation);
        }

        static void ErrorCallback(Exception exception)
        {
            Environment.FailFast("Message consuming loop failed", exception);
        }

        // start consuming
        var consumingLoop = new MessageConsumingLoop(
            table: "endpointTable",
            delay: TimeSpan.FromSeconds(1),
            transactionBuilder: BuildTransaction,
            callback: Callback,
            errorCallback: ErrorCallback);
        consumingLoop.Start();

        // stop consuming
        await consumingLoop.Stop();

        #endregion
    }
}