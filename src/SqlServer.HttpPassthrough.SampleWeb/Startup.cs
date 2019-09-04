using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus.SqlServer.HttpPassthrough;
using NServiceBus.Transport.SqlServerNative;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new PassthroughConfiguration(OpenConnection, AmendMessage,
            dedupCriticalError: exception => { Environment.FailFast("", exception); });
        configuration.AppendClaimsToMessageHeaders();
        services.AddSqlHttpPassthrough(configuration);
        services.AddMvcCore();
    }

    static Task<Table> AmendMessage(HttpContext context, PassthroughMessage message)
    {
        message.ExtraHeaders = new Dictionary<string, string>
        {
            {"MessagePassthrough.Version", AssemblyVersion.Version},
            {"{}\":", "{}\":"}
        };
        return Task.FromResult((Table) message.Destination);
    }

    static Task<SqlConnection> OpenConnection(CancellationToken cancellation)
    {
        return ConnectionHelpers.OpenConnection(Connection.ConnectionString, cancellation);
    }

    public void Configure(IApplicationBuilder builder)
    {
        builder.UseMiddleware<LogContextMiddleware>();
        builder.AddSqlHttpPassthroughBadRequestMiddleware();
        builder.UseMvc();
    }
}