using System.Data;
using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        SqlCommand BuildReadCommand(int batchSize, long startRowVersion)
        {
            var command = connection.CreateCommand(transaction, string.Format(ReadSql, table, batchSize));
            command.Parameters.Add("RowVersion", SqlDbType.BigInt).Value = startRowVersion;
            return command;
        }

        public static readonly string ReadSql = ConnectionHelpers.WrapInNoCount(@"
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