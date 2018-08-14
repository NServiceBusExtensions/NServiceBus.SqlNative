using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Pipeline;
using NServiceBus.Transport.SqlServerDeduplication;

class SendBehavior :
    Behavior<IOutgoingPhysicalMessageContext>
{
    Table table;
    Func<CancellationToken, Task<SqlConnection>> connectionBuilder;

    public SendBehavior(Table table, Func<CancellationToken, Task<SqlConnection>> connectionBuilder)
    {
        this.table = table;
        this.connectionBuilder = connectionBuilder;
    }

    public override async Task Invoke(IOutgoingPhysicalMessageContext context, Func<Task> next)
    {
        var connectionTask = connectionBuilder(CancellationToken.None).ConfigureAwait(false);

        if (!Guid.TryParse(context.MessageId, out var messageId))
        {
            throw new Exception($"Only Guids are supported for message Ids. Invalid value: {context.MessageId}");
        }

        using (var connection = await connectionTask)
        {
            var deduplicationManager = new DeduplicationManager(connection, table);
            await deduplicationManager.WriteDedupRecord(CancellationToken.None, messageId).ConfigureAwait(false);
        }
    }
}