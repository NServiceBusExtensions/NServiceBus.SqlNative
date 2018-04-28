using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Reader
    {
        public virtual async Task<IncomingBytesMessage> ReadBytes(string connection, long rowVersion, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerReadBytes(sqlConnection, rowVersion, cancellation)
                    .ConfigureAwait(false);
            }
        }

        public virtual Task<IncomingBytesMessage> ReadBytes(SqlConnection connection, long rowVersion, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            return InnerReadBytes(connection, rowVersion, cancellation);
        }

        public virtual async Task<IncomingStreamMessage> ReadStream(string connection, long rowVersion, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            var sqlConnection = new SqlConnection(connection);
            await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
            var command = BuildCommand(sqlConnection, 1, rowVersion);
            {
                var reader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false);
                if (!await reader.ReadAsync(cancellation).ConfigureAwait(false))
                {
                    reader.Dispose();
                    sqlConnection.Dispose();
                    return null;
                }

                return reader.ReadStreamMessage(sqlConnection, reader, command);
            }
        }

        public virtual async Task<IncomingStreamMessage> ReadStream(SqlConnection connection, long rowVersion, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            using (var command = BuildCommand(connection, 1, rowVersion))
            {
                var reader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false);
                if (!await reader.ReadAsync(cancellation).ConfigureAwait(false))
                {
                    reader.Dispose();
                    return null;
                }

                return reader.ReadStreamMessage(reader);
            }
        }

        async Task<IncomingBytesMessage> InnerReadBytes(SqlConnection connection, long rowVersion, CancellationToken cancellation)
        {
            using (var command = BuildCommand(connection, 1, rowVersion))
            using (var reader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false))
            {
                return await reader.ReadSingle(MessageReader.ReadBytesMessage, cancellation);
            }
        }
    }
}