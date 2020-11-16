using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class SendTests :
    TestBase
{
    static DateTime dateTime = new(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    string table = "SendTests";

    [Fact]
    public async Task Single_bytes()
    {
        var message = BuildBytesMessage("00000000-0000-0000-0000-000000000001");
        await Send(message);
        await Verifier.Verify(SqlHelper.ReadData(table, SqlConnection));
    }

    [Fact]
    public async Task Single_with_transaction()
    {
        var message = BuildBytesMessage("00000000-0000-0000-0000-000000000001");
        await using var transaction = SqlConnection.BeginTransaction();
        var sender = new QueueManager(table, transaction);
        await sender.Send(message);
        await transaction.CommitAsync();
        await Verifier.Verify(SqlHelper.ReadData(table, SqlConnection));
    }

    [Fact]
    public async Task Single_bytes_nulls()
    {
        var sender = new QueueManager("SendTests", SqlConnection);

        var message = BuildBytesNullMessage("00000000-0000-0000-0000-000000000001");
        await sender.Send(message);
        await Verifier.Verify(SqlHelper.ReadData(table, SqlConnection));
    }

    [Fact]
    public async Task Single_stream()
    {
        var message = BuildStreamMessage("00000000-0000-0000-0000-000000000001");
        await Send(message);
        await Verifier.Verify(SqlHelper.ReadData(table, SqlConnection));
    }

    [Fact]
    public async Task Single_stream_nulls()
    {
        var message = BuildStreamMessage("00000000-0000-0000-0000-000000000001");
        await Send(message);
        await Verifier.Verify(SqlHelper.ReadData(table, SqlConnection));
    }

    [Fact]
    public async Task Batch()
    {
        var messages = new List<OutgoingMessage>
        {
            BuildBytesMessage("00000000-0000-0000-0000-000000000001"),
            BuildStreamMessage("00000000-0000-0000-0000-000000000002")
        };
        await Send(messages);
        await Verifier.Verify(SqlHelper.ReadData(table, SqlConnection));
    }

    [Fact]
    public async Task Batch_nulls()
    {
        var messages = new List<OutgoingMessage>
        {
            BuildBytesNullMessage("00000000-0000-0000-0000-000000000001"),
            BuildStreamNullMessage("00000000-0000-0000-0000-000000000002")
        };
        await Send(messages);

        await Verifier.Verify(SqlHelper.ReadData(table, SqlConnection));
    }

    Task Send(List<OutgoingMessage> messages)
    {
        var sender = new QueueManager(table, SqlConnection);
        return sender.Send(messages);
    }

    Task<long> Send(OutgoingMessage message)
    {
        var sender = new QueueManager(table, SqlConnection);
        return sender.Send(message);
    }

    static OutgoingMessage BuildBytesMessage(string guid)
    {
        return new(new Guid(guid), dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }

    static OutgoingMessage BuildStreamMessage(string guid)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        return new OutgoingMessage(new Guid(guid), dateTime, "headers", stream);
    }

    static OutgoingMessage BuildStreamNullMessage(string guid)
    {
        return new(new Guid(guid), bodyStream: null);
    }

    static OutgoingMessage BuildBytesNullMessage(string guid)
    {
        return new(new Guid(guid), bodyBytes: null);
    }

    public SendTests()
    {
        var manager = new QueueManager(table, SqlConnection);
        manager.Drop().Await();
        manager.Create().Await();
    }
}