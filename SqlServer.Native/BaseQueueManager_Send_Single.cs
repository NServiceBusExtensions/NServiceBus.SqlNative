using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public abstract partial class BaseQueueManager<TIncoming, TOutgoing>
        where TIncoming : IIncomingMessage
    {
        public virtual Task<long> Send(TOutgoing message, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(message, nameof(message));
            return InnerSend(message, cancellation);
        }

        async Task<long> InnerSend(TOutgoing message, CancellationToken cancellation)
        {
            using (var command = CreateSendCommand(message))
            {
                var rowVersion = await command.ExecuteScalarAsync(cancellation).ConfigureAwait(false);
                if (rowVersion == null)
                {
                    return 0;
                }

                return (long) rowVersion;
            }
        }

        protected abstract SqlCommand CreateSendCommand(TOutgoing message);
    }
}