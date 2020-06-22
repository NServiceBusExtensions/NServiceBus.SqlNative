using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class DelayedQueueCreationTests
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
        await Verifier.Verify(connection, settings);
    }
}