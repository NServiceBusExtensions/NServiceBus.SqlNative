using System;
using System.Linq;
using System.Text;
using System.Threading;
using NServiceBus.Transport.SqlServerNative;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class DeduplicationManagerTests : TestBase
{
    static DateTime dateTime = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);

    string table = "DeduplicationManagerTests";

    [Fact]
    public void Should_only_clean_up_old_item()
    {
        var message1 = BuildBytesMessage("00000000-0000-0000-0000-000000000001");
        Send(message1);
        Thread.Sleep(1000);
        var now = DateTime.UtcNow;
        var message2 = BuildBytesMessage("00000000-0000-0000-0000-000000000002");
        Send(message2);
        var cleaner = new DeduplicationManager(SqlConnection);
        cleaner.CleanupItemsOlderThan(now).Await();
        ObjectApprover.VerifyWithJson(SqlHelper.ReadData("Deduplication").Select(x => x.Values.First()));
    }

    void Send(OutgoingMessage message)
    {
        var sender = new QueueManager(table, SqlConnection, true);
        sender.Send(message).Await();
    }

    static OutgoingMessage BuildBytesMessage(string guid)
    {
        return new OutgoingMessage(new Guid(guid), dateTime, "headers", Encoding.UTF8.GetBytes("{}"));
    }

    public DeduplicationManagerTests(ITestOutputHelper output) : base(output)
    {
        var manager = new QueueManager(table, SqlConnection, true);
        manager.Drop().Await();
        manager.Create().Await();
        var deduplication = new DeduplicationManager(SqlConnection);
        deduplication.Drop().Await();
        deduplication.Create().Await();
    }
}