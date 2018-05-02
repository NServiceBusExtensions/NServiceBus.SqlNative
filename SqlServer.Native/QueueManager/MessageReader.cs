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
            replyToAddress: dataReader.ValueOrNull<string>(3),
            expires: dataReader.ValueOrNull<DateTime?>(4),
            headers: dataReader.ValueOrNull<string>(5),
            body: dataReader.ValueOrNull<byte[]>(7)
        );
    }

    public static IncomingStreamMessage ReadStreamMessage(this SqlDataReader dataReader, params IDisposable[] cleanups)
    {
        var id = dataReader.GetGuid(0);
        var rowVersion = dataReader.GetInt64(1);
        var correlationId = dataReader.ValueOrNull<string>(2);
        var replyToAddress = dataReader.ValueOrNull<string>(3);
        var expires = dataReader.ValueOrNull<DateTime?>(4);
        var headers = dataReader.ValueOrNull<string>(5);
        var length = dataReader.ValueOrNull<long?>(6);
        StreamWrapper streamWrapper;
        if (length == null)
        {
            streamWrapper = null;
        }
        else
        {
            streamWrapper = new StreamWrapper(length.Value, dataReader.GetStream(7));
        }

        return new IncomingStreamMessage(
            id: id,
            rowVersion: rowVersion,
            correlationId: correlationId,
            replyToAddress: replyToAddress,
            expires: expires,
            headers: headers,
            body: streamWrapper,
            cleanups
        );
    }
}