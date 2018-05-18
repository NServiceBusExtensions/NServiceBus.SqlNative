using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager: BaseQueueManager<IncomingDelayedMessage, OutgoingDelayedMessage>
    {
        public DelayedQueueManager(string table, SqlConnection connection, string schema = "dbo") :
            base(table, connection, schema, true)
        {
        }

        public DelayedQueueManager(string table, SqlConnection connection, string schema, bool sanitize) :
            base(table, connection, schema, sanitize)
        {
        }

        public DelayedQueueManager(string table, SqlTransaction transaction, string schema = "dbo") :
            base(table, transaction, schema, true)
        {
        }

        public DelayedQueueManager(string table, SqlTransaction transaction, string schema, bool sanitize) :
            base(table, transaction, schema, sanitize)
        {
        }
    }
}