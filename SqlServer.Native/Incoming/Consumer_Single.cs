using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Transport.SqlServerNative
{
    public partial class Consumer
    {
        public virtual async Task<IncomingBytesMessage> Consume(string connection, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                return await InnerConsume(sqlConnection, null, cancellation, MessageReader.ReadBytesMessage).ConfigureAwait(false);
            }
        }

        public virtual Task<IncomingBytesMessage> Consume(SqlConnection connection, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(connection, nameof(connection));
            return InnerConsume(connection, null, cancellation, MessageReader.ReadBytesMessage);
        }

        public virtual Task<IncomingBytesMessage> Consume(SqlTransaction transaction, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(transaction, nameof(transaction));
            return InnerConsume(transaction.Connection, transaction, cancellation, MessageReader.ReadBytesMessage);
        }

        async Task<T> InnerConsume<T>(SqlConnection connection, SqlTransaction transaction, CancellationToken cancellation, Func<SqlDataReader, T> func)
            where T : class
        {
            using (var command = BuildCommand(connection, transaction, 1))
            {
                return await command.ReadSingle(cancellation, func);
            }
        }
    }
}