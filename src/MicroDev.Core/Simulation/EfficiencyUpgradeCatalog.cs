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
    ];

    public static IReadOnlyList<EfficiencyUpgradeDefinition> All => Definitions;

    public static EfficiencyUpgradeDefinition Get(EfficiencyUpgradeType type)
    {
        return Definitions.First(definition => definition.Type == type);
    }
}
