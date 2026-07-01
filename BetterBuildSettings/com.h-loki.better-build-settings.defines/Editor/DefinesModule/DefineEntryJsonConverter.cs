using System;
using Newtonsoft.Json;

public sealed class DefineEntryJsonConverter : JsonConverter<DefineEntry>
{
    public override void WriteJson(JsonWriter writer, DefineEntry value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.value);
    }

    public override DefineEntry ReadJson(
        JsonReader reader,
        Type objectType,
        DefineEntry existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var value = reader.Value?.ToString();

        return string.IsNullOrWhiteSpace(value)
            ? null
            : new DefineEntry(value);
    }
}