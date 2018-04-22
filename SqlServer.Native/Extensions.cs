using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

static class Extensions
{
    public static void SetValueOrDbNull(this SqlParameter corrParam, string value)
    {
        if (value == null)
        {
            corrParam.Value = DBNull.Value;
            return;
        }

        corrParam.Value = value;
    }

    public static async Task<T> ValueOrNull<T>(this SqlDataReader dataReader, int index, CancellationToken cancellation)
    {
        if (await dataReader.IsDBNullAsync(index, cancellation).ConfigureAwait(false))
        {
            return default;
        }

        return await dataReader.GetFieldValueAsync<T>(index, cancellation).ConfigureAwait(false);
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