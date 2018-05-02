using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Reader
    {
        public virtual Task<IncomingStreamMessage> ReadStream(SqlConnection connection, long rowVersion, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            return InnerReadStream(connection, rowVersion, false, cancellation);
        }

        async Task<IncomingStreamMessage> InnerReadStream(SqlConnection connection, long rowVersion, bool connectionOwned, CancellationToken cancellation)
        {
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            var shouldCleanup = false;
            SqlDataReader reader = null;
            try
            {
                using (var command = BuildCommand(connection, 1, rowVersion))
                {
                    reader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false);
                    if (!await reader.ReadAsync(cancellation).ConfigureAwait(false))
                    {
                        shouldCleanup = true;
                        return null;
                    }

                    if (connectionOwned)
                    {
                        return reader.ReadStreamMessage(connection, reader);
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
                        connection.Dispose();
                    }
                    reader?.Dispose();
                }
            }
        }
    }
}