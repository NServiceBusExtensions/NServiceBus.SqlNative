using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

#if (SqlServerDedupe)
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
        public static async Task<DbConnection> OpenConnection(string connectionString, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connectionString, nameof(connectionString));

            var connection = new SqlConnection(connectionString);
            try
            {
                await connection.OpenAsync(cancellation);
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        public static async Task<DbTransaction> BeginTransaction(string connectionString, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connectionString, nameof(connectionString));

            var connection = await OpenConnection(connectionString, cancellation);
            return connection.BeginTransaction();
        }

        internal static string WrapInNoCount(string sql)
        {
            return $@"
declare @nocount varchar(3) = 'off';
if ( (512 & @@options) = 512 ) set @nocount = 'on'
set nocount on;

{sql}

if (@nocount = 'on') set nocount on;
if (@nocount = 'off') set nocount off;";
        }

        internal static Task DropTable(this DbConnection connection, DbTransaction transaction, Table table, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(table, nameof(table));
            return connection.ExecuteCommand(transaction, $"drop table if exists {table}", cancellation);
        }
    }
}