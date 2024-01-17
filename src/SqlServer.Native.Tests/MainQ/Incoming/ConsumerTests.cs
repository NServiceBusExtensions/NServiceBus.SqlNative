using NServiceBus.Transport.SqlServerNative;

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
        await Verify(result!.ToVerifyTarget());
    }

    [Fact]
    public async Task Single_nulls()
    {
        await TestDataBuilder.SendNullData(table);
        var consumer = new QueueManager(table, SqlConnection);
        await using var result = await consumer.Consume();
        await Verify(result!.ToVerifyTarget());
    }

    [Fact]
    public async Task Batch()
    {
        await TestDataBuilder.SendMultipleDataAsync(table);

        var consumer = new QueueManager(table, SqlConnection);
        var messages = new ConcurrentBag<IncomingVerifyTarget>();
        var result = await consumer.Consume(
            size: 3,
            func: (message, _) =>
            {
                messages.Add(message.ToVerifyTarget());
                return Task.CompletedTask;
            });
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
            func: (message, _) =>
            {
                messages.Add(message.ToVerifyTarget());
                return Task.CompletedTask;
            });
        Assert.Equal(5, result.Count);
        await Verify(messages.OrderBy(_ => _.Id));
    }

    public ConsumerTests()
    {
        var manager = new QueueManager(table, SqlConnection);
        manager.Drop().Await();
        manager.Create().Await();
    }
}