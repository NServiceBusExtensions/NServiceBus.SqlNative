using Microsoft.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative;

public class Synonym
{
    SqlConnection sourceDatabase;
    string targetDatabase;
    string sourceSchema;
    string targetSchema;
    SqlTransaction? sourceTransaction;

    public Synonym(SqlConnection sourceDatabase, string targetDatabase, string sourceSchema = "dbo", string targetSchema = "dbo")
    {
        Guard.AgainstNullOrEmpty(targetDatabase);
        Guard.AgainstNullOrEmpty(targetSchema);
        this.sourceDatabase = sourceDatabase;
        this.targetDatabase = targetDatabase;
        this.sourceSchema = sourceSchema;
        this.targetSchema = targetSchema;
    }

    public Synonym(SqlTransaction sourceTransaction, string targetDatabase, string sourceSchema = "dbo", string targetSchema = "dbo")
    {
        Guard.AgainstNullOrEmpty(targetDatabase);
        Guard.AgainstNullOrEmpty(targetSchema);
        this.sourceTransaction = sourceTransaction;
        this.targetDatabase = targetDatabase;
        this.sourceSchema = sourceSchema;
        this.targetSchema = targetSchema;
        sourceDatabase = sourceTransaction.Connection!;
    }

    public async Task Create(string synonym, string? target = null)
    {
        target ??= synonym;
        GuardAgainstCircularAlias(synonym, target);
        using var command = sourceDatabase.CreateCommand();
        command.Transaction = sourceTransaction;
        command.CommandText = $@"
    drop synonym if exists [{sourceSchema}].[{synonym}];
    create synonym [{sourceSchema}].[{synonym}] for [{targetDatabase}].[{targetSchema}].[{target}];
";
        await command.ExecuteNonQueryAsync();
    }

    public async Task DropAll()
    {
        using var command = sourceDatabase.CreateCommand();
        command.Transaction = sourceTransaction;
        command.CommandText = @"
declare @n char(1)
set @n = char(10)

declare @stmt nvarchar(max)

select @stmt = isnull( @stmt + @n, '' ) +
'drop synonym [' + SCHEMA_NAME(schema_id) + '].[' + name + ']'
from sys.synonyms

exec sp_executesql @stmt
";
        await command.ExecuteNonQueryAsync();
    }

    public async Task Drop(string synonym)
    {
        using var command = sourceDatabase.CreateCommand();
        command.Transaction = sourceTransaction;
        command.CommandText = $"drop synonym if exists [{sourceSchema}].[{synonym}];";
        await command.ExecuteNonQueryAsync();
    }

    void GuardAgainstCircularAlias(string synonym, string target)
    {
        if (targetDatabase == sourceDatabase.Database &&
            synonym == target &&
            sourceSchema == targetSchema)
        {
            throw new("Invalid circular alias.");
        }
    }
}