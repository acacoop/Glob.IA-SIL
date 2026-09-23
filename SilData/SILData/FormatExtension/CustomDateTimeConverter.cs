using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SILData.FormatExtension
{
  public class CustomDateTimeConverter : JsonConverter<DateTime>
  {
    private readonly string[] _formats = { "yyyy-MM-dd", "yyyy/MM/dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ssZ", "MM/dd/yyyy hh:mm:ss tt zzz",
		"yyyy-MM-ddTHH:mm:sszzz"};

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
            string? _fecha = reader.GetString();
      if (DateTime.TryParseExact(_fecha, _formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
      {
        return fecha.Date;
      }
      throw new JsonException("Formato de fecha inválido.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
      writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }
  }

}
