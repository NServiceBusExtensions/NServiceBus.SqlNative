using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager : BaseQueueManager<IncomingMessage, OutgoingMessage>
    {
        bool deduplicate;
        Table deduplicationTable;

        public QueueManager(Table table, SqlConnection connection) :
            base(table, connection)
        {
            deduplicate = false;
            InitSendSql();
        }

        public QueueManager(Table table, SqlTransaction transaction) :
            base(table, transaction)
        {
            deduplicate = false;
            InitSendSql();
        }

        public QueueManager(Table table, SqlConnection connection, Table deduplicationTable) :
            base(table, connection)
        {
            Guard.AgainstNull(deduplicationTable, nameof(deduplicationTable));
            deduplicate = true;
            this.deduplicationTable = deduplicationTable;

            InitSendSql();
        }

        public QueueManager(Table table, SqlTransaction transaction, Table deduplicationTable) :
            base(table, transaction)
        {
            Guard.AgainstNull(deduplicationTable, nameof(deduplicationTable));
            deduplicate = true;
            this.deduplicationTable = deduplicationTable;

            InitSendSql();
        }
    }
}