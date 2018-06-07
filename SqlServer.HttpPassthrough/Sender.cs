using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus.Transport.SqlServerNative;
using NServiceBus.Attachments.Sql.Raw;
using NServiceBus.SqlServer.HttpPassthrough;
using Table = NServiceBus.Transport.SqlServerNative.Table;

class Sender
{
    Persister attachments;
    Func<CancellationToken, Task<SqlConnection>> connectionFunc;
    HeadersBuilder headersBuilder;
    Table deduplicationTable;
    ILogger logger;

    public Sender(Func<CancellationToken, Task<SqlConnection>> connectionFunc, HeadersBuilder headersBuilder, Table attachmentsTable, Table deduplicationTable, ILogger logger)
    {
        this.connectionFunc = connectionFunc;
        attachments = new Persister(new  NServiceBus.Attachments.Sql.Raw.Table(attachmentsTable.TableName, attachmentsTable.Schema, false));
        this.headersBuilder = headersBuilder;
        this.deduplicationTable = deduplicationTable;
        this.logger = logger;
    }

    public async Task<long> Send(PassthroughMessage message, Table destination, CancellationToken cancellation)
    {
        try
        {
           return await InnerSend(message, destination, cancellation)
               .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw new SendFailureException(message, exception);
        }
    }

    async Task<long> InnerSend(PassthroughMessage message, Table destination, CancellationToken cancellation)
    {
        using (var connection = await connectionFunc(cancellation).ConfigureAwait(false))
        using (var transaction = connection.BeginTransaction())
        {
            var rowVersion = await SendInsideTransaction(message, destination, cancellation, transaction)
                .ConfigureAwait(false);
            transaction.Commit();
            return rowVersion;
        }
    }

    async Task<long> SendInsideTransaction(PassthroughMessage message, Table destination, CancellationToken cancellation, SqlTransaction transaction)
    {
        var headersString = headersBuilder.GetHeadersString(message);
        LogSend(message);
        var outgoingMessage = new OutgoingMessage(
            message.Id,
            headers: headersString,
            bodyBytes: Encoding.UTF8.GetBytes(message.Body));
        var queueManager = new QueueManager(destination, transaction, deduplicationTable);
        var attachmentExpiry = DateTime.UtcNow.AddDays(10);
        await Task.WhenAll(SendAttachments(transaction, attachmentExpiry, cancellation, message)).ConfigureAwait(false);
        return await queueManager.Send(outgoingMessage, cancellation).ConfigureAwait(false);
    }

    void LogSend(PassthroughMessage message)
    {
        if (!logger.IsEnabled(LogLevel.Information))
        {
            return;
        }

        logger.LogInformation("Send passthrough. Id:{id}, Destination:{destination}, Namespace:{namespace}, Type:{type}, Body:{body}, Attachments:{attachments},",
            message.Id,
            message.Destination,
            message.Namespace,
            message.Type,
            message.Body,
            message.Attachments.Select(x => x.FileName));
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