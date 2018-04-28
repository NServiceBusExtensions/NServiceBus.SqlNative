using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;

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

    public static void SetValueOrDbNull(this SqlParameter corrParam, object value)
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
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync(cancellation).ConfigureAwait(false);
        }
    }


    public static async Task<T> ReadSingle<T>(this SqlCommand command, CancellationToken cancellation, Func<SqlDataReader, T> func)
        where T : class
    {
        using (var reader = await command.ExecuteSingleRowReader(cancellation).ConfigureAwait(false))
        {
            return await ReadSingle(reader, func, cancellation);
        }
    }

    public static async Task<T> ReadSingle<T>(this SqlDataReader reader, Func<SqlDataReader, T> func, CancellationToken cancellation) where T : class
    {
        if (!await reader.ReadAsync(cancellation).ConfigureAwait(false))
        {
            return null;
        }

        return func(reader);
    }

    public static async Task<IncomingResult> ReadMultiple<T>(this SqlCommand command, Func<T, Task> action, CancellationToken cancellation, Func<SqlDataReader, T> func) where T : class, IIncomingMessage
    {
        var count = 0;
        long? lastRowVersion = null;
        using (var reader = await command.ExecuteSequentialReader(cancellation).ConfigureAwait(false))
        {
            while (await reader.ReadAsync(cancellation).ConfigureAwait(false))
            {
                count++;
                cancellation.ThrowIfCancellationRequested();
                var message = func(reader);
                lastRowVersion = message.RowVersion;
                await action(message).ConfigureAwait(false);
            }
        }

        return new IncomingResult
        {
            Count = count,
            LastRowVersion = lastRowVersion
        };
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