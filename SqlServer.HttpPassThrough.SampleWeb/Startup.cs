using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus.Transport.SqlServerNative;
using SqlHttpPassThrough;

public class Startup
{
    string nsbConnectionString;

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        nsbConnectionString = Configuration.GetConnectionString("NServiceBus");
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();
        services.AddSqlHttpPassThrough(OpenConnection,AmendMessage);
        services.AddMvcCore();
    }

    void AmendMessage(HttpContext context, PassThroughMessage message)
    {
        message.ExtraHeaders = new Dictionary<string, string>
        {
            { "MessagePassThrough.Version", AssemblyVersion.Version }
        };
    }

    Task<SqlConnection> OpenConnection(CancellationToken cancellation)
    {
        return ConnectionHelpers.OpenConnection(nsbConnectionString, cancellation);
    }

    public void Configure(IApplicationBuilder builder, IHostingEnvironment env)
    {
        builder.UseMiddleware<LogContextMiddleware>();
        builder.AddSqlHttpPassThroughBadExceptionMiddleware();
        builder.UseCors(options =>
        {
            options.AllowAnyOrigin();
            options.AllowAnyMethod();
            options.AllowAnyHeader();
            options.AllowCredentials();
        });
       builder.UseMvc();
    }
}