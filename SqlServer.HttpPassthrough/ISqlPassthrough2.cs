using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    public interface ISqlPassthrough
    {
        Task Send(HttpContext context, CancellationToken cancellation = default);
    }
}