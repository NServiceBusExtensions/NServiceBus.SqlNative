using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{
    public partial class Finder
    {
        public virtual Task<int> Find(string connection, int size, long startRowVersion, Action<Message> action, CancellationToken cancellation = default)
        {
            return Find(connection, size, startRowVersion, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<int> Find(string connection, int size, long startRowVersion, Func<Message, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNull(action, nameof(action));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerFind(sqlConnection, null, size, startRowVersion, action, cancellation).ConfigureAwait(false);
            }
        }

        public virtual Task<int> Find(SqlConnection connection, int size, long startRowVersion, Action<Message> action, CancellationToken cancellation = default)
        {
            return Find(connection, size, startRowVersion, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<int> Find(SqlConnection connection, int size, long startRowVersion, Func<Message, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNull(action, nameof(action));
            return InnerFind(connection, null, size, startRowVersion, action, cancellation);
        }

        public virtual Task<int> Find(SqlTransaction transaction, int size, long startRowVersion, Action<Message> action, CancellationToken cancellation = default)
        {
            return Find(transaction, size, startRowVersion, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<int> Find(SqlTransaction transaction, int size, long startRowVersion, Func<Message, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNull(action, nameof(action));
            return InnerFind(transaction.Connection, transaction, size, startRowVersion, action, cancellation);
        }

        async Task<int> InnerFind(SqlConnection connection, SqlTransaction transaction, int size, long startRowVersion, Func<Message, Task> action, CancellationToken cancellation)
        {
            var count = 0;
            using (var command = BuildCommand(connection, transaction, size, startRowVersion))
            using (var reader = await command.ExecuteSequentialReader(cancellation).ConfigureAwait(false))
            {
                while (reader.Read())
                {
                    count++;
                    cancellation.ThrowIfCancellationRequested();
                    var message = await reader.ReadMessage(cancellation).ConfigureAwait(false);
                    await action(message).ConfigureAwait(false);
                }
            }

            return count;
        }
    }
}