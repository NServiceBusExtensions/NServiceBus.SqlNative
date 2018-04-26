using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Consumer
    {
        string table;

        public Consumer(string table)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            this.table = table;
        }

        SqlCommand BuildCommand(SqlTransaction transaction, int batchSize)
        {
            return BuildCommand(transaction.Connection, transaction, batchSize);
        }

        SqlCommand BuildCommand(SqlConnection connection, SqlTransaction transaction, int batchSize)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = string.Format(Sql, table, batchSize);
            return command;
        }

        public static readonly string Sql = SqlHelpers.WrapInNoCount(@"
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
");
    }
}