using ApprovalTests;
using NServiceBus.Transport.SqlServerNative;
using Xunit;

public class DelayedQueueCreationTests
{
    [Fact]
    public void Run()
    {
        DbSetup.Setup();
        using (var connection = Connection.OpenConnection())
        {
            var manager = new DelayedQueueManager("DelayedQueueCreationTests", connection);
            manager.Create().Await();
            var sqlScriptBuilder = new SqlScriptBuilder(tables:true, namesToInclude: "DelayedQueueCreationTests");
            Approvals.Verify(sqlScriptBuilder.BuildScript(connection));
        }
    }
}