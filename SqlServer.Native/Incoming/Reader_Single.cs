using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Reader
    {
        public virtual async Task<IncomingMessage> Read(string connection, long rowVersion, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerFind(sqlConnection, rowVersion, cancellation, MessageReader.ReadBytesMessage)
                    .ConfigureAwait(false);
            }
        }

        public virtual Task<IncomingMessage> Read(SqlConnection connection, long rowVersion, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            return InnerFind(connection, rowVersion, cancellation, MessageReader.ReadBytesMessage);
        }

        async Task<T> InnerFind<T>(SqlConnection connection, long rowVersion, CancellationToken cancellation, Func<SqlDataReader, T> func)
            where T : class
        {
            using (var command = BuildCommand(connection, 1, rowVersion))
            {
                return await command.ReadSingle(cancellation, func);
            }
        }
    }
}