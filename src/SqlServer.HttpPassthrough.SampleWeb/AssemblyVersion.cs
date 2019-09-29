static class AssemblyVersion
{
    static AssemblyVersion()
    {
        Version = typeof(AssemblyVersion).Assembly.GetName().Version!.ToString();
    }

    public static readonly string Version;
}