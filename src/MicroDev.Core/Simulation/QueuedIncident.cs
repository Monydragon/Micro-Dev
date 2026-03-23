namespace MicroDev.Core.Simulation;

public readonly record struct QueuedIncident(
    string Id,
    IncidentType Type,
    string Description);
