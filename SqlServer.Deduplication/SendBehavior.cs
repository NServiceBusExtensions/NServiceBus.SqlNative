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

    public SendBehavior(Table table, Func<CancellationToken, Task<SqlConnection>> connectionBuilder)
    {
        this.table = table;
        this.connectionBuilder = connectionBuilder;
    }

    public override async Task Invoke(IOutgoingPhysicalMessageContext context, Func<Task> next)
    {
        if (!DedupePipelineState.TryGet(context, out var dedupePipelineState))
        {
            await next().ConfigureAwait(false);
            return;
        }

        var connectionTask = connectionBuilder(CancellationToken.None).ConfigureAwait(false);
        if (context.Extensions.TryGet(out TransportTransaction transportTransaction))
        {
            throw new NotSupportedException("Deduplication is currently designed to be used from outside the NServiceBus pipeline. For example to dedup messages being sent from inside a web service endpoint.");
        }

        var messageId = GetMessageId(context);

        transportTransaction = new TransportTransaction();
        context.Extensions.Set(transportTransaction);
        using (var connection = await connectionTask)
        using (var transaction = connection.BeginTransaction())
        {
            transportTransaction.Set(connection);
            transportTransaction.Set(transaction);

            var dedupeManager = new DedupeManager(transaction, table);
            var outcome = await dedupeManager.WriteDedupRecord(messageId).ConfigureAwait(false);
            dedupePipelineState.DedupeOutcome = outcome;
            if (outcome == DedupeOutcome.Deduplicated)
            {
                logger.Info($"Message deduplicated. MessageId: {messageId}");
                return;
            }

            await next().ConfigureAwait(false);
            transaction.Commit();
        }
    }

    static bool ShouldDedupe(IOutgoingPhysicalMessageContext context)
    {
        if (context.Extensions.TryGet("SqlServer.Deduplication", out bool shouldDeduplicate))
        {
            return shouldDeduplicate;
        }

        return false;
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