using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        public virtual async Task<IncomingDelayedStreamMessage> ReadStream(long rowVersion, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            var shouldCleanup = false;
            SqlDataReader reader = null;
            try
            {
                using (var command = BuildReadCommand(1, rowVersion))
                {
                    reader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false);
                    if (!await reader.ReadAsync(cancellation).ConfigureAwait(false))
                    {
                        shouldCleanup = true;
                        return null;
                    }

                    return reader.ReadDelayedStreamMessage(reader);
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