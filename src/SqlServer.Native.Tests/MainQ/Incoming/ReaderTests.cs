using NServiceBus.Transport.SqlServerNative;

public class ReaderTests :
    TestBase
{
    string table = "ReaderTests";

    [Fact]
    public async Task Single()
    {
        await TestDataBuilder.SendData(table);
        var reader = new QueueManager(table, SqlConnection);
        await using var result = await reader.Read(1);
        await Verify(result!.ToVerifyTarget());
    }

    [Fact]
    public async Task Single_nulls()
    {
        await TestDataBuilder.SendNullData(table);
        var reader = new QueueManager(table, SqlConnection);
        await using var result = await reader.Read(1);
        await Verify(result!.ToVerifyTarget());
    }

    [Fact]
    public async Task Batch()
    {
        await TestDataBuilder.SendMultipleDataAsync(table);

        var reader = new QueueManager(table, SqlConnection);
        var messages = new ConcurrentBag<IncomingVerifyTarget>();
        var result = await reader.Read(
            size: 3,
            startRowVersion: 2,
            func: (message, _) =>
            {
                messages.Add(message.ToVerifyTarget());
                return Task.CompletedTask;
            });
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
            func: (message, _) =>
            {
                messages.Add(message.ToVerifyTarget());
                return Task.CompletedTask;
            });
        await Verify(messages.OrderBy(_ => _.Id));
    }

    public ReaderTests()
    {
        var manager = new QueueManager(table, SqlConnection);
        manager.Drop().Await();
        manager.Create().Await();
    }
}