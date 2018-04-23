using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{
    public partial class Receiver
    {
        public virtual Task<int> Receive(string connection, int batchSize, Action<Message> action, CancellationToken cancellation = default)
        {
            return Receive(connection, batchSize, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<int> Receive(string connection, int batchSize, Func<Message, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNull(action, nameof(action));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerReceive(sqlConnection, null, batchSize, action, cancellation).ConfigureAwait(false);
            }
        }

        public virtual Task<int> Receive(SqlConnection connection, int batchSize, Action<Message> action, CancellationToken cancellation = default)
        {
            return Receive(connection, batchSize, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<int> Receive(SqlConnection connection, int batchSize, Func<Message, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNull(action, nameof(action));
            return InnerReceive(connection, null, batchSize, action, cancellation);
        }

        public virtual Task<int> Receive(SqlTransaction transaction, int batchSize, Action<Message> action, CancellationToken cancellation = default)
        {
            return Receive(transaction, batchSize, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<int> Receive(SqlTransaction transaction, int batchSize, Func<Message, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            return InnerReceive(transaction.Connection, transaction, batchSize, action, cancellation);
        }

        async Task<int> InnerReceive(SqlConnection connection, SqlTransaction transaction, int batchSize, Func<Message, Task> action, CancellationToken cancellation)
        {
            var count = 0;
            using (var command = BuildCommand(connection, transaction, batchSize))
            using (var dataReader = await command.ExecuteSequentialReader(cancellation).ConfigureAwait(false))
            {
                while (dataReader.Read())
                {
                    count++;
                    cancellation.ThrowIfCancellationRequested();
                    var message = await ReadMessage(cancellation, dataReader).ConfigureAwait(false);
                    await action(message).ConfigureAwait(false);
                }
            }

            return count;
        }
    }
}