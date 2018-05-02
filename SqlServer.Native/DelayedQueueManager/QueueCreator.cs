using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        /// <summary>
        /// Creates a Delayed queue.
        /// </summary>
        public Task Create(string computedColumnSql, CancellationToken cancellation = default)
        {
            return InnerCreate(true, computedColumnSql, cancellation);
        }

        /// <summary>
        /// Creates a Delayed queue.
        /// </summary>
        public async Task Create(bool createDecodedBodyComputedColumn = true, CancellationToken cancellation = default)
        {
            await InnerCreate(createDecodedBodyComputedColumn, null, cancellation).ConfigureAwait(false);
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