using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
#if (SqlServerDeduplication)
namespace NServiceBus.Transport.SqlServerDeduplication
#else
namespace NServiceBus.Transport.SqlServerNative
#endif
{
    public class DeduplicationManager
    {
        internal const string dedupSql = @"
if exists (
    select *
    from {0}
    where Id = @Id)
return

insert into {0} (Id)
values (@Id);";

        SqlConnection connection;
        Table table;
        SqlTransaction transaction;

        public DeduplicationManager(SqlConnection connection, Table table)
        {
            Guard.AgainstNull(table, nameof(table));
            Guard.AgainstNull(connection, nameof(connection));
            this.connection = connection;
            this.table = table;
            InitSendSql();
        }

        public DeduplicationManager(SqlTransaction transaction, Table table)
        {
            Guard.AgainstNull(table, nameof(table));
            Guard.AgainstNull(transaction, nameof(transaction));
            this.transaction = transaction;
            this.table = table;
            connection = transaction.Connection;
            InitSendSql();
        }

        void InitSendSql()
        {
            var resultSql = string.Format(dedupSql, table);
            sendSql = ConnectionHelpers.WrapInNoCount(resultSql);
        }

        SqlCommand CreateDedupRecordCommand(Guid messageId)
        {
            var command = connection.CreateCommand(transaction, string.Format(sendSql, table));
            var parameters = command.Parameters;
            parameters.Add("Id", SqlDbType.UniqueIdentifier).Value = messageId;
            return command;
        }

        public async Task<long> WriteDedupRecord(CancellationToken cancellation, Guid messageId)
        {
            using (var command = CreateDedupRecordCommand(messageId))
            {
                var rowVersion = await command.ExecuteScalarAsync(cancellation).ConfigureAwait(false);
                if (rowVersion == null)
                {
                    return 0;
                }

                return (long) rowVersion;
            }
        }

        public virtual async Task CleanupItemsOlderThan(DateTime dateTime, CancellationToken cancellation = default)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = $"delete from {table} where Created < @date";
                command.Parameters.Add("date", SqlDbType.DateTime2).Value = dateTime;
                await command.ExecuteNonQueryAsync(cancellation).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Drops a queue.
        /// </summary>
        public virtual Task Drop(CancellationToken cancellation = default)
        {
            return connection.DropTable(transaction, table, cancellation);
        }

        /// <summary>
        /// Creates a queue.
        /// </summary>
        public virtual Task Create(CancellationToken cancellation = default)
        {
            var dedupCommandText = string.Format(DeduplicationTableSql, table);
            return connection.ExecuteCommand(transaction, dedupCommandText, cancellation);
        }

        /// <summary>
        /// The sql statements used to create the deduplication table.
        /// </summary>
        public static readonly string DeduplicationTableSql = @"
if exists (
    select *
    from sys.objects
    where object_id = object_id('{0}')
        and type in ('U'))
return

create table {0} (
    Id uniqueidentifier primary key,
    Created datetime2(0) not null default sysutcdatetime(),
);
";

        string sendSql;
    }
}