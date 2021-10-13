using Microsoft.Data.SqlClient;
using NServiceBus.Transport.SqlServerNative;

public class Subscription
{
    SqlConnection sqlConnection = null!;

    async Task CreateTable()
    {
        #region CreateSubscriptionTable

        var manager = new SubscriptionManager("SubscriptionRouting", sqlConnection);
        await manager.Create();

        #endregion
    }

    async Task DeleteTable()
    {
        #region DeleteSubscriptionTable

        var manager = new SubscriptionManager("SubscriptionRouting", sqlConnection);
        await manager.Drop();

        #endregion
    }
}