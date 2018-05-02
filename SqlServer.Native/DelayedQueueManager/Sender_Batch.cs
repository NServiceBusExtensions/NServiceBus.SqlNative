using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedQueueManager
    {
        public virtual async Task<long> Send(IEnumerable<OutgoingDelayedMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(messages, nameof(messages));
            long rowVersion = 0;
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                var parameters = command.Parameters;
                command.CommandText = string.Format(SendSql, table);
                var dueParam = parameters.Add("Due", SqlDbType.DateTime);
                var headersParam = parameters.Add("Headers", SqlDbType.NVarChar);
                var bodyParam = parameters.Add("Body", SqlDbType.VarBinary);
                foreach (var message in messages)
                {
                    cancellation.ThrowIfCancellationRequested();
                    dueParam.Value = message.Due;
                    headersParam.Value = message.Headers;
                    bodyParam.SetValueOrDbNull(message.Body);
                    rowVersion = (long) await command.ExecuteScalarAsync(cancellation).ConfigureAwait(false);
                }
            }

            return rowVersion;
        }
    }
}