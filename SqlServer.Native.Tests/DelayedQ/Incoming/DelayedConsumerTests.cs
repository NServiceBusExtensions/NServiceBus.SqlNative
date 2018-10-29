using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class DelayedConsumerTests : TestBase
{
    string table = "DelayedConsumerTests";

    [Fact]
    public async Task Single()
    {
       await DelayedTestDataBuilder.SendData(table);
        var consumer = new DelayedQueueManager(table, SqlConnection);
        using (var result = consumer.Consume().Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public async Task Single_nulls()
    {
        await DelayedTestDataBuilder.SendNullData(table);
        var consumer = new DelayedQueueManager(table, SqlConnection);
        using (var result = consumer.Consume().Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public async Task Batch()
    {
        await DelayedTestDataBuilder.SendMultipleData(table);

        var consumer = new DelayedQueueManager(table, SqlConnection);
        var messages = new ConcurrentBag<IncomingDelayedVerifyTarget>();
        var result = consumer.Consume(size: 3,
                action: message => { messages.Add(message.ToVerifyTarget()); })
            .Result;
        Assert.Equal(3, result.Count);
        Assert.Equal(3, result.LastRowVersion);
        ObjectApprover.VerifyWithJson(messages.OrderBy(x => x.Due));
    }

    public DelayedConsumerTests(ITestOutputHelper output) : base(output)
    {
        var manager = new DelayedQueueManager(table, SqlConnection);
        manager.Drop().Await();
        manager.Create().Await();
    }
}