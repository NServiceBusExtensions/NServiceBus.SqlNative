using System.Collections.Generic;
using NServiceBus.Transport.SqlServerNative;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class ConsumerTests : TestBase
{
    string table = "ConsumerTests";

    [Fact]
    public void Single_bytes()
    {
        TestDataBuilder.SendData(table);
        var consumer = new Consumer(table);
        var result = consumer.ConsumeBytes(Connection.ConnectionString).Result;
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public void Single_bytes_nulls()
    {
        TestDataBuilder.SendNullData(table);
        var consumer = new Consumer(table);
        var result = consumer.ConsumeBytes(Connection.ConnectionString).Result;
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public void Single_stream()
    {
        TestDataBuilder.SendData(table);
        var consumer = new Consumer(table);
        using (var result = consumer.ConsumeStream(Connection.ConnectionString).Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public void Single_stream_nulls()
    {
        TestDataBuilder.SendNullData(table);
        var consumer = new Consumer(table);
        using (var result = consumer.ConsumeStream(Connection.ConnectionString).Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public void Batch_bytes()
    {
        TestDataBuilder.SendMultipleData(table);

        var consumer = new Consumer(table);
        var messages = new List<IncomingBytesMessage>();
        var result = consumer.ConsumeBytes(
                connection: Connection.ConnectionString,
                size: 3,
                action: message => { messages.Add(message); })
            .Result;
        Assert.Equal(3, result.LastRowVersion);
        Assert.Equal(3, result.Count);
        ObjectApprover.VerifyWithJson(messages);
    }

    [Fact]
    public void Batch_stream()
    {
        TestDataBuilder.SendMultipleData(table);

        var consumer = new Consumer(table);
        var messages = new List<object>();
        var result = consumer.ConsumeStream(
                connection: Connection.ConnectionString,
                size: 3,
                action: message => { messages.Add(message.ToVerifyTarget()); })
            .Result;
        Assert.Equal(3, result.Count);
        Assert.Equal(3, result.LastRowVersion);
        ObjectApprover.VerifyWithJson(messages);
    }

    public ConsumerTests(ITestOutputHelper output) : base(output)
    {
        SqlHelpers.Drop(Connection.ConnectionString, table).Await();
        QueueCreator.Create(Connection.ConnectionString, table).Await();
    }
}