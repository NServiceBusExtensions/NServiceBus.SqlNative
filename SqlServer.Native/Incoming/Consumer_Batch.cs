using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Consumer
    {
        public virtual Task<IncomingResult> Consume(string connection, int size, Action<IncomingMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(action, nameof(action));
            return Consume(connection, size, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> Consume(string connection, int size, Func<IncomingMessage, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(action, nameof(action));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerConsume(sqlConnection, null, size, action, cancellation).ConfigureAwait(false);
            }
        }

        public virtual Task<IncomingResult> Consume(SqlConnection connection, int size, Action<IncomingMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(action, nameof(action));
            return Consume(connection, size, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<IncomingResult> Consume(SqlConnection connection, int size, Func<IncomingMessage, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(action, nameof(action));
            return InnerConsume(connection, null, size, action, cancellation);
        }

        public virtual Task<IncomingResult> Consume(SqlTransaction transaction, int size, Action<IncomingMessage> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(action, nameof(action));
            return Consume(transaction, size, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<IncomingResult> Consume(SqlTransaction transaction, int size, Func<IncomingMessage, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNull(action, nameof(action));
            return InnerConsume(transaction.Connection, transaction, size, action, cancellation);
        }

        Task<IncomingResult> InnerConsume(SqlConnection connection, SqlTransaction transaction, int size, Func<IncomingMessage, Task> action, CancellationToken cancellation)
        {
            return TransactionWrapper.Run(connection, transaction, sqlTransaction => Inner(sqlTransaction, size, action, cancellation, MessageReader.ReadBytesMessage));
        }

        async Task<IncomingResult> Inner<T>(SqlTransaction transaction, int size, Func<T, Task> action, CancellationToken cancellation, Func<SqlDataReader, T> func)
            where T : class,IIncomingMessage
        {
            using (var command = BuildCommand(transaction, size))
            {
                return await command.ReadMultiple(action, cancellation, func);
            }
        }

    }
}