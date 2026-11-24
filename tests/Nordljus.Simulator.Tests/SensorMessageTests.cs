using System.Text.Json;

namespace Nordljus.Simulator.Tests;

public sealed class SensorMessageTests
{
    [Fact]
    public void Meddelandet_har_rasformatets_faltnamn_och_typ_som_text()
    {
        var message = new SensorMessage("jarn-1", "Järn 1", UnitKind.GroundVehicle, 57.9, 15.3,
            new DateTimeOffset(2026, 11, 24, 10, 0, 0, TimeSpan.Zero));

        using var json = JsonDocument.Parse(message.ToJsonBytes());
        var root = json.RootElement;

        Assert.Equal(
            ["unitId", "name", "kind", "lat", "lon", "timestamp"],
            root.EnumerateObject().Select(p => p.Name));
        Assert.Equal("jarn-1", root.GetProperty("unitId").GetString());
        Assert.Equal("Järn 1", root.GetProperty("name").GetString());
        Assert.Equal("groundVehicle", root.GetProperty("kind").GetString());
        Assert.Equal(57.9, root.GetProperty("lat").GetDouble());
        Assert.Equal(15.3, root.GetProperty("lon").GetDouble());
        Assert.Equal("2026-11-24T10:00:00+00:00", root.GetProperty("timestamp").GetString());
    }
}
