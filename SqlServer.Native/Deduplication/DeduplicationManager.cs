using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public class DeduplicationManager
    {
        string table;
        SqlConnection connection;
        SqlTransaction transaction;

        public DeduplicationManager(SqlConnection connection, string table = "Deduplication")
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(connection, nameof(connection));
            this.table = table;
            this.connection = connection;
        }

        public DeduplicationManager(SqlTransaction transaction, string table = "Deduplication")
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(transaction, nameof(transaction));
            this.table = table;
            this.transaction = transaction;
            connection = transaction.Connection;
        }

        public virtual async Task CleanupItemsOlderThan(DateTime dateTime, CancellationToken cancellation = default)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = $"delete from {table} where Created < @date";
                command.Parameters.Add("date", SqlDbType.DateTime2).Value= dateTime;
                await command.ExecuteNonQueryAsync(cancellation).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// Drops a queue.
        /// </summary>
        public Task Drop(CancellationToken cancellation = default)
        {
            return connection.DropTable(transaction, table, cancellation);
        }

        /// <summary>
        /// Creates a queue.
        /// </summary>
        public Task Create(CancellationToken cancellation = default)
        {
            var dedupCommandText = string.Format(DeduplcationTableSql, table);
            return connection.ExecuteCommand(transaction, dedupCommandText, cancellation);
        }

        /// <summary>
        /// The sql statements used to create the deduplcation table
        /// </summary>
        public static readonly string DeduplcationTableSql = @"
if exists (
    select *
    from sys.objects
    where object_id = object_id('{0}')
        and type in ('U'))
return

create table {0} (
    Id uniqueidentifier primary key,
    Created datetime2 not null default sysutcdatetime(),
);
";
    }
}