using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Reader
    {
        public virtual Task<IncomingResult> Read(string connection, int size, long startRowVersion, Action<IncomingMessage> action, CancellationToken cancellation = default)
        {
            return Read(connection, size, startRowVersion, action.ToTaskFunc(), cancellation);
        }

        public virtual async Task<IncomingResult> Read(string connection, int size, long startRowVersion, Func<IncomingMessage, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNull(action, nameof(action));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerFind(sqlConnection, size, startRowVersion, action, cancellation).ConfigureAwait(false);
            }
        }

        public virtual Task<IncomingResult> Read(SqlConnection connection, int size, long startRowVersion, Action<IncomingMessage> action, CancellationToken cancellation = default)
        {
            return Read(connection, size, startRowVersion, action.ToTaskFunc(), cancellation);
        }

        public virtual Task<IncomingResult> Read(SqlConnection connection, int size, long startRowVersion, Func<IncomingMessage, Task> action, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            Guard.AgainstNegativeAndZero(size, nameof(size));
            Guard.AgainstNegativeAndZero(startRowVersion, nameof(startRowVersion));
            Guard.AgainstNull(action, nameof(action));
            return InnerFind(connection, size, startRowVersion, action, cancellation);
        }

        async Task<IncomingResult> InnerFind(SqlConnection connection, int size, long startRowVersion, Func<IncomingMessage, Task> action, CancellationToken cancellation)
        {
            var count = 0;
            long? lastRowVersion =null;
            using (var command = BuildCommand(connection, size, startRowVersion))
            using (var reader = await command.ExecuteSequentialReader(cancellation).ConfigureAwait(false))
            {
                while (reader.Read())
                {
                    count++;
                    cancellation.ThrowIfCancellationRequested();
                    var message = await reader.ReadMessage(cancellation).ConfigureAwait(false);
                    lastRowVersion = message.RowVersion;
                    await action(message).ConfigureAwait(false);
                }
            }

            return new IncomingResult
            {
                Count = count,
                LastRowVersion = lastRowVersion
            };
        }
    }
}