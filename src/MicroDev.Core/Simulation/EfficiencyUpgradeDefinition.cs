namespace MicroDev.Core.Simulation;

public sealed class EfficiencyUpgradeDefinition
{
    public required EfficiencyUpgradeType Type { get; init; }

    public required string Name { get; init; }

    public required double FundsCost { get; init; }

    public required string Description { get; init; }

    public required string SummaryEffect { get; init; }

    public int BonusLinesPerClick { get; init; }

    public double FocusCostReduction { get; init; }

    public double BonusQualityGain { get; init; }

    public double PassiveFocusDrainReduction { get; init; }

    public int PrepPointsOnApplicationStart { get; init; }
}
