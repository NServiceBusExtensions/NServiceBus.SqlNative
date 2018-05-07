using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        string table;
        SqlConnection connection;
        bool deduplicate;
        string deduplicationTable;
        SqlTransaction transaction;

        public QueueManager(string table, SqlConnection connection, bool deduplicate = false, string deduplicationTable = "Deduplication")
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(connection, nameof(connection));
            this.table = table;
            this.connection = connection;
            this.deduplicate = deduplicate;
            this.deduplicationTable = deduplicationTable;
            ValidateDeduplicationTable();
            InitSendSql();
        }

        public QueueManager(string table, SqlTransaction transaction, bool deduplicate = false, string deduplicationTable = "Deduplication")
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(transaction, nameof(transaction));
            this.table = table;
            this.transaction = transaction;
            this.deduplicate = deduplicate;
            connection = transaction.Connection;
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