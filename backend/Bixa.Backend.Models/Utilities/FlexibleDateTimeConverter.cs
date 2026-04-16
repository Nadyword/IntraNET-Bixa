using System.Text.Json.Serialization;
using System.Text.Json;
using System.Globalization;

namespace Bixa.Backend.Models.Utilities;

public class FlexibleDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (value != null)
            {
                // Intenta parsear la fecha en formato ISO 8601 con zona horaria (UTC)
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var fullDate) && fullDate.Kind != DateTimeKind.Unspecified)
                {
                    return fullDate.ToUniversalTime();
                }

                if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var simpleDate))
                {
                    return DateTime.SpecifyKind(simpleDate, DateTimeKind.Utc);
                }
            }
        }
        return default;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Al serializar de vuelta al cliente, siempre usa el formato ISO 8601 con Z para UTC.
        writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
    }
}