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
        const string dedupSql = @"insert into {0} (Id) values (@Id);";

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
            sendSql = string.Format(dedupSql, table);
        }

        SqlCommand CreateDedupRecordCommand(Guid messageId)
        {
            var command = connection.CreateCommand(transaction, string.Format(sendSql, table));
            var parameters = command.Parameters;
            parameters.Add("Id", SqlDbType.UniqueIdentifier).Value = messageId;
            return command;
        }

        public async Task<DeduplicationOutcome> WriteDedupRecord(CancellationToken cancellation, Guid messageId)
        {
            using (var command = CreateDedupRecordCommand(messageId))
            {
                try
                {
                    await command.ExecuteNonQueryAsync(cancellation).ConfigureAwait(false);
                }
                catch (SqlException sqlException)
                {
                    foreach (SqlError sqlError in sqlException.Errors)
                    {
                        //Unique Key Violation = 2627
                        if (sqlError.Number == 2627)
                        {
                            return DeduplicationOutcome.Deduplicated;
                        }
                    }
                    throw;
                }
            }

            return DeduplicationOutcome.Sent;
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

        public virtual async Task PurgeItems(CancellationToken cancellation = default)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = $"delete from {table}";
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
            var command = string.Format(DeduplicationTableSql, table);
            return connection.ExecuteCommand(transaction, command, cancellation);
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