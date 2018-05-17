using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager : BaseQueueManager<IncomingMessage, OutgoingMessage>
    {
        bool deduplicate;
        string deduplicationTable;

        public QueueManager(string table, SqlConnection connection, bool deduplicate = false, string deduplicationTable = "Deduplication", string schema = "dbo") :
            base(table, connection, schema)
        {
            this.deduplicate = deduplicate;
            this.deduplicationTable = deduplicationTable;
            ValidateDeduplicationTable();
            InitSendSql();
        }

        public QueueManager(string table, SqlTransaction transaction, bool deduplicate = false, string deduplicationTable = "Deduplication", string schema = "dbo") :
            base(table, transaction, schema)
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
                Guard.AgainstNullOrEmpty(deduplicationTable, nameof(deduplicationTable));
            }
        }
    }
}