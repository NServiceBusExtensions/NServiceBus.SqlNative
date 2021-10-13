using NServiceBus.Transport.SqlServerNative;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class DelayedReaderTests :
    TestBase
{
    string table = "DelayedReaderTests";

    [Fact]
    public async Task Single()
    {
        await DelayedTestDataBuilder.SendData(table);
        var reader = new DelayedQueueManager(table, SqlConnection);
        await using var result = await reader.Read(1);
        await Verifier.Verify(result!.ToVerifyTarget());
    }

    [Fact]
    public async Task Single_nulls()
    {
        await DelayedTestDataBuilder.SendNullData(table);
        var reader = new DelayedQueueManager(table, SqlConnection);
        await using var result = await reader.Read(1);
        await Verifier.Verify(result!.ToVerifyTarget());
    }

    [Fact]
    public async Task Batch()
    {
        await DelayedTestDataBuilder.SendMultipleData(table);

        var reader = new DelayedQueueManager(table, SqlConnection);
        var messages = new ConcurrentBag<IncomingDelayedVerifyTarget>();
        var result = await reader.Read(
            size: 3,
            startRowVersion: 2,
            action: message => { messages.Add(message.ToVerifyTarget()); });
        Assert.Equal(4, result.LastRowVersion);
        Assert.Equal(3, result.Count);
        await Verifier.Verify(messages.OrderBy(x => x.Due));
    }

    public DelayedReaderTests()
    {
        var manager = new DelayedQueueManager(table, SqlConnection);
        manager.Drop().Await();
        manager.Create().Await();
    }
}