using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

static class WebHostBuilder
{
    public static IWebHost Build()
    {
        var builder = WebHost.CreateDefaultBuilder();
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        builder.UseStartup<Startup>();

        return builder.Build();
    }
}