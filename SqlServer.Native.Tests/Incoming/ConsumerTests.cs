using System;
using System.Collections.Generic;
using System.Text;
using NServiceBus.Transport.SqlServerNative;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class ConsumerTests : TestBase
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    string table = "ConsumerTests";

    [Fact]
    public void Single()
    {
        SqlHelpers.Drop(Connection.ConnectionString, table).Await();
        QueueCreator.Create(Connection.ConnectionString, table).Await();
        var sender = new Sender(table);

        var message = BuildMessage("00000000-0000-0000-0000-000000000001");
        sender.Send(Connection.ConnectionString, message).Await();
        var consumer = new Consumer(table);
        var result = consumer.Consume(Connection.ConnectionString).Result;
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public void Single_with_nulls()
    {
        SqlHelpers.Drop(Connection.ConnectionString, table).Await();
        QueueCreator.Create(Connection.ConnectionString, table).Await();
        var sender = new Sender(table);

        var message = BuildNullMessage("00000000-0000-0000-0000-000000000001");
        sender.Send(Connection.ConnectionString, message).Await();
        var consumer = new Consumer(table);
        var result = consumer.Consume(Connection.ConnectionString).Result;
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public void Batch()
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

        var consumer = new Consumer(table);
        var messages = new List<IncomingMessage>();
        var result = consumer.Consume(
                connection: Connection.ConnectionString,
                size: 3,
                action: message => { messages.Add(message); })
            .Result;
        Assert.Equal(3, result.LastRowVersion);
        Assert.Equal(3, result.Count);
        ObjectApprover.VerifyWithJson(messages);
    }

    [Fact]
    public void Batch_with_nulls()
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

        var consumer = new Consumer(table);
        var messages = new List<IncomingMessage>();
        var result = consumer.Consume(
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

    public ConsumerTests(ITestOutputHelper output) : base(output)
    {
    }
}