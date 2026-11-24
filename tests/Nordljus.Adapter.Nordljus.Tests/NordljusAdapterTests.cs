using System.Text;
using Nordljus.Contracts;

namespace Nordljus.Adapter.Nordljus.Tests;

public sealed class NordljusAdapterTests
{
    private const string _validMessage =
        """{"unitId":"jarn-1","name":"Järn 1","kind":"groundVehicle","lat":57.9,"lon":15.3,"timestamp":"2026-11-24T10:00:00Z"}""";

    [Fact]
    public void Giltigt_meddelande_blir_ett_TrackUpdate()
    {
        var ok = NordljusAdapter.TryParse(Encoding.UTF8.GetBytes(_validMessage), out var update, out var error);

        Assert.True(ok);
        Assert.Null(error);
        Assert.Equal(
            new TrackUpdate("jarn-1", "Järn 1", TrackType.GroundVehicle, 57.9, 15.3,
                new DateTimeOffset(2026, 11, 24, 10, 0, 0, TimeSpan.Zero)),
            update);
    }

    [Fact]
    public void Tid_med_annan_tidszon_blir_UTC()
    {
        var json = _validMessage.Replace("2026-11-24T10:00:00Z", "2026-11-24T11:00:00+01:00");

        Assert.True(NordljusAdapter.TryParse(Encoding.UTF8.GetBytes(json), out var update, out _));

        Assert.Equal(TimeSpan.Zero, update.ObservedAt.Offset);
        Assert.Equal(10, update.ObservedAt.Hour);
    }

    [Theory]
    [InlineData("personnel", TrackType.Personnel)]
    [InlineData("groundVehicle", TrackType.GroundVehicle)]
    [InlineData("helicopter", TrackType.Helicopter)]
    public void Varje_enhetstyp_oversatts(string kind, TrackType expected)
    {
        var json = _validMessage.Replace("\"groundVehicle\"", $"\"{kind}\"");

        Assert.True(NordljusAdapter.TryParse(Encoding.UTF8.GetBytes(json), out var update, out _));
        Assert.Equal(expected, update.Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData("inte json")]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("""{"name":"Järn 1","kind":"groundVehicle","lat":57.9,"lon":15.3,"timestamp":"2026-11-24T10:00:00Z"}""")]
    [InlineData("""{"unitId":null,"name":"Järn 1","kind":"groundVehicle","lat":57.9,"lon":15.3,"timestamp":"2026-11-24T10:00:00Z"}""")]
    [InlineData("""{"unitId":"","name":"Järn 1","kind":"groundVehicle","lat":57.9,"lon":15.3,"timestamp":"2026-11-24T10:00:00Z"}""")]
    [InlineData("""{"unitId":"jarn-1","name":" ","kind":"groundVehicle","lat":57.9,"lon":15.3,"timestamp":"2026-11-24T10:00:00Z"}""")]
    [InlineData("""{"unitId":"jarn-1","name":"Järn 1","kind":"ubat","lat":57.9,"lon":15.3,"timestamp":"2026-11-24T10:00:00Z"}""")]
    [InlineData("""{"unitId":"jarn-1","name":"Järn 1","kind":1,"lat":57.9,"lon":15.3,"timestamp":"2026-11-24T10:00:00Z"}""")]
    // Enum-konverteraren slår ihop kommaseparerade värden till ett odefinierat tal (1|2 = 3).
    [InlineData("""{"unitId":"jarn-1","name":"Järn 1","kind":"groundVehicle, helicopter","lat":57.9,"lon":15.3,"timestamp":"2026-11-24T10:00:00Z"}""")]
    [InlineData("""{"unitId":"jarn-1","name":"Järn 1","kind":"groundVehicle","lat":"57.9","lon":15.3,"timestamp":"2026-11-24T10:00:00Z"}""")]
    [InlineData("""{"unitId":"jarn-1","name":"Järn 1","kind":"groundVehicle","lat":57.9,"timestamp":"2026-11-24T10:00:00Z"}""")]
    [InlineData("""{"unitId":"jarn-1","name":"Järn 1","kind":"groundVehicle","lat":57.9,"lon":15.3,"timestamp":"igår"}""")]
    public void Trasigt_meddelande_ger_fel_med_orsak_och_kastar_inte(string json)
    {
        var ok = NordljusAdapter.TryParse(Encoding.UTF8.GetBytes(json), out var update, out var error);

        Assert.False(ok);
        Assert.Null(update);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Fact]
    public void Ogiltig_UTF8_ger_fel_och_kastar_inte()
    {
        byte[] payload = [.. "{\"unitId\":\""u8, 0xFF, 0xFE, .. "\"}"u8];

        var ok = NordljusAdapter.TryParse(payload, out _, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
    }
}
