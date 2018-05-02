using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Reader
    {
        public virtual Task<IncomingBytesMessage> ReadBytes(SqlConnection connection, long rowVersion, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
            return InnerReadBytes(connection, rowVersion, cancellation);
        }

        async Task<IncomingBytesMessage> InnerReadBytes(SqlConnection connection, long rowVersion, CancellationToken cancellation)
        {
            using (var command = BuildCommand(connection, 1, rowVersion))
            using (var reader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false))
            {
                return await reader.ReadSingleBytes(cancellation);
            }
        }
    }
}