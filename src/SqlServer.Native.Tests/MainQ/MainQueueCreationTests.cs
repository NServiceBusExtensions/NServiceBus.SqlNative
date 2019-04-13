using ApprovalTests;
using NServiceBus.Transport.SqlServerNative;
using Xunit;
using Xunit.Abstractions;

public class MainQueueCreationTests :
    XunitLoggingBase
{
    [Fact]
    public void Run()
    {
        DbSetup.Setup();
        using (var connection = Connection.OpenConnection())
        {
            var manager = new QueueManager("MainQueueCreationTests", connection);
            manager.Drop().Await();
            manager.Create().Await();
            var sqlScriptBuilder = new SqlScriptBuilder(tables: true, namesToInclude: "MainQueueCreationTests");
            Approvals.Verify(sqlScriptBuilder.BuildScript(connection));
        }
    }

    public MainQueueCreationTests(ITestOutputHelper output) :
        base(output)
    {
    }
}