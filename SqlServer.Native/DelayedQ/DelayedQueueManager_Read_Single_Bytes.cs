using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        public virtual async Task<IncomingDelayedBytesMessage> ReadBytes(long rowVersion, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            using (var command = BuildReadCommand(1, rowVersion))
            using (var reader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false))
            {
                return await reader.ReadSingleDelayedBytes(cancellation).ConfigureAwait(false);
            }
        }
    }
}