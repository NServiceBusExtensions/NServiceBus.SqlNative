using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    /// <summary>
    /// Handles creation of transport queues.
    /// </summary>
    public static partial class QueueCreator
    {
        /// <summary>
        /// Creates a queue.
        /// </summary>
        public static async Task Create(string connection, string table, string computedColumnSql, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNullOrEmpty(computedColumnSql, nameof(computedColumnSql));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                await InnerCreate(sqlConnection, null, table, true, computedColumnSql, cancellation).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a queue.
        /// </summary>
        public static Task Create(SqlConnection connection, string table, string computedColumnSql, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNullOrEmpty(computedColumnSql, nameof(computedColumnSql));
            return InnerCreate(connection, null, table, true, computedColumnSql, cancellation);
        }

        /// <summary>
        /// Creates a queue.
        /// </summary>
        public static Task Create(SqlTransaction transaction, string table, string computedColumnSql, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNullOrEmpty(computedColumnSql, nameof(computedColumnSql));
            return InnerCreate(transaction.Connection, transaction, table, true, computedColumnSql, cancellation);
        }
        /// <summary>
        /// Creates a queue.
        /// </summary>
        public static async Task Create(string connection, string table, bool createDecodedBodyComputedColumn = true, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                await InnerCreate(sqlConnection, null, table, createDecodedBodyComputedColumn, null, cancellation).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a queue.
        /// </summary>
        public static Task Create(SqlConnection connection, string table, bool createDecodedBodyComputedColumn = true, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return InnerCreate(connection, null, table, createDecodedBodyComputedColumn, null, cancellation);
        }

        /// <summary>
        /// Creates a queue.
        /// </summary>
        public static Task Create(SqlTransaction transaction, string table, bool createDecodedBodyComputedColumn = true, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return InnerCreate(transaction.Connection, transaction, table, createDecodedBodyComputedColumn, null, cancellation);
        }

        static Task InnerCreate(SqlConnection connection, SqlTransaction transaction, string table, bool createDecodedBodyComputedColumn, string computedColumnSql, CancellationToken cancellation)
        {
            if (createDecodedBodyComputedColumn)
            {
                computedColumnSql = BodyComputedColumnBuilder.Computed(computedColumnSql);
            }
            else
            {
                computedColumnSql = string.Empty;
            }
            var commandText = string.Format(QueueTableSql, table, computedColumnSql);
            return connection.ExecuteCommand(transaction, commandText, cancellation);
        }

        /// <summary>
        /// The sql statements used to create the queue.
        /// </summary>
        public static readonly string QueueTableSql = @"
if exists (
    select *
    from sys.objects
    where object_id = object_id('{0}')
        and type in ('U'))
return

create table {0} (
    Id uniqueidentifier not null,
    CorrelationId varchar(255),
    ReplyToAddress varchar(255),
    Recoverable bit not null,
    Expires datetime,
    Headers nvarchar(max) not null,{1}
    Body varbinary(max),
    RowVersion bigint identity(1,1) not null
);

create clustered index Index_RowVersion on {0}
(
    RowVersion
)

create nonclustered index Index_Expires on {0}
(
    Expires
)
include
(
    Id,
    RowVersion
)
where
    Expires is not null
";
    }
}