using System;
using System.Collections.Generic;
using System.Text;
using ObjectApproval;
using SqlServer.Native;
using Xunit;

public class ReceiverTests
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    static ReceiverTests()
    {
        DbSetup.Setup();
    }

    [Fact]
    public void ReceiveSingle()
    {
        SqlHelpers.Drop(Connection.ConnectionString, "ReceiverTests").Await();
        QueueCreator.Create(Connection.ConnectionString, "ReceiverTests").Await();
        var sender = new Sender("ReceiverTests");

        var message = BuildMessage("00000000-0000-0000-0000-000000000001");
        sender.Send(Connection.ConnectionString, message).Await();
        var receiver = new Receiver("ReceiverTests");
        var received = receiver.Receive(Connection.ConnectionString).Result;
        ObjectApprover.VerifyWithJson(received);
    }

    [Fact]
    public void ReceiveBatch()
    {
        SqlHelpers.Drop(Connection.ConnectionString, "ReceiverTests").Await();
        QueueCreator.Create(Connection.ConnectionString, "ReceiverTests").Await();
        var sender = new Sender("ReceiverTests");

        sender.Send(
            Connection.ConnectionString,
            new List<OutgoingMessage>
            {
                BuildMessage("00000000-0000-0000-0000-000000000001"),
                BuildMessage("00000000-0000-0000-0000-000000000002")
            }).Await();

        var receiver = new Receiver("ReceiverTests");
        var messages = new List<IncomingMessage>();
        receiver.Receive(
            connection: Connection.ConnectionString,
            size: 10,
            action: message => { messages.Add(message); })
            .Await();
        ObjectApprover.VerifyWithJson(messages);
    }

    static OutgoingMessage BuildMessage(string guid)
    {
        return new OutgoingMessage(new Guid(guid), "theCorrelationId", "theReplyToAddress", dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }
}