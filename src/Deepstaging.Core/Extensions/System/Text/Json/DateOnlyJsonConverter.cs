using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deepstaging.Text.Json;

public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateOnly.FromDateTime(
            DateTime.Parse(
                reader.GetString() ?? DateTime.MinValue.ToString(CultureInfo.InvariantCulture)
            )
        );

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) => 
        writer.WriteStringValue(value.ToString());
}