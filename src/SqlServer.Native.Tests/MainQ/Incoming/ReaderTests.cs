using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using VerifyXunit;
using Xunit;

[UsesVerify]
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
        await Verifier.Verify(result!.ToVerifyTarget());
    }

    [Fact]
    public async Task Single_nulls()
    {
        await TestDataBuilder.SendNullData(table);
        var reader = new QueueManager(table, SqlConnection);
        await using var result = await reader.Read(1);
        await Verifier.Verify(result!.ToVerifyTarget());
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
            action: message => { messages.Add(message.ToVerifyTarget()); });
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
        await Verifier.Verify(messages.OrderBy(x => x.Id));
    }

    public ReaderTests()
    {
        var manager = new QueueManager(table, SqlConnection);
        manager.Drop().Await();
        manager.Create().Await();
    }
}