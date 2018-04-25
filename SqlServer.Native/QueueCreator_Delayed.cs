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
        public static async Task CreateDelayed(string connection, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                await InnerCreateDelayed(sqlConnection, null, table, cancellation).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a Delayed queue.
        /// </summary>
        public static Task CreateDelayed(SqlConnection connection, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return InnerCreateDelayed(connection, null, table, cancellation);
        }

        /// <summary>
        /// Creates a Delayed queue.
        /// </summary>
        public static Task CreateDelayed(SqlTransaction transaction, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return InnerCreateDelayed(transaction.Connection, transaction, table, cancellation);
        }

        static Task InnerCreateDelayed(SqlConnection connection, SqlTransaction transaction, string table, CancellationToken cancellation = default)
        {
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