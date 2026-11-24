using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nordljus.Simulator;

/// <summary>Läser in vilka enheter simulatorn ska skicka data för.</summary>
public static class UnitRoster
{
    // Ett saknat eller ogiltigt fält ska ge fel, inte tyst bli null eller ett standardvärde.
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) },
        RespectRequiredConstructorParameters = true,
        RespectNullableAnnotations = true,
    };

    /// <exception cref="JsonException">Filen är inte giltig JSON eller en enhet saknar ett fält.</exception>
    /// <exception cref="InvalidDataException">Listan är tom, ett id eller namn är tomt, eller ett id förekommer två gånger.</exception>
    public static IReadOnlyList<SimulatedUnit> Load(string path)
    {
        using var stream = File.OpenRead(path);
        var units = JsonSerializer.Deserialize<List<SimulatedUnit>>(stream, _options);

        if (units is null || units.Count == 0)
        {
            throw new InvalidDataException($"{path} innehåller inga enheter.");
        }

        if (units.FirstOrDefault(u => string.IsNullOrWhiteSpace(u.Id) || string.IsNullOrWhiteSpace(u.Name)) is { } blank)
        {
            throw new InvalidDataException($"{path}: en enhet har tomt id eller namn ({blank}).");
        }

        if (units.GroupBy(u => u.Id).FirstOrDefault(g => g.Count() > 1) is { } duplicate)
        {
            throw new InvalidDataException($"{path}: id {duplicate.Key} förekommer {duplicate.Count()} gånger.");
        }

        return units;
    }
}
