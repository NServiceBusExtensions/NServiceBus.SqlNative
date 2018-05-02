using System.Collections.Generic;
using NServiceBus.Transport.SqlServerNative;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class ReaderTests : TestBase
{
    string table = "ReaderTests";

    [Fact]
    public void Single_bytes()
    {
        TestDataBuilder.SendData(table);
        var reader = new Reader(table);
        var result = reader.ReadBytes(SqlConnection, 1).Result;
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public void Single_bytes_nulls()
    {
        TestDataBuilder.SendNullData(table);
        var reader = new Reader(table);
        var result = reader.ReadBytes(SqlConnection, 1).Result;
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public void Single_stream()
    {
        TestDataBuilder.SendData(table);
        var reader = new Reader(table);
        using (var result = reader.ReadStream(SqlConnection, 1).Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public void Single_stream_nulls()
    {
        TestDataBuilder.SendNullData(table);
        var reader = new Reader(table);
        using (var result = reader.ReadStream(SqlConnection, 1).Result)
        {
            ObjectApprover.VerifyWithJson(result.ToVerifyTarget());
        }
    }

    [Fact]
    public void Batch_bytes()
    {
        TestDataBuilder.SendMultipleData(table);

        var reader = new Reader(table);
        var messages = new List<IncomingBytesMessage>();
        var result = reader.ReadBytes(
                connection: SqlConnection,
                size: 3,
                startRowVersion: 2,
                action: message => { messages.Add(message); })
            .Result;
        Assert.Equal(4, result.LastRowVersion);
        Assert.Equal(3, result.Count);
        ObjectApprover.VerifyWithJson(messages);
    }

    [Fact]
    public void Batch_stream()
    {
        TestDataBuilder.SendMultipleData(table);

        var reader = new Reader(table);
        var messages = new List<object>();
        var result = reader.ReadStream(
                connection: SqlConnection,
                size: 3,
                startRowVersion: 2,
                action: message => { messages.Add(message.ToVerifyTarget()); })
            .Result;
        Assert.Equal(4, result.LastRowVersion);
        Assert.Equal(3, result.Count);
        ObjectApprover.VerifyWithJson(messages);
    }

    public ReaderTests(ITestOutputHelper output) : base(output)
    {
        SqlHelpers.Drop(SqlConnection, table).Await();
        QueueCreator.Create(SqlConnection, table).Await();
    }
}