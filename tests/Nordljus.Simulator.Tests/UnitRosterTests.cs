namespace Nordljus.Simulator.Tests;

public sealed class UnitRosterTests
{
    // units.json kopieras från simulatorn till testets output-mapp via projektreferensen.
    private static IReadOnlyList<SimulatedUnit> LoadStartData() =>
        UnitRoster.Load(Path.Combine(AppContext.BaseDirectory, "units.json"));

    [Fact]
    public void Startdatan_har_24_personal_3_markfordon_och_2_helikoptrar()
    {
        var units = LoadStartData();

        Assert.Equal(24, units.Count(u => u.Kind == UnitKind.Personnel));
        Assert.Equal(3, units.Count(u => u.Kind == UnitKind.GroundVehicle));
        Assert.Equal(2, units.Count(u => u.Kind == UnitKind.Helicopter));
    }

    [Fact]
    public void Alla_id_ar_unika()
    {
        var units = LoadStartData();

        Assert.Equal(units.Count, units.Select(u => u.Id).Distinct().Count());
    }

    [Fact]
    public void Orns_personal_ar_tre_grupper_med_atta_i_varje()
    {
        var personnel = LoadStartData().Where(u => u.Kind == UnitKind.Personnel).ToList();
        Assert.All(personnel, u => Assert.Matches(@"^orn-\d+-\d+$", u.Id));

        var groups = personnel.GroupBy(u => u.Id[..u.Id.LastIndexOf('-')]);

        Assert.Equal(["orn-1", "orn-2", "orn-3"], groups.Select(g => g.Key).Order());
        Assert.All(groups, g => Assert.Equal(8, g.Count()));
    }

    [Theory]
    [InlineData("""[{ "id": "orn-1-1", "name": "Örn 1:1" }]""")]
    [InlineData("""[{ "name": "Örn 1:1", "kind": "personnel" }]""")]
    [InlineData("""[{ "id": null, "name": "Örn 1:1", "kind": "personnel" }]""")]
    [InlineData("""[{ "id": "orn-1-1", "name": "Örn 1:1", "kind": 7 }]""")]
    public void Enhet_med_saknat_eller_ogiltigt_falt_ger_fel(string json) =>
        Assert.ThrowsAny<System.Text.Json.JsonException>(() => LoadFromText(json));

    [Theory]
    [InlineData("[]")]
    [InlineData("""[{ "id": "", "name": "Örn 1:1", "kind": "personnel" }]""")]
    [InlineData("""[{ "id": "orn-1-1", "name": " ", "kind": "personnel" }]""")]
    [InlineData(
        """[{ "id": "orn-1-1", "name": "Örn 1:1", "kind": "personnel" }, { "id": "orn-1-1", "name": "Örn 1:2", "kind": "personnel" }]""")]
    public void Tom_lista_tomma_namn_och_dubbla_id_ger_fel(string json) =>
        Assert.Throws<InvalidDataException>(() => LoadFromText(json));

    private static IReadOnlyList<SimulatedUnit> LoadFromText(string json)
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, json);
            return UnitRoster.Load(path);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
