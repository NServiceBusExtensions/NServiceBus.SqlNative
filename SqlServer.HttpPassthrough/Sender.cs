using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using NServiceBus.Attachments.Sql.Raw;
using NServiceBus.SqlServer.HttpPassthrough;
using Table = NServiceBus.Transport.SqlServerNative.Table;

class Sender
{
    Persister attachments;
    Func<CancellationToken, Task<SqlConnection>> connectionFunc;
    HeadersBuilder headersBuilder;

    public Sender(Func<CancellationToken, Task<SqlConnection>> connectionFunc, HeadersBuilder headersBuilder, Table attachmentsTable)
    {
        this.connectionFunc = connectionFunc;
        attachments = new Persister(new  NServiceBus.Attachments.Sql.Raw.Table(attachmentsTable.TableName, attachmentsTable.Schema, false));
        this.headersBuilder = headersBuilder;
    }

    public async Task Send(PassthroughMessage message, Table destination, CancellationToken cancellation)
    {
        try
        {
            await InnerSend(message, destination, cancellation);
        }
        catch (Exception exception)
        {
            throw new SendFailureException(message, exception);
        }
    }

    async Task InnerSend(PassthroughMessage message, Table destination, CancellationToken cancellation)
    {
        using (var connection = await connectionFunc(cancellation).ConfigureAwait(false))
        using (var transaction = connection.BeginTransaction())
        {
            await SendInsideTransaction(message, destination, cancellation, transaction)
                .ConfigureAwait(false);
            transaction.Commit();
        }
    }

    Task SendInsideTransaction(PassthroughMessage message, Table destination, CancellationToken cancellation, SqlTransaction transaction)
    {
        var headersString = headersBuilder.GetHeadersString(message);

        var outgoingMessage = new OutgoingMessage(
            message.Id,
            headers: headersString,
            bodyBytes: Encoding.UTF8.GetBytes(message.Body));
        var queueManager = new QueueManager(destination, transaction, "Deduplication");
        var attachmentExpiry = DateTime.UtcNow.AddDays(10);
        var tasks = SendAttachments(transaction, attachmentExpiry, cancellation, message).ToList();
        tasks.Add(queueManager.Send(outgoingMessage, cancellation));
        return Task.WhenAll(tasks);
    }

    IEnumerable<Task> SendAttachments(SqlTransaction transaction, DateTime expiry, CancellationToken cancellation, PassthroughMessage message)
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