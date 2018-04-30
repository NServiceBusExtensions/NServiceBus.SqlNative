using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Consumer
    {
        public virtual async Task<IncomingBytesMessage> ConsumeBytes(string connection, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerConsumeBytes(sqlConnection, null, cancellation).ConfigureAwait(false);
            }
        }

        public virtual Task<IncomingBytesMessage> ConsumeBytes(SqlConnection connection, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            return InnerConsumeBytes(connection, null, cancellation);
        }

        public virtual Task<IncomingBytesMessage> ConsumeBytes(SqlTransaction transaction, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            return InnerConsumeBytes(transaction.Connection, transaction, cancellation);
        }

        async Task<IncomingBytesMessage> InnerConsumeBytes(SqlConnection connection, SqlTransaction transaction, CancellationToken cancellation)
        {
            using (var command = BuildCommand(connection, transaction, 1))
            using (var reader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false))
            {
                return await reader.ReadSingleBytes(cancellation);
            }
        }
    }
}