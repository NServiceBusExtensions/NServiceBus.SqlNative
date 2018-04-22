using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{
    /// <summary>
    /// Handles create of the main transport queue.
    /// </summary>
    public static class MessageQueueCreator
    {
        /// <summary>
        /// Drops a queue.
        /// </summary>
        public static async Task Drop(string connection, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                await Drop(sqlConnection, null, table, cancellation);
            }
        }

        /// <summary>
        /// Drops a queue.
        /// </summary>
        public static Task Drop(SqlConnection connection, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return Drop(connection, null, table, cancellation);
        }

        /// <summary>
        /// Drops a queue.
        /// </summary>
        public static async Task Drop(SqlConnection connection, SqlTransaction transaction, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = $@"
if exists (
select *
    from sys.objects
where
    object_id = object_id('{table}')
    and type in ('U'))
    drop table {table}";
                await command.ExecuteNonQueryAsync(cancellation).ConfigureAwait(false);
            }
        }

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
                await Create(sqlConnection, null, table, cancellation);
            }
        }

        /// <summary>
        /// Creates a queue.
        /// </summary>
        public static Task Create(SqlConnection connection, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return Create(connection, null, table, cancellation);
        }

        /// <summary>
        /// Creates a queue.
        /// </summary>
        public static async Task Create(SqlConnection connection, SqlTransaction transaction, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = string.Format(Sql, table);
                await command.ExecuteNonQueryAsync(cancellation).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The sql statements used to create the queue.
        /// </summary>
        public static readonly string Sql = @"
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
    }
}