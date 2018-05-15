using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager: BaseQueueManager<IncomingDelayedMessage, OutgoingDelayedMessage>
    {
        public DelayedQueueManager(string table, SqlConnection connection):
            base(table, connection)
        {
        }

        public DelayedQueueManager(string table, SqlTransaction transaction) :
            base(table, transaction)
        {
        }
    }
}