using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

public static class Connection
{
    public static string ConnectionString;

    static Connection()
    {
        if (Environment.GetEnvironmentVariable("APPVEYOR") == "True")
        {
            ConnectionString = @"Server=(local)\SQL2017;Database=master;User ID=sa;Password=Password12!";
            return;
        }

        var connectionEnvironmentVariable = Environment.GetEnvironmentVariable("attachmentconnection");
        if (connectionEnvironmentVariable != null)
        {
            ConnectionString = connectionEnvironmentVariable;
            IsUsingEnvironmentVariable = true;
            return;
        }

        ConnectionString = @"Data Source=.\SQLExpress;Database=NServiceBusNativeTests; Integrated Security=True;Max Pool Size=100";
    }

    public static bool IsUsingEnvironmentVariable;

    public static SqlConnection OpenConnection()
    {
        var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    public static async Task<SqlConnection> OpenAsyncConnection(CancellationToken cancellation)
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellation);
        return connection;
    }

    public static SqlConnection NewConnection()
    {
        return new SqlConnection(ConnectionString);
    }
}