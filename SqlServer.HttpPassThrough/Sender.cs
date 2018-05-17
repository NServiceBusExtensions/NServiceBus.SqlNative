using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using NServiceBus.Attachments.Sql;
using SqlHttpPassThrough;

class Sender
{
    Persister attachments = new Persister();
    Func<CancellationToken, Task<SqlConnection>> connectionFunc;
    HeadersBuilder headersBuilder;

    public Sender(Func<CancellationToken, Task<SqlConnection>> connectionFunc, HeadersBuilder headersBuilder)
    {
        this.connectionFunc = connectionFunc;
        this.headersBuilder = headersBuilder;
    }

    public async Task Send(PassThroughMessage message, CancellationToken cancellation)
    {
        try
        {
            await InnerSend(message, cancellation);
        }
        catch (Exception exception)
        {
            throw new SendFailureException(message, exception);
        }
    }

    async Task InnerSend(PassThroughMessage message, CancellationToken cancellation)
    {
        using (var connection = await connectionFunc(cancellation).ConfigureAwait(false))
        using (var transaction = connection.BeginTransaction())
        {
            await SendInsideTransaction(message, cancellation, transaction)
                .ConfigureAwait(false);
            transaction.Commit();
        }
    }

    Task SendInsideTransaction(PassThroughMessage message, CancellationToken cancellation, SqlTransaction transaction)
    {
        var headersString = headersBuilder.GetHeadersString(message);

        var outgoingMessage = new OutgoingMessage(
            message.Id,
            headers: headersString,
            bodyBytes: Encoding.UTF8.GetBytes(message.Body));
        var queueManager = new QueueManager(message.Endpoint, transaction,
            deduplicate: true,
            deduplicationTable: "MessagePassThroughDeduplication");
        var attachmentExpiry = DateTime.UtcNow.AddDays(10);
        var tasks = SendAttachments(transaction, attachmentExpiry, cancellation, message).ToList();
        tasks.Add(queueManager.Send(outgoingMessage, cancellation));
        return Task.WhenAll(tasks);
    }

    IEnumerable<Task> SendAttachments(SqlTransaction transaction, DateTime expiry, CancellationToken cancellation, PassThroughMessage message)
    {
        var connection = transaction.Connection;
        foreach (var file in message.Attachments)
        {
            yield return SendAttachment(transaction, message.Id.ToString(), expiry, cancellation, file, connection);
        }
    }

    async Task SendAttachment(SqlTransaction transaction, string messageId, DateTime expiry, CancellationToken cancellation, Attachment file, SqlConnection connection)
    {
        using (var stream = file.Stream())
        {
            await attachments.SaveStream(connection, transaction, messageId, file.FileName, expiry, stream, cancellation: cancellation)
                .ConfigureAwait(false);
        }
    }
}