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
    ];

    public static IReadOnlyList<EfficiencyUpgradeDefinition> All => Definitions;

    public static EfficiencyUpgradeDefinition Get(EfficiencyUpgradeType type)
    {
        return Definitions.First(definition => definition.Type == type);
    }
}
