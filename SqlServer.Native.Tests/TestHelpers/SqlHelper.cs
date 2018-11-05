﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;

static class SqlHelper
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

    public static async Task<IEnumerable<IncomingVerifyTarget>> ReadData(string table, SqlConnection connection)
    {
        var reader = new QueueManager(table, connection);
        var messages = new ConcurrentBag<IncomingVerifyTarget>();
        await reader.Read(size: 10,
            startRowVersion: 1,
            action: message => { messages.Add(message.ToVerifyTarget()); });
        return messages.OrderBy(x => x.Id);
    }

    public static async Task<IOrderedEnumerable<IncomingDelayedVerifyTarget>> ReadDelayedData(string table, SqlConnection connection)
    {
        var reader = new DelayedQueueManager(table, connection);
        var messages = new ConcurrentBag<IncomingDelayedVerifyTarget>();
        await reader.Read(size: 10,
            startRowVersion: 1,
            action: message => { messages.Add(message.ToVerifyTarget()); });
        return messages.OrderBy(x => x.Due);
    }

    public static IEnumerable<IDictionary<string, object>> ReadDuplicateData(string table, SqlConnection conn)
    {
        using (var command = conn.CreateCommand())
        {
            command.CommandText = $"SELECT Id FROM {table}";
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