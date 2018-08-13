using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Installation;
using NServiceBus.Settings;
using NServiceBus.Transport.SqlServerDeduplication;

class NeedToInstallSomething : INeedToInstallSomething
{
    DeduplicationSettings settings;

    public NeedToInstallSomething(ReadOnlySettings settings)
    {
        this.settings = settings.GetOrDefault<DeduplicationSettings>();
    }

    public async Task Install(string identity)
    {
        if (settings == null || settings.InstallerDisabled)
        {
            return;
        }

        using (var connection = await settings.ConnectionBuilder(CancellationToken.None).ConfigureAwait(false))
        {
            var manager = new DeduplicationManager(connection, settings.Table);
            await manager.Create().ConfigureAwait(false);
        }
    }
}