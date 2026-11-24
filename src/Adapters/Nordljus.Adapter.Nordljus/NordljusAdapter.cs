using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nordljus.Contracts;

namespace Nordljus.Adapter.Nordljus;

/// <summary>
/// Översätter ett meddelande i team Nordljus råformat till <see cref="TrackUpdate"/>.
/// Råformatet är JSON, ett meddelande per UDP-datagram:
/// <c>{"unitId":"jarn-1","name":"Järn 1","kind":"groundVehicle","lat":57.9,"lon":15.3,"timestamp":"2026-11-24T10:00:00Z"}</c>
/// </summary>
public static class NordljusAdapter
{
    // Ett saknat eller ogiltigt fält ska ge fel, inte tyst bli null eller ett standardvärde.
    // Web-standarden läser tal ur strängar ("lat":"57.9") – det stängs av med Strict.
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = JsonNumberHandling.Strict,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) },
        RespectRequiredConstructorParameters = true,
        RespectNullableAnnotations = true,
    };

    /// <summary>
    /// Försöker tolka ett meddelande. Kastar aldrig för trasig data: då returneras
    /// <see langword="false"/> och en orsak som går att logga.
    /// </summary>
    public static bool TryParse(
        ReadOnlySpan<byte> payload,
        [NotNullWhen(true)] out TrackUpdate? update,
        [NotNullWhen(false)] out string? error)
    {
        update = null;

        RawMessage? raw;
        try
        {
            raw = JsonSerializer.Deserialize<RawMessage>(payload, _options);
        }
        catch (JsonException ex)
        {
            error = $"Ogiltig JSON: {ex.Message}";
            return false;
        }

        if (raw is null)
        {
            error = "Meddelandet är tomt (null).";
            return false;
        }

        if (string.IsNullOrWhiteSpace(raw.UnitId) || string.IsNullOrWhiteSpace(raw.Name))
        {
            error = "unitId och name får inte vara tomma.";
            return false;
        }

        if (ToTrackType(raw.Kind) is not { } type)
        {
            error = $"Okänd enhetstyp: {raw.Kind}.";
            return false;
        }

        update = new TrackUpdate(
            raw.UnitId,
            raw.Name,
            type,
            raw.Lat,
            raw.Lon,
            raw.Timestamp.ToUniversalTime());
        error = null;
        return true;
    }

    // Enum-konverteraren kan ge odefinierade värden (t.ex. "groundVehicle, helicopter" → 3),
    // så okänt blir null och ett fel – aldrig ett undantag.
    private static TrackType? ToTrackType(RawKind kind) => kind switch
    {
        RawKind.Personnel => TrackType.Personnel,
        RawKind.GroundVehicle => TrackType.GroundVehicle,
        RawKind.Helicopter => TrackType.Helicopter,
        _ => null,
    };

    // Råformatet har egna namn, skilda från kontraktet. Ändras råformatet ändras bara det här.
    private sealed record RawMessage(
        string UnitId,
        string Name,
        RawKind Kind,
        double Lat,
        double Lon,
        DateTimeOffset Timestamp);

    private enum RawKind
    {
        Personnel,
        GroundVehicle,
        Helicopter,
    }
}
