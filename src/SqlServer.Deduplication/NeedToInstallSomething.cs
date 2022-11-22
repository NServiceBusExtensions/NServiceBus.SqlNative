using NServiceBus.Installation;
using NServiceBus.Settings;
using NServiceBus.Transport.SqlServerDeduplication;

class NeedToInstallSomething :
    INeedToInstallSomething
{
    DedupeSettings? settings;

    public NeedToInstallSomething(IReadOnlySettings settings) =>
        this.settings = settings.GetOrDefault<DedupeSettings>();

    public async Task Install(string identity, CancellationToken cancellation = default)
    {
        if (settings == null || settings.InstallerDisabled)
        {
            return;
        }

        using var connection = await settings.ConnectionBuilder(CancellationToken.None);
        var manager = new DedupeManager(connection, settings.Table);
        await manager.Create(cancellation);
    }
}