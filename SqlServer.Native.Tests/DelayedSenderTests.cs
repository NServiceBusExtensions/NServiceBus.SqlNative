using System;
using System.Collections.Generic;
using System.Text;
using ObjectApproval;
using SqlServer.Native;
using Xunit;

public class DelayedSenderTests
{
    string table = "DelayedSenderTests";
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    static DelayedSenderTests()
    {
        DbSetup.Setup();
    }

    [Fact]
    public void SendSingle()
    {
        SqlHelpers.Drop(Connection.ConnectionString, table).Await();
        QueueCreator.CreateDelayed(Connection.ConnectionString, table).Await();
        var sender = new DelayedSender(table);

        var message = BuildMessage();
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
                BuildMessage(),
                BuildMessage()
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

    static OutgoingDelayedMessage BuildMessage()
    {
        return new OutgoingDelayedMessage(dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }

    static OutgoingDelayedMessage BuildNullMessage()
    {
        return new OutgoingDelayedMessage(dateTime, "headers", null);
    }
}