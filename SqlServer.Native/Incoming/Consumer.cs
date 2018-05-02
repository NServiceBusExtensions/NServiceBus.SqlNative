using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Consumer
    {
        string table;
        SqlTransaction transaction;
        SqlConnection connection;

        public Consumer(string table, SqlConnection connection)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(connection, nameof(connection));
            this.table = table;
            this.connection = connection;
        }

        public Consumer(string table, SqlTransaction transaction)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(transaction, nameof(transaction));
            this.table = table;
            this.transaction = transaction;
            connection = transaction.Connection;
        }

        SqlCommand BuildCommand(int batchSize)
        {
            var command = this.connection.CreateCommand();
            command.Transaction = this.transaction;
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
    datalength(deleted.Body),
    deleted.Body;
");
    }
}