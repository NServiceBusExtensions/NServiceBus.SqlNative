﻿using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using NServiceBus.SqlServer.HttpPassThrough;

static class HeaderReader
{
    public static IncomingHeaders GetIncomingHeaders(IHeaderDictionary headers)
    {
        var messageType = headers.GetHeader("MessageType");

        string messageNamespace = null;
        if (headers.TryGetValue("MessageNamespace", out var value))
        {
            messageNamespace = value.ToString();
            if (string.IsNullOrWhiteSpace(messageNamespace))
            {
                throw new BadRequestException("Header 'MessageNamespace' existed but had no value.");
            }
            if (messageNamespace.Contains("."))
            {
                throw new BadRequestException($"Invalid 'MessageNamespace' header. Contains '.'. MessageNamespace: {messageNamespace}");
            }
        }

        if (messageType.Contains("."))
        {
            throw new BadRequestException($"Invalid 'MessageType' header. Contains '.'. MessageType: {messageType}");
        }

        return new IncomingHeaders
        {
            MessageId = GetMessageId(headers),
            MessageType = messageType,
            MessageNamespace = messageNamespace,
            Destination = headers.GetHeader("Destination"),
            Referrer = headers.GetHeader(HeaderNames.Referer)
        };
    }

    static Guid GetMessageId(IHeaderDictionary headers)
    {
        var messageIdString = headers.GetHeader("MessageId");

        if (Guid.TryParse(messageIdString, out var messageId))
        {
            return messageId;
        }

        throw new BadRequestException($"Header 'MessageId' could not be converted to a Guid. Value: {messageIdString}");
    }

    static string GetHeader(this IHeaderDictionary headers, string key)
    {
        if (!headers.TryGetValue(key, out var value)
            || string.IsNullOrWhiteSpace(value))
        {
            throw new BadRequestException($"Header '{key}' expected to exist.");
        }

        return value.ToString();
    }
}