using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

public static class SnippetConnectionHelpers
{
    #region ConnectionHelpers

    public static async Task<DbConnection> OpenConnection(string connectionString, CancellationToken cancellation)
    {
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

    public static async Task<DbTransaction> BeginTransaction(string connectionString, CancellationToken cancellation)
    {
        var connection = await OpenConnection(connectionString, cancellation);
        return connection.BeginTransaction();
    }

    #endregion
}