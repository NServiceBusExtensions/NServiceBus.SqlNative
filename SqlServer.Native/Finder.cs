using System.Data;
using System.Data.SqlClient;

namespace SqlServer.Native
{
    public partial class Finder
    {
        string table;

        public Finder(string table)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            this.table = table;
        }

        SqlCommand BuildCommand(SqlConnection connection, SqlTransaction transaction, int batchSize, long startRowVersion)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = string.Format(FindSql, table, batchSize);
            command.Parameters.Add("RowVersion", SqlDbType.BigInt).Value = startRowVersion;
            return command;
        }

        public static readonly string FindSql = @"
declare @nocount varchar(3) = 'off';
if ( (512 & @@options) = 512 ) set @nocount = 'on';
set nocount on;

select top({1})
    Id,
    CorrelationId,
    ReplyToAddress,
    Expires,
    Headers,
    Body
from {0}
where RowVersion >= @RowVersion
order by RowVersion

if (@nocount = 'on') set nocount on;
if (@nocount = 'off') set nocount off;";
    }
}