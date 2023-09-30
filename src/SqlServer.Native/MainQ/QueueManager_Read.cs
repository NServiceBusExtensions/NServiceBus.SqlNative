﻿using System.Data;
using Microsoft.Data.SqlClient;

namespace NServiceBus.Transport.SqlServerNative;

public partial class QueueManager
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
          Id,
          RowVersion,
          Expires,
          Headers,
          datalength(Body),
          Body
        from {0}
        with (readpast)
        where RowVersion >= @RowVersion
        order by RowVersion
        """);

    protected override async Task<IncomingMessage> ReadMessage(SqlDataReader dataReader, params Func<ValueTask>[] cleanups)
    {
        var id = dataReader.GetGuid(0);
        var rowVersion = dataReader.GetInt64(1);
        var expires = await dataReader.DatetimeOrNull(2);
        var headers = await dataReader.GetFieldValueAsync<string>(3);
        var length = await dataReader.LongOrNull(4);
        StreamWrapper? streamWrapper;
        if (length == null)
        {
            streamWrapper = null;
        }
        else
        {
            streamWrapper = new(length.Value, dataReader.GetStream(5));
        }

        return new(
            id: id,
            rowVersion: rowVersion,
            expires: expires,
            headers: headers,
            body: streamWrapper,
            cleanups
        );
    }
}