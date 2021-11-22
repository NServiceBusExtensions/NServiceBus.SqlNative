using Microsoft.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative;

public partial class DelayedQueueManager :
    BaseQueueManager<IncomingDelayedMessage, OutgoingDelayedMessage>
{
    public DelayedQueueManager(Table table, SqlConnection connection) :
        base(table, connection)
    {
    }

    public DelayedQueueManager(Table table, SqlTransaction transaction) :
        base(table, transaction)
    {
    }
}