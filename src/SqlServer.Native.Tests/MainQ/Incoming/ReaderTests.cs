﻿using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class ReaderTests : TestBase
{
    string table = "ReaderTests";

    [Fact]
    public async Task Single()
    {
        await TestDataBuilder.SendData(table);
        var reader = new QueueManager(table, SqlConnection);
        using (var result = reader.Read(1).Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public async Task Single_nulls()
    {
        await TestDataBuilder.SendNullData(table);
        var reader = new QueueManager(table, SqlConnection);
        using (var result = reader.Read(1).Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public async Task Batch()
    {
        await TestDataBuilder.SendMultipleDataAsync(table);

        var reader = new QueueManager(table, SqlConnection);
        var messages = new ConcurrentBag<IncomingVerifyTarget>();
        var result = reader.Read(
                size: 3,
                startRowVersion: 2,
                action: message => { messages.Add(message.ToVerifyTarget()); })
            .Result;
        Assert.Equal(4, result.LastRowVersion);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Batch_all()
    {
        await TestDataBuilder.SendMultipleDataAsync(table);

        var reader = new QueueManager(table, SqlConnection);
        var messages = new ConcurrentBag<IncomingVerifyTarget>();
        await reader.Read(
                size: 10,
                startRowVersion: 1,
                action: message => { messages.Add(message.ToVerifyTarget()); });
        ObjectApprover.VerifyWithJson(messages.OrderBy(x => x.Id));
    }

    public ReaderTests(ITestOutputHelper output) : base(output)
    {
        var manager = new QueueManager(table, SqlConnection);
        manager.Drop().Await();
        manager.Create().Await();
    }
}