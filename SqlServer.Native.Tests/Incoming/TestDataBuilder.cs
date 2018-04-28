using System;
using System.Collections.Generic;
using System.Text;
using NServiceBus.Transport.SqlServerNative;

static class TestDataBuilder
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);


    public static void SendData(string table)
    {
        var sender = new Sender(table);

        var message = TestDataBuilder.BuildMessage("00000000-0000-0000-0000-000000000001");
        sender.Send(Connection.ConnectionString, message).Await();
    }
    public static void SendNullData(string table)
    {
        var sender = new Sender(table);

        var message = TestDataBuilder.BuildNullMessage("00000000-0000-0000-0000-000000000001");
        sender.Send(Connection.ConnectionString, message).Await();
    }
    public static void SendMultipleData(string table)
    {
        var sender = new Sender(table);

        sender.Send(
            Connection.ConnectionString,
            new List<OutgoingMessage>
            {
                BuildMessage("00000000-0000-0000-0000-000000000001"),
                BuildNullMessage("00000000-0000-0000-0000-000000000002"),
                BuildMessage("00000000-0000-0000-0000-000000000003"),
                BuildNullMessage("00000000-0000-0000-0000-000000000004"),
                BuildMessage("00000000-0000-0000-0000-000000000005")
            }).Await();
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