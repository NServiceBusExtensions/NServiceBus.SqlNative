using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{
    public class Receiver
    {
        string table;

        public Receiver(string table)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            this.table = table;
        }

        public virtual async Task<Message> Receive(string connection, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerReceive(sqlConnection, null, cancellation).ConfigureAwait(false);
            }
        }

        public virtual Task<Message> Receive(SqlConnection connection, SqlTransaction transaction = null, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            return InnerReceive(connection, transaction, cancellation);
        }

        async Task<Message> InnerReceive(SqlConnection connection, SqlTransaction transaction, CancellationToken cancellation)
        {
            using (var command = BuildCommand(connection, transaction, 1))
            using (var dataReader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false))
            {
                if (!await dataReader.ReadAsync(cancellation).ConfigureAwait(false))
                {
                    return null;
                }

                return await ReadMessage(cancellation, dataReader).ConfigureAwait(false);
            }
        }

        public virtual Task Receive(string connection, int batchSize, Action<Message> action, CancellationToken cancellation = default)
        {
            return Receive(connection, batchSize,
                message =>
                {
                    action(message);
                    return Task.CompletedTask;
                },
                cancellation);
        }

        public virtual async Task Receive(string connection, int batchSize, Func<Message, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNull(action, nameof(action));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                await InnerReceive(sqlConnection, null, batchSize, action, cancellation).ConfigureAwait(false);
            }
        }

        public virtual Task Receive(SqlConnection connection, int batchSize, Action<Message> action, CancellationToken cancellation = default)
        {
            return Receive(connection, batchSize,
                message =>
                {
                    action(message);
                    return Task.CompletedTask;
                },
                cancellation);
        }

        public virtual Task Receive(SqlConnection connection, int batchSize, Func<Message, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(action, nameof(action));
            return InnerReceive(connection, null, batchSize, action, cancellation);
        }

        public virtual Task Receive(SqlConnection connection, int batchSize, Action<Message> action, SqlTransaction transaction = null, CancellationToken cancellation = default)
        {
            return Receive(connection, transaction,  batchSize,
                message =>
                {
                    action(message);
                    return Task.CompletedTask;
                },
                cancellation);
        }

        public virtual Task Receive(SqlConnection connection, SqlTransaction transaction, int batchSize, Func<Message, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(transaction, nameof(transaction));
            return InnerReceive(connection, transaction, batchSize, action, cancellation);
        }

        async Task InnerReceive(SqlConnection connection, SqlTransaction transaction, int batchSize, Func<Message, Task> action, CancellationToken cancellation)
        {
            using (var command = BuildCommand(connection, transaction, batchSize))
            using (var dataReader = await command.ExecuteSequentialReader(cancellation).ConfigureAwait(false))
            {
                while (dataReader.Read())
                {
                    cancellation.ThrowIfCancellationRequested();
                    var message = await ReadMessage(cancellation, dataReader).ConfigureAwait(false);
                    await action(message).ConfigureAwait(false);
                }
            }
        }

        SqlCommand BuildCommand(SqlConnection connection, SqlTransaction transaction, int batchSize)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = string.Format(ReceiveSql, table, batchSize);
            return command;
        }

        static async Task<Message> ReadMessage(CancellationToken cancellation, SqlDataReader dataReader)
        {
            return new Message(
                id: await dataReader.GetFieldValueAsync<Guid>(0, cancellation).ConfigureAwait(false),
                correlationId: await dataReader.ValueOrNull<string>(1, cancellation).ConfigureAwait(false),
                replyToAddress: await dataReader.ValueOrNull<string>(2, cancellation).ConfigureAwait(false),
                expires: await dataReader.ValueOrNull<DateTime?>(3, cancellation).ConfigureAwait(false),
                headers: await dataReader.ValueOrNull<string>(4, cancellation).ConfigureAwait(false),
                body: await dataReader.ValueOrNull<byte[]>(5, cancellation).ConfigureAwait(false)
            );
        }

        public static readonly string ReceiveSql = @"
declare @nocount varchar(3) = 'off';
if ( (512 & @@options) = 512 ) set @nocount = 'on';
set nocount on;

with message as (
    select top({1}) *
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