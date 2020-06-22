using System.Threading.Tasks;
using NServiceBus.Transport.SqlServerNative;
using Xunit;

public class SubscriptionManagerTests :
    TestBase
{
    [Fact]
    public async Task CreateDrop()
    {
        var subscriptionManager = new SubscriptionManager("Subscription", SqlConnection);
        await subscriptionManager.Drop();
        await subscriptionManager.Create();
    }
}