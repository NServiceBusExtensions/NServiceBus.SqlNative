using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    /// <summary>
    /// Helper class for sending passthrough messages from a client.
    /// </summary>
    public class ClientFormSender
    {
        HttpClient client;

        /// <summary>
        /// Initializes a new instance of <see cref="ClientFormSender"/>.
        /// </summary>
        public ClientFormSender(HttpClient client)
        {
            Guard.AgainstNull(client, nameof(client));
            this.client = client;
        }

        /// <summary>
        /// Send a pass through message request.
        /// </summary>
        public virtual Task<(Guid messageId, int httpStatus)> Send(string route, string message, Type messageType, Guid messageId = default, string destination = null, Dictionary<string, byte[]> attachments = null, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(messageType, nameof(messageType));
            var typeName = messageType.Name;
            var typeNamespace = messageType.Namespace;
            return Send(route, message, typeName, messageId, typeNamespace, destination, attachments, cancellation);
        }

        /// <summary>
        /// Send a pass through message request.
        /// </summary>
        public virtual async Task<(Guid messageId, int httpStatus)> Send(string route, string message, string typeName, Guid messageId = default, string typeNamespace = null, string destination = null, Dictionary<string, byte[]> attachments = null, CancellationToken cancellation = default)
        {
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
            {
                content.Add(new StringContent(message), "message");
                var headers = content.Headers;

                headers.Add("MessageType", typeName);
                if (typeNamespace != null)
                {
                    headers.Add("MessageNamespace", typeNamespace);
                }

                if (messageId != default)
                {
                    headers.Add("MessageId", messageId.ToString());
                }

                if (destination != null)
                {
                    headers.Add("Destination", destination);
                }

                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                    {
                        var file = new ByteArrayContent(attachment.Value);
                        content.Add(file, attachment.Key, attachment.Key);
                    }
                }

                using (var response = await client.PostAsync(route, content, cancellation).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    return (messageId, (int)response.StatusCode);
                }
            }
        }
    }
}