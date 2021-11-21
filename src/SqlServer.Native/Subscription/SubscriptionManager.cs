﻿using System.Data.Common;

namespace NServiceBus.Transport.SqlServerNative;

public class SubscriptionManager
{
    DbConnection connection;
    Table table;
    DbTransaction? transaction;
    string createTableSql;
    string unsubscribeSql;
    string getSubscribersSql;
    string subscribeSql;

    public SubscriptionManager(Table table, DbConnection connection):
        this(table, null,connection)
    {
    }

    public SubscriptionManager(Table table, DbTransaction transaction) :
        this(table, transaction, transaction.Connection!)
    {
    }

    SubscriptionManager(Table table, DbTransaction? transaction, DbConnection connection)
    {
        this.transaction = transaction;
        this.table = table;
        this.connection = connection;
        createTableSql = string.Format(SubscriptionTableSql, table);
        unsubscribeSql = string.Format(UnsubscribeSql, table);
        getSubscribersSql = GetSubscribersSql.Replace("{0}", table.FullTableName);
        subscribeSql = string.Format(SubscribeSql, table);
    }

    /// <summary>
    /// Drops the table.
    /// </summary>
    public virtual Task Drop(CancellationToken cancellation = default)
    {
        return connection.DropTable(transaction, table, cancellation);
    }

    /// <summary>
    /// Creates the table.
    /// </summary>
    public virtual Task Create(CancellationToken cancellation = default)
    {
        return connection.RunCommand(transaction, createTableSql, cancellation);
    }

    /// <summary>
    /// The sql statements used to create the subscription table.
    /// </summary>
    public static readonly string SubscriptionTableSql = @"
if exists (
  select *
  from sys.objects
  where object_id = object_id('{0}')
    and type in ('U'))
return

create table {0} (
  QueueAddress nvarchar(200) not null,
  Endpoint nvarchar(200),
  Topic nvarchar(200) not null,
  primary key clustered
  (
    Endpoint,
    Topic
  )
)
";

    /// <summary>
    /// The sql statements used to add a subscription.
    /// </summary>
    public static readonly string SubscribeSql = @"
MERGE {0} WITH (HOLDLOCK, TABLOCK) AS target
USING(SELECT @Endpoint AS Endpoint, @QueueAddress AS QueueAddress, @Topic AS Topic) AS source
ON target.Endpoint = source.Endpoint
AND target.Topic = source.Topic
WHEN MATCHED AND target.QueueAddress <> source.QueueAddress THEN
UPDATE SET QueueAddress = @QueueAddress
WHEN NOT MATCHED THEN
INSERT
(
    QueueAddress,
    Topic,
    Endpoint
)
VALUES
(
    @QueueAddress,
    @Topic,
    @Endpoint
);";

    public async Task Subscribe(string endpoint, string address, string topic)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = subscribeSql;
        command.AddStringParam("Endpoint", endpoint);
        command.AddStringParam("QueueAddress", address);
        command.AddStringParam("Topic", topic);

        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// The sql statements used to unsubscribe from a topic.
    /// </summary>
    public static readonly string UnsubscribeSql = @"
DELETE FROM {0}
WHERE
    Endpoint = @Endpoint and
    Topic = @Topic";

    public async Task Unsubscribe(string endpoint, string topic)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = unsubscribeSql;
        command.AddStringParam("Endpoint", endpoint);
        command.AddStringParam("Topic", topic);
        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// The sql statements used to get subscribers for a topic.
    /// </summary>
    public static readonly string GetSubscribersSql = @"
SELECT DISTINCT QueueAddress
FROM {0}
WHERE Topic IN ({1})
";

    public async Task<List<string>> GetSubscribers(params string[] topics)
    {
        var results = new List<string>();

        var argumentsList = string.Join(", ", Enumerable.Range(0, topics.Length).Select(i => $"@Topic_{i}"));
        var getSubscribersCommand = getSubscribersSql.Replace("{1}", argumentsList);

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = getSubscribersCommand;
            for (var i = 0; i < topics.Length; i++)
            {
                command.AddStringParam($"Topic_{i}", topics[i]);
            }

            await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    results.Add(reader.GetString(0));
                }
            }

            return results;
        }
    }
}