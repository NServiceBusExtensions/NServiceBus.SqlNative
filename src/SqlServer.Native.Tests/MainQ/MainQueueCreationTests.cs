using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class MainQueueCreationTests
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
        await Verifier.Verify(connection, settings);
    }
}