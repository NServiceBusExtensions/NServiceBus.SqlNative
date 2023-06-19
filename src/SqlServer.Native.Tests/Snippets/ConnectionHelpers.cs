using Microsoft.Data.SqlClient;

public static class ConnectionHelpers
{
    #region ConnectionHelpers

    public static async Task<SqlConnection> OpenConnection(
        string connectionString,
        Cancel cancel)
    {
        var connection = new SqlConnection(connectionString);
        try
        {
            await connection.OpenAsync(cancel);
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
        Cancel cancel)
    {
        var connection = await OpenConnection(connectionString, cancel);
        return connection.BeginTransaction();
    }

    #endregion
}