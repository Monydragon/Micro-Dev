namespace MicroDev.Core.Simulation;

public static class EfficiencyUpgradeCatalog
{
    private static readonly IReadOnlyList<EfficiencyUpgradeDefinition> Definitions =
    [
        new()
        {
            Type = EfficiencyUpgradeType.MechanicalKeyboard,
            Name = "Mechanical Keyboard",
            FundsCost = 35,
            Description = "Sharper keys and less hesitation. Typing feels noticeably faster the moment it lands on the desk.",
            SummaryEffect = "+1 line per click.",
            BonusLinesPerClick = 1,
        },
        new()
        {
            Type = EfficiencyUpgradeType.SnippetLibrary,
            Name = "Snippet Library",
            FundsCost = 55,
            Description = "Common patterns are saved and ready. Boilerplate stops eating the whole evening.",
            SummaryEffect = "+1 line per click.",
            BonusLinesPerClick = 1,
        },
        new()
        {
            Type = EfficiencyUpgradeType.LintBot,
            Name = "Lint Bot",
            FundsCost = 60,
            Description = "A tiny guardrail for careless mistakes. Clean passes become easier to keep together.",
            SummaryEffect = "+0.20 quality gain per click.",
            BonusQualityGain = 0.20,
        },
        new()
        {
            Type = EfficiencyUpgradeType.PomodoroTimer,
            Name = "Pomodoro Timer",
            FundsCost = 45,
            Description = "A better cadence keeps the session from shredding your focus as quickly.",
            SummaryEffect = "-0.5 focus cost per click.",
            FocusCostReduction = 0.5,
        },
        new()
        {
            Type = EfficiencyUpgradeType.NoiseCancelingHeadphones,
            Name = "Noise-Canceling Headphones",
            FundsCost = 70,
            Description = "Cuts the ambient chaos down to a dull hum so the day leaks less focus while you juggle everything else.",
            SummaryEffect = "-0.01 passive focus drain per in-game minute.",
            PassiveFocusDrainReduction = 0.01,
        },
        new()
        {
            Type = EfficiencyUpgradeType.InterviewNotebook,
            Name = "Interview Notebook",
            FundsCost = 80,
            Description = "A living page of questions, follow-ups, and observations. Starting an application begins with real prep instead of pure adrenaline.",
            SummaryEffect = "+1 interview prep when applications begin.",
            PrepPointsOnApplicationStart = 1,
        },
        new()
        {
            Type = EfficiencyUpgradeType.AutoFormatter,
            Name = "Auto Formatter",
            FundsCost = 68,
            Description = "A reliable formatting pass keeps the codebase readable and makes clean examples easier to surface on demand.",
            SummaryEffect = "+0.15 quality gain per click.",
            BonusQualityGain = 0.15,
        },
        new()
        {
            Type = EfficiencyUpgradeType.MacroPad,
            Name = "Macro Pad",
            FundsCost = 92,
            Description = "Tiny repeatable shortcuts add up fast. Common editor actions stop taking a whole extra breath every time.",
            SummaryEffect = "+1 line per click.",
            BonusLinesPerClick = 1,
        },
        new()
        {
            Type = EfficiencyUpgradeType.DeskPlant,
            Name = "Desk Plant",
            FundsCost = 38,
            Description = "A tiny bit of living color softens the room and makes the whole grind feel less sterile.",
            SummaryEffect = "+0.12 sanity regen per in-game hour.",
            PassiveSanityRegenPerInGameMinute = 0.002,
        },
        new()
        {
            Type = EfficiencyUpgradeType.MealPrepContainers,
            Name = "Meal Prep Containers",
            FundsCost = 44,
            Description = "Portioning and prep stop dinner from becoming a total production. Homemade food gets faster and cheaper.",
            SummaryEffect = "-$2 food cost and -18m on home-cooked meals.",
            FoodCostReduction = 2,
            HomeCookDurationReductionMinutes = 18,
        },
        new()
        {
            Type = EfficiencyUpgradeType.CourierPrime,
            Name = "Courier Prime",
            FundsCost = 52,
            Description = "Saved delivery profiles and a favorite driver network shave time off every food order that comes from outside.",
            SummaryEffect = "-8m on delivery meals.",
            FoodDeliveryDurationReductionMinutes = 8,
        },
        new()
        {
            Type = EfficiencyUpgradeType.BugTriageBoard,
            Name = "Bug Triage Board",
            FundsCost = 58,
            Description = "Live regressions get routed faster because the fix path is already written down before panic kicks in.",
            SummaryEffect = "-2 focus to squash bugs.",
            BugSquashFocusCostReduction = 2,
        },
        new()
        {
            Type = EfficiencyUpgradeType.FocusLamp,
            Name = "Focus Lamp",
            FundsCost = 64,
            Description = "A deliberate lighting cue helps you drop back into the work instead of spending every click reacquiring context.",
            SummaryEffect = "-0.35 focus cost per click.",
            FocusCostReduction = 0.35,
        },
        new()
        {
            Type = EfficiencyUpgradeType.HydrationHabit,
            Name = "Hydration Habit",
            FundsCost = 48,
            Description = "The water bottle actually stays filled now, which turns out to matter when the day gets long.",
            SummaryEffect = "-0.008 passive focus drain per in-game minute.",
            PassiveFocusDrainReduction = 0.008,
        },
        new()
        {
            Type = EfficiencyUpgradeType.DualMonitorArm,
            Name = "Dual Monitor Arm",
            FundsCost = 72,
            Description = "Docs, logs, and code finally live side by side. Context stops hiding behind tabs.",
            SummaryEffect = "+1 line per click.",
            BonusLinesPerClick = 1,
        },
        new()
        {
            Type = EfficiencyUpgradeType.SearchIndexer,
            Name = "Search Indexer",
            FundsCost = 86,
            Description = "Fast project-wide lookup turns guesswork into a quick jump straight to the right file.",
            SummaryEffect = "+1 line per click.",
            BonusLinesPerClick = 1,
        },
        new()
        {
            Type = EfficiencyUpgradeType.TestHarnessSandbox,
            Name = "Test Harness Sandbox",
            FundsCost = 78,
            Description = "A clean repro lane makes it easier to tighten behavior without smearing bugs everywhere else.",
            SummaryEffect = "+0.18 quality gain per click.",
            BonusQualityGain = 0.18,
        },
        new()
        {
            Type = EfficiencyUpgradeType.SnapshotSuite,
            Name = "Snapshot Suite",
            FundsCost = 88,
            Description = "Golden outputs make visual regressions obvious before the whole release starts drifting.",
            SummaryEffect = "+0.18 quality gain per click.",
            BonusQualityGain = 0.18,
        },
        new()
        {
            Type = EfficiencyUpgradeType.PreCommitHooks,
            Name = "Pre-Commit Hooks",
            FundsCost = 74,
            Description = "The quick automated checks catch messy edges before they slip into the next coding pass.",
            SummaryEffect = "+0.12 quality gain per click.",
            BonusQualityGain = 0.12,
        },
        new()
        {
            Type = EfficiencyUpgradeType.ReviewChecklist,
            Name = "Review Checklist",
            FundsCost = 58,
            Description = "A tiny final pass on naming, edge cases, and cleanup keeps each session a little more coherent.",
            SummaryEffect = "+0.10 quality gain per click.",
            BonusQualityGain = 0.10,
        },
        new()
        {
            Type = EfficiencyUpgradeType.StandingDeskMat,
            Name = "Standing Desk Mat",
            FundsCost = 64,
            Description = "Long sessions stop feeling like pure attrition, so each click asks slightly less out of you.",
            SummaryEffect = "-0.25 focus cost per click.",
            FocusCostReduction = 0.25,
        },
        new()
        {
            Type = EfficiencyUpgradeType.ErgonomicChair,
            Name = "Ergonomic Chair",
            FundsCost = 92,
            Description = "Better posture means less of the day gets spent wrestling the chair instead of the code.",
            SummaryEffect = "-0.20 focus cost per click.",
            FocusCostReduction = 0.20,
        },
        new()
        {
            Type = EfficiencyUpgradeType.LoFiPlaylist,
            Name = "Lo-Fi Playlist",
            FundsCost = 42,
            Description = "A reliable groove softens the ambient friction and helps long desk stretches stay steady.",
            SummaryEffect = "-0.005 passive focus drain per in-game minute.",
            PassiveFocusDrainReduction = 0.005,
        },
        new()
        {
            Type = EfficiencyUpgradeType.WindowAirConditioner,
            Name = "Window Air Conditioner",
            FundsCost = 96,
            Description = "The room finally stops cooking you through the afternoon, and the whole desk leaks less energy.",
            SummaryEffect = "-0.005 passive focus drain per in-game minute.",
            PassiveFocusDrainReduction = 0.005,
        },
        new()
        {
            Type = EfficiencyUpgradeType.TeaThermos,
            Name = "Tea Thermos",
            FundsCost = 46,
            Description = "A warm cup within reach takes the edge off the grind without turning into a full break.",
            SummaryEffect = "+0.09 sanity regen per in-game hour.",
            PassiveSanityRegenPerInGameMinute = 0.0015,
        },
        new()
        {
            Type = EfficiencyUpgradeType.BlueLightGlasses,
            Name = "Blue-Light Glasses",
            FundsCost = 40,
            Description = "Late-night screens stay a little less brutal, which matters when the run keeps going past dinner.",
            SummaryEffect = "+0.07 sanity regen per in-game hour.",
            PassiveSanityRegenPerInGameMinute = 0.0012,
        },
        new()
        {
            Type = EfficiencyUpgradeType.InterviewFlashCards,
            Name = "Interview Flash Cards",
            FundsCost = 54,
            Description = "Short repetitions turn common recruiter questions into something you can actually answer on command.",
            SummaryEffect = "+1 interview prep when applications begin.",
            PrepPointsOnApplicationStart = 1,
        },
        new()
        {
            Type = EfficiencyUpgradeType.OfferSpreadsheet,
            Name = "Offer Spreadsheet",
            FundsCost = 62,
            Description = "Tracking requirements, follow-ups, and tradeoffs makes every application start with cleaner intent.",
            SummaryEffect = "+1 interview prep when applications begin.",
            PrepPointsOnApplicationStart = 1,
        },
        new()
        {
            Type = EfficiencyUpgradeType.GroceryStaples,
            Name = "Grocery Staples",
            FundsCost = 36,
            Description = "Rice, sauce, eggs, and pantry basics keep the food loop from turning expensive every single day.",
            SummaryEffect = "-$1.5 food cost.",
            FoodCostReduction = 1.5,
        },
        new()
        {
            Type = EfficiencyUpgradeType.RiceCooker,
            Name = "Rice Cooker",
            FundsCost = 58,
            Description = "A dependable side stops home-cooked meals from eating the whole evening and trims the grocery hit too.",
            SummaryEffect = "-$1 food cost and -12m on home-cooked meals.",
            FoodCostReduction = 1,
            HomeCookDurationReductionMinutes = 12,
        },
        new()
        {
            Type = EfficiencyUpgradeType.DeliveryFavorites,
            Name = "Delivery Favorites",
            FundsCost = 48,
            Description = "Saved repeat orders mean fewer taps, fewer mistakes, and faster handoff when the desk is on fire.",
            SummaryEffect = "-6m on delivery meals.",
            FoodDeliveryDurationReductionMinutes = 6,
        },
        new()
        {
            Type = EfficiencyUpgradeType.LunchCalendar,
            Name = "Lunch Calendar",
            FundsCost = 44,
            Description = "Actually planning meals ahead turns home cooking from a mood into a repeatable system.",
            SummaryEffect = "-10m on home-cooked meals.",
            HomeCookDurationReductionMinutes = 10,
        },
        new()
        {
            Type = EfficiencyUpgradeType.IncidentRunbook,
            Name = "Incident Runbook",
            FundsCost = 52,
            Description = "When a bug breaks loose, the first response path is already written down instead of improvised under stress.",
            SummaryEffect = "-1 focus to squash bugs.",
            BugSquashFocusCostReduction = 1,
        },
        new()
        {
            Type = EfficiencyUpgradeType.SentryDashboard,
            Name = "Sentry Dashboard",
            FundsCost = 82,
            Description = "Errors stop feeling like rumors because the crash trail points straight at the real hot path.",
            SummaryEffect = "-1 focus to squash bugs.",
            BugSquashFocusCostReduction = 1,
        },
    ];

    public static IReadOnlyList<EfficiencyUpgradeDefinition> All => Definitions;

    public static EfficiencyUpgradeDefinition Get(EfficiencyUpgradeType type)
    {
        return Definitions.First(definition => definition.Type == type);
    }
}
