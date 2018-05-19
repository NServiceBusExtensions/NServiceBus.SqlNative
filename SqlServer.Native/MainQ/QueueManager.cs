using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager : BaseQueueManager<IncomingMessage, OutgoingMessage>
    {
        bool deduplicate;
        Table deduplicationTable;

        public QueueManager(Table table, SqlConnection connection, bool deduplicate = false, Table deduplicationTable = null):
            base(table, connection)
        {
            this.deduplicate = deduplicate;
            this.deduplicationTable = deduplicationTable;
            ValidateDeduplicationTable();
            InitSendSql();
        }

        public QueueManager(Table table, SqlTransaction transaction, bool deduplicate = false, Table deduplicationTable = null) :
            base(table, transaction)
        {
            this.deduplicate = deduplicate;
            this.deduplicationTable = deduplicationTable;
            ValidateDeduplicationTable();
            InitSendSql();
        }

        void ValidateDeduplicationTable()
        {
            if (deduplicate)
            {
                Guard.AgainstNull(deduplicationTable, nameof(deduplicationTable));
            }
        }
    }
}