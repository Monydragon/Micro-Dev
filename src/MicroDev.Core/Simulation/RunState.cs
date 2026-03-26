namespace MicroDev.Core.Simulation;

public sealed class RunState
{
    public GameDifficulty Difficulty { get; set; } = GameDifficulty.Normal;

    public GameplayLoopMode GameplayMode { get; set; } = GameplayLoopMode.Interview;

    public bool IsRealisticMode { get; set; }

    public int RunSeed { get; set; }

    public int Day { get; set; }

    public double TimeOfDayMinutes { get; set; }

    public double DeskMinutesElapsed { get; set; }

    public double Funds { get; set; }

    public double Focus { get; set; }

    public double Sanity { get; set; }

    public int LinesOfCode { get; set; }

    public double CodeQuality { get; set; }

    public int GameplayResumeProof { get; set; }

    public int UiResumeProof { get; set; }

    public int ToolingResumeProof { get; set; }

    public double SluggishMinutesRemaining { get; set; }

    public double DeepWorkMinutesRemaining { get; set; }

    public double ContextSwitchMinutesRemaining { get; set; }

    public double MinutesSinceLastMeal { get; set; }

    public double MinutesSinceLastSleep { get; set; }

    public int SuccessfulApplications { get; set; }

    public int GeneratedJobListingCount { get; set; }

    public int GeneratedModifierIncidentCount { get; set; }

    public double NextGuaranteedJobDeskMinute { get; set; }

    public double NextModifierDeskMinute { get; set; }

    public bool HasFirstCoin { get; set; }

    public bool FirstCoinDecisionPending { get; set; }

    public double FirstCoinRescueDeficit { get; set; }

    public PendingLifeEvent? PendingLifeEvent { get; set; }

    public int RelationshipProgress { get; set; }

    public bool HasFoundLove { get; set; }

    public string? PartnerName { get; set; }

    public string? RelationshipCandidateName { get; set; }

    public int RelationshipCandidateCompatibility { get; set; }

    public BossDisposition BossDisposition { get; set; } = BossDisposition.Supportive;

    public string BossName { get; set; } = string.Empty;

    public string BossTitle { get; set; } = string.Empty;

    public int CorporateStanding { get; set; }

    public bool HasApartment { get; set; }

    public bool HasHouse { get; set; }

    public bool HasStartedFamily { get; set; }

    public string StudioName { get; set; } = string.Empty;

    public int CurrentPortfolioLinesOfCode { get; set; }

    public int CurrentProgramIndex { get; set; }

    public int CurrentProgramVisibleLineCount { get; set; }

    public int PublishedAppCount { get; set; }

    public int PublishedAppSaleCount { get; set; }

    public double NextPublishedAppSaleDeskMinute { get; set; } = double.PositiveInfinity;

    public string? LastPublishedAppName { get; set; }

    public string? RecentCompletedFileName { get; set; }

    public double FileCompletionCelebrationMinutesRemaining { get; set; }

    public RunStatus Status { get; set; } = RunStatus.InProgress;

    public string? OutcomeMessage { get; set; }

    public ActiveFoodDelivery? ActiveFoodDelivery { get; set; }

    public ActiveCatInterruption? ActiveCatInterruption { get; set; }

    public ActiveTechDebtBug? ActiveTechDebtBug { get; set; }

    public ActiveJobListing? ActiveJobListing { get; set; }

    public ActiveJobApplication? ActiveJobApplication { get; set; }

    public ActiveFreelanceGig? ActiveFreelanceGig { get; set; }

    public ProjectBlueprint CurrentProjectBlueprint { get; set; } = new();

    public VersionControlState VersionControl { get; set; } = new();

    public List<string> EventLog { get; } = [];

    public List<QueuedIncident> QueuedIncidents { get; } = [];

    public HashSet<string> TriggeredIncidentIds { get; } = [];

    public HashSet<EfficiencyUpgradeType> PurchasedUpgrades { get; } = [];

    public string ClockText
    {
        get
        {
            var totalMinutes = Math.Max(0, (int)Math.Floor(TimeOfDayMinutes));
            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;
            return $"{hours:00}:{minutes:00}";
        }
    }

    public RunState Clone()
    {
        var clone = new RunState
        {
            Difficulty = Difficulty,
            GameplayMode = GameplayMode,
            IsRealisticMode = IsRealisticMode,
            RunSeed = RunSeed,
            Day = Day,
            TimeOfDayMinutes = TimeOfDayMinutes,
            DeskMinutesElapsed = DeskMinutesElapsed,
            Funds = Funds,
            Focus = Focus,
            Sanity = Sanity,
            LinesOfCode = LinesOfCode,
            CodeQuality = CodeQuality,
            GameplayResumeProof = GameplayResumeProof,
            UiResumeProof = UiResumeProof,
            ToolingResumeProof = ToolingResumeProof,
            SluggishMinutesRemaining = SluggishMinutesRemaining,
            DeepWorkMinutesRemaining = DeepWorkMinutesRemaining,
            ContextSwitchMinutesRemaining = ContextSwitchMinutesRemaining,
            MinutesSinceLastMeal = MinutesSinceLastMeal,
            MinutesSinceLastSleep = MinutesSinceLastSleep,
            SuccessfulApplications = SuccessfulApplications,
            GeneratedJobListingCount = GeneratedJobListingCount,
            GeneratedModifierIncidentCount = GeneratedModifierIncidentCount,
            NextGuaranteedJobDeskMinute = NextGuaranteedJobDeskMinute,
            NextModifierDeskMinute = NextModifierDeskMinute,
            HasFirstCoin = HasFirstCoin,
            FirstCoinDecisionPending = FirstCoinDecisionPending,
            FirstCoinRescueDeficit = FirstCoinRescueDeficit,
            PendingLifeEvent = PendingLifeEvent?.Clone(),
            RelationshipProgress = RelationshipProgress,
            HasFoundLove = HasFoundLove,
            PartnerName = PartnerName,
            RelationshipCandidateName = RelationshipCandidateName,
            RelationshipCandidateCompatibility = RelationshipCandidateCompatibility,
            BossDisposition = BossDisposition,
            BossName = BossName,
            BossTitle = BossTitle,
            CorporateStanding = CorporateStanding,
            HasApartment = HasApartment,
            HasHouse = HasHouse,
            HasStartedFamily = HasStartedFamily,
            StudioName = StudioName,
            CurrentPortfolioLinesOfCode = CurrentPortfolioLinesOfCode,
            CurrentProgramIndex = CurrentProgramIndex,
            CurrentProgramVisibleLineCount = CurrentProgramVisibleLineCount,
            PublishedAppCount = PublishedAppCount,
            PublishedAppSaleCount = PublishedAppSaleCount,
            NextPublishedAppSaleDeskMinute = NextPublishedAppSaleDeskMinute,
            LastPublishedAppName = LastPublishedAppName,
            RecentCompletedFileName = RecentCompletedFileName,
            FileCompletionCelebrationMinutesRemaining = FileCompletionCelebrationMinutesRemaining,
            Status = Status,
            OutcomeMessage = OutcomeMessage,
            ActiveFoodDelivery = ActiveFoodDelivery?.Clone(),
            ActiveCatInterruption = ActiveCatInterruption?.Clone(),
            ActiveTechDebtBug = ActiveTechDebtBug?.Clone(),
            ActiveJobListing = ActiveJobListing?.Clone(),
            ActiveJobApplication = ActiveJobApplication?.Clone(),
            ActiveFreelanceGig = ActiveFreelanceGig?.Clone(),
            CurrentProjectBlueprint = CurrentProjectBlueprint.Clone(),
            VersionControl = VersionControl.Clone(),
        };

        clone.EventLog.AddRange(EventLog);
        clone.QueuedIncidents.AddRange(QueuedIncidents);
        clone.TriggeredIncidentIds.UnionWith(TriggeredIncidentIds);
        clone.PurchasedUpgrades.UnionWith(PurchasedUpgrades);
        return clone;
    }

    public void ResetFrom(RunState other)
    {
        Difficulty = other.Difficulty;
        GameplayMode = other.GameplayMode;
        IsRealisticMode = other.IsRealisticMode;
        RunSeed = other.RunSeed;
        Day = other.Day;
        TimeOfDayMinutes = other.TimeOfDayMinutes;
        DeskMinutesElapsed = other.DeskMinutesElapsed;
        Funds = other.Funds;
        Focus = other.Focus;
        Sanity = other.Sanity;
        LinesOfCode = other.LinesOfCode;
        CodeQuality = other.CodeQuality;
        GameplayResumeProof = other.GameplayResumeProof;
        UiResumeProof = other.UiResumeProof;
        ToolingResumeProof = other.ToolingResumeProof;
        SluggishMinutesRemaining = other.SluggishMinutesRemaining;
        DeepWorkMinutesRemaining = other.DeepWorkMinutesRemaining;
        ContextSwitchMinutesRemaining = other.ContextSwitchMinutesRemaining;
        MinutesSinceLastMeal = other.MinutesSinceLastMeal;
        MinutesSinceLastSleep = other.MinutesSinceLastSleep;
        SuccessfulApplications = other.SuccessfulApplications;
        GeneratedJobListingCount = other.GeneratedJobListingCount;
        GeneratedModifierIncidentCount = other.GeneratedModifierIncidentCount;
        NextGuaranteedJobDeskMinute = other.NextGuaranteedJobDeskMinute;
        NextModifierDeskMinute = other.NextModifierDeskMinute;
        HasFirstCoin = other.HasFirstCoin;
        FirstCoinDecisionPending = other.FirstCoinDecisionPending;
        FirstCoinRescueDeficit = other.FirstCoinRescueDeficit;
        PendingLifeEvent = other.PendingLifeEvent?.Clone();
        RelationshipProgress = other.RelationshipProgress;
        HasFoundLove = other.HasFoundLove;
        PartnerName = other.PartnerName;
        RelationshipCandidateName = other.RelationshipCandidateName;
        RelationshipCandidateCompatibility = other.RelationshipCandidateCompatibility;
        BossDisposition = other.BossDisposition;
        BossName = other.BossName;
        BossTitle = other.BossTitle;
        CorporateStanding = other.CorporateStanding;
        HasApartment = other.HasApartment;
        HasHouse = other.HasHouse;
        HasStartedFamily = other.HasStartedFamily;
        StudioName = other.StudioName;
        CurrentPortfolioLinesOfCode = other.CurrentPortfolioLinesOfCode;
        CurrentProgramIndex = other.CurrentProgramIndex;
        CurrentProgramVisibleLineCount = other.CurrentProgramVisibleLineCount;
        PublishedAppCount = other.PublishedAppCount;
        PublishedAppSaleCount = other.PublishedAppSaleCount;
        NextPublishedAppSaleDeskMinute = other.NextPublishedAppSaleDeskMinute;
        LastPublishedAppName = other.LastPublishedAppName;
        RecentCompletedFileName = other.RecentCompletedFileName;
        FileCompletionCelebrationMinutesRemaining = other.FileCompletionCelebrationMinutesRemaining;
        Status = other.Status;
        OutcomeMessage = other.OutcomeMessage;
        ActiveFoodDelivery = other.ActiveFoodDelivery?.Clone();
        ActiveCatInterruption = other.ActiveCatInterruption?.Clone();
        ActiveTechDebtBug = other.ActiveTechDebtBug?.Clone();
        ActiveJobListing = other.ActiveJobListing?.Clone();
        ActiveJobApplication = other.ActiveJobApplication?.Clone();
        ActiveFreelanceGig = other.ActiveFreelanceGig?.Clone();
        CurrentProjectBlueprint = other.CurrentProjectBlueprint.Clone();
        VersionControl = other.VersionControl.Clone();

        EventLog.Clear();
        EventLog.AddRange(other.EventLog);

        QueuedIncidents.Clear();
        QueuedIncidents.AddRange(other.QueuedIncidents);

        TriggeredIncidentIds.Clear();
        TriggeredIncidentIds.UnionWith(other.TriggeredIncidentIds);

        PurchasedUpgrades.Clear();
        PurchasedUpgrades.UnionWith(other.PurchasedUpgrades);
    }
}
