namespace MicroDev.Core.Simulation;

public sealed class IncidentScheduler
{
    private static readonly TimelineWindow[] TimelineWindows =
    [
        new("desk-1", IncidentCategory.Warmup, 105, 165),
        new("desk-2", IncidentCategory.Disruption, 190, 280),
        new("desk-3", IncidentCategory.Crisis, 290, 385),
        new("job-1", IncidentCategory.JobOpportunity, 400, 495),
        new("life-1", IncidentCategory.Life, 520, 620),
        new("desk-4", IncidentCategory.BoostOrDrag, 620, 730),
        new("desk-5", IncidentCategory.Disruption, 735, 860),
        new("relationship-1", IncidentCategory.Relationship, 820, 950),
        new("job-2", IncidentCategory.JobOpportunity, 955, 1095),
    ];
    private static readonly Dictionary<int, IReadOnlyList<ScheduledIncident>> TimelineBySeed = [];

    public IReadOnlyList<QueuedIncident> Update(RunState state, double elapsedInGameMinutes, SimulationConfig config)
    {
        if (state.Status != RunStatus.InProgress ||
            state.FirstCoinDecisionPending ||
            state.PendingLifeEvent is not null ||
            elapsedInGameMinutes <= 0)
        {
            return Array.Empty<QueuedIncident>();
        }

        var startingDeskMinutes = state.DeskMinutesElapsed;
        state.DeskMinutesElapsed += elapsedInGameMinutes;

        List<QueuedIncident>? queued = null;

        foreach (var scheduledIncident in GetTimeline(state.RunSeed))
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
            var incidentType = ResolveTimelineIncidentType(state, scheduledIncident);
            queued ??= [];
            queued.Add(new QueuedIncident(
                scheduledIncident.Id,
                incidentType,
                ProceduralRunContent.GetIncidentDescription(state.RunSeed, scheduledIncident.Id, incidentType)));
        }

        var guaranteedJobInterval = GetGuaranteedJobInterval(state, config);
        if (guaranteedJobInterval > 0 &&
            state.NextGuaranteedJobDeskMinute <= state.DeskMinutesElapsed &&
            state.ActiveJobListing is null &&
            state.ActiveJobApplication is null)
        {
            state.GeneratedJobListingCount++;
            state.NextGuaranteedJobDeskMinute = state.DeskMinutesElapsed + guaranteedJobInterval;
            queued ??= [];
            queued.Add(new QueuedIncident(
                $"job-dyn-{state.GeneratedJobListingCount}",
                IncidentType.JobListing,
                ProceduralRunContent.GetIncidentDescription(
                    state.RunSeed,
                    $"job-dyn-{state.GeneratedJobListingCount}",
                    IncidentType.JobListing)));
        }

        var modifierInterval = GetModifierIncidentInterval(state, config);
        while (modifierInterval > 0 &&
               state.NextModifierDeskMinute <= state.DeskMinutesElapsed)
        {
            state.GeneratedModifierIncidentCount++;
            state.NextModifierDeskMinute += modifierInterval +
                                            GetModifierIntervalOffset(state);
            var incidentType = PickDeskEventType(state);
            queued ??= [];
            queued.Add(new QueuedIncident(
                $"mod-{state.GeneratedModifierIncidentCount}",
                incidentType,
                ProceduralRunContent.GetIncidentDescription(
                    state.RunSeed,
                    $"mod-{state.GeneratedModifierIncidentCount}",
                    incidentType)));
        }

        while (state.PublishedAppCount > 0 &&
               config.PublishedAppSaleIntervalMinMinutes > 0 &&
               state.NextPublishedAppSaleDeskMinute <= state.DeskMinutesElapsed)
        {
            var saleNumber = state.PublishedAppSaleCount + 1;
            var triggerMinute = state.NextPublishedAppSaleDeskMinute;
            state.PublishedAppSaleCount = saleNumber;
            state.NextPublishedAppSaleDeskMinute = triggerMinute +
                                                   GetPublishedSaleInterval(state, config, saleNumber + 1);
            queued ??= [];
            queued.Add(new QueuedIncident(
                $"app-sale-{saleNumber}",
                IncidentType.PublishedAppSale,
                ProceduralRunContent.GetIncidentDescription(
                    state.RunSeed,
                    $"app-sale-{saleNumber}",
                    IncidentType.PublishedAppSale)));
        }

        return queued is null ? Array.Empty<QueuedIncident>() : queued;
    }

    private static IReadOnlyList<ScheduledIncident> GetTimeline(int runSeed)
    {
        var normalizedSeed = runSeed == 0 ? 17 : runSeed;
        if (TimelineBySeed.TryGetValue(normalizedSeed, out var cached))
        {
            return cached;
        }

        var built = TimelineWindows
            .Select(window => new ScheduledIncident(
                window.Id,
                window.Category,
                RollInRange(normalizedSeed, $"{window.Id}:minute", window.MinimumDeskMinutes, window.MaximumDeskMinutes)))
            .OrderBy(static incident => incident.TriggerDeskMinutes)
            .ToArray();
        TimelineBySeed[normalizedSeed] = built;
        return built;
    }

    private static IncidentType ResolveTimelineIncidentType(RunState state, ScheduledIncident incident)
    {
        return incident.Category switch
        {
            IncidentCategory.Warmup => PickFrom(
                state.RunSeed,
                $"{incident.Id}:type",
                state.GameplayMode == GameplayLoopMode.Founder
                    ? [IncidentType.DeepWorkWindow, IncidentType.RubberDuckInsight, IncidentType.CoffeeBounce, IncidentType.ExpenseSpike]
                    : state.GameplayMode == GameplayLoopMode.Indie
                    ? [IncidentType.DeepWorkWindow, IncidentType.RubberDuckInsight, IncidentType.CoffeeBounce, IncidentType.IndieFundingSwing]
                    : state.GameplayMode == GameplayLoopMode.Corporate
                        ? [IncidentType.BossCheckIn, IncidentType.CoworkerInterruption, IncidentType.CoffeeBounce, IncidentType.DeepWorkWindow]
                        : [IncidentType.DeepWorkWindow, IncidentType.CoffeeBounce, IncidentType.RubberDuckInsight, IncidentType.MentorNudge]),
            IncidentCategory.Disruption => PickFrom(
                state.RunSeed,
                $"{incident.Id}:type",
                state.GameplayMode == GameplayLoopMode.Corporate
                    ? [IncidentType.ContextSwitch, IncidentType.TechDebtBug, IncidentType.CoworkerInterruption, IncidentType.DoomscrollSpiral]
                    : state.GameplayMode == GameplayLoopMode.Founder
                        ? [IncidentType.TechDebtBug, IncidentType.ExpenseSpike, IncidentType.CatInterruption, IncidentType.ContextSwitch]
                    : [IncidentType.CatInterruption, IncidentType.TechDebtBug, IncidentType.ContextSwitch, IncidentType.DoomscrollSpiral]),
            IncidentCategory.Crisis => PickFrom(
                state.RunSeed,
                $"{incident.Id}:type",
                state.IsRealisticMode
                    ? [IncidentType.ComputerFreeze, IncidentType.ExpenseSpike, IncidentType.TechDebtBug, IncidentType.CatInterruption]
                    : [IncidentType.ComputerFreeze, IncidentType.TechDebtBug, IncidentType.ExpenseSpike, IncidentType.CatInterruption]),
            IncidentCategory.JobOpportunity => state.GameplayMode switch
            {
                GameplayLoopMode.Interview => IncidentType.JobListing,
                GameplayLoopMode.Corporate => IncidentType.CoworkerInterruption,
                GameplayLoopMode.Indie => IncidentType.IndieFundingSwing,
                _ => IncidentType.ExpenseSpike,
            },
            IncidentCategory.Life => state.HasFoundLove
                ? IncidentType.PartnerCheckIn
                : PickFrom(
                    state.RunSeed,
                    $"{incident.Id}:type",
                    state.IsRealisticMode
                        ? [IncidentType.OnlineMatch, IncidentType.StreamingBinge, IncidentType.OnlineMatch]
                        : [IncidentType.StreamingBinge, IncidentType.OnlineMatch, IncidentType.StreamingBinge]),
            IncidentCategory.Relationship => state.HasFoundLove
                ? IncidentType.PartnerCheckIn
                : IncidentType.OnlineMatch,
            _ => PickFrom(
                state.RunSeed,
                $"{incident.Id}:type",
                state.GameplayMode == GameplayLoopMode.Founder
                    ? [IncidentType.DeepWorkWindow, IncidentType.ExpenseSpike, IncidentType.MicroSale, IncidentType.CoffeeBounce, IncidentType.StreamingBinge]
                    : state.GameplayMode == GameplayLoopMode.Indie
                    ? [IncidentType.DeepWorkWindow, IncidentType.IndieFundingSwing, IncidentType.RubberDuckInsight, IncidentType.DoomscrollSpiral, IncidentType.CoffeeBounce]
                    : [IncidentType.DeepWorkWindow, IncidentType.ContextSwitch, IncidentType.BossCheckIn, IncidentType.CoworkerInterruption, IncidentType.DoomscrollSpiral]),
        };
    }

    private static IncidentType PickDeskEventType(RunState state)
    {
        var choices = state.HasFoundLove
            ? new[]
            {
                IncidentType.DeepWorkWindow,
                IncidentType.ContextSwitch,
                IncidentType.CoffeeBounce,
                IncidentType.MentorNudge,
                IncidentType.ExpenseSpike,
                IncidentType.RubberDuckInsight,
                IncidentType.MicroSale,
                IncidentType.DoomscrollSpiral,
                IncidentType.ComputerFreeze,
                IncidentType.StreamingBinge,
                IncidentType.PartnerCheckIn,
            }
            : new[]
            {
                IncidentType.DeepWorkWindow,
                IncidentType.ContextSwitch,
                IncidentType.CoffeeBounce,
                IncidentType.MentorNudge,
                IncidentType.ExpenseSpike,
                IncidentType.RubberDuckInsight,
                IncidentType.MicroSale,
                IncidentType.DoomscrollSpiral,
                IncidentType.ComputerFreeze,
                IncidentType.StreamingBinge,
                IncidentType.OnlineMatch,
            };

        if (state.GameplayMode == GameplayLoopMode.Corporate)
        {
            choices =
            [
                IncidentType.BossCheckIn,
                IncidentType.CoworkerInterruption,
                IncidentType.ContextSwitch,
                IncidentType.MentorNudge,
                IncidentType.TechDebtBug,
                IncidentType.CoffeeBounce,
                IncidentType.ExpenseSpike,
                IncidentType.DoomscrollSpiral,
                state.HasFoundLove ? IncidentType.PartnerCheckIn : IncidentType.OnlineMatch,
            ];
        }
        else if (state.GameplayMode == GameplayLoopMode.Indie)
        {
            choices =
            [
                IncidentType.DeepWorkWindow,
                IncidentType.IndieFundingSwing,
                IncidentType.RubberDuckInsight,
                IncidentType.MicroSale,
                IncidentType.CoffeeBounce,
                IncidentType.CatInterruption,
                IncidentType.StreamingBinge,
                state.HasFoundLove ? IncidentType.PartnerCheckIn : IncidentType.OnlineMatch,
            ];
        }
        else if (state.GameplayMode == GameplayLoopMode.Founder)
        {
            choices =
            [
                IncidentType.DeepWorkWindow,
                IncidentType.ExpenseSpike,
                IncidentType.MicroSale,
                IncidentType.TechDebtBug,
                IncidentType.CatInterruption,
                IncidentType.CoffeeBounce,
                IncidentType.StreamingBinge,
                state.HasFoundLove ? IncidentType.PartnerCheckIn : IncidentType.OnlineMatch,
            ];
        }

        return PickFrom(state.RunSeed, $"modifier:{state.GeneratedModifierIncidentCount}", choices);
    }

    private static double GetGuaranteedJobInterval(RunState state, SimulationConfig config)
    {
        if (state.GameplayMode != GameplayLoopMode.Interview)
        {
            return 0;
        }

        var interval = config.GuaranteedJobListingIntervalMinutes;

        if (state.IsRealisticMode)
        {
            interval += 30;
        }

        return Math.Max(120, interval);
    }

    private static double GetModifierIncidentInterval(RunState state, SimulationConfig config)
    {
        var interval = config.ModifierIncidentIntervalMinutes;
        if (state.GameplayMode == GameplayLoopMode.Corporate)
        {
            interval -= 15;
        }
        else if (state.GameplayMode == GameplayLoopMode.Indie)
        {
            interval += 10;
        }
        else if (state.GameplayMode == GameplayLoopMode.Founder)
        {
            interval += 5;
        }

        if (state.IsRealisticMode)
        {
            interval -= 10;
        }

        return Math.Max(120, interval);
    }

    private static int GetModifierIntervalOffset(RunState state)
    {
        var seed = CreateSeed(state.RunSeed, state.GeneratedModifierIncidentCount + 97);
        return seed % 75;
    }

    private static int GetPublishedSaleInterval(RunState state, SimulationConfig config, int saleNumber)
    {
        var saleOffset = state.GameplayMode switch
        {
            GameplayLoopMode.Corporate => 30,
            GameplayLoopMode.Indie => -30,
            GameplayLoopMode.Founder => -45,
            _ => 0,
        };
        if (state.IsRealisticMode)
        {
            saleOffset += 15;
        }

        var minimum = (int)Math.Round(Math.Min(config.PublishedAppSaleIntervalMinMinutes, config.PublishedAppSaleIntervalMaxMinutes) + saleOffset);
        var maximum = (int)Math.Round(Math.Max(config.PublishedAppSaleIntervalMinMinutes, config.PublishedAppSaleIntervalMaxMinutes) + saleOffset);
        if (maximum <= minimum)
        {
            return Math.Max(1, minimum);
        }

        var seed = CreateSeed(state.RunSeed, saleNumber + 211);
        return minimum + (seed % ((maximum - minimum) + 1));
    }

    private static int CreateSeed(int runSeed, int value)
    {
        unchecked
        {
            var seed = runSeed == 0 ? 17 : runSeed;
            return ((seed * 31) + value) & int.MaxValue;
        }
    }

    private static int CreateSeed(int runSeed, string key)
    {
        unchecked
        {
            var seed = runSeed == 0 ? 17 : runSeed;
            foreach (var character in key)
            {
                seed = (seed * 31) + character;
            }

            return seed & int.MaxValue;
        }
    }

    private static int RollInRange(int runSeed, string key, int minimum, int maximum)
    {
        if (maximum <= minimum)
        {
            return minimum;
        }

        var seed = CreateSeed(runSeed, key);
        return minimum + (seed % ((maximum - minimum) + 1));
    }

    private static IncidentType PickFrom(int runSeed, string key, params IncidentType[] values)
    {
        if (values.Length == 0)
        {
            return IncidentType.None;
        }

        var seed = CreateSeed(runSeed, key);
        return values[seed % values.Length];
    }

    private sealed record ScheduledIncident(
        string Id,
        IncidentCategory Category,
        double TriggerDeskMinutes);

    private sealed record TimelineWindow(
        string Id,
        IncidentCategory Category,
        int MinimumDeskMinutes,
        int MaximumDeskMinutes);

    private enum IncidentCategory
    {
        Warmup = 0,
        Disruption,
        Crisis,
        JobOpportunity,
        Life,
        Relationship,
        BoostOrDrag,
    }
}
