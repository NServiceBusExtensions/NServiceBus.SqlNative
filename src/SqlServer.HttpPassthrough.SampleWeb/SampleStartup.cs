using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus.SqlServer.HttpPassthrough;
using NServiceBus.Transport.SqlServerNative;

public class SampleStartup
{
    public SampleStartup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new PassthroughConfiguration(
            connectionFunc: () => new SqlConnection(Connection.ConnectionString),
            callback: AmendMessage,
            dedupCriticalError: exception => { Environment.FailFast("", exception); });
        configuration.AppendClaimsToMessageHeaders();
        services.AddSqlHttpPassthrough(configuration);
        services.AddMvcCore(options => options.EnableEndpointRouting = false);
    }

    static Task<Table> AmendMessage(HttpContext context, PassthroughMessage message)
    {
        message.ExtraHeaders = new Dictionary<string, string>
        {
            {"MessagePassthrough.Version", AssemblyVersion.Version},
            {"{}\":", "{}\":"}
        };
        return Task.FromResult((Table) message.Destination!);
    }

    public void Configure(IApplicationBuilder builder)
    {
        builder.UseMiddleware<LogContextMiddleware>();
        builder.AddSqlHttpPassthroughBadRequestMiddleware();
        builder.UseMvc();
    }
}