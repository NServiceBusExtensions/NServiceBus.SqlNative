using Microsoft.AspNetCore.Mvc;
using NServiceBus.SqlServer.HttpPassthrough;

#region Controller

[Route("SendMessage")]
public class PassthroughController : ControllerBase
{
    ISqlPassthrough sender;

    public PassthroughController(ISqlPassthrough sender) =>
        this.sender = sender;

    [HttpPost]
    public Task Post(Cancellation cancellation) =>
        sender.Send(HttpContext, cancellation);
}

#endregion