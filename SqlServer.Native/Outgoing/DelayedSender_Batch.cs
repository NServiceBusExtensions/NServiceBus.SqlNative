using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class DelayedSender
    {
        public virtual Task<long> Send(SqlConnection connection, IEnumerable<OutgoingDelayedMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(messages, nameof(messages));
            return InnerSend(connection, null, messages, cancellation);
        }

        public virtual Task<long> Send(SqlTransaction transaction, IEnumerable<OutgoingDelayedMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNull(messages, nameof(messages));
            return InnerSend(transaction.Connection, transaction, messages, cancellation);
        }

        Task<long> InnerSend(SqlConnection connection, SqlTransaction transaction, IEnumerable<OutgoingDelayedMessage> messages, CancellationToken cancellation)
        {
            return TransactionWrapper.Run(sqlTransaction => InnerSend(sqlTransaction, messages, cancellation), connection, transaction);
        }

        async Task<long> InnerSend(SqlTransaction transaction, IEnumerable<OutgoingDelayedMessage> messages, CancellationToken cancellation)
        {
            long rowVersion = 0;
            using (var command = transaction.Connection.CreateCommand())
            {
                command.Transaction = transaction;
                var parameters = command.Parameters;
                command.CommandText = string.Format(Sql, table);
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