using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class WithDeduplicationTests : TestBase
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    string table = "WithDeduplicationTests";

    [Fact]
    public async Task Single()
    {
        var message = BuildBytesMessage("00000000-0000-0000-0000-000000000001");
        await Send(message);
        ObjectApprover.VerifyWithJson(await SqlHelper.ReadData(table, SqlConnection));
    }

    [Fact]
    public async Task Single_WithDuplicate()
    {
        var message = BuildBytesMessage("00000000-0000-0000-0000-000000000001");
        await Send(message);
        await Send(message);
        ObjectApprover.VerifyWithJson(await SqlHelper.ReadData(table, SqlConnection));
    }

    //[Fact]
    //public void Single_WithPurgedDuplicate()
    //{
    //    var message = BuildBytesMessage("00000000-0000-0000-0000-000000000001");
    //    Send(message);
    //    Send(message);
    //    ObjectApprover.VerifyWithJson(SqlHelper.ReadData(table));
    //}

    [Fact]
    public async Task Batch()
    {
        var messages = new List<OutgoingMessage>
        {
            BuildBytesMessage("00000000-0000-0000-0000-000000000001"),
            BuildBytesMessage("00000000-0000-0000-0000-000000000002")
        };
        await Send(messages);
        ObjectApprover.VerifyWithJson(await SqlHelper.ReadData(table, SqlConnection));
    }

    [Fact]
    public async Task Batch_WithFirstDuplicate()
    {
        var message = BuildBytesMessage("00000000-0000-0000-0000-000000000001");
        await Send(message);
        var messages = new List<OutgoingMessage>
        {
            BuildBytesMessage("00000000-0000-0000-0000-000000000001"),
            BuildBytesMessage("00000000-0000-0000-0000-000000000002")
        };
        await Send(messages);
        ObjectApprover.VerifyWithJson(await SqlHelper.ReadData(table, SqlConnection));
    }

    [Fact]
    public async Task Batch_WithSecondDuplicate()
    {
        var message = BuildBytesMessage("00000000-0000-0000-0000-000000000002");
        await Send(message);
        var messages = new List<OutgoingMessage>
        {
            BuildBytesMessage("00000000-0000-0000-0000-000000000001"),
            BuildBytesMessage("00000000-0000-0000-0000-000000000002")
        };
        await Send(messages);
        ObjectApprover.VerifyWithJson(await SqlHelper.ReadData(table, SqlConnection));
    }

    Task Send(List<OutgoingMessage> messages)
    {
        var sender = new QueueManager(table, SqlConnection, "Deduplication");
        return sender.Send(messages);
    }

    Task<long> Send(OutgoingMessage message)
    {
        var sender = new QueueManager(table, SqlConnection, "Deduplication");
        return sender.Send(message);
    }

    static OutgoingMessage BuildBytesMessage(string guid)
    {
        return new OutgoingMessage(new Guid(guid), dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }

    public WithDeduplicationTests(ITestOutputHelper output) : base(output)
    {
        var manager = new QueueManager(table, SqlConnection, "Deduplication");
        manager.Drop().Await();
        manager.Create().Await();
        var deduplication = new DeduplicationManager(SqlConnection, "Deduplication");
        deduplication.Drop().Await();
        deduplication.Create().Await();
    }
}