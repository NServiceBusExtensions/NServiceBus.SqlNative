using System;
using System.Collections.Generic;
using System.Text;
using NServiceBus.Transport.SqlServerNative;

static class DelayedTestDataBuilder
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    public static void SendData(string table)
    {
        using (var connection = Connection.OpenConnection())
        {
            var sender = new DelayedQueueManager(table, connection);

            var message = BuildMessage();
            sender.Send(message).Await();
        }
    }
    public static void SendNullData(string table)
    {
        using (var connection = Connection.OpenConnection())
        {
            var sender = new DelayedQueueManager(table, connection);

            var message = BuildNullMessage();
            sender.Send(message).Await();
        }
    }
    public static void SendMultipleData(string table)
    {
        using (var connection = Connection.OpenConnection())
        {
            var sender = new DelayedQueueManager(table, connection);
            sender.Send(new List<OutgoingDelayedMessage>
                {
                    BuildMessage(),
                    BuildNullMessage(),
                    BuildMessage(),
                    BuildNullMessage(),
                    BuildMessage()
                }).Await();
        }
    }

    public static OutgoingDelayedMessage BuildMessage()
    {
        return new OutgoingDelayedMessage(dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }

    public static OutgoingDelayedMessage BuildNullMessage()
    {
        return new OutgoingDelayedMessage(dateTime, "{}",bodyBytes: null);
    }
}