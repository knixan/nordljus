namespace Nordljus.Simulator;

/// <summary>En fiktiv enhet som simulatorn låtsas vara sensor för.</summary>
public sealed record SimulatedUnit(string Id, string Name, UnitKind Kind);

public enum UnitKind
{
    Personnel,
    GroundVehicle,
    Helicopter,
}
