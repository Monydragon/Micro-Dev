namespace MicroDev.Core.Simulation;

public sealed class ActiveJobApplication
{
    public string ListingTitle { get; set; } = string.Empty;

    public string TechStack { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public GameplayLoopMode OfferMode { get; set; } = GameplayLoopMode.Corporate;

    public string ChallengeTitle { get; set; } = string.Empty;

    public string ChallengeDescription { get; set; } = string.Empty;

    public int ResumeLinesSpent { get; set; }

    public int PortfolioLinesSnapshot { get; set; }

    public double CodeQualitySnapshot { get; set; }

    public int MinimumPortfolioLines { get; set; }

    public double MinimumCodeQuality { get; set; }

    public ResumeTrack ResumeTrack { get; set; }

    public int ResumeProofSnapshot { get; set; }

    public int VisibleLineCount { get; set; }

    public int CurrentQuestionIndex { get; set; }

    public int CorrectAnswers { get; set; }

    public int MinimumCorrectAnswers { get; set; }

    public int PrepPoints { get; set; }

    public List<string> CodeLines { get; } = [];

    public List<InterviewQuestion> Questions { get; } = [];

    public bool TakeHomeComplete => VisibleLineCount >= CodeLines.Count;

    public bool InterviewComplete => CurrentQuestionIndex >= Questions.Count;

    public ActiveJobApplication Clone()
    {
        var clone = new ActiveJobApplication
        {
            ListingTitle = ListingTitle,
            TechStack = TechStack,
            CompanyName = CompanyName,
            OfferMode = OfferMode,
            ChallengeTitle = ChallengeTitle,
            ChallengeDescription = ChallengeDescription,
            ResumeLinesSpent = ResumeLinesSpent,
            PortfolioLinesSnapshot = PortfolioLinesSnapshot,
            CodeQualitySnapshot = CodeQualitySnapshot,
            MinimumPortfolioLines = MinimumPortfolioLines,
            MinimumCodeQuality = MinimumCodeQuality,
            ResumeTrack = ResumeTrack,
            ResumeProofSnapshot = ResumeProofSnapshot,
            VisibleLineCount = VisibleLineCount,
            CurrentQuestionIndex = CurrentQuestionIndex,
            CorrectAnswers = CorrectAnswers,
            MinimumCorrectAnswers = MinimumCorrectAnswers,
            PrepPoints = PrepPoints,
        };

        clone.CodeLines.AddRange(CodeLines);
        clone.Questions.AddRange(Questions.Select(static question => question.Clone()));
        return clone;
    }
}
