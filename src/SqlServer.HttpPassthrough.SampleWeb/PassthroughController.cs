using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NServiceBus.SqlServer.HttpPassthrough;

[Authorize]
[Route("SendMessage")]
public class PassthroughController :
    ControllerBase
{
    ISqlPassthrough sender;

    public PassthroughController(ISqlPassthrough sender) =>
        this.sender = sender;

    [HttpPost]
    public async Task Post(Cancel cancel)
    {
        try
        {
            await sender.Send(HttpContext, cancel);
        }
        catch (SendFailureException exception)
        {
            exception.Data.Add("message", exception.PassthroughMessage.ToDictionary());
            exception.CaptureAndThrow();
        }
    }
}