using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        public virtual async Task<IncomingDelayedBytesMessage> ConsumeBytes(CancellationToken cancellation = default)
        {
            using (var command = BuildConsumeCommand(1))
            using (var reader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false))
            {
                return await reader.ReadSingleDelayedBytes(cancellation);
            }
        }
    }
}