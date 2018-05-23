using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NServiceBus.SqlServer.HttpPassthrough;

[Route("SendMessage")]
public class PassThroughController : ControllerBase
{
    ISqlPassthrough sender;

    public PassThroughController(ISqlPassthrough sender)
    {
        this.sender = sender;
    }

    [HttpPost]
    public async Task Post(CancellationToken cancellation)
    {
        try
        {
            await sender.Send(HttpContext, cancellation)
                .ConfigureAwait(false);
        }
        catch (SendFailureException exception)
        {
            exception.Data.Add("message", exception.PassThroughMessage.ToDictionary());
            exception.CaptureAndThrow();
        }
    }
}