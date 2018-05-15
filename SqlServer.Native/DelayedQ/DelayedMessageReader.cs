using System;
using System.Data.SqlClient;
using NServiceBus.Transport.SqlServerNative;

static class DelayedMessageReader
{
    public static IncomingDelayedMessage ReadDelayedMessage(this SqlDataReader dataReader, params IDisposable[] cleanups)
    {
        var rowVersion = dataReader.GetInt64(0);
        var due = dataReader.ValueOrNull<DateTime>(1);
        var headers = dataReader.ValueOrNull<string>(2);
        var length = dataReader.ValueOrNull<long?>(3);
        StreamWrapper streamWrapper;
        if (length == null)
        {
            streamWrapper = null;
        }
        else
        {
            streamWrapper = new StreamWrapper(length.Value, dataReader.GetStream(4));
        }

        return new IncomingDelayedMessage(
            rowVersion: rowVersion,
            due: due,
            headers: headers,
            body: streamWrapper,
            cleanups
        );
    }
}