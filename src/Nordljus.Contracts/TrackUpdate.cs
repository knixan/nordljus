namespace Nordljus.Contracts;

/// <summary>
/// En position för ett spår (en enhet) i lägesbilden. Varje adapter översätter sitt
/// teams råformat till den här typen, och kärnan, backend och frontend känner bara till den.
/// </summary>
/// <param name="TrackId">Stabilt id för enheten, t.ex. <c>jarn-1</c>.</param>
/// <param name="Name">Namnet som operatören ser, t.ex. <c>Järn 1</c>.</param>
/// <param name="Type">Vilken sorts enhet det är.</param>
/// <param name="Latitude">Latitud i grader (WGS 84).</param>
/// <param name="Longitude">Longitud i grader (WGS 84).</param>
/// <param name="ObservedAt">När sensorn mätte positionen, i UTC.</param>
public sealed record TrackUpdate(
    string TrackId,
    string Name,
    TrackType Type,
    double Latitude,
    double Longitude,
    DateTimeOffset ObservedAt);

/// <summary>Vilken sorts enhet ett spår är.</summary>
public enum TrackType
{
    Personnel,
    GroundVehicle,
    Helicopter,
}
