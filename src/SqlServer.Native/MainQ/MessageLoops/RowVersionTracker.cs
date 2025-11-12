using System.Data;

namespace NServiceBus.Transport.SqlServerNative;

public class RowVersionTracker
{
    string table;

    public RowVersionTracker(string table = "RowVersionTracker")
    {
        Guard.AgainstNullOrEmpty(table);
        this.table = table;
    }

    public Task CreateTable(SqlConnection connection, Cancel cancel = default) =>
        CreateTable(connection, null, cancel);

    public Task CreateTable(SqlTransaction transaction, Cancel cancel = default) =>
        CreateTable(transaction.Connection!, transaction, cancel);

    Task CreateTable(SqlConnection connection, SqlTransaction? transaction, Cancel cancel) =>
        connection.RunCommand(transaction, string.Format(Sql, table), cancel);

    public Task Save(SqlConnection connection, long rowVersion, Cancel cancel = default)
    {
        Guard.AgainstNegativeAndZero(rowVersion);
        return Save(connection, null, rowVersion, cancel);
    }

    public Task Save(SqlTransaction transaction, long rowVersion, Cancel cancel = default)
    {
        Guard.AgainstNegativeAndZero(rowVersion);
        return Save(transaction.Connection!, transaction, rowVersion, cancel);
    }

    async Task Save(SqlConnection connection, SqlTransaction? transaction, long rowVersion, Cancel cancel)
    {
        using var command = connection.CreateCommand(
            transaction: transaction,
            sql: $"""
                  update {table}
                  set RowVersion = @RowVersion
                  if @@rowcount = 0
                    insert into {table} (RowVersion)
                    values (@RowVersion)
                  """);
        var parameter = command.CreateParameter();
        parameter.ParameterName = "RowVersion";
        parameter.DbType = DbType.Int64;
        parameter.Value = rowVersion;
        command.Parameters.Add(parameter);
        await command.RunNonQuery(cancel);
    }

    public async Task<long> Get(SqlConnection connection, Cancel cancel = default)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             select top (1) RowVersion
             from {table}
             """;
        var result = await command.RunScalar(cancel);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (result == null)
        {
            return 1;
        }

        return (long) result;
    }

    static string Sql = """
                        if exists (
                          select *
                          from sys.objects
                          where object_id = object_id('{0}')
                            and type in ('U'))
                        return

                        create table {0} (
                          RowVersion bigint not null
                        );
                        """;
}