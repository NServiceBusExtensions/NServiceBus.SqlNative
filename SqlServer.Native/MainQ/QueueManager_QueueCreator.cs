using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    /// <summary>
    /// Handles creation of transport queues.
    /// </summary>
    public partial class QueueManager
    {
        /// <summary>
        /// Creates a queue.
        /// </summary>
        public Task Create(string computedColumnSql, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(computedColumnSql, nameof(computedColumnSql));
            return InnerCreate(true, computedColumnSql, cancellation);
        }

        /// <summary>
        /// Drops a queue.
        /// </summary>
        public async Task Drop(CancellationToken cancellation = default)
        {
            await connection.DropTable(transaction, table, cancellation).ConfigureAwait(false);
            if (deduplicate)
            {
                await connection.DropTable(transaction, deduplicationTable, cancellation).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a queue.
        /// </summary>
        public Task Create(bool createDecodedBodyComputedColumn = true, CancellationToken cancellation = default)
        {
            return InnerCreate(createDecodedBodyComputedColumn, null, cancellation);
        }

        Task InnerCreate(bool createDecodedBodyComputedColumn, string computedColumnSql, CancellationToken cancellation)
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