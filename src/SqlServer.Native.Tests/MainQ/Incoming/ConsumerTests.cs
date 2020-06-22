using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class ConsumerTests :
    TestBase
{
    string table = "ConsumerTests";

    [Fact]
    public async Task Single()
    {
        await TestDataBuilder.SendData(table);
        var consumer = new QueueManager(table, SqlConnection);
        await using var result = await consumer.Consume();
        await Verifier.Verify(result!.ToVerifyTarget());
    }

    [Fact]
    public async Task Single_nulls()
    {
        await TestDataBuilder.SendNullData(table);
        var consumer = new QueueManager(table, SqlConnection);
        await using var result = await consumer.Consume();
        await Verifier.Verify(result!.ToVerifyTarget());
    }

    [Fact]
    public async Task Batch()
    {
        await TestDataBuilder.SendMultipleDataAsync(table);

        var consumer = new QueueManager(table, SqlConnection);
        var messages = new ConcurrentBag<IncomingVerifyTarget>();
        var result = await consumer.Consume(
            size: 3,
            action: message => { messages.Add(message.ToVerifyTarget()); });
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Batch_all()
    {
        await TestDataBuilder.SendMultipleDataAsync(table);

        var consumer = new QueueManager(table, SqlConnection);
        var messages = new ConcurrentBag<IncomingVerifyTarget>();
        var result = await consumer.Consume(
            size: 10,
            action: message => { messages.Add(message.ToVerifyTarget()); });
        Assert.Equal(5, result.Count);
        await Verifier.Verify(messages.OrderBy(x => x.Id));
    }

    public ConsumerTests()
    {
        var manager = new QueueManager(table, SqlConnection);
        manager.Drop().Await();
        manager.Create().Await();
    }
}