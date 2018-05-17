using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Route("")]
[Produces("application/json")]
public class InfoController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(Dictionary<string, string>))]
    public Task Get()
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName();
        return Response.WriteAsync($"{{\"version\":\"{assemblyName.Version}\"}}");
    }

    [HttpGet("user")]
    [Authorize]
    [ProducesResponseType(200, Type = typeof(Dictionary<string, string>))]
    public Task GetUser()
    {
        return Response.WriteAsync($"{{\"name\":\"{User.Identity.Name}\"}}");
    }
}