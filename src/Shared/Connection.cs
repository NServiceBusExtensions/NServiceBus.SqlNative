using Microsoft.Data.SqlClient;

public static class Connection
{
    public static string ConnectionString;

    static Connection()
    {
        if (Environment.GetEnvironmentVariable("AppVeyor") == "True")
        {
            ConnectionString = @"Server=(local)\SQL2017;Database=master;User ID=sa;Password=Password12!;Max Pool Size=100;TrustServerCertificate=True";
            return;
        }

        ConnectionString = @"Server=.\;Database=NServiceBusNativeTests; Integrated Security=True;Max Pool Size=100;TrustServerCertificate=True";
    }

    public static SqlConnection OpenConnection()
    {
        var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    public static SqlConnection OpenConnectionFromNewClient()
    {
        var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    public static async Task<SqlConnection> OpenAsyncConnection(CancellationToken cancellation = default)
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellation);
        return connection;
    }

    public static async Task<SqlConnection> OpenAsyncConnection()
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    public static SqlConnection NewConnection()
    {
        return new(ConnectionString);
    }
}