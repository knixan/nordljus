using System.Diagnostics.CodeAnalysis;
using Nordljus.Contracts;

namespace Nordljus.Core;

/// <summary>
/// Kontrollerar att en position är rimlig innan den visas för operatören. Gäller alla team:
/// adaptern kontrollerar formatet, den här klassen värdena.
/// </summary>
/// <param name="time">Klockan. I tester en styrd klocka.</param>
/// <param name="maxClockSkew">
/// Hur långt fram i tiden en sensors klocka får gå. En position längre fram avvisas: den
/// skulle annars se färsk ut och, eftersom äldre positioner ignoreras, frysa markören.
/// </param>
public sealed class TrackValidator(TimeProvider time, TimeSpan maxClockSkew)
{
    public bool IsValid(TrackUpdate update, [NotNullWhen(false)] out string? reason)
    {
        // Jämförelserna är skrivna så att NaN också faller (NaN är aldrig >= eller <= något).
        if (!(update.Latitude >= -90 && update.Latitude <= 90))
        {
            reason = $"Latituden {update.Latitude} ligger utanför -90 till 90.";
            return false;
        }

        if (!(update.Longitude >= -180 && update.Longitude <= 180))
        {
            reason = $"Longituden {update.Longitude} ligger utanför -180 till 180.";
            return false;
        }

        var latest = time.GetUtcNow() + maxClockSkew;
        if (update.ObservedAt > latest)
        {
            reason = $"Tiden {update.ObservedAt:O} ligger i framtiden (mer än {maxClockSkew.TotalSeconds} s).";
            return false;
        }

        reason = null;
        return true;
    }
}
