using System;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;

static class DelayedTestDataBuilder
{
    static DateTime dateTime = new(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    public static async Task SendData(string table)
    {
        await using var connection = Connection.OpenConnection();
        var sender = new DelayedQueueManager(table, connection);

        var message = BuildMessage();
        await sender.Send(message);
    }

    public static async Task SendNullData(string table)
    {
        await using var connection = Connection.OpenConnection();
        var sender = new DelayedQueueManager(table, connection);

        var message = BuildNullMessage();
        await sender.Send(message);
    }

    public static async Task SendMultipleData(string table)
    {
        await using var connection = Connection.OpenConnection();
        var sender = new DelayedQueueManager(table, connection);
        var time = dateTime;
        await sender.Send(new OutgoingDelayedMessage(time, "headers", Encoding.UTF8.GetBytes("{}")));
        time = time.AddSeconds(1);
        await sender.Send(new OutgoingDelayedMessage(time, "{}", bodyBytes: null));
        time = time.AddSeconds(1);
        await sender.Send(new OutgoingDelayedMessage(time, "headers", Encoding.UTF8.GetBytes("{}")));
        time = time.AddSeconds(1);
        await sender.Send(new OutgoingDelayedMessage(time, "{}", bodyBytes: null));
        time = time.AddSeconds(1);
        await sender.Send(new OutgoingDelayedMessage(time, "headers", Encoding.UTF8.GetBytes("{}")));
    }

    public static OutgoingDelayedMessage BuildMessage()
    {
        return new(dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }

    public static OutgoingDelayedMessage BuildNullMessage()
    {
        return new(dateTime, "{}", bodyBytes: null);
    }
}