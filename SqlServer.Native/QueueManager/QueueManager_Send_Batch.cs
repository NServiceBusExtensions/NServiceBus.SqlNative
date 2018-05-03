using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class QueueManager
    {
        public virtual async Task<long> Send(IEnumerable<OutgoingMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(messages, nameof(messages));
            long rowVersion = 0;
            using (var command = connection.CreateCommand(transaction, string.Format(SendSql, table)))
            {
                var parameters = command.Parameters;
                var idParam = parameters.Add("Id", SqlDbType.UniqueIdentifier);
                var corrParam = parameters.Add("CorrelationId", SqlDbType.VarChar);
                var replyParam = parameters.Add("ReplyToAddress", SqlDbType.VarChar);
                var expiresParam = parameters.Add("Expires", SqlDbType.DateTime);
                var headersParam = parameters.Add("Headers", SqlDbType.NVarChar);
                var bodyParam = parameters.Add("Body", SqlDbType.VarBinary);
                foreach (var message in messages)
                {
                    cancellation.ThrowIfCancellationRequested();
                    idParam.Value = message.Id;
                    corrParam.SetValueOrDbNull(message.CorrelationId);
                    replyParam.SetValueOrDbNull(message.ReplyToAddress);
                    expiresParam.SetValueOrDbNull(message.Expires);
                    headersParam.Value = message.Headers;
                    bodyParam.SetValueOrDbNull(message.Body);
                    rowVersion = (long) await command.ExecuteScalarAsync(cancellation).ConfigureAwait(false);
                }
            }

            return rowVersion;
        }
    }
}