using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{
    public class Receiver
    {
        public virtual async Task<Message> Receive(string connection, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation);
                return await Receive(sqlConnection, table, cancellation);
            }
        }

        public virtual Task<Message> Receive(SqlConnection connection, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return InnerReceive(connection, null, table, cancellation);
        }

        public virtual Task<Message> Receive(SqlConnection connection, SqlTransaction transaction, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return InnerReceive(connection, transaction, table, cancellation);
        }

        static async Task<Message> InnerReceive(SqlConnection connection, SqlTransaction transaction, string table, CancellationToken cancellation)
        {
            using (var command = BuildCommand(connection, transaction, table))
            using (var dataReader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false))
            {
                if (!dataReader.Read())
                {
                    return null;
                }

                return await ReadMessage(cancellation, dataReader);
            }
        }

        public virtual Task Receive(string connection, string table, Action<Message> action, CancellationToken cancellation = default)
        {
            return Receive(connection, table,
                message =>
                {
                    action(message);
                    return Task.CompletedTask;
                },
                cancellation);
        }

        public virtual async Task Receive(string connection, string table, Func<Message, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation);
                await InnerReceive(sqlConnection, null, table, action, cancellation);
            }
        }

        public virtual Task Receive(SqlConnection connection, string table, Action<Message> action, CancellationToken cancellation = default)
        {
            return Receive(connection, table,
                message =>
                {
                    action(message);
                    return Task.CompletedTask;
                },
                cancellation);
        }

        public virtual Task Receive(SqlConnection connection, string table, Func<Message, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return InnerReceive(connection, null, table, action, cancellation);
        }

        public virtual Task Receive(SqlConnection connection, SqlTransaction transaction, string table, Action<Message> action, CancellationToken cancellation = default)
        {
            return Receive(connection, transaction, table,
                message =>
                {
                    action(message);
                    return Task.CompletedTask;
                },
                cancellation);
        }

        public virtual Task Receive(SqlConnection connection, SqlTransaction transaction, string table, Func<Message, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return InnerReceive(connection, transaction, table, action, cancellation);
        }

        static async Task InnerReceive(SqlConnection connection, SqlTransaction transaction, string table, Func<Message, Task> action, CancellationToken cancellation)
        {
            using (var command = BuildCommand(connection, transaction, table))
            using (var dataReader = await command.ExecuteSequentialReader(cancellation).ConfigureAwait(false))
            {
                while (dataReader.Read())
                {
                    cancellation.ThrowIfCancellationRequested();
                    var message = await ReadMessage(cancellation, dataReader);
                    await action(message).ConfigureAwait(false);
                }
            }
        }

        static SqlCommand BuildCommand(SqlConnection connection, SqlTransaction transaction, string table)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = string.Format(ReceiveSql, table);
            return command;
        }

        static async Task<Message> ReadMessage(CancellationToken cancellation, SqlDataReader dataReader)
        {
            return new Message(
                id: await dataReader.GetFieldValueAsync<Guid>(0, cancellation).ConfigureAwait(false),
                correlationId: await dataReader.ValueOrNull<string>(1, cancellation).ConfigureAwait(false),
                replyToAddress: await dataReader.ValueOrNull<string>(2, cancellation).ConfigureAwait(false),
                expires: await dataReader.ValueOrNull<DateTime>(3, cancellation).ConfigureAwait(false),
                headers: await dataReader.ValueOrNull<string>(4, cancellation).ConfigureAwait(false),
                body: await dataReader.ValueOrNull<byte[]>(5, cancellation).ConfigureAwait(false)
            );
        }

        public static readonly string ReceiveSql = @"
declare @nocount varchar(3) = 'off';
if ( (512 & @@options) = 512 ) set @nocount = 'on';
set nocount on;

with message as (
    select top(1) *
    from {0} with (updlock, readpast, rowlock)
    order by RowVersion)
delete from message
output
    deleted.Id,
    deleted.CorrelationId,
    deleted.ReplyToAddress,
    deleted.Expires,
    deleted.Headers,
    deleted.Body;

if (@nocount = 'on') set nocount on;
if (@nocount = 'off') set nocount off;";
    }
}