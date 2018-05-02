using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NServiceBus.Transport.SqlServerNative;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class DelayedSenderTests : TestBase
{
    string table = "DelayedSenderTests";
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    [Fact]
    public void Single_bytes()
    {
        var message = BuildBytesMessage();
        Send(message);
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }
    [Fact]
    public void Single_bytes_nulls()
    {
        var message = BuildBytesNullMessage();
        Send(message);
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }

    [Fact]
    public void Single_stream()
    {
        var message = BuildStreamMessage();
        Send(message);
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }

    [Fact]
    public void Single_stream_nulls()
    {
        var sender = new DelayedQueueManager(table, SqlConnection);

        var message = BuildBytesNullMessage();
        sender.Send( message).Await();
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }

    [Fact]
    public void Batch()
    {
        var messages = new List<OutgoingDelayedMessage>
        {
            BuildBytesMessage(),
            BuildStreamMessage()
        };
        Send(messages);
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }

    [Fact]
    public void Batch_nulls()
    {
        var messages = new List<OutgoingDelayedMessage>
        {
            BuildBytesNullMessage(),
            BuildStreamNullMessage()
        };
        Send(messages);
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }

    void Send(OutgoingDelayedMessage message)
    {
        var sender = new DelayedQueueManager(table, SqlConnection);

        sender.Send(message).Await();
    }

    void Send(List<OutgoingDelayedMessage> messages)
    {
        var sender = new DelayedQueueManager(table, SqlConnection);

        sender.Send(messages).Await();
    }

    static OutgoingDelayedMessage BuildBytesMessage()
    {
        return new OutgoingDelayedMessage(dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }

    static OutgoingDelayedMessage BuildStreamMessage()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        return new OutgoingDelayedMessage(dateTime, "headers", stream);
    }

    static OutgoingDelayedMessage BuildBytesNullMessage()
    {
        return new OutgoingDelayedMessage(dateTime, null, bodyBytes: null);
    }

    static OutgoingDelayedMessage BuildStreamNullMessage()
    {
        return new OutgoingDelayedMessage(dateTime, null, bodyStream: null);
    }

    public DelayedSenderTests(ITestOutputHelper output) : base(output)
    {
        SqlHelpers.Drop(SqlConnection, table).Await();
        var manager = new DelayedQueueManager(table, SqlConnection);
        manager.Create().Await();
    }
}