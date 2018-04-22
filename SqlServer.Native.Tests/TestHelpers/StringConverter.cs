using System;
using Newtonsoft.Json;
using JsonReader = Newtonsoft.Json.JsonReader;

public class StringConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var input = (string)value;

        if (Guid.TryParse(input, out _))
        {
            writer.WriteValue("A Guid");
            return;
        }
        if (DateTime.TryParse(input, out _))
        {
            writer.WriteValue("A DateTime");
            return;
        }
        writer.WriteValue(input);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(string);
    }
}