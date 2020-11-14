using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

public class FakeHttpRequest :
    HttpRequest
{
    public Dictionary<string, StringValues> HeadersDictionary;

    public FakeHttpRequest(Dictionary<string, StringValues> headersDictionary, MemoryStream body, FormCollection form)
    {
        HeadersDictionary = headersDictionary;
        Body = body;
        Form = form;
    }

    public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(Form);
    }

    public override HttpContext HttpContext { get; } = null!;
    public override string Method { get; set; } = null!;
    public override string Scheme { get; set; } = null!;
    public override bool IsHttps { get; set; }
    public override HostString Host { get; set; }
    public override PathString PathBase { get; set; }
    public override PathString Path { get; set; }
    public override QueryString QueryString { get; set; }
    public override IQueryCollection Query { get; set; } = null!;
    public override string Protocol { get; set; } = null!;
    public override IHeaderDictionary Headers => new HeaderDictionary(HeadersDictionary);
    public override IRequestCookieCollection Cookies { get; set; } = null!;
    public override long? ContentLength { get; set; } = null!;
    public override string ContentType { get; set; } = null!;
    public override Stream Body { get; set; }
    public override bool HasFormContentType { get; }
    public override IFormCollection Form { get; set; }
}