namespace MicroDev.Core.Simulation;

public sealed class ActiveCatInterruption
{
    public DeskDistractionKind Kind { get; set; } = DeskDistractionKind.Cat;

    public string Title { get; set; } = "Desk Distraction";

    public string Description { get; set; } = string.Empty;

    public string ManualActionLabel { get; set; } = "Clear It";

    public string FocusActionLabel { get; set; } = "Refocus";

    public string QuickResolveLabel { get; set; } = "Quick Fix";

    public int PatsRemaining { get; set; }

    public double RemainingInGameMinutes { get; set; }

    public int LinesDeletionPenalty { get; set; }

    public double MinutesUntilNextTypingBurst { get; set; }

    public int PhantomBugCount { get; set; }

    public int GibberishLinesTyped { get; set; }

    public double TotalQualityLoss { get; set; }

    public int VisualSeed { get; set; }

    public double FocusActionFocusCost { get; set; }

    public int FocusActionPatReduction { get; set; }

    public double QuickResolveFundsCost { get; set; }

    public double QuickResolveFocusCost { get; set; }

    public ActiveCatInterruption Clone()
    {
        return new ActiveCatInterruption
        {
            Kind = Kind,
            Title = Title,
            Description = Description,
            ManualActionLabel = ManualActionLabel,
            FocusActionLabel = FocusActionLabel,
            QuickResolveLabel = QuickResolveLabel,
            PatsRemaining = PatsRemaining,
            RemainingInGameMinutes = RemainingInGameMinutes,
            LinesDeletionPenalty = LinesDeletionPenalty,
            MinutesUntilNextTypingBurst = MinutesUntilNextTypingBurst,
            PhantomBugCount = PhantomBugCount,
            GibberishLinesTyped = GibberishLinesTyped,
            TotalQualityLoss = TotalQualityLoss,
            VisualSeed = VisualSeed,
            FocusActionFocusCost = FocusActionFocusCost,
            FocusActionPatReduction = FocusActionPatReduction,
            QuickResolveFundsCost = QuickResolveFundsCost,
            QuickResolveFocusCost = QuickResolveFocusCost,
        };
    }
}
