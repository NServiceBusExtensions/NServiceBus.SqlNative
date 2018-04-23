using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using SqlServer.Native;

static class MessageReader
{
    public static async Task<IncomingMessage> ReadMessage(this SqlDataReader dataReader, CancellationToken cancellation)
    {
        return new IncomingMessage(
            id: await dataReader.GetFieldValueAsync<Guid>(0, cancellation).ConfigureAwait(false),
            rowVersion: await dataReader.GetFieldValueAsync<long>(1, cancellation).ConfigureAwait(false),
            correlationId: await dataReader.ValueOrNull<string>(2, cancellation).ConfigureAwait(false),
            replyToAddress: await dataReader.ValueOrNull<string>(3, cancellation).ConfigureAwait(false),
            expires: await dataReader.ValueOrNull<DateTime?>(4, cancellation).ConfigureAwait(false),
            headers: await dataReader.ValueOrNull<string>(5, cancellation).ConfigureAwait(false),
            body: await dataReader.ValueOrNull<byte[]>(6, cancellation).ConfigureAwait(false)
        );
    }
}