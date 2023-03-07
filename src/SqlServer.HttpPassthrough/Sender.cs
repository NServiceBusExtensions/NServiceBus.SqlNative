using Microsoft.Data.SqlClient;
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
    Table dedupeTable;
    ILogger logger;

    public Sender(Func<CancellationToken, Task<SqlConnection>> connectionFunc, HeadersBuilder headersBuilder, Table attachmentsTable, Table dedupeTable, ILogger logger)
    {
        this.connectionFunc = connectionFunc;
        attachments = new(new(attachmentsTable.TableName, attachmentsTable.Schema, false));
        this.headersBuilder = headersBuilder;
        this.dedupeTable = dedupeTable;
        this.logger = logger;
    }

    public async Task<long> Send(PassthroughMessage message, Table destination, Cancellation cancellation)
    {
        try
        {
           return await InnerSend(message, destination, cancellation);
        }
        catch (Exception exception)
        {
            throw new SendFailureException(message, exception);
        }
    }

    async Task<long> InnerSend(PassthroughMessage message, Table destination, Cancellation cancellation)
    {
        using var connection = await connectionFunc(cancellation);
        using var transaction = connection.BeginTransaction();
        var rowVersion = await SendInsideTransaction(message, destination, cancellation, transaction);
        transaction.Commit();
        return rowVersion;
    }

    async Task<long> SendInsideTransaction(PassthroughMessage message, Table destination, Cancellation cancellation, SqlTransaction transaction)
    {
        var headersString = headersBuilder.GetHeadersString(message);
        LogSend(message);
        var outgoingMessage = new OutgoingMessage(
            message.Id,
            headers: headersString,
            bodyBytes: Encoding.UTF8.GetBytes(message.Body));
        var queueManager = new QueueManager(destination, transaction, dedupeTable);
        var attachmentExpiry = DateTime.UtcNow.AddDays(10);
        await SendAttachments(transaction, attachmentExpiry, cancellation, message);
        return await queueManager.Send(outgoingMessage, cancellation);
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

    async Task SendAttachments(SqlTransaction transaction, DateTime expiry, Cancellation cancellation, PassthroughMessage message)
    {
        var connection = transaction.Connection!;
        foreach (var file in message.Attachments)
        {
            await SendAttachment(transaction, message.Id.ToString(), expiry, cancellation, file, connection);
        }
    }

    async Task SendAttachment(SqlTransaction transaction, string messageId, DateTime expiry, Cancellation cancellation, Attachment file, SqlConnection connection)
    {
        using var stream = file.Stream();
        await attachments.SaveStream(connection, transaction, messageId, file.FileName, expiry, stream, cancellation: cancellation);
    }
}