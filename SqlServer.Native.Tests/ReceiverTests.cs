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
        MessageQueueCreator.Drop(Connection.ConnectionString, "SendTests").Await();
        MessageQueueCreator.Create(Connection.ConnectionString, "SendTests").Await();
        var sender = new Sender();

        var message = BuildMessage("00000000-0000-0000-0000-000000000001");
        sender.Send(Connection.ConnectionString, "SendTests", message).Await();
        var receiver = new Receiver();
        var received = receiver.Receive(Connection.ConnectionString, "SendTests").Result;
        ObjectApprover.VerifyWithJson(received);
    }

    [Fact]
    public void ReceiveBatch()
    {
        MessageQueueCreator.Drop(Connection.ConnectionString, "SendTests").Await();
        MessageQueueCreator.Create(Connection.ConnectionString, "SendTests").Await();
        var sender = new Sender();

        sender.Send(
            Connection.ConnectionString, "SendTests",
            new List<Message>
            {
                BuildMessage("00000000-0000-0000-0000-000000000001"),
                BuildMessage("00000000-0000-0000-0000-000000000002")
            }).Await();

        var receiver = new Receiver();
        var messages = new List<Message>();
        receiver.Receive(Connection.ConnectionString, "SendTests", message =>
        {
            messages.Add(message);
        }).Await();
        ObjectApprover.VerifyWithJson(messages);
    }

    static Message BuildMessage(string guid)
    {
        return new Message(new Guid(guid), "theCorrelationId", "theReplyToAddress", dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }
}