using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Reader
    {
        public virtual async Task<IncomingBytesMessage> ReadBytes(long rowVersion, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            using (var command = BuildCommand(1, rowVersion))
            using (var reader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false))
            {
                return await reader.ReadSingleBytes(cancellation);
            }
        }
    }
}