namespace Nordljus.Simulator;

/// <summary>En position i grader (WGS 84).</summary>
public readonly record struct GeoPosition(double Latitude, double Longitude);

/// <summary>
/// En enkel rutt: enheten kör runt en cirkel med konstant fart, moturs, och börjar rakt
/// öster om centrum. Räcker för den tunna skivan – riktiga rutter kommer med simulatorkortet.
/// </summary>
public sealed record CircularRoute(GeoPosition Center, double RadiusMeters, TimeSpan LapTime)
{
    // Ungefär så många meter går det på en breddgrad. Tillräckligt noggrant för korta avstånd.
    private const double _metersPerDegreeLatitude = 111_320;

    public GeoPosition PositionAt(TimeSpan elapsed)
    {
        var angle = 2 * Math.PI * (elapsed / LapTime);
        var east = RadiusMeters * Math.Cos(angle);
        var north = RadiusMeters * Math.Sin(angle);

        // En längdgrad blir kortare ju längre norrut man kommer.
        var metersPerDegreeLongitude = _metersPerDegreeLatitude * Math.Cos(Center.Latitude * Math.PI / 180);

        return new GeoPosition(
            Center.Latitude + north / _metersPerDegreeLatitude,
            Center.Longitude + east / metersPerDegreeLongitude);
    }
}
