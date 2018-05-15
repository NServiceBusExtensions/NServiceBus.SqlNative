using System.Collections.Generic;
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
        using (var result = consumer.ConsumeStream().Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public void Single_nulls()
    {
        TestDataBuilder.SendNullData(table);
        var consumer = new QueueManager(table, SqlConnection);
        using (var result = consumer.ConsumeStream().Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public void Batch()
    {
        TestDataBuilder.SendMultipleData(table);

        var consumer = new QueueManager(table, SqlConnection);
        var messages = new List<object>();
        var result = consumer.ConsumeStream(size: 3,
                action: message => { messages.Add(message.ToVerifyTarget()); })
            .Result;
        Assert.Equal(3, result.Count);
        Assert.Equal(3, result.LastRowVersion);
        ObjectApprover.VerifyWithJson(messages);
    }

    public ConsumerTests(ITestOutputHelper output) : base(output)
    {
        var manager = new QueueManager(table, SqlConnection);
        manager.Drop().Await();
        manager.Create().Await();
    }
}