using System.Data;
using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Reader
    {
        string table;
        SqlConnection connection;
        SqlTransaction transaction;

        public Reader(string table, SqlConnection connection)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(connection, nameof(connection));
            this.table = table;
            this.connection = connection;
        }

        public Reader(string table, SqlTransaction transaction)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(transaction, nameof(transaction));
            this.table = table;
            this.transaction = transaction;
            connection = transaction.Connection;
        }

        SqlCommand BuildCommand(int batchSize, long startRowVersion)
        {
            var command = connection.CreateCommand();
            command.CommandText = string.Format(Sql, table, batchSize);
            command.Parameters.Add("RowVersion", SqlDbType.BigInt).Value = startRowVersion;
            return command;
        }

        public static readonly string Sql = SqlHelpers.WrapInNoCount(@"
select top({1})
    Id,
    RowVersion,
    CorrelationId,
    ReplyToAddress,
    Expires,
    Headers,
    datalength(Body),
    Body
from {0}
with (readpast)
where RowVersion >= @RowVersion
order by RowVersion
");

    }
}