using NServiceBus.Installation;
using NServiceBus.Settings;
using NServiceBus.Transport.SqlServerDeduplication;

class NeedToInstallSomething(IReadOnlySettings settings) :
    INeedToInstallSomething
{
    DedupeSettings? settings = settings.GetOrDefault<DedupeSettings>();

    public async Task Install(string identity, Cancel cancel = default)
    {
        if (settings == null || settings.InstallerDisabled)
        {
            return;
        }

        using var connection = await settings.ConnectionBuilder(Cancel.None);
        var manager = new DedupeManager(connection, settings.Table);
        await manager.Create(cancel);
    }
}