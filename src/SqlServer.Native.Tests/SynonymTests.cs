using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using NServiceBus.Transport.SqlServerNative;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class SynonymTests :
    TestBase
{
    [Fact]
    public async Task DropAll()
    {
        var synonym = new Synonym(SqlConnection, SqlConnection.Database);
        await CreateTable();

        await synonym.Create("mySynonym3", "target");
        await synonym.DropAll();
    }

    async Task CreateTable()
    {
        await using var command = SqlConnection.CreateCommand();
        command.CommandText = "create table target(id int);";
        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (SqlException)
        {
        }
    }

    [Fact]
    public async Task CreateDrop()
    {
        var synonym = new Synonym(SqlConnection, SqlConnection.Database);
        await CreateTable();
        await synonym.Drop("mySynonym1", "target");
        await synonym.Create("mySynonym1", "target");
    }

    [Fact]
    public async Task CreateDropNoTarget()
    {
        var synonym = new Synonym(SqlConnection, SqlConnection.Database);
        await CreateTable();

        await synonym.Drop("mySynonym2");
        await synonym.Create("mySynonym2");
    }
}