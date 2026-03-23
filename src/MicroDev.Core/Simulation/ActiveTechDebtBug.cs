namespace MicroDev.Core.Simulation;

public sealed class ActiveTechDebtBug
{
    public string Summary { get; set; } = string.Empty;

    public double RemainingInGameMinutes { get; set; }

    public double QualityDrainPerMinute { get; set; }

    public ActiveTechDebtBug Clone()
    {
        return new ActiveTechDebtBug
        {
            Summary = Summary,
            RemainingInGameMinutes = RemainingInGameMinutes,
            QualityDrainPerMinute = QualityDrainPerMinute,
        };
    }
}
