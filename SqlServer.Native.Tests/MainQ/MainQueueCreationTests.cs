using ApprovalTests;
using NServiceBus.Transport.SqlServerNative;
using Xunit;

public class MainQueueCreationTests
{
    [Fact]
    public void Run()
    {
        DbSetup.Setup();
        using (var connection = Connection.OpenConnection())
        {
            var manager = new QueueManager("MainQueueCreationTests", connection);
            manager.Create().Await();
            var sqlScriptBuilder = new SqlScriptBuilder(tables: true, namesToInclude: "MainQueueCreationTests");
            Approvals.Verify(sqlScriptBuilder.BuildScript(connection));
        }
    }
}