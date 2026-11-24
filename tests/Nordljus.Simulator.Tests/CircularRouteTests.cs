namespace Nordljus.Simulator.Tests;

public sealed class CircularRouteTests
{
    private static readonly GeoPosition _center = new(57.90, 15.30);
    private static readonly CircularRoute _route = new(_center, RadiusMeters: 200, LapTime: TimeSpan.FromMinutes(2));

    [Fact]
    public void Startar_rakt_oster_om_centrum()
    {
        var start = _route.PositionAt(TimeSpan.Zero);

        Assert.Equal(_center.Latitude, start.Latitude, precision: 9);
        Assert.True(start.Longitude > _center.Longitude);
    }

    [Fact]
    public void Efter_ett_halvt_varv_ar_enheten_vaster_om_centrum()
    {
        var half = _route.PositionAt(TimeSpan.FromMinutes(1));

        Assert.Equal(_center.Latitude, half.Latitude, precision: 9);
        Assert.True(half.Longitude < _center.Longitude);
    }

    [Fact]
    public void Ett_helt_varv_slutar_dar_det_borjade()
    {
        var start = _route.PositionAt(TimeSpan.Zero);
        var lap = _route.PositionAt(TimeSpan.FromMinutes(2));

        Assert.Equal(start.Latitude, lap.Latitude, precision: 9);
        Assert.Equal(start.Longitude, lap.Longitude, precision: 9);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(17)]
    [InlineData(45)]
    [InlineData(90)]
    public void Avstandet_till_centrum_ar_radien(int seconds)
    {
        var position = _route.PositionAt(TimeSpan.FromSeconds(seconds));

        // Inom en meter räcker – ekvirektangulär approximation på 200 m.
        Assert.InRange(DistanceInMeters(_center, position), 199, 201);
    }

    [Fact]
    public void En_sekund_ar_ungefar_tio_meter_med_38_km_i_timmen()
    {
        var a = _route.PositionAt(TimeSpan.FromSeconds(10));
        var b = _route.PositionAt(TimeSpan.FromSeconds(11));

        // 2π · 200 m / 120 s ≈ 10,5 m/s ≈ 38 km/h – rimligt för ett markfordon.
        Assert.InRange(DistanceInMeters(a, b), 10, 11);
    }

    // Haversine – oberoende av hur rutten räknar, så att testet inte bara upprepar koden.
    private static double DistanceInMeters(GeoPosition a, GeoPosition b)
    {
        const double earthRadius = 6_371_000;
        static double Rad(double degrees) => degrees * Math.PI / 180;

        var dLat = Rad(b.Latitude - a.Latitude);
        var dLon = Rad(b.Longitude - a.Longitude);
        var h = Math.Pow(Math.Sin(dLat / 2), 2)
            + Math.Cos(Rad(a.Latitude)) * Math.Cos(Rad(b.Latitude)) * Math.Pow(Math.Sin(dLon / 2), 2);
        return 2 * earthRadius * Math.Asin(Math.Sqrt(h));
    }
}
