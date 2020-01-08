using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using Verify;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

public class DelayedQueueCreationTests :
    VerifyBase
{
    [Fact]
    public async Task Run()
    {
        await using var connection = Connection.OpenConnectionFromNewClient();
        var manager = new DelayedQueueManager("DelayedQueueCreationTests", connection);
        await manager.Drop();
        await manager.Create();
        var settings = new VerifySettings();
        settings.SchemaSettings(includeItem: s => s == "DelayedQueueCreationTests");
        await Verify(connection, settings);
    }

    public DelayedQueueCreationTests(ITestOutputHelper output) :
        base(output)
    {
    }
}