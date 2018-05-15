using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        protected override SqlCommand BuildConsumeCommand(int batchSize)
        {
            return connection.CreateCommand(transaction, string.Format(ConsumeSql, table, batchSize));
        }

        public static readonly string ConsumeSql = ConnectionHelpers.WrapInNoCount(@"
with message as (
    select top({1}) *
    from {0} with (updlock, readpast, rowlock)
    order by RowVersion)
delete from message
output
    deleted.RowVersion,
    deleted.Due,
    deleted.Headers,
    datalength(deleted.Body),
    deleted.Body;
");
    }
}