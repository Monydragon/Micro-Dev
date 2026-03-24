namespace MicroDev.Core.Simulation;

public sealed class ActiveCatInterruption
{
    public int PatsRemaining { get; set; }

    public double RemainingInGameMinutes { get; set; }

    public int LinesDeletionPenalty { get; set; }

    public double MinutesUntilNextTypingBurst { get; set; }

    public int PhantomBugCount { get; set; }

    public int GibberishLinesTyped { get; set; }

    public double TotalQualityLoss { get; set; }

    public int VisualSeed { get; set; }

    public ActiveCatInterruption Clone()
    {
        return new ActiveCatInterruption
        {
            PatsRemaining = PatsRemaining,
            RemainingInGameMinutes = RemainingInGameMinutes,
            LinesDeletionPenalty = LinesDeletionPenalty,
            MinutesUntilNextTypingBurst = MinutesUntilNextTypingBurst,
            PhantomBugCount = PhantomBugCount,
            GibberishLinesTyped = GibberishLinesTyped,
            TotalQualityLoss = TotalQualityLoss,
            VisualSeed = VisualSeed,
        };
    }
}
