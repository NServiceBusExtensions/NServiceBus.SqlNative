using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;

public class MainQueue
{
    SqlConnection sqlConnection = null!;

    async Task CreateQueue()
    {
        #region CreateQueue

        var manager = new QueueManager("endpointTable", sqlConnection);
        await manager.Create();

        #endregion
    }

    async Task DeleteQueue()
    {
        #region DeleteQueue

        var manager = new QueueManager("endpointTable", sqlConnection);
        await manager.Drop();

        #endregion
    }

    async Task Send()
    {
        string headers = null!;
        byte[] body = null!;

        #region Send

        var manager = new QueueManager("endpointTable", sqlConnection);
        var message = new OutgoingMessage(
            id: Guid.NewGuid(),
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

        #region SendBatch

        var manager = new QueueManager("endpointTable", sqlConnection);
        var messages = new List<OutgoingMessage>
        {
            new OutgoingMessage(
                id: Guid.NewGuid(),
                headers: headers1,
                bodyBytes: body1),
            new OutgoingMessage(
                id: Guid.NewGuid(),
                headers: headers2,
                bodyBytes: body2),
        };
        await manager.Send(messages);

        #endregion
    }

    async Task Read()
    {
        #region Read

        var manager = new QueueManager("endpointTable", sqlConnection);
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
        #region ReadBatch

        var manager = new QueueManager("endpointTable", sqlConnection);
        var result = await manager.Read(
            size: 5,
            startRowVersion: 10,
            action: async message =>
            {
                Console.WriteLine(message.Headers);
                if (message.Body == null)
                {
                    return;
                }

                using var reader = new StreamReader(message.Body);
                var bodyText = await reader.ReadToEndAsync();
                Console.WriteLine(bodyText);
            });

        Console.WriteLine(result.Count);
        Console.WriteLine(result.LastRowVersion);

        #endregion
    }

    async Task Consume()
    {
        #region Consume

        var manager = new QueueManager("endpointTable", sqlConnection);
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
        #region ConsumeBatch

        var manager = new QueueManager("endpointTable", sqlConnection);
        var result = await manager.Consume(
            size: 5,
            action: async message =>
            {
                Console.WriteLine(message.Headers);
                if (message.Body == null)
                {
                    return;
                }

                using var reader = new StreamReader(message.Body);
                var bodyText = await reader.ReadToEndAsync();
                Console.WriteLine(bodyText);
            });

        Console.WriteLine(result.Count);
        Console.WriteLine(result.LastRowVersion);

        #endregion
    }
}