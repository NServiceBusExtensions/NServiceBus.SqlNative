using System.Collections.Generic;
using NServiceBus.Transport.SqlServerNative;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class ReaderTests : TestBase
{
    string table = "ReaderTests";


    [Fact]
    public void Single()
    {
        TestDataBuilder.SendData(table);
        var reader = new QueueManager(table, SqlConnection);
        using (var result = reader.ReadStream(1).Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public void Single_nulls()
    {
        TestDataBuilder.SendNullData(table);
        var reader = new QueueManager(table, SqlConnection);
        using (var result = reader.ReadStream(1).Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public void Batch()
    {
        TestDataBuilder.SendMultipleData(table);

        var reader = new QueueManager(table, SqlConnection);
        var messages = new List<object>();
        var result = reader.ReadStream(size: 3,
                startRowVersion: 2,
                action: message => { messages.Add(message.ToVerifyTarget()); })
            .Result;
        Assert.Equal(4, result.LastRowVersion);
        Assert.Equal(3, result.Count);
        ObjectApprover.VerifyWithJson(messages);
    }

    public ReaderTests(ITestOutputHelper output) : base(output)
    {
        var manager = new QueueManager(table, SqlConnection);
        manager.Drop().Await();
        manager.Create().Await();
    }
}