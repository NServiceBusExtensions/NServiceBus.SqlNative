using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class RequestParserTests : TestBase
{
    [Fact]
    public void Simple()
    {
        var attachmentBytes = Encoding.UTF8.GetBytes("Attachment Text");
        var request = new FakeHttpRequest
        {
            HeadersDictionary = new Dictionary<string, StringValues>
            {
                {"Destination", "TheEndpoint"},
                {"MessageNamespace", "TheMessageNamespace"},
                {"MessageType", "TheMessageType"},
                {"MessageId", new Guid("00000000-0000-0000-0000-000000000001").ToString()},
                {HeaderNames.Referer, "TheReferer"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes("{}")),
            Form = new FormCollection(
                new Dictionary<string, StringValues>
                {
                    {"message", "{}"}
                },
                new FormFileCollection
                {
                    new FormFile(new MemoryStream(attachmentBytes), 0, attachmentBytes.Length, "attachment", "attachment.txt")
                })
        };
        var extract = RequestParser.Extract(request, CancellationToken.None).GetAwaiter().GetResult();
        ObjectApprover.VerifyWithJson(new
        {
            extract.Attachments.Single().FileName,
            extract.Body,
            extract.ClientUrl,
            extract.Destination,
            extract.Id,
            extract.Type
        });
    }

    public RequestParserTests(ITestOutputHelper output) : base(output)
    {
    }
}