using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SqlHttpPassThrough
{
    public interface ISqlPassThrough
    {
        Task Send(HttpContext context, CancellationToken cancellation = default);
    }
}