using System;
using System.Collections.Generic;
using System.Text;
using NServiceBus.Transport.SqlServerNative;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class FinderTests : TestBase
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    string table = "FinderTests";

    [Fact]
    public void FinderSingle()
    {
        SqlHelpers.Drop(Connection.ConnectionString, table).Await();
        QueueCreator.Create(Connection.ConnectionString, table).Await();
        var sender = new Sender(table);

        var message = BuildMessage("00000000-0000-0000-0000-000000000001");
        sender.Send(Connection.ConnectionString, message).Await();
        var finder = new Finder(table);
        var received = finder.Find(Connection.ConnectionString, 1).Result;
        ObjectApprover.VerifyWithJson(received);
    }

    [Fact]
    public void FinderBatch()
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

        var finder = new Finder(table);
        var messages = new List<IncomingMessage>();
        var result = finder.Find(
                connection: Connection.ConnectionString,
                size: 3,
                startRowVersion: 2,
                action: message => { messages.Add(message); })
            .Result;
        Assert.Equal(4, result.LastRowVersion);
        Assert.Equal(3, result.Count);
        ObjectApprover.VerifyWithJson(messages);
    }

    static OutgoingMessage BuildMessage(string guid)
    {
        return new OutgoingMessage(new Guid(guid), "theCorrelationId", "theReplyToAddress", dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }

    public FinderTests(ITestOutputHelper output) : base(output)
    {
    }
}