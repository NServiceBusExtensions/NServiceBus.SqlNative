using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

static class Extensions
{
    public static Func<T, Task> ToTaskFunc<T>(this Action<T> action)
    {
        Guard.AgainstNull(action, nameof(action));
        return x =>
        {
            action(x);
            return Task.CompletedTask;
        };
    }

    public static void SetValueOrDbNull(this SqlParameter corrParam, string value)
    {
        if (value == null)
        {
            corrParam.Value = DBNull.Value;
            return;
        }

        corrParam.Value = value;
    }

    public static void SetValueOrDbNull(this SqlParameter corrParam, DateTime? value)
    {
        if (value == null)
        {
            corrParam.Value = DBNull.Value;
            return;
        }

        corrParam.Value = value;
    }

    public static void SetBinaryOrDbNull(this SqlParameter corrParam, object value)
    {
        if (value == null)
        {
            corrParam.Value = DBNull.Value;
            return;
        }

        corrParam.Value = value;
    }

    public static async Task ExecuteCommand(this SqlConnection connection, SqlTransaction transaction, string sql, CancellationToken cancellation = default)
    {
        Guard.AgainstNull(connection, nameof(connection));
        using (var command = connection.CreateCommand(transaction, sql))
        {
            await command.ExecuteNonQueryAsync(cancellation).ConfigureAwait(false);
        }
    }

    public static SqlCommand CreateCommand(this SqlConnection connection, SqlTransaction transaction, string sql)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        return command;
    }

    public static T ValueOrNull<T>(this SqlDataReader dataReader, int index)
    {
        if (dataReader.IsDBNull(index))
        {
            return default;
        }

        return dataReader.GetFieldValue<T>(index);
    }

    public static Task<SqlDataReader> ExecuteSequentialReader(this SqlCommand command, CancellationToken cancellation)
    {
        return command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellation);
    }

    public static Task<SqlDataReader> ExecuteSingleRowReader(this SqlCommand command, CancellationToken cancellation)
    {
        return command.ExecuteReaderAsync(CommandBehavior.SingleRow | CommandBehavior.SequentialAccess, cancellation);
    }
}