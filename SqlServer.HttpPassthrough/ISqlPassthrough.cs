using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    /// <summary>
    /// Used to send a passthrough message.
    /// Can be accessed by dependency inject after using <see cref="ConfigurationExtensions.AddSqlHttpPassthrough(IServiceCollection,PassthroughConfiguration)"/>.
    /// </summary>
    public interface ISqlPassthrough
    {
        Task Send(HttpContext context, CancellationToken cancellation = default);
    }
}