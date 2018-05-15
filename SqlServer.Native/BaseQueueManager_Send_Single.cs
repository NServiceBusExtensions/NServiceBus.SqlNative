using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
        where TIncoming : IIncomingMessage
    {
        public virtual async Task<long> Send(TOutgoing message, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(message, nameof(message));
            using (var command = CreateSendCommand(message))
            {
                var rowVersion = await command.ExecuteScalarAsync(cancellation).ConfigureAwait(false);
                if (rowVersion == null)
                {
                    return 0;
                }

                return (long)rowVersion;
            }
        }

        protected abstract SqlCommand CreateSendCommand(TOutgoing message);
    }
}