using System.Text.Json;
using Nordljus.Simulator;

// Själva sändningen (UDP, tappad kontakt, trasig data) kommer i V2.
var path = Path.Combine(AppContext.BaseDirectory, "units.json");

IReadOnlyList<SimulatedUnit> units;
try
{
    units = UnitRoster.Load(path);
}
catch (Exception ex) when (ex is IOException or JsonException or InvalidDataException)
{
    Console.Error.WriteLine($"Simulatorn kunde inte läsa startdatan i {path}: {ex.Message}");
    return 1;
}

var personnel = units.Count(u => u.Kind == UnitKind.Personnel);
var groundVehicles = units.Count(u => u.Kind == UnitKind.GroundVehicle);
var helicopters = units.Count(u => u.Kind == UnitKind.Helicopter);

Console.WriteLine(
    $"Simulatorn har {units.Count} enheter: {personnel} personal, {groundVehicles} markfordon, {helicopters} helikoptrar.");
return 0;
