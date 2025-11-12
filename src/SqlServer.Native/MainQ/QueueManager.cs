namespace NServiceBus.Transport.SqlServerNative;

public partial class QueueManager :
    BaseQueueManager<IncomingMessage, OutgoingMessage>
{
    bool dedupe;
    Table? dedupeTable;

    public QueueManager(Table table, SqlConnection connection) :
        base(table, connection)
    {
        dedupe = false;
        InitSendSql();
    }

    public QueueManager(Table table, SqlTransaction transaction) :
        base(table, transaction)
    {
        dedupe = false;
        InitSendSql();
    }

    public QueueManager(Table table, SqlConnection connection, Table dedupeTable) :
        base(table, connection)
    {
        dedupe = true;
        this.dedupeTable = dedupeTable;

        InitSendSql();
    }

    public QueueManager(Table table, SqlTransaction transaction, Table dedupeTable) :
        base(table, transaction)
    {
        dedupe = true;
        this.dedupeTable = dedupeTable;

        InitSendSql();
    }
}