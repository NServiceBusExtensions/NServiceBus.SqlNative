using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Pipeline;
using NServiceBus.Transport.SqlServerDeduplication;

class SendRegistration :
    RegisterStep
{
    public SendRegistration(Table table, Func<CancellationToken, Task<SqlConnection>> connectionBuilder, Action<IOutgoingPhysicalMessageContext> callback)
        : base(
            stepId: $"{AssemblyHelper.Name}Send",
            behavior: typeof(SendBehavior),
            description: "Saves the outgoing message id to a secondary store to allow message deduplication",
            factoryMethod: builder => new SendBehavior(table, connectionBuilder, callback))
    {
    }
}