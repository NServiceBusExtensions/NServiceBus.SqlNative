using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
        where TIncoming : IIncomingMessage
    {
        protected abstract SqlCommand BuildReadCommand(int batchSize, long startRowVersion);

        public virtual async Task<TIncoming> Read(long rowVersion, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            var shouldCleanup = false;
            SqlDataReader reader = null;
            try
            {
                using (var command = BuildReadCommand(1, rowVersion))
                {
                    reader = await command.ExecuteSingleRowReader(cancellation);
                    if (!await reader.ReadAsync(cancellation))
                    {
                        shouldCleanup = true;
                        return default;
                    }

                    return ReadMessage(reader, reader);
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
                    reader?.Dispose();
                }
            }
        }
    }
}