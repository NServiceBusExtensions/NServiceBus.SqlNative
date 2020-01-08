using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using Verify;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

public class MainQueueCreationTests :
    VerifyBase
{
    [Fact]
    public async Task Run()
    {
        await using var connection = Connection.OpenConnectionFromNewClient();
        var manager = new QueueManager("MainQueueCreationTests", connection);
        await manager.Drop();
        await manager.Create();
        var settings = new VerifySettings();
        settings.SchemaSettings(includeItem: s => s == "MainQueueCreationTests");
        await Verify(connection, settings);
    }

    public MainQueueCreationTests(ITestOutputHelper output) :
        base(output)
    {
    }
}