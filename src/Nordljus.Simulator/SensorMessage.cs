using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nordljus.Simulator;

/// <summary>
/// Ett meddelande i team Nordljus råformat, så som en sensor skickar det. Fältnamnen är
/// råformatets (<c>unitId</c>, <c>lat</c>, <c>lon</c> …), inte kontraktets.
/// </summary>
public sealed record SensorMessage(
    string UnitId,
    string Name,
    UnitKind Kind,
    double Lat,
    double Lon,
    DateTimeOffset Timestamp)
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public byte[] ToJsonBytes() => JsonSerializer.SerializeToUtf8Bytes(this, _options);
}
