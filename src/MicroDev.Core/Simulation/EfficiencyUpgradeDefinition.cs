using System.Collections.Generic;

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

    public double PassiveSanityRegenPerInGameMinute { get; init; }

    public double FoodCostReduction { get; init; }

    public double FoodDeliveryDurationReductionMinutes { get; init; }

    public double HomeCookDurationReductionMinutes { get; init; }

    public double BugSquashFocusCostReduction { get; init; }

    public string GetTotalEffectSummary(int tier)
    {
        if (tier <= 0)
        {
            return "No tier installed yet.";
        }

        var parts = new List<string>();
        if (BonusLinesPerClick > 0)
        {
            parts.Add($"+{BonusLinesPerClick * tier} lines per click");
        }

        if (FocusCostReduction > 0)
        {
            parts.Add($"-{FocusCostReduction * tier:0.##} focus per click");
        }

        if (BonusQualityGain > 0)
        {
            parts.Add($"+{BonusQualityGain * tier:0.##} quality per click");
        }

        if (PassiveFocusDrainReduction > 0)
        {
            parts.Add($"-{PassiveFocusDrainReduction * tier:0.###} passive focus drain per minute");
        }

        if (PrepPointsOnApplicationStart > 0)
        {
            parts.Add($"+{PrepPointsOnApplicationStart * tier} prep when applications start");
        }

        if (PassiveSanityRegenPerInGameMinute > 0)
        {
            parts.Add($"+{PassiveSanityRegenPerInGameMinute * tier * 60:0.##} sanity per hour");
        }

        if (FoodCostReduction > 0)
        {
            parts.Add($"-${FoodCostReduction * tier:0.##} food cost");
        }

        if (FoodDeliveryDurationReductionMinutes > 0)
        {
            parts.Add($"-{FoodDeliveryDurationReductionMinutes * tier:0}m delivery ETA");
        }

        if (HomeCookDurationReductionMinutes > 0)
        {
            parts.Add($"-{HomeCookDurationReductionMinutes * tier:0}m home-cook ETA");
        }

        if (BugSquashFocusCostReduction > 0)
        {
            parts.Add($"-{BugSquashFocusCostReduction * tier:0.##} bug-fix focus");
        }

        return parts.Count > 0
            ? string.Join(", ", parts)
            : SummaryEffect;
    }
}
