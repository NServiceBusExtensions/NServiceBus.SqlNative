using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using Xunit;
using Xunit.Abstractions;

public class DelayedConsumerTests :
    TestBase
{
    string table = "DelayedConsumerTests";

    [Fact]
    public async Task Single()
    {
        await DelayedTestDataBuilder.SendData(table);
        var consumer = new DelayedQueueManager(table, SqlConnection);
        await using var result = await consumer.Consume();
        ObjectApprover.Verify(result!.ToVerifyTarget());
    }

    [Fact]
    public async Task Single_nulls()
    {
        await DelayedTestDataBuilder.SendNullData(table);
        var consumer = new DelayedQueueManager(table, SqlConnection);
        await using var result = await consumer.Consume();
        ObjectApprover.Verify(result!.ToVerifyTarget());
    }

    [Fact]
    public async Task Batch()
    {
        await DelayedTestDataBuilder.SendMultipleData(table);

        var consumer = new DelayedQueueManager(table, SqlConnection);
        var messages = new ConcurrentBag<IncomingDelayedVerifyTarget>();
        var result = await consumer.Consume(size: 3,
            action: message => { messages.Add(message.ToVerifyTarget()); }).ConfigureAwait(false);
        Assert.Equal(3, result.Count);
        Assert.Equal(3, result.LastRowVersion);
        ObjectApprover.Verify(messages.OrderBy(x => x.Due));
    }

    public DelayedConsumerTests(ITestOutputHelper output) : base(output)
    {
        var manager = new DelayedQueueManager(table, SqlConnection);
        manager.Drop().Await();
        manager.Create().Await();
    }
}