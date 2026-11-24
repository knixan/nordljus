using System.ComponentModel.DataAnnotations;

namespace Nordljus.Backend.Ingest;

/// <summary>Inställningar för valideringen av positioner, från sektionen <c>Validation</c>.</summary>
public sealed class ValidationOptions
{
    public const string Section = "Validation";

    /// <summary>Hur långt fram i tiden en sensors klocka får gå innan positionen avvisas.</summary>
    [Range(typeof(TimeSpan), "00:00:00", "00:05:00")]
    public TimeSpan MaxClockSkew { get; init; } = TimeSpan.FromSeconds(5);
}
