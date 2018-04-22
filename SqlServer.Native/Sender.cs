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
        public virtual async Task Send(string connection, string table, IEnumerable<Message> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(messages, nameof(messages));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation);
                var transaction = sqlConnection.BeginTransaction();
                try
                {
                    await InnerSend(sqlConnection, transaction, table, messages, cancellation);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public virtual Task Send(SqlConnection connection, string table, IEnumerable<Message> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(messages, nameof(messages));
            return InnerSend(connection, null, table, messages, cancellation);
        }

        public virtual Task Send(SqlConnection connection, SqlTransaction transaction, string table, IEnumerable<Message> messages, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(messages, nameof(messages));
            return InnerSend(connection, transaction, table, messages, cancellation);
        }

        static async Task InnerSend(SqlConnection connection, SqlTransaction transaction, string table, IEnumerable<Message> messages, CancellationToken cancellation)
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

        public virtual async Task Send(string connection, string table, Message message, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(message, nameof(message));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation);
                await InnerSend(sqlConnection, null, message, table, cancellation);
            }
        }

        public virtual Task Send(SqlConnection connection, string table, Message message, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            Guard.AgainstNull(message, nameof(message));
            return InnerSend(connection, null, message, table, cancellation);
        }

        public virtual Task Send(SqlConnection connection, SqlTransaction transaction, string table, Message message, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(message, nameof(message));
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return InnerSend(connection, transaction, message, table, cancellation);
        }

        static async Task InnerSend(SqlConnection connection, SqlTransaction transaction, Message message, string table, CancellationToken cancellation)
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

                await command.ExecuteNonQueryAsync(cancellation).ConfigureAwait(false);
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