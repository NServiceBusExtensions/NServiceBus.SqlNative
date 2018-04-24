using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServer.Native
{
    /// <summary>
    /// SqlHelpers.
    /// </summary>
    public static class SqlHelpers
    {
        /// <summary>
        /// Drops a table.
        /// </summary>
        public static async Task Drop(string connection, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(connection, nameof(connection));
            Guard.AgainstNullOrEmpty(table, nameof(table));
            using (var sqlConnection = new SqlConnection(connection))
            {
                await sqlConnection.OpenAsync(cancellation).ConfigureAwait(false);
                await Drop(sqlConnection, null, table, cancellation).ConfigureAwait(false);
            }
        }

        public static async Task<SqlConnection> OpenConnection(string connectionString, CancellationToken cancellation = default)
        {
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

        /// <summary>
        /// Drops a table.
        /// </summary>
        public static Task Drop(SqlConnection connection, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return Drop(connection, null, table, cancellation);
        }

        /// <summary>
        /// Drops a table.
        /// </summary>
        public static Task Drop(SqlConnection connection, SqlTransaction transaction, string table, CancellationToken cancellation = default)
        {
            Guard.AgainstNullOrEmpty(table, nameof(table));
            return connection.ExecuteCommand(transaction, $"drop table if exists {table}", cancellation);
        }
    }
}