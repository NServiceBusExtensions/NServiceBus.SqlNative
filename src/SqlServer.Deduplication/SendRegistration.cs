using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Pipeline;
using NServiceBus.Transport.SqlServerDeduplication;

class SendRegistration :
    RegisterStep
{
    public SendRegistration(Table table, Func<CancellationToken, Task<DbConnection>> connectionBuilder)
        : base(
            stepId: $"{AssemblyHelper.Name}Send",
            behavior: typeof(SendBehavior),
            description: "Saves the outgoing message id to a secondary store to allow message deduplication",
            factoryMethod: builder => new SendBehavior(table, connectionBuilder))
    {
    }
}