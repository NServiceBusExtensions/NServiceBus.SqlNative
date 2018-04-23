using System.Data.SqlClient;

namespace SqlServer.Native
{
    public partial class Receiver
    {
        string table;

        public Receiver(string table)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            this.table = table;
        }

        SqlCommand BuildCommand(SqlConnection connection, SqlTransaction transaction, int batchSize)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = string.Format(ReceiveSql, table, batchSize);
            return command;
        }

        public static readonly string ReceiveSql = @"
declare @nocount varchar(3) = 'off';
if ( (512 & @@options) = 512 ) set @nocount = 'on';
set nocount on;

with message as (
    select top({1}) *
    from {0} with (updlock, readpast, rowlock)
    order by RowVersion)
delete from message
output
    deleted.Id,
    deleted.RowVersion,
    deleted.CorrelationId,
    deleted.ReplyToAddress,
    deleted.Expires,
    deleted.Headers,
    deleted.Body;

if (@nocount = 'on') set nocount on;
if (@nocount = 'off') set nocount off;";
    }
}