using Microsoft.Data.SqlClient;
using NServiceBus.Pipeline;
using NServiceBus.Transport.SqlServerDeduplication;

class SendRegistration :
    RegisterStep
{
    public SendRegistration(Table table, Func<Cancellation, Task<SqlConnection>> connectionBuilder) :
        base(
            stepId: $"{AssemblyHelper.Name}Send",
            behavior: typeof(SendBehavior),
            description: "Saves the outgoing message id to a secondary store to allow message deduplication",
            factoryMethod: _ => new SendBehavior(table, connectionBuilder))
    {
    }
}