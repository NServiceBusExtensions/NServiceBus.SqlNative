using Microsoft.Data.SqlClient;

public static class ConnectionHelpers
{
    #region ConnectionHelpers

    public static async Task<SqlConnection> OpenConnection(
        string connectionString,
        Cancellation cancellation)
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

    public static async Task<SqlTransaction> BeginTransaction(
        string connectionString,
        Cancellation cancellation)
    {
        var connection = await OpenConnection(connectionString, cancellation);
        return connection.BeginTransaction();
    }

    #endregion
}