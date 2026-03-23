namespace MicroDev.Core.Simulation;

public sealed class ActiveJobListing
{
    public string Title { get; set; } = string.Empty;

    public string TechStack { get; set; } = string.Empty;

    public double RemainingInGameMinutes { get; set; }

    public int ResumeCostLines { get; set; }

    public int MinimumPortfolioLines { get; set; }

    public double MinimumCodeQuality { get; set; }

    public ActiveJobListing Clone()
    {
        return new ActiveJobListing
        {
            Title = Title,
            TechStack = TechStack,
            RemainingInGameMinutes = RemainingInGameMinutes,
            ResumeCostLines = ResumeCostLines,
            MinimumPortfolioLines = MinimumPortfolioLines,
            MinimumCodeQuality = MinimumCodeQuality,
        };
    }
}
