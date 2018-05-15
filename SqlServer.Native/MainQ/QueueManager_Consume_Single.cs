using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        public virtual async Task<IncomingMessage> Consume(CancellationToken cancellation = default)
        {
            var shouldCleanup = false;
            SqlDataReader reader = null;
            try
            {
                using (var command = BuildConsumeCommand(1))
                {
                    reader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false);
                    if (!await reader.ReadAsync(cancellation).ConfigureAwait(false))
                    {
                        reader.Dispose();
                        return null;
                    }

                    return reader.ReadMessage(reader);
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