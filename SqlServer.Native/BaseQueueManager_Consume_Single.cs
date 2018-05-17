using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public abstract partial class BaseQueueManager<TIncoming,TOutgoing>
        where TIncoming: IIncomingMessage
    {
        public virtual async Task<TIncoming> Consume(CancellationToken cancellation = default)
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