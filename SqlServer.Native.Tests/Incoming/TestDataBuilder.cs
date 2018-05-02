using System;
using System.Collections.Generic;
using System.Text;
using NServiceBus.Transport.SqlServerNative;

static class TestDataBuilder
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    public static void SendData(string table)
    {
        using (var connection = Connection.OpenConnection())
        {
            var sender = new Sender(table);

            var message = BuildMessage("00000000-0000-0000-0000-000000000001");
            sender.Send(connection, message).Await();
        }
    }
    public static void SendNullData(string table)
    {
        using (var connection = Connection.OpenConnection())
        {
            var sender = new Sender(table);

            var message = BuildNullMessage("00000000-0000-0000-0000-000000000001");
            sender.Send(connection, message).Await();
        }
    }
    public static void SendMultipleData(string table)
    {
        var sender = new Sender(table);

        using (var connection = Connection.OpenConnection())
        {
            sender.Send(
                connection,
                new List<OutgoingMessage>
                {
                    BuildMessage("00000000-0000-0000-0000-000000000001"),
                    BuildNullMessage("00000000-0000-0000-0000-000000000002"),
                    BuildMessage("00000000-0000-0000-0000-000000000003"),
                    BuildNullMessage("00000000-0000-0000-0000-000000000004"),
                    BuildMessage("00000000-0000-0000-0000-000000000005")
                }).Await();
        }
    }

    public static OutgoingMessage BuildMessage(string guid)
    {
        return new OutgoingMessage(new Guid(guid), "theCorrelationId", "theReplyToAddress", dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }

    public static OutgoingMessage BuildNullMessage(string guid)
    {
        return new OutgoingMessage(new Guid(guid), bodyBytes: null);
    }
}