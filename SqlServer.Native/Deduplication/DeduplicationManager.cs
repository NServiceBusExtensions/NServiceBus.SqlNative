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
        SqlTransaction transaction;
        string fullTableName;

        public DeduplicationManager(SqlConnection connection, string table = "Deduplication", string schema = "dbo") :
            this(connection, table, schema, true)
        {
        }

        public DeduplicationManager(SqlConnection connection, string table, string schema, bool sanitize)
        {
            Guard.AgainstNullOrEmpty(schema, nameof(schema));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(connection, nameof(connection));
            this.connection = connection;

            if (sanitize)
            {
                table = SqlSanitizer.Sanitize(table);
                schema = SqlSanitizer.Sanitize(schema);
            }

            fullTableName = $"{schema}.{table}";
        }

        public DeduplicationManager(SqlTransaction transaction, string table = "Deduplication", string schema = "dbo") :
            this(transaction, table, schema, true)
        {
        }

        public DeduplicationManager(SqlTransaction transaction, string table, string schema, bool sanitize)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNullOrEmpty(schema, nameof(schema));
            Guard.AgainstNull(transaction, nameof(transaction));
            this.transaction = transaction;
            connection = transaction.Connection;
            if (sanitize)
            {
                table = SqlSanitizer.Sanitize(table);
                schema = SqlSanitizer.Sanitize(schema);
            }

            fullTableName = $"{schema}.{table}";
        }

        public virtual async Task CleanupItemsOlderThan(DateTime dateTime, CancellationToken cancellation = default)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = $"delete from {fullTableName} where Created < @date";
                command.Parameters.Add("date", SqlDbType.DateTime2).Value = dateTime;
                await command.ExecuteNonQueryAsync(cancellation).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Drops a queue.
        /// </summary>
        public Task Drop(CancellationToken cancellation = default)
        {
            return connection.DropTable(transaction, fullTableName, cancellation);
        }

        /// <summary>
        /// Creates a queue.
        /// </summary>
        public Task Create(CancellationToken cancellation = default)
        {
            var dedupCommandText = string.Format(DeduplicationTableSql, fullTableName);
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