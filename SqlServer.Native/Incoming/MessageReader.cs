using System;
using System.Data.SqlClient;
using System.IO;
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
            body: dataReader.ValueOrNull<byte[]>(6)
        );
    }
    public static IncomingStreamMessage ReadStreamMessage(this SqlDataReader dataReader)
    {
        return new IncomingStreamMessage(
            id: dataReader.GetGuid(0),
            rowVersion: dataReader.GetInt64(1),
            correlationId: dataReader.ValueOrNull<string>(2),
            replyToAddress: dataReader.ValueOrNull<string>(3),
            expires: dataReader.ValueOrNull<DateTime?>(4),
            headers: dataReader.ValueOrNull<string>(5),
            body: dataReader.ValueOrNull<Stream>(6)
        );
    }
}