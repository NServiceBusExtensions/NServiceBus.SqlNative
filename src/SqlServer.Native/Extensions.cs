using System.Data;
using System.Data.Common;

static class Extensions
{
    public static Func<T, Task> ToTaskFunc<T>(this Action<T> action)
    {
        return x =>
        {
            action(x);
            return Task.CompletedTask;
        };
    }

    public static void SetValueOrDbNull(this DbParameter corrParam, DateTime? value)
    {
        if (value == null)
        {
            corrParam.Value = DBNull.Value;
            return;
        }

        corrParam.Value = value;
    }

    public static void SetBinaryOrDbNull(this DbParameter corrParam, object? value)
    {
        if (value == null)
        {
            corrParam.Value = DBNull.Value;
            return;
        }

        corrParam.Value = value;
    }

    public static async Task RunCommand(this DbConnection connection, DbTransaction? transaction, string sql, CancellationToken cancellation = default)
    {
        await using var command = connection.CreateCommand(transaction, sql);
        await command.RunNonQuery(cancellation);
    }

    public static DbCommand CreateCommand(this DbConnection connection, DbTransaction? transaction, string sql)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        return command;
    }

    public static long? LongOrNull(this DbDataReader dataReader, int index)
    {
        if (dataReader.IsDBNull(index))
        {
            return default;
        }

        return dataReader.GetFieldValue<long>(index);
    }

    public static DateTime? DatetimeOrNull(this DbDataReader dataReader, int index)
    {
        if (dataReader.IsDBNull(index))
        {
            return default;
        }

        return dataReader.GetFieldValue<DateTime>(index);
    }

    public static async Task<DbDataReader> RunSequentialReader(this DbCommand command, CancellationToken cancellation)
    {
        try
        {
            return await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellation);
        }
        catch (DbException exception)
        {
            SetCommandData(command, exception);
            throw;
        }
    }

    public static async Task<object?> RunScalar(this DbCommand command, CancellationToken cancellation)
    {
        try
        {
            return await command.ExecuteScalarAsync(cancellation);
        }
        catch (DbException exception)
        {
            SetCommandData(command, exception);
            throw;
        }
    }

    public static async Task<DbDataReader> RunSingleRowReader(this DbCommand command, CancellationToken cancellation)
    {
        try
        {
            return await command.ExecuteReaderAsync(CommandBehavior.SingleRow | CommandBehavior.SequentialAccess, cancellation);
        }
        catch (DbException exception)
        {
            SetCommandData(command, exception);
            throw;
        }
    }

    public static async Task RunNonQuery(this DbCommand command, CancellationToken cancellation)
    {
        try
        {
            await command.ExecuteNonQueryAsync(cancellation);
        }
        catch (DbException exception)
        {
            SetCommandData(command, exception);
            throw;
        }
    }

    public static DbParameter AddStringParam(this DbCommand command, string name, string value)
    {
        var bodyParameter = command.CreateParameter();
        bodyParameter.ParameterName = name;
        bodyParameter.DbType = DbType.String;
        bodyParameter.Value = value;
        command.Parameters.Add(bodyParameter);
        return bodyParameter;
    }

    public static void SetCommandData(this DbCommand command, DbException exception)
    {
        exception.Data["Sql"] = command.CommandText;
    }
}