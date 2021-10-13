using System.Data.Common;
using NServiceBus.Transport.SqlServerNative;
using Xunit;

public class MessageProcessingLoopTests :
    TestBase
{
    static DateTime dateTime = new(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    string table = "MessageProcessingLoopTests";

    [Fact]
    public async Task Should_not_throw_when_run_over_end()
    {
        await SqlConnection.DropTable(null, table);
        var manager = new QueueManager(table, SqlConnection);
        await manager.Create();
        await SendMessages();

        Exception? exception = null;
        await using var loop = new MessageProcessingLoop(
            table: table,
            startingRow: 1,
            connectionBuilder: Connection.OpenAsyncConnection,
            callback: (_, _, _) => Task.CompletedTask,
            errorCallback: innerException => { exception = innerException; },
            persistRowVersion: (_, _, _) => Task.CompletedTask
        );
        loop.Start();
        Thread.Sleep(1000);
        Assert.Null(exception!);
    }

    [Fact]
    public async Task Should_get_correct_count()
    {
        var resetEvent = new ManualResetEvent(false);
        await SqlConnection.DropTable(null, table);
        var manager = new QueueManager(table, SqlConnection);
        await manager.Create();
        await SendMessages();

        var count = 0;

        Task Callback(DbConnection connection, IncomingMessage incomingBytesMessage, CancellationToken arg3)
        {
            count++;
            if (count == 5)
            {
                resetEvent.Set();
            }

            return Task.CompletedTask;
        }

        await using var loop = new MessageProcessingLoop(
            table: table,
            startingRow: 1,
            connectionBuilder: Connection.OpenAsyncConnection,
            callback: Callback,
            errorCallback: _ => { },
            persistRowVersion: (_, _, _) => Task.CompletedTask);
        loop.Start();
        resetEvent.WaitOne(TimeSpan.FromSeconds(30));
        Assert.Equal(5, count);
    }

    [Fact]
    public async Task Should_get_correct_next_row_version()
    {
        var resetEvent = new ManualResetEvent(false);
        await SqlConnection.DropTable(null, table);
        var manager = new QueueManager(table, SqlConnection);
        await manager.Create();
        await SendMessages();

        long rowVersion = 0;

        Task PersistRowVersion(DbConnection sqlConnection, long currentRowVersion, CancellationToken arg3)
        {
            rowVersion = currentRowVersion;
            if (rowVersion == 6)
            {
                resetEvent.Set();
            }

            return Task.CompletedTask;
        }

        await using var loop = new MessageProcessingLoop(
            table: table,
            startingRow: 1,
            connectionBuilder: Connection.OpenAsyncConnection,
            callback: (_, _, _) => Task.CompletedTask,
            errorCallback: _ => { },
            persistRowVersion: PersistRowVersion);
        loop.Start();
        resetEvent.WaitOne(TimeSpan.FromSeconds(30));
        Assert.Equal(6, rowVersion);
    }

    Task SendMessages()
    {
        var sender = new QueueManager(table, SqlConnection);

        return sender.Send(new List<OutgoingMessage>
        {
            BuildMessage("00000000-0000-0000-0000-000000000001"),
            BuildMessage("00000000-0000-0000-0000-000000000002"),
            BuildMessage("00000000-0000-0000-0000-000000000003"),
            BuildMessage("00000000-0000-0000-0000-000000000004"),
            BuildMessage("00000000-0000-0000-0000-000000000005")
        });
    }

    static OutgoingMessage BuildMessage(string guid)
    {
        return new(new(guid), dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }
}