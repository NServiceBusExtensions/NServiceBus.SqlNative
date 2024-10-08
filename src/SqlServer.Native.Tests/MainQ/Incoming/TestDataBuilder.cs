﻿static class TestDataBuilder
{
    static DateTime dateTime = new(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    public static async Task SendData(string table)
    {
        await using var connection = Connection.OpenConnection();
        var sender = new QueueManager(table, connection);

        var message = BuildMessage("00000000-0000-0000-0000-000000000001");
        await sender.Send(message);
    }

    public static async Task SendNullData(string table)
    {
        await using var connection = Connection.OpenConnection();
        var sender = new QueueManager(table, connection);

        var message = BuildNullMessage("00000000-0000-0000-0000-000000000001");
        await sender.Send(message);
    }

    public static async Task SendMultipleDataAsync(string table)
    {
        await using var connection = Connection.OpenConnection();
        var sender = new QueueManager(table, connection);
        await sender.Send(
        [
            BuildMessage("00000000-0000-0000-0000-000000000001"),
            BuildNullMessage("00000000-0000-0000-0000-000000000002"),
            BuildMessage("00000000-0000-0000-0000-000000000003"),
            BuildNullMessage("00000000-0000-0000-0000-000000000004"),
            BuildMessage("00000000-0000-0000-0000-000000000005")
        ]);
    }

    public static OutgoingMessage BuildMessage(string guid) =>
        new(new(guid), dateTime, "headers", "{}"u8.ToArray());

    public static OutgoingMessage BuildNullMessage(string guid) =>
        new(new(guid), bodyBytes: null);
}