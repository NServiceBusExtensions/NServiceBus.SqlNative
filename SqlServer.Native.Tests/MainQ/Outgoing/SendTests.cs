using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NServiceBus.Transport.SqlServerNative;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class SendTests : TestBase
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    string table = "SendTests";

    [Fact]
    public void Single_bytes()
    {
        var message = BuildBytesMessage("00000000-0000-0000-0000-000000000001");
        Send(message);
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }

    [Fact]
    public void Single_with_transaction()
    {
        var message = BuildBytesMessage("00000000-0000-0000-0000-000000000001");
        using (var transaction = SqlConnection.BeginTransaction())
        {
            var sender = new QueueManager(table, transaction);
            sender.Send(message).Await();
            transaction.Commit();
        }

        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }

    [Fact]
    public void Single_bytes_nulls()
    {
        var sender = new QueueManager("SendTests", SqlConnection);

        var message = BuildBytesNullMessage("00000000-0000-0000-0000-000000000001");
        sender.Send(message).Await();
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }

    [Fact]
    public void Single_stream()
    {
        var message = BuildStreamMessage("00000000-0000-0000-0000-000000000001");
        Send(message);
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }

    [Fact]
    public void Single_stream_nulls()
    {
        var message = BuildStreamMessage("00000000-0000-0000-0000-000000000001");
        Send(message);
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }

    [Fact]
    public void Batch()
    {
        var messages = new List<OutgoingMessage>
        {
            BuildBytesMessage("00000000-0000-0000-0000-000000000001"),
            BuildStreamMessage("00000000-0000-0000-0000-000000000002")
        };
        Send(messages);
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }

    [Fact]
    public void Batch_nulls()
    {
        var messages = new List<OutgoingMessage>
        {
            BuildBytesNullMessage("00000000-0000-0000-0000-000000000001"),
            BuildStreamNullMessage("00000000-0000-0000-0000-000000000002")
        };
        Send(messages);

        ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    }

    void Send(List<OutgoingMessage> messages)
    {
        var sender = new QueueManager(table, SqlConnection);
        sender.Send(messages).Await();
    }

    void Send(OutgoingMessage message)
    {
        var sender = new QueueManager(table, SqlConnection);
        sender.Send(message).Await();
    }

    static OutgoingMessage BuildBytesMessage(string guid)
    {
        return new OutgoingMessage(new Guid(guid), dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }

    static OutgoingMessage BuildStreamMessage(string guid)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        return new OutgoingMessage(new Guid(guid), dateTime, "headers", stream);
    }

    static OutgoingMessage BuildStreamNullMessage(string guid)
    {
        return new OutgoingMessage(new Guid(guid), bodyStream: null);
    }

    static OutgoingMessage BuildBytesNullMessage(string guid)
    {
        return new OutgoingMessage(new Guid(guid), bodyBytes: null);
    }

    public SendTests(ITestOutputHelper output) : base(output)
    {
        var manager = new QueueManager(table, SqlConnection);
        manager.Drop().Await();
        manager.Create().Await();
    }
}