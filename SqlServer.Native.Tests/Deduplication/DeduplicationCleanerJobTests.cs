﻿using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class DeduplicationCleanerJobTests : TestBase
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    string table = "DeduplicationCleanerJobTests";

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
        var expireWindow = DateTime.UtcNow - now;
        var cleaner = new DeduplicationCleanerJob(
            "Deduplication",
            Connection.OpenAsyncConnection,
            exception => { },
            expireWindow,
            frequencyToRunCleanup: TimeSpan.FromMilliseconds(10));
        cleaner.Start();
        Thread.Sleep(100);
        cleaner.Stop().Await();
        ObjectApprover.VerifyWithJson(SqlHelper.ReadDuplicateData("Deduplication", SqlConnection));
    }

    Task<long> Send(OutgoingMessage message)
    {
        var sender = new QueueManager(table, SqlConnection, "Deduplication");
       return sender.Send(message);
    }

    static OutgoingMessage BuildBytesMessage(string guid)
    {
        return new OutgoingMessage(new Guid(guid), dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }

    public DeduplicationCleanerJobTests(ITestOutputHelper output) : base(output)
    {
        var manager = new QueueManager(table, SqlConnection, "Deduplication");
        manager.Drop().Await();
        manager.Create().Await();
        var deduplication = new DeduplicationManager(SqlConnection, "Deduplication");
        deduplication.Drop().Await();
        deduplication.Create().Await();
    }
}