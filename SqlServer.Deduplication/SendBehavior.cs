using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Logging;
using NServiceBus.Pipeline;
using NServiceBus.Transport;
using NServiceBus.Transport.SqlServerDeduplication;

class SendBehavior :
    Behavior<IOutgoingPhysicalMessageContext>
{
    ILog logger = LogManager.GetLogger("DeduplicationSendBehavior");
    Table table;
    Func<CancellationToken, Task<SqlConnection>> connectionBuilder;
    Action<IOutgoingPhysicalMessageContext> callback;

    public SendBehavior(Table table, Func<CancellationToken, Task<SqlConnection>> connectionBuilder, Action<IOutgoingPhysicalMessageContext> callback)
    {
        this.table = table;
        this.connectionBuilder = connectionBuilder;
        this.callback = callback;
    }

    public override async Task Invoke(IOutgoingPhysicalMessageContext context, Func<Task> next)
    {
        var shouldDeduplicate = context.Extensions.Get<bool>("SqlServer.Deduplication");
        if (!shouldDeduplicate)
        {
            await next().ConfigureAwait(false);
            return;
        }

        var connectionTask = connectionBuilder(CancellationToken.None).ConfigureAwait(false);

        var messageId = GetMessageId(context);

        var transportTransaction = new TransportTransaction();
        context.Extensions.Set(transportTransaction);
        using (var connection = await connectionTask)
        using (var transaction = connection.BeginTransaction())
        {
            transportTransaction.Set(connection);
            transportTransaction.Set(transaction);

            var deduplicationManager = new DeduplicationManager(transaction, table);
            if (await deduplicationManager.WriteDedupRecord(CancellationToken.None, messageId).ConfigureAwait(false))
            {
                logger.Info($"Message deduplicated. MessageId: {messageId}");
                callback?.Invoke(context);
                return;
            }

            await next().ConfigureAwait(false);
            transaction.Commit();
        }
    }

    static Guid GetMessageId(IOutgoingPhysicalMessageContext context)
    {
        if (Guid.TryParse(context.MessageId, out var messageId))
        {
            return messageId;
        }
        throw new Exception($"Only Guids are supported for message Ids. Invalid value: {context.MessageId}");
    }
}