using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using NServiceBus.Transport.SqlServerNative;
using VerifyXunit;
using VerifyTests;
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
        await Verifier.Verify(SqlConnection)
            .SchemaSettings(tables: false);
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
        await synonym.Drop("mySynonym1");
        await synonym.Create("mySynonym1", "target");
        await Verifier.Verify(SqlConnection)
            .SchemaSettings(tables: false);
    }

    [Fact]
    public async Task CreateDropNoTarget()
    {
        var synonym = new Synonym(SqlConnection, "master", "dbo", "sys");
        await synonym.Drop("mySynonym2");
        await synonym.Create("mySynonym2", "all_columns");
        await Verifier.Verify(SqlConnection)
            .SchemaSettings(tables: false);
    }
}