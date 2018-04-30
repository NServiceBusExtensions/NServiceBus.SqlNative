using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Consumer
    {
        public virtual async Task<IncomingStreamMessage> ConsumeStream(string connection, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            var sqlConnection = new SqlConnection(connection);
            try
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerConsume(null, sqlConnection, true, cancellation);
            }
            catch
            {
                sqlConnection.Dispose();
                throw;
            }
        }

        public virtual Task<IncomingStreamMessage> ConsumeStream(SqlConnection connection, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            return InnerConsume(null, connection, false, cancellation);
        }

        public virtual Task<IncomingStreamMessage> ConsumeStream(SqlTransaction transaction, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            return InnerConsume(transaction, transaction.Connection, false, cancellation);
        }

        async Task<IncomingStreamMessage> InnerConsume(SqlTransaction transaction, SqlConnection connection, bool connectionOwned, CancellationToken cancellation)
        {
            var shouldCleanup = false;
            SqlDataReader reader = null;
            try
            {
                using (var command = BuildCommand(connection, transaction, 1))
                {
                    reader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false);
                    if (!await reader.ReadAsync(cancellation).ConfigureAwait(false))
                    {
                        reader.Dispose();
                        return null;
                    }

                    if (connectionOwned)
                    {
                        return reader.ReadStreamMessage(connection, transaction, reader);
                    }

                    return reader.ReadStreamMessage(reader);
                }
            }
            catch
            {
                shouldCleanup = true;
                throw;
            }
            finally
            {
                if (shouldCleanup)
                {
                    if (connectionOwned)
                    {
                        connection?.Dispose();
                        transaction?.Dispose();
                    }

                    reader?.Dispose();
                }
            }
        }
    }
}