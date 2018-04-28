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

        public virtual async Task<IncomingResult> ConsumeBytes(string connection, int size, Func<IncomingBytesMessage, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(action, nameof(action));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await TransactionWrapper.Run(sqlConnection, null, sqlTransaction => InnerReadBytes(sqlTransaction, size, action, cancellation)).ConfigureAwait(false);
            }
        }

        public virtual Task<IncomingResult> ConsumeBytes(SqlConnection connection, int size, Action<IncomingBytesMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(action, nameof(action));
            return ConsumeBytes(connection, size, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<IncomingResult> ConsumeBytes(SqlConnection connection, int size, Func<IncomingBytesMessage, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(action, nameof(action));
            return TransactionWrapper.Run(connection, null, sqlTransaction => InnerReadBytes(sqlTransaction, size, action, cancellation));
        }

        public virtual Task<IncomingResult> ConsumeBytes(SqlTransaction transaction, int size, Action<IncomingBytesMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(action, nameof(action));
            return ConsumeBytes(transaction, size, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<IncomingResult> ConsumeBytes(SqlTransaction transaction, int size, Func<IncomingBytesMessage, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(action, nameof(action));
            return TransactionWrapper.Run(transaction.Connection, transaction, sqlTransaction => InnerReadBytes(sqlTransaction, size, action, cancellation));
        }
        public virtual Task<IncomingResult> ConsumeStream(string connection, int size, Action<IncomingStreamMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(action, nameof(action));
            return ConsumeStream(connection, size, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> ConsumeStream(string connection, int size, Func<IncomingStreamMessage, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(action, nameof(action));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await TransactionWrapper.Run(sqlConnection, null, sqlTransaction => InnerReadStream(sqlTransaction, size, action, cancellation)).ConfigureAwait(false);
            }
        }

        public virtual Task<IncomingResult> ConsumeStream(SqlConnection connection, int size, Action<IncomingStreamMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(action, nameof(action));
            return ConsumeStream(connection, size, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<IncomingResult> ConsumeStream(SqlConnection connection, int size, Func<IncomingStreamMessage, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(action, nameof(action));
            return TransactionWrapper.Run(connection, null, sqlTransaction => InnerReadStream(sqlTransaction, size, action, cancellation));
        }

        public virtual Task<IncomingResult> ConsumeStream(SqlTransaction transaction, int size, Action<IncomingStreamMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(action, nameof(action));
            return ConsumeStream(transaction, size, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<IncomingResult> ConsumeStream(SqlTransaction transaction, int size, Func<IncomingStreamMessage, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(action, nameof(action));
            return TransactionWrapper.Run(transaction.Connection, transaction, sqlTransaction => InnerReadStream(sqlTransaction, size, action, cancellation));
        }

        async Task<IncomingResult> InnerReadBytes(SqlTransaction transaction, int size, Func<IncomingBytesMessage, Task> action, CancellationToken cancellation)
        {
            using (var command = BuildCommand(transaction, size))
            {
                return await command.ReadMultiple(action, cancellation, MessageReader.ReadBytesMessage);
            }
        }

        async Task<IncomingResult> InnerReadStream(SqlTransaction transaction, int size, Func<IncomingStreamMessage, Task> action, CancellationToken cancellation)
        {
            using (var command = BuildCommand(transaction, size))
            {
                return await command.ReadMultiple(action, cancellation, reader => reader.ReadStreamMessage());
            }
        }
    }
}