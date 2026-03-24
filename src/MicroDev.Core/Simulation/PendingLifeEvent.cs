namespace MicroDev.Core.Simulation;

public sealed class PendingLifeEvent
{
    public IncidentType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? SubjectName { get; set; }

    public int SubjectScore { get; set; }

    public PendingLifeEvent Clone()
    {
        return new PendingLifeEvent
        {
            Type = Type,
            Title = Title,
            Description = Description,
            SubjectName = SubjectName,
            SubjectScore = SubjectScore,
        };
    }
}
