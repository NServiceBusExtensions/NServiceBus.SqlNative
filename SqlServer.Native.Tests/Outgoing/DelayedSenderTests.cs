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
    public void Send_bytes()
    {
        var message = BuildBytesMessage();
        Send(message);
    }

    [Fact]
    public void Send_stream()
    {
        var message = BuildStreamMessage();
        Send(message);
    }

    void Send(OutgoingDelayedMessage message)
    {
        SqlHelpers.Drop(Connection.ConnectionString, table).Await();
        QueueCreator.CreateDelayed(Connection.ConnectionString, table).Await();
        var sender = new DelayedSender(table);

        sender.Send(Connection.ConnectionString, message).Await();
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }

    [Fact]
    public void Can_send_single_with_nulls()
    {
        SqlHelpers.Drop(Connection.ConnectionString, table).Await();
        QueueCreator.CreateDelayed(Connection.ConnectionString, table).Await();
        var sender = new DelayedSender(table);

        var message = BuildNullMessage();
        sender.Send(Connection.ConnectionString, message).Await();
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }

    [Fact]
    public void SendBatch()
    {
        SqlHelpers.Drop(Connection.ConnectionString, table).Await();
        QueueCreator.CreateDelayed(Connection.ConnectionString, table).Await();
        var sender = new DelayedSender(table);

        sender.Send(
            Connection.ConnectionString,
            new List<OutgoingDelayedMessage>
            {
                BuildBytesMessage(),
                BuildStreamMessage()
            }).Await();
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }

    [Fact]
    public void Can_send_batch_with_nulls()
    {
        SqlHelpers.Drop(Connection.ConnectionString, table).Await();
        QueueCreator.CreateDelayed(Connection.ConnectionString, table).Await();
        var sender = new DelayedSender(table);

        sender.Send(
            Connection.ConnectionString,
            new List<OutgoingDelayedMessage>
            {
                BuildNullMessage(),
                BuildNullMessage()
            }).Await();
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
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

    static OutgoingDelayedMessage BuildNullMessage()
    {
        return new OutgoingDelayedMessage(dateTime, null, bodyBytes: null);
    }

    public DelayedSenderTests(ITestOutputHelper output) : base(output)
    {
    }
}