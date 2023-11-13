using Microsoft.Data.SqlClient;
using NServiceBus.Transport.SqlServerNative;
// ReSharper disable ReplaceAsyncWithTaskReturn

public class DelayedQueue
{
    SqlConnection sqlConnection = null!;

    // ReSharper disable once ReplaceAsyncWithTaskReturn
    async Task CreateQueue()
    {
        #region CreateDelayedQueue

        var manager = new DelayedQueueManager("endpointTable.Delayed", sqlConnection);
        await manager.Create();

        #endregion
    }

    async Task DeleteQueue()
    {
        #region DeleteDelayedQueue

        var manager = new DelayedQueueManager("endpointTable.Delayed", sqlConnection);
        await manager.Drop();

        #endregion
    }

    async Task Send()
    {
        string headers = null!;
        byte[] body = null!;

        #region SendDelayed

        var manager = new DelayedQueueManager("endpointTable.Delayed", sqlConnection);
        var message = new OutgoingDelayedMessage(
            due: DateTime.UtcNow.AddDays(1),
            headers: headers,
            bodyBytes: body);
        await manager.Send(message);

        #endregion
    }

    async Task SendBatch()
    {
        string headers1 = null!;
        byte[] body1 = null!;
        string headers2 = null!;
        byte[] body2 = null!;

        #region SendDelayedBatch

        var manager = new DelayedQueueManager("endpointTable.Delayed", sqlConnection);
        var messages = new List<OutgoingDelayedMessage>
        {
            new(
                due: DateTime.UtcNow.AddDays(1),
                headers: headers1,
                bodyBytes: body1),
            new(
                due: DateTime.UtcNow.AddDays(1),
                headers: headers2,
                bodyBytes: body2),
        };
        await manager.Send(messages);

        #endregion
    }

    async Task Read()
    {
        #region ReadDelayed

        var manager = new DelayedQueueManager("endpointTable", sqlConnection);
        var message = await manager.Read(rowVersion: 10);

        if (message != null)
        {
            Console.WriteLine(message.Headers);
            if (message.Body != null)
            {
                using var reader = new StreamReader(message.Body);
                var bodyText = await reader.ReadToEndAsync();
                Console.WriteLine(bodyText);
            }
        }

        #endregion
    }

    async Task ReadBatch()
    {
        #region ReadDelayedBatch

        var manager = new DelayedQueueManager("endpointTable", sqlConnection);
        var result = await manager.Read(
            size: 5,
            startRowVersion: 10,
            func: async (message, cancel) =>
            {
                Console.WriteLine(message.Headers);
                if (message.Body == null)
                {
                    return;
                }

                using var reader = new StreamReader(message.Body);
                var bodyText = await reader.ReadToEndAsync(cancel);
                Console.WriteLine(bodyText);
            });

        Console.WriteLine(result.Count);
        Console.WriteLine(result.LastRowVersion);

        #endregion
    }

    async Task Consume()
    {
        #region ConsumeDelayed

        var manager = new DelayedQueueManager("endpointTable", sqlConnection);
        var message = await manager.Consume();

        if (message != null)
        {
            Console.WriteLine(message.Headers);
            if (message.Body != null)
            {
                using var reader = new StreamReader(message.Body);
                var bodyText = await reader.ReadToEndAsync();
                Console.WriteLine(bodyText);
            }
        }

        #endregion
    }

    async Task ConsumeBatch()
    {
        #region ConsumeDelayedBatch

        var manager = new DelayedQueueManager("endpointTable", sqlConnection);
        var result = await manager.Consume(
            size: 5,
            func: async (message, cancel) =>
            {
                Console.WriteLine(message.Headers);
                if (message.Body == null)
                {
                    return;
                }

                using var reader = new StreamReader(message.Body);
                var bodyText = await reader.ReadToEndAsync(cancel);
                Console.WriteLine(bodyText);
            });

        Console.WriteLine(result.Count);
        Console.WriteLine(result.LastRowVersion);

        #endregion
    }
}