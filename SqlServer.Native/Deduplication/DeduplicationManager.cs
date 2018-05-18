using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public class DeduplicationManager
    {
        SqlConnection connection;
        Table table;
        SqlTransaction transaction;

        public DeduplicationManager(SqlConnection connection, Table table)
        {
            Guard.AgainstNull(table, nameof(table));
            Guard.AgainstNull(connection, nameof(connection));
            this.connection = connection;
            this.table = table;
        }

        public DeduplicationManager(SqlTransaction transaction, Table table)
        {
            Guard.AgainstNull(table, nameof(table));
            Guard.AgainstNull(transaction, nameof(transaction));
            this.transaction = transaction;
            this.table = table;
            connection = transaction.Connection;
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
        public Task Drop(CancellationToken cancellation = default)
        {
            return connection.DropTable(transaction, table, cancellation);
        }

        /// <summary>
        /// Creates a queue.
        /// </summary>
        public Task Create(CancellationToken cancellation = default)
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
    }
}