using Microsoft.AspNetCore.SignalR;

namespace Nordljus.Backend.Situation;

/// <summary>
/// Lägesbilden i realtid. Hubben är tunn: den har inga metoder som klienten anropar.
/// Uppdateringar skickas ut via <c>IHubContext&lt;SituationHub, ISituationClient&gt;</c>
/// från ingest.
/// </summary>
public sealed class SituationHub : Hub<ISituationClient>;
