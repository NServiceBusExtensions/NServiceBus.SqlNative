

// ReSharper disable ReplaceAsyncWithTaskReturn

public class ConsumingLoop
{
    string connectionString = null!;

    async Task ConsumeLoop()
    {
        #region ConsumeLoop

        static async Task Callback(
            SqlTransaction transaction,
            IncomingMessage message,
            Cancel cancel)
        {
            if (message.Body != null)
            {
                using var reader = new StreamReader(message.Body);
                var bodyText = await reader.ReadToEndAsync(cancel);
                Console.WriteLine($"Reply received:\r\n{bodyText}");
            }
        }

        Task<SqlTransaction> BuildTransaction(Cancel cancel) =>
            ConnectionHelpers.BeginTransaction(connectionString, cancel);

        static void ErrorCallback(Exception exception) =>
            Environment.FailFast("Message consuming loop failed", exception);

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