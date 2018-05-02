using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedSender
    {
        string table;
        SqlConnection connection;
        SqlTransaction transaction;

        public DelayedSender(string table, SqlConnection connection)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(connection, nameof(connection));
            this.table = table;
            this.connection = connection;
        }

        public DelayedSender(string table, SqlTransaction transaction)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(transaction, nameof(transaction));
            this.table = table;
            this.transaction = transaction;
            connection = transaction.Connection;
        }

        public static readonly string Sql = SqlHelpers.WrapInNoCount(
            @"
insert into {0} (
    Due,
    Headers,
    Body)
output inserted.RowVersion
values (
    @Due,
    @Headers,
    @Body);");

    }
}