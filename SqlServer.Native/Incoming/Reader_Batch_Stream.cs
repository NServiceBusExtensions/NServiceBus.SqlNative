using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Reader
    {
        public virtual async Task<IncomingResult> ReadStream(string connection, int size, long startRowVersion, Func<IncomingStreamMessage, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNull(func, nameof(func));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerReadStream(sqlConnection, size, startRowVersion, func, cancellation)
                    .ConfigureAwait(false);
            }
        }

        public virtual Task<IncomingResult> ReadStream(SqlConnection connection, int size, long startRowVersion, Action<IncomingStreamMessage> action, CancellationToken cancellation = default)
        {
            return ReadStream(connection, size, startRowVersion, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<IncomingResult> ReadStream(SqlConnection connection, int size, long startRowVersion, Func<IncomingStreamMessage, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNull(func, nameof(func));
            return InnerReadStream(connection, size, startRowVersion, func, cancellation);
        }

        async Task<IncomingResult> InnerReadStream(SqlConnection connection, int size, long startRowVersion, Func<IncomingStreamMessage, Task> func, CancellationToken cancellation)
        {
            using (var command = BuildCommand(connection, size, startRowVersion))
            {
                return await command.ReadMultipleStream(func, cancellation);
            }
        }
    }
}