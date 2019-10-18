using System;
using System.Collections.Generic;
using System.Data.Common;
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

    static async Task<DbConnection> OpenConnection(CancellationToken cancellation)
    {
        var connection = new SqlConnection(Connection.ConnectionString);
        try
        {
            await connection.OpenAsync(cancellation);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    public void Configure(IApplicationBuilder builder)
    {
        builder.UseMiddleware<LogContextMiddleware>();
        builder.AddSqlHttpPassthroughBadRequestMiddleware();
        builder.UseMvc();
    }
}