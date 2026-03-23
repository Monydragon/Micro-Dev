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

    public IReadOnlyList<QueuedIncident> Update(RunState state, double elapsedInGameMinutes, SimulationConfig config)
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

        if (config.GuaranteedJobListingIntervalMinutes > 0 &&
            state.NextGuaranteedJobDeskMinute <= state.DeskMinutesElapsed &&
            state.ActiveJobListing is null &&
            state.ActiveJobApplication is null)
        {
            state.GeneratedJobListingCount++;
            state.NextGuaranteedJobDeskMinute = state.DeskMinutesElapsed + config.GuaranteedJobListingIntervalMinutes;
            queued ??= [];
            queued.Add(new QueuedIncident(
                $"job-dyn-{state.GeneratedJobListingCount}",
                IncidentType.JobListing,
                "A recruiter follow-up lands in the inbox. Another shot opens up."));
        }

        while (config.ModifierIncidentIntervalMinutes > 0 &&
               state.NextModifierDeskMinute <= state.DeskMinutesElapsed)
        {
            state.GeneratedModifierIncidentCount++;
            state.NextModifierDeskMinute += config.ModifierIncidentIntervalMinutes +
                                            GetModifierIntervalOffset(state);
            var incidentType = PickDeskEventType(state);
            var description = GetDeskEventDescription(incidentType);
            queued ??= [];
            queued.Add(new QueuedIncident(
                $"mod-{state.GeneratedModifierIncidentCount}",
                incidentType,
                description));
        }

        return queued is null ? Array.Empty<QueuedIncident>() : queued;
    }

    private static IncidentType PickDeskEventType(RunState state)
    {
        var seed = CreateSeed(state.RunSeed, state.GeneratedModifierIncidentCount);
        return (seed % 8) switch
        {
            0 => IncidentType.DeepWorkWindow,
            1 => IncidentType.ContextSwitch,
            2 => IncidentType.CoffeeBounce,
            3 => IncidentType.MentorNudge,
            4 => IncidentType.ExpenseSpike,
            5 => IncidentType.RubberDuckInsight,
            6 => IncidentType.MicroSale,
            _ => IncidentType.DoomscrollSpiral,
        };
    }

    private static string GetDeskEventDescription(IncidentType incidentType)
    {
        return incidentType switch
        {
            IncidentType.DeepWorkWindow => "A clean deep-work pocket opens up. The desk finally clicks into place.",
            IncidentType.ContextSwitch => "Pings, tabs, and context switching shred the next block of focus.",
            IncidentType.CoffeeBounce => "You find one last good coffee. The next stretch suddenly feels survivable again.",
            IncidentType.MentorNudge => "A mentor message lands with a sharp note about what recruiters actually notice.",
            IncidentType.ExpenseSpike => "A surprise expense hits the account and the whole desk gets tighter.",
            IncidentType.RubberDuckInsight => "You explain the problem out loud and the shape of the fix suddenly clicks.",
            IncidentType.MicroSale => "A tiny payout from older work lands out of nowhere and buys back a little breathing room.",
            IncidentType.DoomscrollSpiral => "One stray tab turns into a doomscroll spiral and drains the next block of energy.",
            _ => string.Empty,
        };
    }

    private static int GetModifierIntervalOffset(RunState state)
    {
        var seed = CreateSeed(state.RunSeed, state.GeneratedModifierIncidentCount + 97);
        return seed % 75;
    }

    private static int CreateSeed(int runSeed, int value)
    {
        unchecked
        {
            var seed = runSeed == 0 ? 17 : runSeed;
            return ((seed * 31) + value) & int.MaxValue;
        }
    }

    private sealed record ScheduledIncident(
        string Id,
        IncidentType Type,
        double TriggerDeskMinutes,
        string Description);
}
