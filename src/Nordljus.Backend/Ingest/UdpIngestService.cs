using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Nordljus.Adapter.Nordljus;
using Nordljus.Backend.Situation;
using Nordljus.Core;

namespace Nordljus.Backend.Ingest;

/// <summary>
/// Tar emot sensormeddelanden över UDP, översätter dem med adaptern, kontrollerar värdena
/// och skickar giltiga positioner till alla anslutna klienter. Ett trasigt eller orimligt
/// meddelande loggas och hoppas över – tjänsten får aldrig krascha, för då stoppas hela backend.
/// </summary>
public sealed class UdpIngestService(
    IOptions<UdpIngestOptions> options,
    TrackValidator validator,
    IHubContext<SituationHub, ISituationClient> hub,
    ILogger<UdpIngestService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var port = options.Value.Port;

        UdpClient udp;
        try
        {
            udp = new UdpClient(new IPEndPoint(IPAddress.Any, port));
        }
        catch (SocketException ex)
        {
            // Utan port finns ingen lägesbild – då är det bättre att backend stannar med ett tydligt fel.
            logger.LogCritical(ex, "Kan inte lyssna på UDP-port {Port}. Är porten redan upptagen?", port);
            throw;
        }

        using (udp)
        {
            logger.LogInformation("Tar emot sensordata på UDP-port {Port}", port);

            while (!stoppingToken.IsCancellationRequested)
            {
                UdpReceiveResult received;
                try
                {
                    received = await udp.ReceiveAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (SocketException ex)
                {
                    logger.LogWarning(ex, "Fel vid mottagning på UDP-port {Port}. Fortsätter.", port);
                    continue;
                }

                try
                {
                    await HandleAsync(received);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Kunde inte hantera meddelandet från {Source}. Fortsätter.", received.RemoteEndPoint);
                }
            }
        }
    }

    private async Task HandleAsync(UdpReceiveResult received)
    {
        if (!NordljusAdapter.TryParse(received.Buffer, out var update, out var error))
        {
            // Rådatan loggas inte: den kan vara stor eller innehålla vad som helst. Den sparas i
            // händelseloggen när den finns (ingest-kortet).
            logger.LogWarning(
                "Trasigt meddelande från {Source} ({Bytes} byte) kastades: {Reason}",
                received.RemoteEndPoint, received.Buffer.Length, error);
            return;
        }

        if (!validator.IsValid(update, out var reason))
        {
            logger.LogWarning(
                "Orimlig position för {TrackId} från {Source} kastades: {Reason}",
                update.TrackId, received.RemoteEndPoint, reason);
            return;
        }

        await hub.Clients.All.TracksUpdated([update]);
    }
}
