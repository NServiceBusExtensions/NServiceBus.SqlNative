using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class DelayedReaderTests : TestBase
{
    string table = "DelayedReaderTests";

    [Fact]
    public async Task Single()
    {
        await DelayedTestDataBuilder.SendData(table);
        var reader = new DelayedQueueManager(table, SqlConnection);
        using (var result = reader.Read(1).Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public async Task Single_nulls()
    {
        await DelayedTestDataBuilder.SendNullData(table);
        var reader = new DelayedQueueManager(table, SqlConnection);
        using (var result = reader.Read(1).Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public async Task Batch()
    {
        await DelayedTestDataBuilder.SendMultipleData(table);

        var reader = new DelayedQueueManager(table, SqlConnection);
        var messages = new ConcurrentBag<IncomingDelayedVerifyTarget>();
        var result = reader.Read(
                size: 3,
                startRowVersion: 2,
                action: message => { messages.Add(message.ToVerifyTarget()); })
            .Result;
        Assert.Equal(4, result.LastRowVersion);
        Assert.Equal(3, result.Count);
        ObjectApprover.VerifyWithJson(messages.OrderBy(x => x.Due));
    }

    public DelayedReaderTests(ITestOutputHelper output) : base(output)
    {
        var manager = new DelayedQueueManager(table, SqlConnection);
        manager.Drop().Await();
        manager.Create().Await();
    }
}