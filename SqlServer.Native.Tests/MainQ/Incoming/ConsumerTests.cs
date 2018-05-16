﻿using System.Collections.Concurrent;
using System.Linq;
using NServiceBus.Transport.SqlServerNative;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class ConsumerTests : TestBase
{
    string table = "ConsumerTests";

    [Fact]
    public void Single()
    {
        TestDataBuilder.SendData(table);
        var consumer = new QueueManager(table, SqlConnection);
        using (var result = consumer.Consume().Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public void Single_nulls()
    {
        TestDataBuilder.SendNullData(table);
        var consumer = new QueueManager(table, SqlConnection);
        using (var result = consumer.Consume().Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public void Batch()
    {
        TestDataBuilder.SendMultipleData(table);

        var consumer = new QueueManager(table, SqlConnection);
        var messages = new ConcurrentBag<IncomingVerifyTarget>();
        var result = consumer.Consume(
                size: 3,
                action: message => { messages.Add(message.ToVerifyTarget()); })
            .Result;
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Batch_all()
    {
        TestDataBuilder.SendMultipleData(table);

        var consumer = new QueueManager(table, SqlConnection);
        var messages = new ConcurrentBag<IncomingVerifyTarget>();
        var result = consumer.Consume(
                size: 10,
                action: message => { messages.Add(message.ToVerifyTarget()); })
            .Result;
        Assert.Equal(5, result.Count);
        ObjectApprover.VerifyWithJson(messages.OrderBy(x => x.Id));
    }

    public ConsumerTests(ITestOutputHelper output) : base(output)
    {
        var manager = new QueueManager(table, SqlConnection);
        manager.Drop().Await();
        manager.Create().Await();
    }
}