using System.Data.Common;

namespace NServiceBus.Transport.SqlServerNative;

public partial class DelayedQueueManager :
    BaseQueueManager<IncomingDelayedMessage, OutgoingDelayedMessage>
{
    public DelayedQueueManager(Table table, DbConnection connection) :
        base(table, connection)
    {
    }

    public DelayedQueueManager(Table table, DbTransaction transaction) :
        base(table, transaction)
    {
    }
}