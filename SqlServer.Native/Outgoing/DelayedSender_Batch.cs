using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{
    public partial class DelayedSender
    {
        public virtual async Task Send(string connection, IEnumerable<OutgoingDelayedMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNull(messages, nameof(messages));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                await InnerSend(sqlConnection, null, messages, cancellation).ConfigureAwait(false);
            }
        }

        public virtual Task Send(SqlConnection connection, IEnumerable<OutgoingDelayedMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(messages, nameof(messages));
            return InnerSend(connection, null, messages, cancellation);
        }

        public virtual Task Send(SqlTransaction transaction, IEnumerable<OutgoingDelayedMessage> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNull(messages, nameof(messages));
            return InnerSend(transaction.Connection, transaction, messages, cancellation);
        }

        async Task InnerSend(SqlConnection connection, SqlTransaction transaction, IEnumerable<OutgoingDelayedMessage> messages, CancellationToken cancellation)
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
                    var dueParam = parameters.Add("Due", SqlDbType.DateTime);
                    var headersParam = parameters.Add("Headers", SqlDbType.NVarChar);
                    var bodyParam = parameters.Add("Body", SqlDbType.VarBinary);
                    foreach (var message in messages)
                    {
                        cancellation.ThrowIfCancellationRequested();
                        dueParam.Value = message.Due;
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