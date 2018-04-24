using System;
using System.Collections.Generic;
using System.Text;
using ObjectApproval;
using SqlServer.Native;
using Xunit;
using Xunit.Abstractions;

public class ReceiverTests : TestBase
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    string table = "ReceiverTests";

    [Fact]
    public void ReceiveSingle()
    {
        SqlHelpers.Drop(Connection.ConnectionString, table).Await();
        QueueCreator.Create(Connection.ConnectionString, table).Await();
        var sender = new Sender("ReceiverTests");

        var message = BuildMessage("00000000-0000-0000-0000-000000000001");
        sender.Send(Connection.ConnectionString, message).Await();
        var receiver = new Receiver(table);
        var received = receiver.Receive(Connection.ConnectionString).Result;
        ObjectApprover.VerifyWithJson(received);
    }

    [Fact]
    public void Can_receive_single_with_nulls()
    {
        SqlHelpers.Drop(Connection.ConnectionString, table).Await();
        QueueCreator.Create(Connection.ConnectionString, table).Await();
        var sender = new Sender("ReceiverTests");

        var message = BuildNullMessage("00000000-0000-0000-0000-000000000001");
        sender.Send(Connection.ConnectionString, message).Await();
        var receiver = new Receiver(table);
        var received = receiver.Receive(Connection.ConnectionString).Result;
        ObjectApprover.VerifyWithJson(received);
    }

    [Fact]
    public void ReceiveBatch()
    {
        SqlHelpers.Drop(Connection.ConnectionString, table).Await();
        QueueCreator.Create(Connection.ConnectionString, table).Await();
        var sender = new Sender(table);

        sender.Send(
            Connection.ConnectionString,
            new List<OutgoingMessage>
            {
                BuildMessage("00000000-0000-0000-0000-000000000001"),
                BuildMessage("00000000-0000-0000-0000-000000000002"),
                BuildMessage("00000000-0000-0000-0000-000000000003"),
                BuildMessage("00000000-0000-0000-0000-000000000004"),
                BuildMessage("00000000-0000-0000-0000-000000000005")
            }).Await();

        var receiver = new Receiver(table);
        var messages = new List<IncomingMessage>();
        var result = receiver.Receive(
                connection: Connection.ConnectionString,
                size: 3,
                action: message => { messages.Add(message); })
            .Result;
        Assert.Equal(3, result.LastRowVersion);
        Assert.Equal(3, result.Count);
        ObjectApprover.VerifyWithJson(messages);
    }

    [Fact]
    public void Can_receive_batch_with_nulls()
    {
        SqlHelpers.Drop(Connection.ConnectionString, table).Await();
        QueueCreator.Create(Connection.ConnectionString, table).Await();
        var sender = new Sender(table);

        sender.Send(
            Connection.ConnectionString,
            new List<OutgoingMessage>
            {
                BuildNullMessage("00000000-0000-0000-0000-000000000001"),
                BuildNullMessage("00000000-0000-0000-0000-000000000002"),
                BuildNullMessage("00000000-0000-0000-0000-000000000003"),
                BuildNullMessage("00000000-0000-0000-0000-000000000004"),
                BuildNullMessage("00000000-0000-0000-0000-000000000005")
            }).Await();

        var receiver = new Receiver(table);
        var messages = new List<IncomingMessage>();
        var result = receiver.Receive(
                connection: Connection.ConnectionString,
                size: 3,
                action: message => { messages.Add(message); })
            .Result;
        Assert.Equal(3, result.LastRowVersion);
        Assert.Equal(3, result.Count);
        ObjectApprover.VerifyWithJson(messages);
    }

    static OutgoingMessage BuildMessage(string guid)
    {
        return new OutgoingMessage(new Guid(guid), "theCorrelationId", "theReplyToAddress", dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }

    static OutgoingMessage BuildNullMessage(string guid)
    {
        return new OutgoingMessage(new Guid(guid), null, null, null, "headers", null);
    }

    public ReceiverTests(ITestOutputHelper output) : base(output)
    {
    }
}