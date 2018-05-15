using System;
using System.Data.SqlClient;
using NServiceBus.Transport.SqlServerNative;

static class MessageReader
{
    public static IncomingStreamMessage ReadStreamMessage(this SqlDataReader dataReader, params IDisposable[] cleanups)
    {
        var id = dataReader.GetGuid(0);
        var rowVersion = dataReader.GetInt64(1);
        var expires = dataReader.ValueOrNull<DateTime?>(2);
        var headers = dataReader.ValueOrNull<string>(3);
        var length = dataReader.ValueOrNull<long?>(4);
        StreamWrapper streamWrapper;
        if (length == null)
        {
            streamWrapper = null;
        }
        else
        {
            streamWrapper = new StreamWrapper(length.Value, dataReader.GetStream(5));
        }

        return new IncomingStreamMessage(
            id: id,
            rowVersion: rowVersion,
            expires: expires,
            headers: headers,
            body: streamWrapper,
            cleanups
        );
    }
}