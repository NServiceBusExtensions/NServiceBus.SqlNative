using System;
using System.Data.SqlClient;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    /// <summary>
    /// Configuration extensions to add Sql HTTP Passthrough to asp.net core
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Add Sql HTTP Passthrough to an instance of <see cref="IServiceCollection"/>.
        /// Used from <code>Startup.ConfigureServices.</code>
        /// </summary>
        /// <param name="connectionFunc">Creates a instance of a new and open <see cref="SqlConnection"/>.</param>
        public static void AddSqlHttpPassThrough(
            this IServiceCollection services,
            Func<CancellationToken, Task<SqlConnection>> connectionFunc)
        {
            AddSqlHttpPassThrough(services, new PassthroughConfiguration(connectionFunc));
        }

        /// <summary>
        /// Add Sql HTTP Passthrough to an instance of <see cref="IServiceCollection"/>.
        /// Used from <code>Startup.ConfigureServices</code>.
        /// </summary>
        public static void AddSqlHttpPassThrough(
            this IServiceCollection services,
            PassthroughConfiguration configuration)
        {
            Guard.AgainstNull(services, nameof(services));
            Guard.AgainstNull(configuration, nameof(configuration));

            var headersBuilder = new HeadersBuilder(configuration.originatingEndpoint, configuration.originatingMachine);
            var sender = new Sender(configuration.connectionFunc, headersBuilder,configuration.attachmentsTable);
            var sqlPassThrough = new SqlPassThrough(configuration.sendCallback, sender);
            services.AddSingleton<ISqlPassthrough>(sqlPassThrough);
            var dedupService = new DedupService(configuration.deduplicationTable, configuration.connectionFunc);
            services.AddSingleton<IHostedService>(dedupService);
        }

        /// <summary>
        /// Add a asp.net core middleware for handling <see cref="BadRequestException"/>s and returning a <see cref="HttpStatusCode.BadRequest"/> to the client.
        /// Used from <code>Startup.Configure</code>
        /// </summary>
        public static void AddSqlHttpPassThroughBadRequestMiddleware(
            this IApplicationBuilder builder)
        {
            Guard.AgainstNull(builder, nameof(builder));
            builder.UseMiddleware<BadRequestMiddleware>();
        }
    }
}