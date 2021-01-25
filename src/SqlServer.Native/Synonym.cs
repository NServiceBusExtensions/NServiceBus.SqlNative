using System.Data.Common;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public class Synonym
    {
        DbConnection connection;
        string targetDatabase;
        DbTransaction? transaction;

        public Synonym(DbConnection connection, string targetDatabase)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(targetDatabase, nameof(targetDatabase));
            this.connection = connection;
            this.targetDatabase = targetDatabase;
        }

        public Synonym(DbTransaction transaction, string targetDatabase)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNullOrEmpty(targetDatabase, nameof(targetDatabase));
            this.transaction = transaction;
            this.targetDatabase = targetDatabase;
            connection = transaction.Connection;
        }

        public async Task Create(string synonym, string? target = null)
        {
            if (target == null)
            {
                target = synonym;
            }
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $@"
if not exists (
    select 0
    from sys.synonyms
    where [name]=N'{synonym}'
)
begin
    create synonym [{synonym}]
    for {targetDatabase}.[{target}];
end
";
            await command.ExecuteNonQueryAsync();
        }

        public async Task DropAll()
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
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
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $@"
if exists (
    select 0
    from sys.synonyms
    where [name]=N'{synonym}'
)
begin
    drop synonym [{synonym}];
end
";
            await command.ExecuteNonQueryAsync();
        }
    }
}