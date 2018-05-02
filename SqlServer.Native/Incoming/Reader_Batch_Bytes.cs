using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Reader
    {
        public virtual async Task<IncomingResult> ReadBytes(string connection, int size, long startRowVersion, Func<IncomingBytesMessage, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNull(func, nameof(func));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerReadBytes(sqlConnection, size, startRowVersion, func, cancellation)
                    .ConfigureAwait(false);
            }
        }

        public virtual Task<IncomingResult> ReadBytes(SqlConnection connection, int size, long startRowVersion, Action<IncomingBytesMessage> action, CancellationToken cancellation = default)
        {
            return ReadBytes(connection, size, startRowVersion, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<IncomingResult> ReadBytes(SqlConnection connection, int size, long startRowVersion, Func<IncomingBytesMessage, Task> func, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNull(func, nameof(func));
            return InnerReadBytes(connection, size, startRowVersion, func, cancellation);
        }


        async Task<IncomingResult> InnerReadBytes(SqlConnection connection, int size, long startRowVersion, Func<IncomingBytesMessage, Task> func, CancellationToken cancellation)
        {
            using (var command = BuildCommand(connection, size, startRowVersion))
            {
                return await command.ReadMultipleBytes(func, cancellation);
            }
        }

    }
}