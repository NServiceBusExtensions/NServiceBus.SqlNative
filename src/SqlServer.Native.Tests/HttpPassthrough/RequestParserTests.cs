﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
            headersDictionary: new()
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
            headersDictionary: new()
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
        return new(Encoding.UTF8.GetBytes("{}"));
    }

    static Task Verify(FakeHttpRequest request)
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
        return new(
            new()
            {
                {"message", "{}"}
            },
            new FormFileCollection
            {
                new FormFile(new MemoryStream(attachmentBytes), 0, attachmentBytes.Length, "attachment", "attachment.txt")
            });
    }
}