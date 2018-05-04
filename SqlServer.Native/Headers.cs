using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace NServiceBus.Transport.SqlServerNative
{
    public static partial class Headers
    {
        static Dictionary<string, string> emptyHeaders = new Dictionary<string, string>();
        public readonly static string EmptyHeadersJson = "{}";

        public static IReadOnlyDictionary<string, string> EmptyHeaders => emptyHeaders;

        public static string Serialize(IDictionary<string, string> instance)
        {
            if (instance == null)
            {
                return null;
            }

            var serializer = BuildSerializer();
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, instance);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public static IDictionary<string, string> DeSerialize(string json)
        {
            if (json == null)
            {
                return emptyHeaders;
            }

            var serializer = BuildSerializer();
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (Dictionary<string, string>) serializer.ReadObject(stream);
            }
        }

        static DataContractJsonSerializer BuildSerializer()
        {
            var settings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true
            };
            return new DataContractJsonSerializer(typeof(Dictionary<string, string>), settings);
        }

        public const string WireDateTimeFormat = "yyyy-MM-dd HH:mm:ss:ffffff Z";

        public static string ToWireFormattedString(DateTime dateTime)
        {
            return dateTime.ToUniversalTime()
                .ToString(WireDateTimeFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime ToUtcDateTime(string wireFormattedString)
        {
            return DateTime.ParseExact(wireFormattedString, WireDateTimeFormat, CultureInfo.InvariantCulture)
                .ToUniversalTime();
        }
    }
}