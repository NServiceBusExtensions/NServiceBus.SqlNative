using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class TestController : ControllerBase
{
    [HttpGet]
    [Route("test")]
    public IActionResult Test()
    {
        var file = Path.Combine(Directory.GetCurrentDirectory(), "test.html");
        return PhysicalFile(file, "text/html");
    }
}