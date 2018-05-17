using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SqlHttpPassThrough
{
    public static class Configuration
    {
        public static void AddSqlHttpPassThrough(this IServiceCollection services,
            Func<CancellationToken, Task<SqlConnection>> connectionFunc,

            Action<HttpContext, PassThroughMessage> sendCallback = null,
            string originatingEndpoint = "SqlHttpPassThrough",
            string originatingMachine = null)
        {
            Guard.AgainstNull(services, nameof(services));
            Guard.AgainstNull(originatingEndpoint, nameof(originatingEndpoint));
            Guard.AgainstNull(connectionFunc, nameof(connectionFunc));

            if (sendCallback == null)
            {
                sendCallback = (context, message) => { };
            }

            if (originatingMachine == null)
            {
                originatingMachine = Environment.MachineName;
            }
            else
            {
                Guard.AgainstEmpty(originatingMachine, nameof(originatingMachine));
            }

            var headersBuilder = new HeadersBuilder(originatingEndpoint, originatingMachine);
            var sender = new Sender(connectionFunc, headersBuilder);
            var sqlPassThrough = new SqlPassThrough(sendCallback, sender);
            services.AddSingleton<ISqlPassThrough>(sqlPassThrough);
            services.AddSingleton<IHostedService>(new DedupService(connectionFunc));
        }

        public static void AddSqlHttpPassThroughBadExceptionMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<BadRequestMiddleware>();
        }
    }
}