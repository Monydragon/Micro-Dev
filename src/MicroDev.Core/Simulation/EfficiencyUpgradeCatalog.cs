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
    ];

    public static IReadOnlyList<EfficiencyUpgradeDefinition> All => Definitions;

    public static EfficiencyUpgradeDefinition Get(EfficiencyUpgradeType type)
    {
        return Definitions.First(definition => definition.Type == type);
    }
}
