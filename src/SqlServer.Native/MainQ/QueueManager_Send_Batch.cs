namespace NServiceBus.Transport.SqlServerNative;

public partial class QueueManager
{
    public virtual async Task Send(IEnumerable<OutgoingMessage> messages, CancellationToken cancellation = default)
    {
        using var command = Connection.CreateCommand(Transaction, string.Format(sendSql, Table));
        var parameters = command.Parameters;

        var idParameter = CreateIdParameter(command, parameters);
        var expiresParameter = CreateExpiresParameter(command, parameters);
        var headersParameter = CreateHeadersParameter(command, parameters);
        var bodyParameter = CreateBodyParameter(command, parameters);
        foreach (var message in messages)
        {
            idParameter.Value = message.Id;
            expiresParameter.SetValueOrDbNull(message.Expires);
            headersParameter.Value = message.Headers;
            bodyParameter.SetBinaryOrDbNull(message.Body);
            await command.RunNonQuery(cancellation);
        }
    }

    public virtual async Task Send(IAsyncEnumerable<OutgoingMessage> messages, CancellationToken cancellation = default)
    {
        using var command = Connection.CreateCommand(Transaction, string.Format(sendSql, Table));
        var parameters = command.Parameters;

        var idParameter = CreateIdParameter(command, parameters);
        var expiresParameter = CreateExpiresParameter(command, parameters);
        var headersParameter = CreateHeadersParameter(command, parameters);
        var bodyParameter = CreateBodyParameter(command, parameters);
        await foreach (var message in messages.WithCancellation(cancellation))
        {
            idParameter.Value = message.Id;
            expiresParameter.SetValueOrDbNull(message.Expires);
            headersParameter.Value = message.Headers;
            bodyParameter.SetBinaryOrDbNull(message.Body);
            await command.RunNonQuery(cancellation);
        }
    }
}