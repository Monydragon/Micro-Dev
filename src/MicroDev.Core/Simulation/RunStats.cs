namespace MicroDev.Core.Simulation;

public sealed record class RunStats
{
    public double TotalInGameMinutes { get; set; }

    public double TotalSleepMinutes { get; set; }

    public int HighestDayReached { get; set; } = 1;

    public int TotalLinesTyped { get; set; }

    public int PortfolioLinesTyped { get; set; }

    public int FreelanceLinesTyped { get; set; }

    public int TakeHomeLinesTyped { get; set; }

    public int CodingActions { get; set; }

    public int PortfolioFilesCompleted { get; set; }

    public int FreelanceGigsStarted { get; set; }

    public int FreelanceGigsCompleted { get; set; }

    public int MealsOrdered { get; set; }

    public int HomeCookedMealsOrdered { get; set; }

    public int ExpeditedDeliveries { get; set; }

    public int OrdersReviewed { get; set; }

    public int CleanMeals { get; set; }

    public int SluggishMeals { get; set; }

    public int SleepSessions { get; set; }

    public int JobListingsSeen { get; set; }

    public int JobListingsExpired { get; set; }

    public int JobApplicationsStarted { get; set; }

    public int ResumeLinesSpent { get; set; }

    public int TakeHomesCompleted { get; set; }

    public int InterviewsAttempted { get; set; }

    public int InterviewQuestionsAnswered { get; set; }

    public int InterviewQuestionsCorrect { get; set; }

    public int JobOffersEarned { get; set; }

    public int RejectionsReceived { get; set; }

    public int BugsSpawned { get; set; }

    public int NeedDrivenBugsSpawned { get; set; }

    public int BugEscalations { get; set; }

    public int BugsSquashed { get; set; }

    public int CatInterruptions { get; set; }

    public int DistractionPats { get; set; }

    public int DistractionsManuallyCleared { get; set; }

    public int DistractionsFocusCleared { get; set; }

    public int DistractionsQuickCleared { get; set; }

    public int DistractionsTimedOut { get; set; }

    public int UncommittedLinesLost { get; set; }

    public int DeepWorkWindows { get; set; }

    public int ContextSwitches { get; set; }

    public int CoffeeBounces { get; set; }

    public int MentorNudges { get; set; }

    public int RubberDuckInsights { get; set; }

    public int ExpenseSpikes { get; set; }

    public int MicroSales { get; set; }

    public int ComputerFreezes { get; set; }

    public int ComputerFreezeSelfRepairs { get; set; }

    public int ComputerFreezeTechSupportCalls { get; set; }

    public int ComputerFreezeRepairShopTrips { get; set; }

    public int StreamingChoicesMade { get; set; }

    public int OnlineMatchMoments { get; set; }

    public int PartnerCheckInsHandled { get; set; }

    public int BossCheckInsHandled { get; set; }

    public int CoworkerInterruptionsHandled { get; set; }

    public int IndieFundingSwings { get; set; }

    public int ProjectPlanChanges { get; set; }

    public int ProjectConceptRerolls { get; set; }

    public int ContactsDiscovered { get; set; }

    public int MessagesSent { get; set; }

    public int CallsMade { get; set; }

    public int RelationshipsStarted { get; set; }

    public int LifeEventsResolved { get; set; }

    public int CareerRouteChoicesMade { get; set; }

    public int FirstCoinUses { get; set; }

    public double TotalFundsEarned { get; set; }

    public double TotalFundsSpent { get; set; }

    public double FoodSpend { get; set; }

    public double UpgradeSpend { get; set; }

    public double MilestoneSpend { get; set; }

    public double BillSpend { get; set; }

    public double FreelanceIncome { get; set; }

    public double PublishIncome { get; set; }

    public double SaleIncome { get; set; }

    public double SalaryIncome { get; set; }

    public double ApplicationIncome { get; set; }

    public double MiscIncome { get; set; }

    public double HighestFundsBalance { get; set; }

    public double LowestFundsBalance { get; set; } = double.PositiveInfinity;

    public double HighestFocus { get; set; }

    public double LowestSanity { get; set; } = double.PositiveInfinity;

    public double HighestCodeQuality { get; set; }

    public double LowestCodeQuality { get; set; } = double.PositiveInfinity;

    public HashSet<string> UnlockedAchievementIds { get; init; } = [];

    public List<string> AchievementUnlockOrder { get; init; } = [];

    public int AchievementUnlockCount => UnlockedAchievementIds.Count;

    public RunStats DeepCopy()
    {
        return this with
        {
            UnlockedAchievementIds = [.. UnlockedAchievementIds],
            AchievementUnlockOrder = [.. AchievementUnlockOrder],
        };
    }
}
