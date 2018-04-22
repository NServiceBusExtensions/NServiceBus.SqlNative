using System;
using System.Collections.Generic;
using System.Text;
using ObjectApproval;
using SqlServer.Native;
using Xunit;

public class SendTests
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    static SendTests()
    {
        DbSetup.Setup();
    }

    [Fact]
    public void SendSingle()
    {
        MessageQueueCreator.Drop(Connection.ConnectionString, "SendTests").Await();
        MessageQueueCreator.Create(Connection.ConnectionString, "SendTests").Await();
        var sender = new Sender();

        var message = BuildMessage("00000000-0000-0000-0000-000000000001");
        sender.Send(Connection.ConnectionString, "SendTests", message).Await();
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData("SendTests"));
    }

    [Fact]
    public void SendBatch()
    {
        MessageQueueCreator.Drop(Connection.ConnectionString, "SendTests").Await();
        MessageQueueCreator.Create(Connection.ConnectionString, "SendTests").Await();
        var sender = new Sender();

        sender.Send(
            Connection.ConnectionString, "SendTests",
            new List<OutgoingMessage>
            {
                BuildMessage("00000000-0000-0000-0000-000000000001"),
                BuildMessage("00000000-0000-0000-0000-000000000002")
            }).Await();
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData("SendTests"));
    }

    static OutgoingMessage BuildMessage(string guid)
    {
        var headers = new Dictionary<string, string>
        {
            {"headerKey1", "headerValue1"},
            {"headerKey2", "headerValue2"}
        };

        return new OutgoingMessage(new Guid(guid), "theCorrelationId", "theReplyToAddress", dateTime, headers, Encoding.UTF8.GetBytes("{}"));
    }
}