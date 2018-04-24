using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{
    public partial class Sender
    {
        public virtual async Task Send(string connection, IEnumerable<OutgoingMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNull(messages, nameof(messages));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                await InnerSend(sqlConnection, null, messages, cancellation).ConfigureAwait(false);
            }
        }

        public virtual Task Send(SqlConnection connection, IEnumerable<OutgoingMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(messages, nameof(messages));
            return InnerSend(connection, null, messages, cancellation);
        }

        public virtual Task Send(SqlTransaction transaction, IEnumerable<OutgoingMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNull(messages, nameof(messages));
            return InnerSend(transaction.Connection, transaction, messages, cancellation);
        }

        async Task InnerSend(SqlConnection connection, SqlTransaction transaction, IEnumerable<OutgoingMessage> messages, CancellationToken cancellation)
        {
            var ownsTransaction = false;
            if (transaction == null)
            {
                ownsTransaction = true;
                transaction = connection.BeginTransaction();
            }

            try
            {
                using (var command = connection.CreateCommand())
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
                        await command.ExecuteNonQueryAsync(cancellation).ConfigureAwait(false);
                    }
                }
                if (ownsTransaction)
                {
                    transaction.Commit();
                }
            }
            catch (Exception)
            {
                if (ownsTransaction)
                {
                    transaction.Rollback();
                }

                throw;
            }
            finally
            {
                if (ownsTransaction)
                {
                    transaction.Dispose();
                }
            }
        }
    }
}