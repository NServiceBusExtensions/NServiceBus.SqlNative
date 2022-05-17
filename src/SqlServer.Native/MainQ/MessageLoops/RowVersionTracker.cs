using System.Data;
using Microsoft.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative;

public class RowVersionTracker
{
    string table;

    public RowVersionTracker(string table = "RowVersionTracker")
    {
        Guard.AgainstNullOrEmpty(table, nameof(table));
        this.table = table;
    }

    public Task CreateTable(SqlConnection connection, CancellationToken cancellation = default) =>
        CreateTable(connection, null, cancellation);

    public Task CreateTable(SqlTransaction transaction, CancellationToken cancellation = default) =>
        CreateTable(transaction.Connection!, transaction, cancellation);

    Task CreateTable(SqlConnection connection, SqlTransaction? transaction, CancellationToken cancellation) =>
        connection.RunCommand(transaction, string.Format(Sql, table), cancellation);

    public Task Save(SqlConnection connection, long rowVersion, CancellationToken cancellation = default)
    {
        Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
        return Save(connection, null, rowVersion, cancellation);
    }

    public Task Save(SqlTransaction transaction, long rowVersion, CancellationToken cancellation = default)
    {
        Guard.AgainstNegativeAndZero(rowVersion, nameof(rowVersion));
        return Save(transaction.Connection!, transaction, rowVersion, cancellation);
    }

    async Task Save(SqlConnection connection, SqlTransaction? transaction, long rowVersion, CancellationToken cancellation)
    {
        await using var command = connection.CreateCommand(
            transaction: transaction,
            sql: $@"
update {table}
set RowVersion = @RowVersion
if @@rowcount = 0
  insert into {table} (RowVersion)
  values (@RowVersion)
");
        var parameter = command.CreateParameter();
        parameter.ParameterName = "RowVersion";
        parameter.DbType = DbType.Int64;
        parameter.Value = rowVersion;
        command.Parameters.Add(parameter);
        await command.RunNonQuery(cancellation);
    }

    public async Task<long> Get(SqlConnection connection, CancellationToken cancellation = default)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $@"
select top (1) RowVersion
from {table}";
        var result = await command.RunScalar(cancellation);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (result == null)
        {
            return 1;
        }

        return (long) result;
    }

    static string Sql = @"
if exists (
  select *
  from sys.objects
  where object_id = object_id('{0}')
    and type in ('U'))
return

create table {0} (
  RowVersion bigint not null
);
";
}