using System.Data.SqlClient;

public static class TestConnection
{
    public static string ConnectionString = @"Data Source=.\SQLExpress;Database=MessageHttpPassThroughTests; Integrated Security=True;Max Pool Size=100;MultipleActiveResultSets=True";

    static TestConnection()
    {
        EnsureDatabaseExists();
    }

    public static SqlConnection OpenConnection()
    {
        var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    static void EnsureDatabaseExists()
    {
        var builder = new SqlConnectionStringBuilder(ConnectionString);
        var database = builder.InitialCatalog;

        var masterConnection = ConnectionString.Replace(builder.InitialCatalog, "master");

        using (var connection = new SqlConnection(masterConnection))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $@"
if(db_id('{database}') is null)
    create database [{database}]
";
                command.ExecuteNonQuery();
            }
        }
    }
}