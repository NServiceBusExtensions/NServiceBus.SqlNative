using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Newtonsoft.Json;

public static class SqlHelper
{
    public static void EnsureDatabaseExists(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var database = builder.InitialCatalog;

        var masterConnection = connectionString.Replace(builder.InitialCatalog, "master");

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

    public static IEnumerable<IDictionary<string, object>> ReadData(string table)
    {
        using (var conn = new SqlConnection(Connection.ConnectionString))
        {
            conn.Open();
            using (var command = conn.CreateCommand())
            {
                command.CommandText = $"SELECT * FROM {table}";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        IDictionary<string, object> record = new Dictionary<string, object>();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            record.Add(reader.GetName(i), reader[i]);
                        }
                        yield return record;
                    }
                }
            }
        }
    }
}