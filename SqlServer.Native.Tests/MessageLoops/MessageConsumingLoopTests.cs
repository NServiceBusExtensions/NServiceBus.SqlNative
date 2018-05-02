using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using Xunit;
using Xunit.Abstractions;

public class MessageConsumingLoopTests : TestBase
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    string table = "MessageConsumingLoopTests";

    [Fact]
    public async Task Should_not_throw_when_run_over_end()
    {
        await SqlHelpers.Drop(SqlConnection, table);
        await QueueCreator.Create(SqlConnection, table);
        await SendMessages(table);

        Exception exception = null;
        using (var loop = new MessageConsumingLoop(
            table: table,
            connectionBuilder: Connection.OpenAsyncConnection,
            callback: (message, cancellation) => Task.CompletedTask,
            errorCallback: innerException => { exception = innerException;}
            ))
        {
            loop.Start();
            Thread.Sleep(1000);
        }

        Assert.Null(exception);
    }
    [Fact]
    public async Task Should_get_correct_count()
    {
        var resetEvent = new ManualResetEvent(false);
        await SqlHelpers.Drop(SqlConnection, table);
        await QueueCreator.Create(SqlConnection, table);
        await SendMessages(table);

        var count = 0;

        Task Callback(IncomingBytesMessage message, CancellationToken cancellation)
        {
            count++;
            if (count == 5)
            {
                resetEvent.Set();
            }

            return Task.CompletedTask;
        }

        using (var loop = new MessageConsumingLoop(
            table: table,
            connectionBuilder: Connection.OpenAsyncConnection,
            callback: Callback,
            errorCallback: exception => { }))
        {
            loop.Start();
            resetEvent.WaitOne(TimeSpan.FromSeconds(30));
        }

        Assert.Equal(5, count);
    }

    async Task SendMessages(string table)
    {
        var sender = new Sender(table);

        await sender.Send(
            SqlConnection,
            new List<OutgoingMessage>
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
        return new OutgoingMessage(new Guid(guid), "theCorrelationId", "theReplyToAddress", dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }

    public MessageConsumingLoopTests(ITestOutputHelper output) : base(output)
    {
    }
}