using Microsoft.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative;

public partial class QueueManager
{
    protected override SqlCommand BuildConsumeCommand(int batchSize) =>
        Connection.CreateCommand(Transaction, string.Format(ConsumeSql, Table, batchSize));

    public static readonly string ConsumeSql = ConnectionHelpers.WrapInNoCount(
        """
        with message as (
          select top({1}) *
          from {0} with (updlock, readpast, rowlock)
          order by RowVersion)
        delete from message
        output
          deleted.Id,
          deleted.RowVersion,
          deleted.Expires,
          deleted.Headers,
          datalength(deleted.Body),
          deleted.Body;
        """);
}