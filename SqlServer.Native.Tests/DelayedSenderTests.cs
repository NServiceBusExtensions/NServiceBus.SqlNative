using System;
using System.Collections.Generic;
using System.Text;
using ObjectApproval;
using SqlServer.Native;
using Xunit;

public class DelayedSenderTests
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    static DelayedSenderTests()
    {
        DbSetup.Setup();
    }

    [Fact]
    public void SendSingle()
    {
        SqlHelpers.Drop(Connection.ConnectionString, "DelayedSenderTests").Await();
        QueueCreator.CreateDelayed(Connection.ConnectionString, "DelayedSenderTests").Await();
        var sender = new DelayedSender("DelayedSenderTests");

        var message = BuildMessage();
        sender.Send(Connection.ConnectionString, message).Await();
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData("DelayedSenderTests"));
    }

    [Fact]
    public void SendBatch()
    {
        SqlHelpers.Drop(Connection.ConnectionString, "DelayedSenderTests").Await();
        QueueCreator.CreateDelayed(Connection.ConnectionString, "DelayedSenderTests").Await();
        var sender = new DelayedSender("DelayedSenderTests");

        sender.Send(
            Connection.ConnectionString,
            new List<OutgoingDelayedMessage>
            {
                BuildMessage(),
                BuildMessage()
            }).Await();
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData("DelayedSenderTests"));
    }

    static OutgoingDelayedMessage BuildMessage()
    {
        return new OutgoingDelayedMessage(dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }
}