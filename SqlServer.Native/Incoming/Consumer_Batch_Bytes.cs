using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Consumer
    {
        public virtual Task<IncomingResult> ConsumeBytes(string connection, int size, Action<IncomingBytesMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(action, nameof(action));
            return ConsumeBytes(connection, size, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> ConsumeBytes(string connection, int size, Func<IncomingBytesMessage, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(func, nameof(func));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await TransactionWrapper.Run(sqlTransaction => InnerReadBytes(sqlTransaction, size, func, cancellation), sqlConnection).ConfigureAwait(false);
            }
        }

        public virtual Task<IncomingResult> ConsumeBytes(SqlConnection connection, int size, Action<IncomingBytesMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(action, nameof(action));
            return ConsumeBytes(connection, size, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<IncomingResult> ConsumeBytes(SqlConnection connection, int size, Func<IncomingBytesMessage, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(func, nameof(func));
            return TransactionWrapper.Run(sqlTransaction => InnerReadBytes(sqlTransaction, size, func, cancellation), connection);
        }

        public virtual Task<IncomingResult> ConsumeBytes(SqlTransaction transaction, int size, Action<IncomingBytesMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(action, nameof(action));
            return ConsumeBytes(transaction, size, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<IncomingResult> ConsumeBytes(SqlTransaction transaction, int size, Func<IncomingBytesMessage, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(func, nameof(func));
            return TransactionWrapper.Run(sqlTransaction => InnerReadBytes(sqlTransaction, size, func, cancellation), transaction.Connection, transaction);
        }

        async Task<IncomingResult> InnerReadBytes(SqlTransaction transaction, int size, Func<IncomingBytesMessage, Task> func, CancellationToken cancellation)
        {
            using (var command = BuildCommand(transaction, size))
            {
                return await command.ReadMultipleBytes(func, cancellation);
            }
        }
    }
}