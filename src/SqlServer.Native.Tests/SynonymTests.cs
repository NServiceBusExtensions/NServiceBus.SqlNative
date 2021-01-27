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
    public async Task Create()
    {
        var synonym = new Synonym(SqlConnection, SqlConnection.Database);
        await CreateTable();
        await synonym.DropAll();
        await synonym.Create("mySynonym1", "target");
        await Verifier.Verify(SqlConnection)
            .SchemaSettings(tables: false);
    }

    [Fact]
    public async Task Drop()
    {
        var synonym = new Synonym(SqlConnection, SqlConnection.Database);
        await CreateTable();
        await synonym.DropAll();
        await synonym.Create("mySynonym1", "target");
        await synonym.Drop("mySynonym1");
        await Verifier.Verify(SqlConnection)
            .SchemaSettings(tables: false);
    }
}