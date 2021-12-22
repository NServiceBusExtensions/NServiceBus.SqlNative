﻿using NServiceBus.Transport.SqlServerNative;

[UsesVerify]
public class DedupeManagerTests :
    TestBase
{
    static DateTime dateTime = new(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    string table = "DedupeManagerTests";

    [Fact]
    public async Task Should_only_clean_up_old_item()
    {
        var message1 = BuildBytesMessage("00000000-0000-0000-0000-000000000001");
        await Send(message1);
        Thread.Sleep(1000);
        var now = DateTime.UtcNow;
        Thread.Sleep(1000);
        var message2 = BuildBytesMessage("00000000-0000-0000-0000-000000000002");
        await Send(message2);
        var cleaner = new DedupeManager(SqlConnection, "Deduplication");
        SqlRecording.StartRecording();
        await cleaner.CleanupItemsOlderThan(now);
        var sqlEntries = SqlRecording.FinishRecording();
        await Verify(
            new
            {
                ReplicationData = SqlHelper.ReadDuplicateData("Deduplication", SqlConnection),
                sqlEntries
            });
    }

    Task Send(OutgoingMessage message)
    {
        var sender = new QueueManager(table, SqlConnection, "Deduplication");
        return sender.Send(message);
    }

    static OutgoingMessage BuildBytesMessage(string guid)
    {
        return new(new(guid), dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }

    public DedupeManagerTests()
    {
        var manager = new QueueManager(table, SqlConnection, "Deduplication");
        manager.Drop().Await();
        manager.Create().Await();
        var dedupeManager = new DedupeManager(SqlConnection, "Deduplication");
        dedupeManager.Drop().Await();
        dedupeManager.Create().Await();
    }
}