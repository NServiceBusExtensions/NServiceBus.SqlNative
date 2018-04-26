using System;
using System.Data.SqlClient;
using NServiceBus.Transport.SqlServerNative;

static class MessageReader
{
    public static IncomingMessage ReadBytesMessage(this SqlDataReader dataReader)
    {
        return new IncomingMessage(
            id: dataReader.GetGuid(0),
            rowVersion: dataReader.GetInt64(1),
            correlationId: dataReader.ValueOrNull<string>(2),
            replyToAddress: dataReader.ValueOrNull<string>(3),
            expires: dataReader.ValueOrNull<DateTime?>(4),
            headers: dataReader.ValueOrNull<string>(5),
            body: dataReader.ValueOrNull<byte[]>(6)
        );
    }
}