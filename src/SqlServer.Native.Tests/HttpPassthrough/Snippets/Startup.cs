﻿#pragma warning disable CA1822
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

#region Startup

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new PassthroughConfiguration(
            connectionFunc: OpenConnection,
            callback: Callback,
            dedupCriticalError: exception =>
            {
                Environment.FailFast("Dedup cleanup failure", exception);
            });
        services.AddSqlHttpPassthrough(configuration);
        services.AddMvcCore();
        // other ASP.MVC config
    }

    static Task<Table> Callback(HttpContext http, PassthroughMessage message)
    {
        //TODO: validate that the message type is allowed
        //TODO: validate that the destination is allowed
        if (message.Destination == null)
        {
            var customDestination = new Table("Custom");
            return Task.FromResult(customDestination);
        }

        var destination = new Table(message.Destination);
        return Task.FromResult(destination);
    }

    public void Configure(IApplicationBuilder builder)
    {
        builder.AddSqlHttpPassthroughBadRequestMiddleware();
        builder.UseMvc();
        // other ASP.MVC config
    }

    static Task<SqlConnection> OpenConnection(Cancel cancel) =>
        //TODO open and return a SqlConnection
        null!;
}

#endregion