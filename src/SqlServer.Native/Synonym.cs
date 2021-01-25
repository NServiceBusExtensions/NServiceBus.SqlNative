using System.Data.Common;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public class Synonym
    {
        DbConnection sourceDatabase;
        string targetDatabase;
        string sourceSchema;
        string targetSchema;
        DbTransaction? sourceTransaction;

        public Synonym(DbConnection sourceDatabase, string targetDatabase, string sourceSchema = "dbo", string targetSchema = "dbo")
        {
            Guard.AgainstNull(sourceDatabase, nameof(sourceDatabase));
            Guard.AgainstNullOrEmpty(targetDatabase, nameof(targetDatabase));
            Guard.AgainstNullOrEmpty(targetSchema, nameof(targetSchema));
            this.sourceDatabase = sourceDatabase;
            this.targetDatabase = targetDatabase;
            this.sourceSchema = sourceSchema;
            this.targetSchema = targetSchema;
        }

        public Synonym(DbTransaction sourceTransaction, string targetDatabase, string sourceSchema = "dbo", string targetSchema = "dbo")
        {
            Guard.AgainstNull(sourceTransaction, nameof(sourceTransaction));
            Guard.AgainstNullOrEmpty(targetDatabase, nameof(targetDatabase));
            Guard.AgainstNullOrEmpty(targetSchema, nameof(targetSchema));
            this.sourceTransaction = sourceTransaction;
            this.targetDatabase = targetDatabase;
            this.sourceSchema = sourceSchema;
            this.targetSchema = targetSchema;
            sourceDatabase = sourceTransaction.Connection;
        }

        public async Task Create(string synonym, string? target = null)
        {
            target ??= synonym;
            using var command = sourceDatabase.CreateCommand();
            command.Transaction = sourceTransaction;
            command.CommandText = $@"
if not exists (
    select 0
    from sys.synonyms
    where [base_object_name]=N'[{targetDatabase}].[{targetSchema}].[{target}]'
)
begin
    create synonym [{sourceSchema}].[{synonym}]
    for [{targetDatabase}].[{targetSchema}].[{target}];
end
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

        public async Task Drop(string synonym, string? target = null)
        {
            target ??= synonym;
            using var command = sourceDatabase.CreateCommand();
            command.Transaction = sourceTransaction;
            command.CommandText = $@"
if exists (
    select 0
    from sys.synonyms
    where [base_object_name]=N'[{targetDatabase}].[{targetSchema}].[{target}]'
)
begin
    drop synonym [{sourceSchema}].[{synonym}];
end
";
            await command.ExecuteNonQueryAsync();
        }
    }
}