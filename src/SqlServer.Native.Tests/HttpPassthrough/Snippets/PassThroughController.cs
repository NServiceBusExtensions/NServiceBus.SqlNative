using Microsoft.AspNetCore.Mvc;

#region Controller

[Route("SendMessage")]
public class PassthroughController(ISqlPassthrough sender) : ControllerBase
{
    [HttpPost]
    public Task Post(Cancel cancel) =>
        sender.Send(HttpContext, cancel);
}

#endregion