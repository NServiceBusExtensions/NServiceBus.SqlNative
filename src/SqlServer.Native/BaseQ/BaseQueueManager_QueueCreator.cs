using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
        where TIncoming : class, IIncomingMessage
    {
        /// <summary>
        /// Creates a queue.
        /// </summary>
        public virtual Task Create(string computedColumnSql, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(computedColumnSql, nameof(computedColumnSql));
            return InnerCreate(true, computedColumnSql, cancellation);
        }

        /// <summary>
        /// Creates a queue.
        /// </summary>
        public virtual Task Create(bool createDecodedBodyComputedColumn = true, CancellationToken cancellation = default)
        {
            return InnerCreate(createDecodedBodyComputedColumn, null, cancellation);
        }

        /// <summary>
        /// Drops a queue.
        /// </summary>
        public virtual Task Drop(CancellationToken cancellation = default)
        {
            return Connection.DropTable(Transaction, Table, cancellation);
        }

        Task InnerCreate(bool createDecodedBodyComputedColumn, string? computedColumnSql, CancellationToken cancellation)
        {
            if (createDecodedBodyComputedColumn)
            {
                computedColumnSql = BodyComputedColumnBuilder.Computed(computedColumnSql);
            }
            else
            {
                computedColumnSql = string.Empty;
            }

            var commandText = string.Format(CreateTableSql, Table, computedColumnSql);
            return Connection.RunCommand(Transaction, commandText, cancellation);
        }

        /// <summary>
        /// The sql statements used to create the queue.
        /// </summary>
        public abstract string CreateTableSql { get; }
    }
}