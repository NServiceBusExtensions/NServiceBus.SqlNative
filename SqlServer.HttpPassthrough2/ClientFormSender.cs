using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.SqlServer.HttpPassThrough
{
    public static class ClientFormSender
    {
        public static Task<Guid> Send(HttpClient client, string route, string message, Type messageType, Guid messageId = default, string destination = null, Dictionary<string, byte[]> attachments = null, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(messageType, nameof(messageType));
            var typeName = messageType.Name;
            var typeNamespace = messageType.Namespace;
            return Send(client, route, message, typeName, messageId, typeNamespace, destination, attachments, cancellation);
        }

        public static async Task<Guid> Send(HttpClient client, string route, string message, string typeName, Guid messageId = default, string typeNamespace = null, string destination = null, Dictionary<string, byte[]> attachments = null, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(client, nameof(client));
            Guard.AgainstNullOrEmpty(route, nameof(route));
            Guard.AgainstNullOrEmpty(typeName, nameof(typeName));
            Guard.AgainstNullOrEmpty(message, nameof(message));
            Guard.AgainstEmpty(typeNamespace, nameof(typeNamespace));
            Guard.AgainstEmpty(destination, nameof(destination));
            if (messageId == default)
            {
                messageId = Guid.NewGuid();
            }
            using (var content = new MultipartFormDataContent())
            using (var stringContent = new StringContent(message))
            {
                content.Add(stringContent, "message");
                content.Headers.Add("MessageId", messageId.ToString());
                content.Headers.Add("Destination", destination);
                content.Headers.Add("MessageType", typeName);
                content.Headers.Add("MessageNamespace", typeNamespace);
                List<ByteArrayContent> files;
                if (attachments == null)
                {
                    files = new List<ByteArrayContent>();
                }
                else
                {
                    files = new List<ByteArrayContent>(attachments.Count);
                    foreach (var attachment in attachments)
                    {
                        var file = new ByteArrayContent(attachment.Value);
                        content.Add(file, attachment.Key, attachment.Key);
                        files.Add(file);
                    }
                }

                try
                {
                    using (var response = await client.PostAsync(route, content, cancellation).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
                finally
                {
                    foreach (var file in files)
                    {
                        file.Dispose();
                    }
                }

                return messageId;
            }
        }
    }
}