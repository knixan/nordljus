using Nordljus.Contracts;

namespace Nordljus.Backend.Situation;

/// <summary>
/// Det som servern kan anropa i webbläsaren. Metodnamnet blir meddelandets namn i
/// SignalR, så frontend lyssnar på <c>TracksUpdated</c>.
/// </summary>
public interface ISituationClient
{
    /// <summary>
    /// Nya positioner. Alltid en lista, så att formen inte ändras när hubben börjar
    /// batcha per tick.
    /// </summary>
    Task TracksUpdated(IReadOnlyList<TrackUpdate> updates);
}
