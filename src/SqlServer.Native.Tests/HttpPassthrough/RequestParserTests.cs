using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class RequestParserTests :
    TestBase
{
    [Fact]
    public Task Optional()
    {
        var request = new FakeHttpRequest
        (
            headersDictionary: new Dictionary<string, StringValues>
            {
                {"MessageType", "TheMessageType"},
                {"MessageId", new Guid("00000000-0000-0000-0000-000000000001").ToString()},
            },
            body: Body(),
            form: Form()
        );
        return Verify(request);
    }

    [Fact]
    public Task Simple()
    {
        var request = new FakeHttpRequest
        (
            headersDictionary: new Dictionary<string, StringValues>
            {
                {"Destination", "TheEndpoint"},
                {"MessageNamespace", "TheMessageNamespace"},
                {"MessageType", "TheMessageType"},
                {"MessageId", new Guid("00000000-0000-0000-0000-000000000001").ToString()},
                {HeaderNames.Referer, "TheReferer"}
            },
            body: Body(),
            form: Form()
        );
       return Verify(request);
    }

    static MemoryStream Body()
    {
        return new MemoryStream(Encoding.UTF8.GetBytes("{}"));
    }

    Task Verify(FakeHttpRequest request)
    {
        var extract = RequestParser.Extract(request, CancellationToken.None).GetAwaiter().GetResult();
        return Verifier.Verify(new
        {
            extract.Attachments.Single().FileName,
            extract.Body,
            extract.ClientUrl,
            extract.Destination,
            extract.Id,
            extract.Type
        });
    }

    static FormCollection Form()
    {
        var attachmentBytes = Encoding.UTF8.GetBytes("Attachment Text");
        return new FormCollection(
            new Dictionary<string, StringValues>
            {
                {"message", "{}"}
            },
            new FormFileCollection
            {
                new FormFile(new MemoryStream(attachmentBytes), 0, attachmentBytes.Length, "attachment", "attachment.txt")
            });
    }
}