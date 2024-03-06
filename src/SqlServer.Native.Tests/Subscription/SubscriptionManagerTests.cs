public class SubscriptionManagerTests :
    TestBase
{
    [Fact]
    public async Task Create()
    {
        var subscriptionManager = new SubscriptionManager("Subscription", SqlConnection);
        await subscriptionManager.Drop();
        await subscriptionManager.Create();
        await Verify(SqlConnection)
            .SchemaSettings(includeItem: name => name == "Subscription");
    }

    [Fact]
    public async Task Drop()
    {
        var subscriptionManager = new SubscriptionManager("Subscription", SqlConnection);
        await subscriptionManager.Drop();
        await subscriptionManager.Create();
        await subscriptionManager.Drop();
        await Verify(SqlConnection)
            .SchemaSettings(includeItem: name => name == "Subscription");
    }

    [Fact]
    public async Task Subscribe()
    {
        var subscriptionManager = new SubscriptionManager("Subscription", SqlConnection);
        await subscriptionManager.Drop();
        await subscriptionManager.Create();
        await subscriptionManager.Subscribe("endpoint1", "address1", "topic1");
        await subscriptionManager.Subscribe("endpoint2", "address2", "topic1");
        await Verify(subscriptionManager.GetSubscribers("topic1"));
    }

    [Fact]
    public async Task NoMatchingTopic()
    {
        var subscriptionManager = new SubscriptionManager("Subscription", SqlConnection);
        await subscriptionManager.Drop();
        await subscriptionManager.Create();
        await subscriptionManager.Subscribe("endpoint1", "address1", "topic1");
        await subscriptionManager.Subscribe("endpoint2", "address2", "topic2");
        await Verify(subscriptionManager.GetSubscribers("topic3"));
    }

    [Fact]
    public async Task SingleTopic()
    {
        var subscriptionManager = new SubscriptionManager("Subscription", SqlConnection);
        await subscriptionManager.Drop();
        await subscriptionManager.Create();
        await subscriptionManager.Subscribe("endpoint1", "address1", "topic1");
        await subscriptionManager.Subscribe("endpoint2", "address2", "topic1");
        await subscriptionManager.Subscribe("endpoint3", "address3", "topic3");
        await Verify(subscriptionManager.GetSubscribers("topic1"));
    }

    [Fact]
    public async Task MultipleTopic()
    {
        var subscriptionManager = new SubscriptionManager("Subscription", SqlConnection);
        await subscriptionManager.Drop();
        await subscriptionManager.Create();
        await subscriptionManager.Subscribe("endpoint1", "address1", "topic1");
        await subscriptionManager.Subscribe("endpoint2", "address2", "topic2");
        await subscriptionManager.Subscribe("endpoint3", "address3", "topic3");
        await subscriptionManager.Subscribe("endpoint4", "address4", "topic4");
        await Verify(subscriptionManager.GetSubscribers("topic1", "topic4"));
    }
}