using System.Data;
using Microsoft.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative;

public partial class DelayedQueueManager
{
    protected override SqlCommand BuildReadCommand(int batchSize, long startRowVersion)
    {
        var command = Connection.CreateCommand(Transaction, string.Format(ReadSql, Table, batchSize));
        var parameter = command.CreateParameter();
        parameter.ParameterName = "RowVersion";
        parameter.DbType = DbType.Int64;
        parameter.Value = startRowVersion;
        command.Parameters.Add(parameter);
        return command;
    }

    public static readonly string ReadSql = ConnectionHelpers.WrapInNoCount(
        """
        select top({1})
          RowVersion,
          Due,
          Headers,
          datalength(Body),
          Body
        from {0}
        with (readpast)
        where RowVersion >= @RowVersion
        order by RowVersion
        """);

    protected override async Task<IncomingDelayedMessage> ReadMessage(SqlDataReader dataReader, params Func<ValueTask>[] cleanups)
    {
        var rowVersion = dataReader.GetInt64(0);
        var due = await dataReader.DatetimeOrNull(1);
        var headers = await dataReader.GetFieldValueAsync<string>(2);
        var length = await dataReader.LongOrNull(3);
        StreamWrapper? streamWrapper;
        if (length == null)
        {
            streamWrapper = null;
        }
        else
        {
            streamWrapper = new(length.Value, dataReader.GetStream(4));
        }

        return new(
            rowVersion: rowVersion,
            due: due,
            headers: headers,
            body: streamWrapper,
            cleanups
        );
    }
}