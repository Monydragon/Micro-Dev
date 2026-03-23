namespace MicroDev.Core.Simulation;

public sealed class IncidentScheduler
{
    private static readonly ScheduledIncident[] IncidentTimeline =
    [
        new("bug-1", IncidentType.TechDebtBug, 120, "A stubborn regression starts eating your momentum."),
        new("cat-1", IncidentType.CatInterruption, 240, "The cat launches onto the desk and blocks the editor."),
        new("job-1", IncidentType.JobListing, 420, "A fresh C# / .NET listing popped up in the inbox."),
        new("bug-2", IncidentType.TechDebtBug, 600, "Another bug slips in while you're rushing."),
        new("cat-2", IncidentType.CatInterruption, 780, "The cat is back and even more convinced this is its workstation."),
        new("job-2", IncidentType.JobListing, 960, "A second .NET job listing appears before the week closes."),
    ];

    public IReadOnlyList<QueuedIncident> Update(RunState state, double elapsedInGameMinutes)
    {
        if (state.Status != RunStatus.InProgress || elapsedInGameMinutes <= 0)
        {
            return Array.Empty<QueuedIncident>();
        }

        var startingDeskMinutes = state.DeskMinutesElapsed;
        state.DeskMinutesElapsed += elapsedInGameMinutes;

        List<QueuedIncident>? queued = null;

        foreach (var scheduledIncident in IncidentTimeline)
        {
            if (state.TriggeredIncidentIds.Contains(scheduledIncident.Id))
            {
                continue;
            }

            if (scheduledIncident.TriggerDeskMinutes <= startingDeskMinutes ||
                scheduledIncident.TriggerDeskMinutes > state.DeskMinutesElapsed)
            {
                continue;
            }

            state.TriggeredIncidentIds.Add(scheduledIncident.Id);
            queued ??= [];
            queued.Add(new QueuedIncident(
                scheduledIncident.Id,
                scheduledIncident.Type,
                scheduledIncident.Description));
        }

        return queued is null ? Array.Empty<QueuedIncident>() : queued;
    }

    private sealed record ScheduledIncident(
        string Id,
        IncidentType Type,
        double TriggerDeskMinutes,
        string Description);
}
