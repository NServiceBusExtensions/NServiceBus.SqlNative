using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{
    public class Sender
    {
        string table;

        public Sender(string table)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            this.table = table;
        }

        public virtual async Task Send(string connection, IEnumerable<Message> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNull(messages, nameof(messages));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                var transaction = sqlConnection.BeginTransaction();
                try
                {
                    await InnerSend(sqlConnection, transaction, messages, cancellation).ConfigureAwait(false);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public virtual Task Send(SqlConnection connection, IEnumerable<Message> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(messages, nameof(messages));
            return InnerSend(connection, null,  messages, cancellation);
        }

        public virtual Task Send(SqlConnection connection, SqlTransaction transaction,  IEnumerable<Message> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNull(messages, nameof(messages));
            return InnerSend(connection, transaction, messages, cancellation);
        }

        async Task InnerSend(SqlConnection connection, SqlTransaction transaction, IEnumerable<Message> messages, CancellationToken cancellation)
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
                    expiresParam.Value = message.Expires;
                    headersParam.Value = message.Headers;
                    bodyParam.Value = message.Body;
                    await command.ExecuteNonQueryAsync(cancellation).ConfigureAwait(false);
                }
            }
        }

        public virtual async Task<long> Send(string connection, Message message, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNull(message, nameof(message));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                await InnerSend(sqlConnection, null, message, cancellation).ConfigureAwait(false);
            }
        }

        public virtual Task<long> Send(SqlConnection connection, Message message, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(message, nameof(message));
            return InnerSend(connection, null, message, cancellation);
        }

        public virtual Task<long> Send(SqlConnection connection, SqlTransaction transaction, Message message, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(message, nameof(message));
            Guard.AgainstNull(transaction, nameof(transaction));
            return InnerSend(connection, transaction, message,  cancellation);
        }

        async Task<long> InnerSend(SqlConnection connection, SqlTransaction transaction, Message message, CancellationToken cancellation)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                var parameters = command.Parameters;
                command.CommandText = string.Format(Sql, table);
                parameters.Add("Id", SqlDbType.UniqueIdentifier).Value = message.Id;
                parameters.Add("CorrelationId", SqlDbType.VarChar).SetValueOrDbNull(message.CorrelationId);
                parameters.Add("ReplyToAddress", SqlDbType.VarChar).SetValueOrDbNull(message.ReplyToAddress);
                parameters.Add("Expires", SqlDbType.DateTime).Value = message.Expires;
                parameters.Add("Headers", SqlDbType.NVarChar).Value = message.Headers;
                parameters.Add("Body", SqlDbType.VarBinary).Value = message.Body;

                var rowVersion = await command.ExecuteScalarAsync(cancellation).ConfigureAwait(false);
                return (long)rowVersion;
            }
        }

        public static readonly string Sql =
            @"
declare @nocount varchar(3) = 'off';
if ( (512 & @@options) = 512 ) set @nocount = 'on'
set nocount on;

insert into {0} (
    Id,
    CorrelationId,
    ReplyToAddress,
    Recoverable,
    Expires,
    Headers,
    Body)
output inserted.RowVersion
values (
    @Id,
    @CorrelationId,
    @ReplyToAddress,
    1,
    @Expires,
    @Headers,
    @Body);

if (@nocount = 'on') set nocount on;
if (@nocount = 'off') set nocount off;";
    }
}