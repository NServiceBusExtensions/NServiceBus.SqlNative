using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        public virtual async Task<long> Send(OutgoingDelayedMessage message, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(message, nameof(message));
            using (var command = connection.CreateCommand(transaction, string.Format(SendSql, table)))
            {
                var parameters = command.Parameters;
                parameters.Add("Due", SqlDbType.DateTime).Value = message.Due;
                parameters.Add("Headers", SqlDbType.NVarChar).Value = message.Headers;
                parameters.Add("Body", SqlDbType.VarBinary).SetBinaryOrDbNull(message.Body);

                var rowVersion = await command.ExecuteScalarAsync(cancellation).ConfigureAwait(false);
                return (long) rowVersion;
            }
        }
    }
}