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
        await Verify(connection, settings);
    }
}