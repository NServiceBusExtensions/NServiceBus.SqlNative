using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Reader
    {
        public virtual Task<IncomingResult> ReadBytes(string connection, int size, long startRowVersion, Action<IncomingBytesMessage> action, CancellationToken cancellation = default)
        {
            return ReadBytes(connection, size, startRowVersion, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> ReadBytes(string connection, int size, long startRowVersion, Func<IncomingBytesMessage, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNull(action, nameof(action));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerRead(sqlConnection, size, startRowVersion, action, cancellation, MessageReader.ReadBytesMessage)
                    .ConfigureAwait(false);
            }
        }

        public virtual Task<IncomingResult> ReadBytes(SqlConnection connection, int size, long startRowVersion, Action<IncomingBytesMessage> action, CancellationToken cancellation = default)
        {
            return ReadBytes(connection, size, startRowVersion, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<IncomingResult> ReadBytes(SqlConnection connection, int size, long startRowVersion, Func<IncomingBytesMessage, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNull(action, nameof(action));
            return InnerRead(connection, size, startRowVersion, action, cancellation, MessageReader.ReadBytesMessage);
        }

        public virtual Task<IncomingResult> ReadStream(string connection, int size, long startRowVersion, Action<IncomingStreamMessage> action, CancellationToken cancellation = default)
        {
            return ReadStream(connection, size, startRowVersion, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> ReadStream(string connection, int size, long startRowVersion, Func<IncomingStreamMessage, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNull(action, nameof(action));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerRead(sqlConnection, size, startRowVersion, action, cancellation, MessageReader.ReadStreamMessage)
                    .ConfigureAwait(false);
            }
        }

        public virtual Task<IncomingResult> ReadStream(SqlConnection connection, int size, long startRowVersion, Action<IncomingStreamMessage> action, CancellationToken cancellation = default)
        {
            return ReadStream(connection, size, startRowVersion, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<IncomingResult> ReadStream(SqlConnection connection, int size, long startRowVersion, Func<IncomingStreamMessage, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNull(action, nameof(action));
            return InnerRead(connection, size, startRowVersion, action, cancellation, MessageReader.ReadStreamMessage);
        }

        async Task<IncomingResult> InnerRead<T>(SqlConnection connection, int size, long startRowVersion, Func<T, Task> action, CancellationToken cancellation, Func<SqlDataReader, T> func)
            where T : class, IIncomingMessage
        {
            using (var command = BuildCommand(connection, size, startRowVersion))
            {
                return await command.ReadMultiple(action, cancellation, func);
            }
        }
    }
}