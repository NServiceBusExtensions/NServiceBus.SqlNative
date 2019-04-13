using ApprovalTests;
using NServiceBus.Transport.SqlServerNative;
using Xunit;
using Xunit.Abstractions;

public class DelayedQueueCreationTests :
    XunitLoggingBase
{
    [Fact]
    public void Run()
    {
        DbSetup.Setup();
        using (var connection = Connection.OpenConnection())
        {
            var manager = new DelayedQueueManager("DelayedQueueCreationTests", connection);
            manager.Drop().Await();
            manager.Create().Await();
            var sqlScriptBuilder = new SqlScriptBuilder(tables:true, namesToInclude: "DelayedQueueCreationTests");
            Approvals.Verify(sqlScriptBuilder.BuildScript(connection));
        }
    }

    public DelayedQueueCreationTests(ITestOutputHelper output) :
        base(output)
    {
    }
}