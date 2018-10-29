using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

#if (SqlServerDeduplication)
namespace NServiceBus.Transport.SqlServerDeduplication
#else
namespace NServiceBus.Transport.SqlServerNative
#endif
{
    /// <summary>
    /// ConnectionHelpers.
    /// </summary>
    public static class ConnectionHelpers
    {
        public static async Task<SqlConnection> OpenConnection(string connectionString, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connectionString, nameof(connectionString));

            var connection = new SqlConnection(connectionString);
            try
            {
                await connection.OpenAsync(cancellation).ConfigureAwait(false);
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        public static async Task<SqlTransaction> BeginTransaction(string connectionString, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connectionString, nameof(connectionString));

            var connection = await OpenConnection(connectionString, cancellation).ConfigureAwait(false);
            return connection.BeginTransaction();
        }

        internal static Task DropTable(this SqlConnection connection, SqlTransaction transaction, Table table, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(table, nameof(table));
            return connection.ExecuteCommand(transaction, $"drop table if exists {table}", cancellation);
        }
    }
}