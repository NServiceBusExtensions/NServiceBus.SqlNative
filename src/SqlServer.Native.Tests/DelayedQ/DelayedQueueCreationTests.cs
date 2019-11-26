using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

public class DelayedQueueCreationTests :
    VerifyBase
{
    [Fact]
    public async Task Run()
    {
        await using var connection = Connection.OpenConnection();
        var manager = new DelayedQueueManager("DelayedQueueCreationTests", connection);
        await manager.Drop();
        await manager.Create();
        var sqlScriptBuilder = new SqlScriptBuilder(tables:true, namesToInclude: "DelayedQueueCreationTests");
        await Verify(sqlScriptBuilder.BuildScript(connection));
    }

    public DelayedQueueCreationTests(ITestOutputHelper output) :
        base(output)
    {
    }
}