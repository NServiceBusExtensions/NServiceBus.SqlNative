using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace NServiceBus.SqlServer.HttpPassthrough
{
    /// <summary>
    /// Helper class for appending a list of claims to a headers <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <remarks>
    /// The key of each item will be the <see cref="Claim.Type"/> with a prefix.
    /// The value of each item will be the <see cref="Claim.Value"/>s for all claims matching <see cref="Claim.Type"/>,
    /// the resultant values will be json encoded to create one string.
    /// </remarks>
    public static class ClaimsAppender
    {
        /// <summary>
        /// Append a list of <see cref="Claim"/> to a to a headers <see cref="IDictionary{TKey,TValue}"/>.
        /// Note that only the <see cref="Claim.Type"/>s and <see cref="Claim.Value"/>s are persisted.
        /// </summary>
        public static void Append(IEnumerable<Claim> claims, IDictionary<string, string> headers, string? prefix)
        {
            Guard.AgainstNull(claims, nameof(claims));
            Guard.AgainstNull(headers, nameof(headers));
            Guard.AgainstEmpty(prefix, nameof(prefix));
            prefix ??= "";
            foreach (var claim in claims.GroupBy(x => x.Type))
            {
                var items = claim.Select(x => x.Value).ToList();
                headers.Add(prefix + claim.Key, Serializer.SerializeList(items));
            }
        }

        /// <summary>
        /// Extracts a list of <see cref="Claim"/> from a headers <see cref="IDictionary{TKey,TValue}"/>
        /// that have been added using <see cref="Append"/>.
        /// </summary>
        public static IEnumerable<Claim> Extract(IDictionary<string, string> headers, string prefix)
        {
            Guard.AgainstNull(headers, nameof(headers));
            Guard.AgainstNullOrEmpty(prefix, nameof(prefix));
            foreach (var header in headers)
            {
                var key = header.Key;
                if (!key.StartsWith(prefix))
                {
                    continue;
                }

                key = key.Substring(prefix.Length, key.Length - prefix.Length);
                var list = Serializer.DeSerializeList(header.Value);
                foreach (var value in list)
                {
                    yield return new Claim(key, value);
                }
            }
        }
    }
}