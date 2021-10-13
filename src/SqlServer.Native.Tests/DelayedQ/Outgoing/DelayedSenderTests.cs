using NServiceBus.Transport.SqlServerNative;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class DelayedSenderTests :
    TestBase
{
    string table = "DelayedSenderTests";
    static DateTime dateTime = new(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    [Fact]
    public async Task Single_bytes()
    {
        var message = BuildBytesMessage();
        await Send(message);
        await Verifier.Verify(SqlHelper.ReadDelayedData(table, SqlConnection));
    }

    [Fact]
    public async Task Single_bytes_nulls()
    {
        var message = BuildBytesNullMessage();
        await Send(message);
        await Verifier.Verify(SqlHelper.ReadDelayedData(table, SqlConnection));
    }

    [Fact]
    public async Task Single_stream()
    {
        var message = BuildStreamMessage();
        await  Send(message);
        await Verifier.Verify(SqlHelper.ReadDelayedData(table, SqlConnection));
    }

    [Fact]
    public async Task Single_stream_nulls()
    {
        var sender = new DelayedQueueManager(table, SqlConnection);

        var message = BuildBytesNullMessage();
        await sender.Send(message);
        await Verifier.Verify(SqlHelper.ReadDelayedData(table, SqlConnection));
    }

    [Fact]
    public async Task Batch()
    {
        var messages = new List<OutgoingDelayedMessage>
        {
            BuildBytesMessage(),
            BuildStreamMessage()
        };
        await Send(messages);
        await Verifier.Verify(SqlHelper.ReadDelayedData(table, SqlConnection));
    }

    [Fact]
    public async Task Batch_nulls()
    {
        var messages = new List<OutgoingDelayedMessage>
        {
            BuildBytesNullMessage(),
            BuildStreamNullMessage()
        };
        await Send(messages);
        await Verifier.Verify(SqlHelper.ReadDelayedData(table, SqlConnection));
    }

    Task<long> Send(OutgoingDelayedMessage message)
    {
        var sender = new DelayedQueueManager(table, SqlConnection);

        return sender.Send(message);
    }

    Task Send(List<OutgoingDelayedMessage> messages)
    {
        var sender = new DelayedQueueManager(table, SqlConnection);

        return sender.Send(messages);
    }

    static OutgoingDelayedMessage BuildBytesMessage()
    {
        return new(dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }

    static OutgoingDelayedMessage BuildStreamMessage()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        return new(dateTime, "headers", stream);
    }

    static OutgoingDelayedMessage BuildBytesNullMessage()
    {
        return new(dateTime, null, bodyBytes: null);
    }

    static OutgoingDelayedMessage BuildStreamNullMessage()
    {
        return new(dateTime, null, bodyStream: null);
    }

    public DelayedSenderTests()
    {
        var manager = new DelayedQueueManager(table, SqlConnection);
        manager.Drop().Await();
        manager.Create().Await();
    }
}