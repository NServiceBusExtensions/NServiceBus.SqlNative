using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{
    /// <summary>
    /// Handles create of the main transport queue.
    /// </summary>
    public static class QueueCreator
    {
        /// <summary>
        /// Creates a queue.
        /// </summary>
        public static async Task Create(string connection, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                await Create(sqlConnection, null, table, cancellation).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a queue.
        /// </summary>
        public static Task Create(SqlConnection connection, string table, CancellationToken cancellation = default)
        {
            return Create(connection, null, table, cancellation);
        }

        /// <summary>
        /// Creates a queue.
        /// </summary>
        public static Task Create(SqlConnection connection, SqlTransaction transaction, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            var commandText = string.Format(QueueTableSql, table);
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
    Headers nvarchar(max) not null,
    BodyString as cast(Body as nvarchar(max)),
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

        /// <summary>
        /// Creates a Delayed queue.
        /// </summary>
        public static async Task CreateDelayed(string connection, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                await Create(sqlConnection, null, table, cancellation).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a Delayed queue.
        /// </summary>
        public static Task CreateDelayed(SqlConnection connection, string table, CancellationToken cancellation = default)
        {
            return CreateDelayed(connection, null, table, cancellation);
        }

        /// <summary>
        /// Creates a Delayed queue.
        /// </summary>
        public static Task CreateDelayed(SqlConnection connection, SqlTransaction transaction, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            var commandText = string.Format(DelayedTableSql, table);
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
    Headers nvarchar(max) not null,
    BodyString as cast(Body as nvarchar(max)),
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