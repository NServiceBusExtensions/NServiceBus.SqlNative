using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Sender
    {
        public virtual Task<long> Send(SqlConnection connection, OutgoingMessage message, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(message, nameof(message));
            return InnerSend(connection, null, message, cancellation);
        }

        public virtual Task<long> Send(SqlTransaction transaction, OutgoingMessage message, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(message, nameof(message));
            Guard.AgainstNull(transaction, nameof(transaction));
            return InnerSend(transaction.Connection, transaction, message, cancellation);
        }

        async Task<long> InnerSend(SqlConnection connection, SqlTransaction transaction, OutgoingMessage message, CancellationToken cancellation)
        {
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