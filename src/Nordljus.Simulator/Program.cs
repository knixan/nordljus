using System.Globalization;
using System.Net.Sockets;
using System.Text.Json;
using Nordljus.Simulator;

// Tunna skivan: en enhet (Järn 1) kör i en cirkel och skickar sin position över UDP en gång
// per sekund. Fler enheter, rutter, tappad kontakt och trasig data kommer med simulatorkortet.
const string unitId = "jarn-1";

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

if (units.FirstOrDefault(u => u.Id == unitId) is not { } unit)
{
    Console.Error.WriteLine($"Enheten {unitId} finns inte i {path}.");
    return 1;
}

var host = Environment.GetEnvironmentVariable("NORDLJUS_UDP_HOST") ?? "127.0.0.1";
var portText = Environment.GetEnvironmentVariable("NORDLJUS_UDP_PORT") ?? "5300";
if (!int.TryParse(portText, NumberStyles.None, CultureInfo.InvariantCulture, out var port) || port is < 1 or > 65535)
{
    Console.Error.WriteLine($"NORDLJUS_UDP_PORT måste vara ett portnummer (1–65535), inte \"{portText}\".");
    return 1;
}

// Fiktivt område: en skogstrakt utan militär anknytning. Frontend visar samma område
// (SIMULATOR_AREA i frontend/src/app/pages/situation-page.tsx) tills riktig karta finns.
var route = new CircularRoute(new GeoPosition(57.90, 15.30), RadiusMeters: 200, LapTime: TimeSpan.FromMinutes(2));

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // Avsluta snyggt i stället för att döda processen.
    cancellation.Cancel();
};

var time = TimeProvider.System;
var start = time.GetTimestamp();
using var udp = new UdpClient();
using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1), time);

Console.WriteLine($"Simulatorn skickar {unit.Name} till {host}:{port} varje sekund. Avsluta med Ctrl+C.");

try
{
    do
    {
        var position = route.PositionAt(time.GetElapsedTime(start));
        var message = new SensorMessage(unit.Id, unit.Name, unit.Kind, position.Latitude, position.Longitude, time.GetUtcNow());

        try
        {
            await udp.SendAsync(message.ToJsonBytes(), host, port, cancellation.Token);
            Console.WriteLine(FormattableString.Invariant(
                $"{message.Timestamp:HH:mm:ss} {unit.Name} {position.Latitude:F5}, {position.Longitude:F5}"));
        }
        catch (SocketException ex)
        {
            // Backend nere eller värden okänd: fortsätt försöka, som en riktig sensor.
            Console.Error.WriteLine($"Kunde inte skicka till {host}:{port}: {ex.Message}");
        }
    }
    while (await timer.WaitForNextTickAsync(cancellation.Token));
}
catch (OperationCanceledException)
{
    // Ctrl+C.
}

Console.WriteLine("Simulatorn avslutad.");
return 0;
