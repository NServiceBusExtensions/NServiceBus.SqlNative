using System;
using System.Collections.Generic;
using System.Text;
using ObjectApproval;
using SqlServer.Native;
using Xunit;

public class FinderTests
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    static FinderTests()
    {
        DbSetup.Setup();
    }

    [Fact]
    public void FinderSingle()
    {
        SqlHelpers.Drop(Connection.ConnectionString, "FinderTests").Await();
        QueueCreator.Create(Connection.ConnectionString, "FinderTests").Await();
        var sender = new Sender("FinderTests");

        var message = BuildMessage("00000000-0000-0000-0000-000000000001");
        sender.Send(Connection.ConnectionString, message).Await();
        var finder = new Finder("FinderTests");
        var received = finder.Find(Connection.ConnectionString, 1).Result;
        ObjectApprover.VerifyWithJson(received);
    }

    [Fact]
    public void FinderBatch()
    {
        SqlHelpers.Drop(Connection.ConnectionString, "FinderTests").Await();
        QueueCreator.Create(Connection.ConnectionString, "FinderTests").Await();
        var sender = new Sender("FinderTests");

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

        var finder = new Finder("FinderTests");
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
}