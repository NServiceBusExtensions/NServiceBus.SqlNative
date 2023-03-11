using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus.Transport.SqlServerNative;

namespace NServiceBus.SqlServer.HttpPassthrough;

/// <summary>
/// Configuration extensions to add Sql HTTP Passthrough to asp.net core
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Add Sql HTTP Passthrough to an instance of <see cref="IServiceCollection"/>.
    /// Used from <code>Startup.ConfigureServices.</code>
    /// </summary>
    /// <param name="connectionFunc">Creates a instance of a new and un-open <see cref="SqlConnection"/>.</param>
    /// <param name="callback">Manipulate or verify a <see cref="PassthroughMessage"/> prior to it being sent. Returns the destination <see cref="Table"/>.</param>
    /// <param name="dedupCriticalError">Called when failed to clean expired records after 10 consecutive unsuccessful attempts. The most likely cause of this is connectivity issues with the database.</param>
    public static void AddSqlHttpPassthrough(
        this IServiceCollection services,
        Func<SqlConnection> connectionFunc,
        Func<HttpContext, PassthroughMessage, Task<Table>> callback,
        Action<Exception> dedupCriticalError) =>
        AddSqlHttpPassthrough(services, new(connectionFunc, callback, dedupCriticalError));

    /// <summary>
    /// Add Sql HTTP Passthrough to an instance of <see cref="IServiceCollection"/>.
    /// Used from <code>Startup.ConfigureServices.</code>
    /// </summary>
    /// <param name="connectionFunc">Creates a instance of a new and open <see cref="SqlConnection"/>.</param>
    /// <param name="callback">Manipulate or verify a <see cref="PassthroughMessage"/> prior to it being sent. Returns the destination <see cref="Table"/>.</param>
    /// <param name="dedupCriticalError">Called when failed to clean expired records after 10 consecutive unsuccessful attempts. The most likely cause of this is connectivity issues with the database.</param>
    public static void AddSqlHttpPassthrough(
        this IServiceCollection services,
        Func<Cancellation, Task<SqlConnection>> connectionFunc,
        Func<HttpContext, PassthroughMessage, Task<Table>> callback,
        Action<Exception> dedupCriticalError) =>
        AddSqlHttpPassthrough(services, new(connectionFunc, callback, dedupCriticalError));

    /// <summary>
    /// Add Sql HTTP Passthrough to an instance of <see cref="IServiceCollection"/>.
    /// Used from <code>Startup.ConfigureServices</code>.
    /// </summary>
    public static void AddSqlHttpPassthrough(
        this IServiceCollection services,
        PassthroughConfiguration configuration)
    {
        var headersBuilder = new HeadersBuilder(configuration.OriginatingEndpoint, configuration.OriginatingMachine);
        services.AddSingleton<ISqlPassthrough>(serviceProvider =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<ISqlPassthrough>();
            var sender = new Sender(configuration.ConnectionFunc, headersBuilder, configuration.AttachmentsTable, configuration.DedupeTable, logger);
            return new SqlPassthrough(configuration.SendCallback, sender, configuration.AppendClaims, configuration.ClaimsHeaderPrefix, logger);
        });
        var dedupService = new DedupService(configuration.DedupeTable, configuration.ConnectionFunc, configuration.DedupCriticalError);
        services.AddSingleton<IHostedService>(dedupService);
    }

    /// <summary>
    /// Add a asp.net core middleware for handling <see cref="BadRequestException"/>s and returning a <see cref="HttpStatusCode.BadRequest"/> to the client.
    /// Used from <code>Startup.Configure</code>
    /// </summary>
    public static void AddSqlHttpPassthroughBadRequestMiddleware(
        this IApplicationBuilder builder) =>
        builder.UseMiddleware<BadRequestMiddleware>();
}