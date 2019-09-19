﻿using System.Data.Common;
using System.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager :
        BaseQueueManager<IncomingMessage, OutgoingMessage>
    {
        bool dedupe;
        Table dedupeTable;

        public QueueManager(Table table, DbConnection connection) :
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

        public QueueManager(Table table, DbConnection connection, Table dedupeTable) :
            base(table, connection)
        {
            Guard.AgainstNull(dedupeTable, nameof(dedupeTable));
            dedupe = true;
            this.dedupeTable = dedupeTable;

            InitSendSql();
        }

        public QueueManager(Table table, SqlTransaction transaction, Table dedupeTable) :
            base(table, transaction)
        {
            Guard.AgainstNull(dedupeTable, nameof(dedupeTable));
            dedupe = true;
            this.dedupeTable = dedupeTable;

            InitSendSql();
        }
    }
}