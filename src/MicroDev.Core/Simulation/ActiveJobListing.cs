namespace MicroDev.Core.Simulation;

public sealed class ActiveJobListing
{
    public string ListingId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string TechStack { get; set; } = string.Empty;

    public double RemainingInGameMinutes { get; set; }

    public int ResumeCostLines { get; set; }

    public int MinimumPortfolioLines { get; set; }

    public double MinimumCodeQuality { get; set; }

    public ResumeTrack ResumeTrack { get; set; }

    public int RequiredResumeProof { get; set; }

    public ActiveJobListing Clone()
    {
        return new ActiveJobListing
        {
            ListingId = ListingId,
            Title = Title,
            TechStack = TechStack,
            RemainingInGameMinutes = RemainingInGameMinutes,
            ResumeCostLines = ResumeCostLines,
            MinimumPortfolioLines = MinimumPortfolioLines,
            MinimumCodeQuality = MinimumCodeQuality,
            ResumeTrack = ResumeTrack,
            RequiredResumeProof = RequiredResumeProof,
        };
    }
}
