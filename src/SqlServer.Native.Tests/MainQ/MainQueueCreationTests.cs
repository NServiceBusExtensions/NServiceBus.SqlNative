using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
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
        await Verifier.Verify(connection)
            .SchemaSettings(includeItem: s => s == "MainQueueCreationTests");
    }

    [Fact]
    public async Task MigrateToNonClusteredIndex()
    {
        await using var connection = Connection.OpenConnectionFromNewClient();
        var tableName = "MigrateToNonClusteredIndexTest";

        var manager = new QueueManager(tableName, connection);
        await manager.Drop();
        await manager.Create();

        // Simulate old version by dropping the Index_RowVersion and replace it with clustered index
        await using var makeOldIndexCommand = new SqlCommand($@"
drop index Index_RowVersion on {tableName};
create clustered index Index_RowVersion on {tableName}
(
	RowVersion
)", connection);
        await makeOldIndexCommand.ExecuteNonQueryAsync();

        // Rerun Create should drop the clustered index and create a non-clustered index
        await manager.Create();

        await using var getIndexTypeDesCommand = new SqlCommand($@"
select type_desc from sys.indexes
where object_id = object_id('{tableName}')
and name = 'Index_RowVersion'", connection);
        await Verifier.Verify(getIndexTypeDesCommand.ExecuteScalarAsync());
    }
}