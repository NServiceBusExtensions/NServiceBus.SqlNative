using System.Data;
using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Reader
    {
        string table;

        public Reader(string table)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            this.table = table;
        }

        SqlCommand BuildCommand(SqlConnection connection, int batchSize, long startRowVersion)
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