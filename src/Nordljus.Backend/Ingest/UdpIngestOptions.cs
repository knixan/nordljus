using System.ComponentModel.DataAnnotations;

namespace Nordljus.Backend.Ingest;

/// <summary>Inställningar för UDP-ingest, från sektionen <c>Ingest:Udp</c>.</summary>
public sealed class UdpIngestOptions
{
    public const string Section = "Ingest:Udp";

    /// <summary>Porten som sensorerna skickar till.</summary>
    [Range(1, 65535)]
    public int Port { get; init; }
}
