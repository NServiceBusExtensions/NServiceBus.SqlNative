using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using SqlServer.Native;

static class MessageReader
{
    public static async Task<Message> ReadMessage(this SqlDataReader dataReader, CancellationToken cancellation)
    {
        return new Message(
            id: await dataReader.GetFieldValueAsync<Guid>(0, cancellation).ConfigureAwait(false),
            correlationId: await dataReader.ValueOrNull<string>(1, cancellation).ConfigureAwait(false),
            replyToAddress: await dataReader.ValueOrNull<string>(2, cancellation).ConfigureAwait(false),
            expires: await dataReader.ValueOrNull<DateTime?>(3, cancellation).ConfigureAwait(false),
            headers: await dataReader.ValueOrNull<string>(4, cancellation).ConfigureAwait(false),
            body: await dataReader.ValueOrNull<byte[]>(5, cancellation).ConfigureAwait(false)
        );
    }
}