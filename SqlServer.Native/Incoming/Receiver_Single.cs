using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{
    public partial class Receiver
    {
        public virtual async Task<IncomingMessage> Receive(string connection, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerReceive(sqlConnection, null, cancellation).ConfigureAwait(false);
            }
        }

        public virtual Task<IncomingMessage> Receive(SqlConnection connection, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            return InnerReceive(connection, null, cancellation);
        }

        public virtual Task<IncomingMessage> Receive(SqlTransaction transaction, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            return InnerReceive(transaction.Connection, transaction, cancellation);
        }

        async Task<IncomingMessage> InnerReceive(SqlConnection connection, SqlTransaction transaction, CancellationToken cancellation)
        {
            using (var command = BuildCommand(connection, transaction, 1))
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