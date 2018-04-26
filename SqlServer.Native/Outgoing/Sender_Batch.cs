using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Sender
    {
        public virtual async Task<long> Send(string connection, IEnumerable<OutgoingMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNull(messages, nameof(messages));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerSend(sqlConnection, null, messages, cancellation).ConfigureAwait(false);
            }
        }

        public virtual Task<long> Send(SqlConnection connection, IEnumerable<OutgoingMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(messages, nameof(messages));
            return InnerSend(connection, null, messages, cancellation);
        }

        public virtual Task<long> Send(SqlTransaction transaction, IEnumerable<OutgoingMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNull(messages, nameof(messages));
            return InnerSend(transaction.Connection, transaction, messages, cancellation);
        }

        Task<long> InnerSend(SqlConnection connection, SqlTransaction transaction, IEnumerable<OutgoingMessage> messages, CancellationToken cancellation)
        {
            return TransactionWrapper.Run(connection, transaction, sqlTransaction => InnerSend(sqlTransaction, messages, cancellation));
        }

        async Task<long> InnerSend(SqlTransaction transaction, IEnumerable<OutgoingMessage> messages, CancellationToken cancellation)
        {
            long rowVersion = 0;
            using (var command = transaction.Connection.CreateCommand())
            {
                command.Transaction = transaction;
                var parameters = command.Parameters;
                command.CommandText = string.Format(Sql, table);
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