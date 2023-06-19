namespace NServiceBus.Transport.SqlServerNative;

public partial class DelayedQueueManager
{
    public virtual async Task Send(IEnumerable<OutgoingDelayedMessage> messages, Cancel cancel = default)
    {
        using var command = Connection.CreateCommand(Transaction, string.Format(SendSql, Table));
        var dueParameter = CreateDueParameter(command);
        var headersParameter = CreateHeadersParameter(command);
        var bodyParameter = CreateBodyParameter(command);

        foreach (var message in messages)
        {
            dueParameter.Value = message.Due;
            headersParameter.Value = message.Headers;
            bodyParameter.SetBinaryOrDbNull(message.Body);
            await command.RunNonQuery(cancel);
        }
    }

    public virtual async Task Send(IAsyncEnumerable<OutgoingDelayedMessage> messages, Cancel cancel = default)
    {
        using var command = Connection.CreateCommand(Transaction, string.Format(SendSql, Table));
        var dueParameter = CreateDueParameter(command);
        var headersParameter = CreateHeadersParameter(command);
        var bodyParameter = CreateBodyParameter(command);

        await foreach (var message in messages.WithCancellation(cancel))
        {
            dueParameter.Value = message.Due;
            headersParameter.Value = message.Headers;
            bodyParameter.SetBinaryOrDbNull(message.Body);
            await command.RunNonQuery(cancel);
        }
    }
}