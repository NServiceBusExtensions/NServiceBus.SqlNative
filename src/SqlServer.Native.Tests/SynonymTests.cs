using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using NServiceBus.Transport.SqlServerNative;
using Xunit;

public class SynonymTests :
    TestBase
{
    [Fact]
    public async Task CreateDrop()
    {
        var subscriptionManager = new Synonym(SqlConnection, SqlConnection.Database);
        await using (var command = SqlConnection.CreateCommand())
        {
            command.CommandText = "create table target(id int);";
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException)
            {
            }
        }

        await subscriptionManager.Drop("mySynonym");
        await subscriptionManager.Create("mySynonym", "target");
    }
}