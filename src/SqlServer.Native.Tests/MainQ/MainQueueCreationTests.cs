using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

public class MainQueueCreationTests :
    VerifyBase
{
    [Fact]
    public async Task Run()
    {
        await using var connection = Connection.OpenConnection();
        var manager = new QueueManager("MainQueueCreationTests", connection);
        await manager.Drop();
        await manager.Create();
        var sqlScriptBuilder = new SqlScriptBuilder(tables: true, namesToInclude: "MainQueueCreationTests");
        await Verify(sqlScriptBuilder.BuildScript(connection));
    }

    public MainQueueCreationTests(ITestOutputHelper output) :
        base(output)
    {
    }
}