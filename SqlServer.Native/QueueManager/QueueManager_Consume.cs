using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        SqlCommand BuildConsumeCommand(int batchSize)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = string.Format(ConsumeSql, table, batchSize);
            return command;
        }

        public static readonly string ConsumeSql = SqlHelpers.WrapInNoCount(@"
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