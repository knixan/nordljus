using Nordljus.Contracts;

namespace Nordljus.Core.Tests;

public sealed class TrackValidatorTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 24, 10, 0, 0, TimeSpan.Zero);
    private static readonly TrackValidator _validator = new(new FixedTimeProvider(_now), TimeSpan.FromSeconds(5));

    private static TrackUpdate Jarn1(double latitude = 57.9, double longitude = 15.3, DateTimeOffset? observedAt = null) =>
        new("jarn-1", "Järn 1", TrackType.GroundVehicle, latitude, longitude, observedAt ?? _now);

    [Fact]
    public void Rimlig_position_ar_giltig()
    {
        Assert.True(_validator.IsValid(Jarn1(), out var reason));
        Assert.Null(reason);
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(0)]
    [InlineData(5)]
    public void Tid_nyss_eller_lite_fore_klockan_ar_giltig(int secondsFromNow) =>
        Assert.True(_validator.IsValid(Jarn1(observedAt: _now.AddSeconds(secondsFromNow)), out _));

    // En enda position långt fram i tiden fick tidigare markören att frysa med "0 s sedan",
    // eftersom frontend ignorerar allt som är äldre än det som visas (#5, granska-kritiskt).
    [Theory]
    [InlineData(6)]
    [InlineData(3600)]
    public void Tid_i_framtiden_over_marginalen_ar_ogiltig(int secondsFromNow)
    {
        Assert.False(_validator.IsValid(Jarn1(observedAt: _now.AddSeconds(secondsFromNow)), out var reason));
        Assert.Contains("framtiden", reason);
    }

    [Fact]
    public void Ar_2099_ar_ogiltigt() =>
        Assert.False(_validator.IsValid(Jarn1(observedAt: new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero)), out _));

    [Theory]
    [InlineData(90.0001, 15.3)]
    [InlineData(-90.0001, 15.3)]
    [InlineData(57.9, 180.0001)]
    [InlineData(57.9, -180.0001)]
    [InlineData(double.PositiveInfinity, 15.3)]
    [InlineData(57.9, double.NegativeInfinity)]
    [InlineData(double.NaN, 15.3)]
    public void Position_utanfor_jorden_ar_ogiltig(double latitude, double longitude)
    {
        Assert.False(_validator.IsValid(Jarn1(latitude, longitude), out var reason));
        Assert.False(string.IsNullOrWhiteSpace(reason));
    }

    [Theory]
    [InlineData(90, 180)]
    [InlineData(-90, -180)]
    public void Granserna_ar_giltiga(double latitude, double longitude) =>
        Assert.True(_validator.IsValid(Jarn1(latitude, longitude), out _));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
