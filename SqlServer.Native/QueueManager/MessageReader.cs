using System;
using System.Data.SqlClient;
using NServiceBus.Transport.SqlServerNative;

static class MessageReader
{
    public static IncomingBytesMessage ReadBytesMessage(this SqlDataReader dataReader)
    {
        return new IncomingBytesMessage(
            id: dataReader.GetGuid(0),
            rowVersion: dataReader.GetInt64(1),
            correlationId: dataReader.ValueOrNull<string>(2),
            expires: dataReader.ValueOrNull<DateTime?>(3),
            headers: dataReader.ValueOrNull<string>(4),
            body: dataReader.ValueOrNull<byte[]>(6)
        );
    }

    public static IncomingStreamMessage ReadStreamMessage(this SqlDataReader dataReader, params IDisposable[] cleanups)
    {
        var id = dataReader.GetGuid(0);
        var rowVersion = dataReader.GetInt64(1);
        var correlationId = dataReader.ValueOrNull<string>(2);
        var expires = dataReader.ValueOrNull<DateTime?>(3);
        var headers = dataReader.ValueOrNull<string>(4);
        var length = dataReader.ValueOrNull<long?>(5);
        StreamWrapper streamWrapper;
        if (length == null)
        {
            streamWrapper = null;
        }
        else
        {
            streamWrapper = new StreamWrapper(length.Value, dataReader.GetStream(6));
        }

        return new IncomingStreamMessage(
            id: id,
            rowVersion: rowVersion,
            correlationId: correlationId,
            expires: expires,
            headers: headers,
            body: streamWrapper,
            cleanups
        );
    }
}