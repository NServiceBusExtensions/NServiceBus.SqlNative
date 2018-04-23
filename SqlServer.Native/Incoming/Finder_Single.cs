using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{
    public partial class Finder
    {
        public virtual async Task<IncomingMessage> Find(string connection, long rowVersion, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerFind(sqlConnection, rowVersion, cancellation).ConfigureAwait(false);
            }
        }

        public virtual Task<IncomingMessage> Find(SqlConnection connection, long rowVersion, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            return InnerFind(connection, rowVersion, cancellation);
        }

        async Task<IncomingMessage> InnerFind(SqlConnection connection, long rowVersion, CancellationToken cancellation)
        {
            using (var command = BuildCommand(connection, 1, rowVersion))
            using (var reader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false))
            {
                if (!await reader.ReadAsync(cancellation).ConfigureAwait(false))
                {
                    return null;
                }

                return await reader.ReadMessage(cancellation).ConfigureAwait(false);
            }
        }
    }
}