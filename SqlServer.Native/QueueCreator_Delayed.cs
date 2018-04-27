using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public static partial class QueueCreator
    {
        /// <summary>
        /// Creates a Delayed queue.
        /// </summary>
        public static async Task CreateDelayed(string connection, string table, string computedColumnSql, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                await InnerCreateDelayed(sqlConnection, null, table, true, computedColumnSql, cancellation).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a Delayed queue.
        /// </summary>
        public static Task CreateDelayed(SqlConnection connection, string table, string computedColumnSql, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return InnerCreateDelayed(connection, null, table, true, computedColumnSql, cancellation);
        }

        /// <summary>
        /// Creates a Delayed queue.
        /// </summary>
        public static Task CreateDelayed(SqlTransaction transaction, string table, string computedColumnSql, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return InnerCreateDelayed(transaction.Connection, transaction, table, true, computedColumnSql, cancellation);
        }

        /// <summary>
        /// Creates a Delayed queue.
        /// </summary>
        public static async Task CreateDelayed(string connection, string table, bool createDecodedBodyComputedColumn = true, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                await InnerCreateDelayed(sqlConnection, null, table, createDecodedBodyComputedColumn, null, cancellation).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a Delayed queue.
        /// </summary>
        public static Task CreateDelayed(SqlConnection connection, string table, bool createDecodedBodyComputedColumn = true, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return InnerCreateDelayed(connection, null, table, createDecodedBodyComputedColumn, null, cancellation);
        }

        /// <summary>
        /// Creates a Delayed queue.
        /// </summary>
        public static Task CreateDelayed(SqlTransaction transaction, string table, bool createDecodedBodyComputedColumn = true, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return InnerCreateDelayed(transaction.Connection, transaction, table, createDecodedBodyComputedColumn, null, cancellation);
        }

        static Task InnerCreateDelayed(SqlConnection connection, SqlTransaction transaction, string table, bool createDecodedBodyComputedColumn, string computedColumnSql, CancellationToken cancellation)
        {
            if (createDecodedBodyComputedColumn)
            {
                computedColumnSql = BodyComputedColumnBuilder.Computed(computedColumnSql);
            }
            else
            {
                computedColumnSql = string.Empty;
            }

            var commandText = string.Format(DelayedTableSql, table, computedColumnSql);
            return connection.ExecuteCommand(transaction, commandText, cancellation);
        }

        /// <summary>
        /// The sql statements used to create the Delayed queue.
        /// </summary>
        public static readonly string DelayedTableSql = @"
if exists (
    select *
    from sys.objects
    where object_id = object_id('{0}')
        and type in ('U'))
return

create table {0} (
    Headers nvarchar(max) not null,{1}
    Body varbinary(max),
    Due datetime not null,
    RowVersion bigint identity(1,1) not null
);

create nonclustered index [Index_Due] on {0}
(
    [Due]
)
";
    }
}