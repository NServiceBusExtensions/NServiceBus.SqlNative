using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Sender
    {
        public virtual async Task<long> Send(OutgoingMessage message, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(message, nameof(message));
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                var parameters = command.Parameters;
                command.CommandText = string.Format(Sql, table);
                parameters.Add("Id", SqlDbType.UniqueIdentifier).Value = message.Id;
                parameters.Add("CorrelationId", SqlDbType.VarChar).SetValueOrDbNull(message.CorrelationId);
                parameters.Add("ReplyToAddress", SqlDbType.VarChar).SetValueOrDbNull(message.ReplyToAddress);
                parameters.Add("Expires", SqlDbType.DateTime).SetValueOrDbNull(message.Expires);
                parameters.Add("Headers", SqlDbType.NVarChar).Value = message.Headers;
                parameters.Add("Body", SqlDbType.VarBinary).SetValueOrDbNull(message.Body);

                var rowVersion = await command.ExecuteScalarAsync(cancellation).ConfigureAwait(false);
                return (long) rowVersion;
            }
        }
    }
}