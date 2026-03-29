using System.Globalization;

namespace MicroDev.Core.Simulation;

public sealed record RunStatDefinition(string Label, Func<RunState, string> ValueFactory);

public sealed record RunStatSectionDefinition(string Title, string Description, IReadOnlyList<RunStatDefinition> Stats);

public sealed record RunAchievementDefinition(
    string Id,
    string Title,
    string Description,
    Func<RunState, bool> IsUnlocked,
    Func<RunState, string> ProgressFactory);

public static class RunStatsCatalog
{
    public static IReadOnlyList<RunStatSectionDefinition> Sections { get; } =
    [
        new(
            "Production",
            "Everything the run has shipped, typed, or committed.",
            [
                new("Lines of code typed", state => FormatCount(state.Stats.TotalLinesTyped)),
                new("Portfolio lines typed", state => FormatCount(state.Stats.PortfolioLinesTyped)),
                new("Freelance lines delivered", state => FormatCount(state.Stats.FreelanceLinesTyped)),
                new("Take-home lines typed", state => FormatCount(state.Stats.TakeHomeLinesTyped)),
                new("Coding clicks", state => FormatCount(state.Stats.CodingActions)),
                new("Portfolio files completed", state => FormatCount(state.Stats.PortfolioFilesCompleted)),
                new("Projects released", state => FormatCount(state.PublishedAppCount)),
                new("Storefront sales", state => FormatCount(state.PublishedAppSaleCount)),
                new("Freelance gigs started", state => FormatCount(state.Stats.FreelanceGigsStarted)),
                new("Freelance gigs completed", state => FormatCount(state.Stats.FreelanceGigsCompleted)),
                new("Commits made", state => FormatCount(state.VersionControl.CommitCount)),
                new("Upgrade tiers bought", state => FormatCount(GetPurchasedUpgradeTierTotal(state))),
            ]),
        new(
            "Career",
            "Recruiter pressure, applications, and interview throughput.",
            [
                new("Job listings seen", state => FormatCount(state.Stats.JobListingsSeen)),
                new("Job listings expired", state => FormatCount(state.Stats.JobListingsExpired)),
                new("Job applications attempted", state => FormatCount(state.Stats.JobApplicationsStarted)),
                new("Resume lines spent", state => FormatCount(state.Stats.ResumeLinesSpent)),
                new("Take-homes finished", state => FormatCount(state.Stats.TakeHomesCompleted)),
                new("Interviews attempted", state => FormatCount(state.Stats.InterviewsAttempted)),
                new("Interview questions answered", state => FormatCount(state.Stats.InterviewQuestionsAnswered)),
                new("Interview questions correct", state => FormatCount(state.Stats.InterviewQuestionsCorrect)),
                new("Offers landed", state => FormatCount(state.Stats.JobOffersEarned)),
                new("Successful applications", state => FormatCount(state.SuccessfulApplications)),
                new("Rejections", state => FormatCount(state.Stats.RejectionsReceived)),
                new("Resume proof now", state => $"G {state.GameplayResumeProof}  U {state.UiResumeProof}  T {state.ToolingResumeProof}"),
            ]),
        new(
            "Money",
            "Income and spend broken out across the full run.",
            [
                new("Funds earned", state => FormatFunds(state.Stats.TotalFundsEarned)),
                new("Funds spent", state => FormatFunds(state.Stats.TotalFundsSpent)),
                new("Freelance income", state => FormatFunds(state.Stats.FreelanceIncome)),
                new("Release income", state => FormatFunds(state.Stats.PublishIncome)),
                new("Store income", state => FormatFunds(state.Stats.SaleIncome)),
                new("Daily route income", state => FormatFunds(state.Stats.SalaryIncome)),
                new("Offer income", state => FormatFunds(state.Stats.ApplicationIncome)),
                new("Misc income", state => FormatFunds(state.Stats.MiscIncome)),
                new("Food spend", state => FormatFunds(state.Stats.FoodSpend)),
                new("Upgrade spend", state => FormatFunds(state.Stats.UpgradeSpend)),
                new("Milestone spend", state => FormatFunds(state.Stats.MilestoneSpend)),
                new("Bills paid", state => FormatFunds(state.Stats.BillSpend)),
            ]),
        new(
            "Survival",
            "Meals, sleep, bugs, and all the ways the run fought back.",
            [
                new("Meals ordered", state => FormatCount(state.Stats.MealsOrdered)),
                new("Home-cooked meals", state => FormatCount(state.Stats.HomeCookedMealsOrdered)),
                new("Expedited deliveries", state => FormatCount(state.Stats.ExpeditedDeliveries)),
                new("Orders double-checked", state => FormatCount(state.Stats.OrdersReviewed)),
                new("Clean meal deliveries", state => FormatCount(state.Stats.CleanMeals)),
                new("Sluggish meals", state => FormatCount(state.Stats.SluggishMeals)),
                new("Sleep sessions", state => FormatCount(state.Stats.SleepSessions)),
                new("First coin rescues", state => FormatCount(state.Stats.FirstCoinUses)),
                new("Bugs spawned", state => FormatCount(state.Stats.BugsSpawned)),
                new("Need-driven bugs", state => FormatCount(state.Stats.NeedDrivenBugsSpawned)),
                new("Bug escalations", state => FormatCount(state.Stats.BugEscalations)),
                new("Bugs squashed", state => FormatCount(state.Stats.BugsSquashed)),
                new("Cat interruptions", state => FormatCount(state.Stats.CatInterruptions)),
                new("Uncommitted lines lost", state => FormatCount(state.Stats.UncommittedLinesLost)),
                new("Computer freezes", state => FormatCount(state.Stats.ComputerFreezes)),
                new("Deep work windows", state => FormatCount(state.Stats.DeepWorkWindows)),
                new("Context switches", state => FormatCount(state.Stats.ContextSwitches)),
                new("Expense spikes", state => FormatCount(state.Stats.ExpenseSpikes)),
            ]),
        new(
            "Social",
            "People met, messages sent, and life events handled between the code.",
            [
                new("Contacts discovered", state => FormatCount(state.Stats.ContactsDiscovered)),
                new("Contacts available now", state => FormatCount(state.KnownContacts.Count)),
                new("Messages sent", state => FormatCount(state.Stats.MessagesSent)),
                new("Calls made", state => FormatCount(state.Stats.CallsMade)),
                new("Online match moments", state => FormatCount(state.Stats.OnlineMatchMoments)),
                new("Partner check-ins handled", state => FormatCount(state.Stats.PartnerCheckInsHandled)),
                new("Relationships started", state => FormatCount(state.Stats.RelationshipsStarted)),
                new("Relationship progress", state => FormatCount(state.RelationshipProgress)),
                new("Boss check-ins handled", state => FormatCount(state.Stats.BossCheckInsHandled)),
                new("Coworker interruptions handled", state => FormatCount(state.Stats.CoworkerInterruptionsHandled)),
                new("Life events resolved", state => FormatCount(state.Stats.LifeEventsResolved)),
                new("Career route choices", state => FormatCount(state.Stats.CareerRouteChoicesMade)),
            ]),
        new(
            "Milestones",
            "High-water marks, run length, and long-form progression.",
            [
                new("Time in run", state => FormatMinutes(state.Stats.TotalInGameMinutes)),
                new("Time spent sleeping", state => FormatMinutes(state.Stats.TotalSleepMinutes)),
                new("Highest day reached", state => FormatCount(state.Stats.HighestDayReached)),
                new("Highest cash balance", state => FormatFunds(state.Stats.HighestFundsBalance)),
                new("Lowest cash balance", state => FormatFundsOrDash(state.Stats.LowestFundsBalance)),
                new("Highest focus", state => FormatValue(state.Stats.HighestFocus)),
                new("Lowest sanity", state => FormatValueOrDash(state.Stats.LowestSanity)),
                new("Highest code quality", state => FormatValue(state.Stats.HighestCodeQuality)),
                new("Lowest code quality", state => FormatValueOrDash(state.Stats.LowestCodeQuality)),
                new("Apartment reached", state => FormatFlag(state.HasApartment)),
                new("House bought", state => FormatFlag(state.HasHouse)),
                new("Retired", state => FormatFlag(state.HasRetired)),
                new("Current route", state => state.GameplayMode.ToString()),
                new("Current status", state => state.Status.ToString()),
            ]),
    ];

    private static int GetPurchasedUpgradeTierTotal(RunState state)
    {
        return state.PurchasedUpgrades.Values.Sum();
    }

    private static string FormatCount(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture);
    }

    private static string FormatFunds(double value)
    {
        return $"${value:0}";
    }

    private static string FormatFundsOrDash(double value)
    {
        return double.IsFinite(value)
            ? FormatFunds(value)
            : "-";
    }

    private static string FormatValue(double value)
    {
        return value.ToString("0.#", CultureInfo.InvariantCulture);
    }

    private static string FormatValueOrDash(double value)
    {
        return double.IsFinite(value)
            ? FormatValue(value)
            : "-";
    }

    private static string FormatFlag(bool value)
    {
        return value ? "Yes" : "No";
    }

    private static string FormatMinutes(double totalMinutes)
    {
        var minutes = Math.Max(0, (int)Math.Ceiling(totalMinutes));
        var hours = minutes / 60;
        var remainingMinutes = minutes % 60;
        return hours > 0
            ? $"{hours}h {remainingMinutes:00}m"
            : $"{remainingMinutes}m";
    }
}

public static class RunAchievementCatalog
{
    public static IReadOnlyList<RunAchievementDefinition> All { get; } = CreateAchievements();

    private static IReadOnlyList<RunAchievementDefinition> CreateAchievements()
    {
        List<RunAchievementDefinition> achievements =
        [
            new(
                "first_commit",
                "First Commit",
                "Lock in the first clean commit.",
                state => state.VersionControl.CommitCount >= 1,
                state => FormatCountProgress(state.VersionControl.CommitCount, 1, "commits")),
            new(
                "ship_it",
                "Ship It",
                "Publish the first project release.",
                state => state.PublishedAppCount >= 1,
                state => FormatCountProgress(state.PublishedAppCount, 1, "projects released")),
            new(
                "storefront_sale",
                "Storefront Sale",
                "See the first sale land after shipping.",
                state => state.PublishedAppSaleCount >= 1,
                state => FormatCountProgress(state.PublishedAppSaleCount, 1, "storefront sales")),
            new(
                "freelancer",
                "Freelancer",
                "Finish a paid freelance gig.",
                state => state.Stats.FreelanceGigsCompleted >= 1,
                state => FormatCountProgress(state.Stats.FreelanceGigsCompleted, 1, "gigs finished")),
            new(
                "bug_squasher",
                "Bug Squasher",
                "Stabilize the desk by fixing a live compile mess.",
                state => state.Stats.BugsSquashed >= 1,
                state => FormatCountProgress(state.Stats.BugsSquashed, 1, "bugs squashed")),
            new(
                "interview_loop",
                "Interview Loop",
                "Make it into at least one interview.",
                state => state.Stats.InterviewsAttempted >= 1,
                state => FormatCountProgress(state.Stats.InterviewsAttempted, 1, "interviews attempted")),
            new(
                "offer_letter",
                "Offer Letter",
                "Land a successful application.",
                state => state.SuccessfulApplications >= 1,
                state => FormatCountProgress(state.SuccessfulApplications, 1, "successful applications")),
            new(
                "meal_prep",
                "Meal Prep",
                "Lean on a home-cooked meal at least once.",
                state => state.Stats.HomeCookedMealsOrdered >= 1,
                state => FormatCountProgress(state.Stats.HomeCookedMealsOrdered, 1, "home-cooked meals")),
            new(
                "networker",
                "Networker",
                "Actually use Communication instead of just opening it.",
                state => GetCommunicationActionTotal(state) >= 5,
                state => FormatCountProgress(GetCommunicationActionTotal(state), 5, "messages or calls")),
            new(
                "rig_upgrade",
                "Rig Upgrade",
                "Stack three upgrade tiers across the desk setup.",
                state => GetPurchasedUpgradeTierTotal(state) >= 3,
                state => FormatCountProgress(GetPurchasedUpgradeTierTotal(state), 3, "upgrade tiers")),
            new(
                "codebase_builder",
                "Codebase Builder",
                "Type 250 lines across the run.",
                state => state.Stats.TotalLinesTyped >= 250,
                state => FormatCountProgress(state.Stats.TotalLinesTyped, 250, "lines typed")),
            new(
                "safety_net",
                "Safety Net",
                "Break the first coin to survive one more bill cycle.",
                state => state.Stats.FirstCoinUses >= 1,
                state => FormatCountProgress(state.Stats.FirstCoinUses, 1, "first-coin rescues")),
            new(
                "moved_out",
                "Moved Out",
                "Reach the apartment milestone.",
                state => state.HasApartment,
                state => state.HasApartment ? "Apartment reached" : "Apartment not reached yet"),
            new(
                "homeowner",
                "Homeowner",
                "Turn the run into real housing stability.",
                state => state.HasHouse,
                state => state.HasHouse ? "House bought" : "Still saving for the house"),
            new(
                "found_connection",
                "Found Connection",
                "Turn one of the relationship threads into something real.",
                state => state.HasFoundLove,
                state => state.HasFoundLove ? "Relationship started" : "No lasting relationship yet"),
            new(
                "retired",
                "Long Game",
                "Finish the life-sim arc by retiring.",
                state => state.HasRetired,
                state => state.HasRetired ? "Retired" : "Retirement still ahead"),
        ];

        AddCountAchievements(
            achievements,
            "commit_cadence",
            "Commit Cadence",
            target => $"Make {FormatTargetCount(target, "commit", "commits")} in a single run.",
            "commits",
            state => state.VersionControl.CommitCount,
            ("I", 3),
            ("II", 5),
            ("III", 10),
            ("IV", 20));

        AddCountAchievements(
            achievements,
            "codebase_builder",
            "Codebase Builder",
            target => $"Type {FormatTargetCount(target, "line", "lines")} across the run.",
            "lines typed",
            state => state.Stats.TotalLinesTyped,
            ("II", 500),
            ("III", 1_000),
            ("IV", 2_500),
            ("V", 5_000));

        AddCountAchievements(
            achievements,
            "file_finisher",
            "File Finisher",
            target => $"Complete {FormatTargetCount(target, "portfolio file", "portfolio files")}.",
            "files completed",
            state => state.Stats.PortfolioFilesCompleted,
            ("I", 3),
            ("II", 6),
            ("III", 10),
            ("IV", 15));

        AddCountAchievements(
            achievements,
            "release_train",
            "Release Train",
            target => $"Publish {FormatTargetCount(target, "project release", "project releases")}.",
            "projects released",
            state => state.PublishedAppCount,
            ("I", 2),
            ("II", 3),
            ("III", 5),
            ("IV", 10));

        AddCountAchievements(
            achievements,
            "store_momentum",
            "Store Momentum",
            target => $"Land {FormatTargetCount(target, "storefront sale", "storefront sales")} after shipping.",
            "storefront sales",
            state => state.PublishedAppSaleCount,
            ("I", 5),
            ("II", 10),
            ("III", 25),
            ("IV", 50));

        AddCountAchievements(
            achievements,
            "client_ladder",
            "Client Ladder",
            target => $"Finish {FormatTargetCount(target, "paid freelance gig", "paid freelance gigs")}.",
            "gigs finished",
            state => state.Stats.FreelanceGigsCompleted,
            ("I", 3),
            ("II", 5),
            ("III", 10),
            ("IV", 20));

        AddCountAchievements(
            achievements,
            "bug_exorcist",
            "Bug Exorcist",
            target => $"Squash {FormatTargetCount(target, "bug", "bugs")} before the desk eats the run.",
            "bugs squashed",
            state => state.Stats.BugsSquashed,
            ("I", 5),
            ("II", 10),
            ("III", 25),
            ("IV", 50));

        AddCountAchievements(
            achievements,
            "kitchen_rotation",
            "Kitchen Rotation",
            target => $"Lean on {FormatTargetCount(target, "home-cooked meal", "home-cooked meals")} in one run.",
            "home-cooked meals",
            state => state.Stats.HomeCookedMealsOrdered,
            ("I", 3),
            ("II", 5),
            ("III", 10),
            ("IV", 20));

        AddCountAchievements(
            achievements,
            "social_battery",
            "Social Battery",
            target => $"Send or place {FormatTargetCount(target, "message or call", "messages or calls")} from Communication.",
            "messages or calls",
            state => GetCommunicationActionTotal(state),
            ("I", 10),
            ("II", 25),
            ("III", 50),
            ("IV", 100));

        AddCountAchievements(
            achievements,
            "desk_stack",
            "Desk Stack",
            target => $"Buy {FormatTargetCount(target, "upgrade tier", "upgrade tiers")} across the setup.",
            "upgrade tiers",
            state => GetPurchasedUpgradeTierTotal(state),
            ("I", 5),
            ("II", 10),
            ("III", 15),
            ("IV", 20));

        AddCountAchievements(
            achievements,
            "application_sprint",
            "Application Sprint",
            target => $"Attempt {FormatTargetCount(target, "job application", "job applications")}.",
            "job applications",
            state => state.Stats.JobApplicationsStarted,
            ("I", 1),
            ("II", 3),
            ("III", 5),
            ("IV", 10));

        AddCountAchievements(
            achievements,
            "interview_circuit",
            "Interview Circuit",
            target => $"Reach {FormatTargetCount(target, "interview", "interviews")} in a single run.",
            "interviews attempted",
            state => state.Stats.InterviewsAttempted,
            ("I", 3),
            ("II", 5),
            ("III", 10),
            ("IV", 20));

        AddCountAchievements(
            achievements,
            "offer_stack",
            "Offer Stack",
            target => $"Land {FormatTargetCount(target, "successful application", "successful applications")}.",
            "successful applications",
            state => state.SuccessfulApplications,
            ("I", 3),
            ("II", 5),
            ("III", 10),
            ("IV", 20));

        AddCountAchievements(
            achievements,
            "take_home_gauntlet",
            "Take-Home Gauntlet",
            target => $"Finish {FormatTargetCount(target, "take-home", "take-homes")} before the interview loop closes.",
            "take-homes finished",
            state => state.Stats.TakeHomesCompleted,
            ("I", 1),
            ("II", 3),
            ("III", 5),
            ("IV", 10));

        AddCountAchievements(
            achievements,
            "contact_web",
            "Contact Web",
            target => $"Discover {FormatTargetCount(target, "contact", "contacts")} across the run.",
            "contacts discovered",
            state => state.Stats.ContactsDiscovered,
            ("I", 1),
            ("II", 3),
            ("III", 5),
            ("IV", 10));

        AddCountAchievements(
            achievements,
            "rest_cycle",
            "Rest Cycle",
            target => target == 1
                ? "Sleep once without dropping the run."
                : $"Sleep {FormatCount(target)} times without dropping the run.",
            "sleep sessions",
            state => state.Stats.SleepSessions,
            ("I", 1),
            ("II", 3),
            ("III", 5),
            ("IV", 10));

        AddCountAchievements(
            achievements,
            "delivery_rotation",
            "Delivery Rotation",
            target => $"Place {FormatTargetCount(target, "meal order", "meal orders")} in a single run.",
            "meals ordered",
            state => state.Stats.MealsOrdered,
            ("I", 5),
            ("II", 10),
            ("III", 20),
            ("IV", 40));

        AddCountAchievements(
            achievements,
            "life_outside",
            "Life Outside",
            target => $"Resolve {FormatTargetCount(target, "life event", "life events")} outside the editor.",
            "life events resolved",
            state => state.Stats.LifeEventsResolved,
            ("I", 1),
            ("II", 5),
            ("III", 10),
            ("IV", 20));

        AddCountAchievements(
            achievements,
            "day_counter",
            "Day Counter",
            target => $"Reach day {FormatCount(target)} in the same run.",
            "days reached",
            state => state.Stats.HighestDayReached,
            ("I", 3),
            ("II", 7),
            ("III", 14),
            ("IV", 30));

        AddFundsAchievements(
            achievements,
            "cash_flow",
            "Cash Flow",
            target => $"Earn {FormatFunds(target)} total across the run.",
            "earned",
            state => state.Stats.TotalFundsEarned,
            ("I", 100),
            ("II", 500),
            ("III", 1_000),
            ("IV", 5_000));

        AddCountAchievements(
            achievements,
            "thread_starter",
            "Thread Starter",
            target => $"Send {FormatTargetCount(target, "message", "messages")} through Communication.",
            "messages sent",
            state => state.Stats.MessagesSent,
            ("I", 10),
            ("II", 25),
            ("III", 50),
            ("IV", 100));

        return ValidateAchievements(achievements);
    }

    private static void AddCountAchievements(
        ICollection<RunAchievementDefinition> achievements,
        string idPrefix,
        string titleStem,
        Func<int, string> descriptionFactory,
        string progressLabel,
        Func<RunState, int> valueFactory,
        params (string TierLabel, int Target)[] tiers)
    {
        foreach (var (tierLabel, target) in tiers)
        {
            achievements.Add(new(
                $"{idPrefix}_{target}",
                $"{titleStem} {tierLabel}",
                descriptionFactory(target),
                state => valueFactory(state) >= target,
                state => FormatCountProgress(valueFactory(state), target, progressLabel)));
        }
    }

    private static void AddFundsAchievements(
        ICollection<RunAchievementDefinition> achievements,
        string idPrefix,
        string titleStem,
        Func<int, string> descriptionFactory,
        string progressLabel,
        Func<RunState, double> valueFactory,
        params (string TierLabel, int Target)[] tiers)
    {
        foreach (var (tierLabel, target) in tiers)
        {
            achievements.Add(new(
                $"{idPrefix}_{target}",
                $"{titleStem} {tierLabel}",
                descriptionFactory(target),
                state => valueFactory(state) >= target,
                state => FormatFundsProgress(valueFactory(state), target, progressLabel)));
        }
    }

    private static IReadOnlyList<RunAchievementDefinition> ValidateAchievements(List<RunAchievementDefinition> achievements)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var achievement in achievements)
        {
            if (!ids.Add(achievement.Id))
            {
                throw new InvalidOperationException($"Duplicate run achievement id detected: {achievement.Id}");
            }
        }

        if (achievements.Count != 100)
        {
            throw new InvalidOperationException($"Run achievement catalog must contain exactly 100 achievements, but contains {achievements.Count}.");
        }

        return achievements;
    }

    private static int GetCommunicationActionTotal(RunState state)
    {
        return state.Stats.MessagesSent + state.Stats.CallsMade;
    }

    private static int GetPurchasedUpgradeTierTotal(RunState state)
    {
        return state.PurchasedUpgrades.Values.Sum();
    }

    private static string FormatTargetCount(int target, string singular, string plural)
    {
        return $"{FormatCount(target)} {(target == 1 ? singular : plural)}";
    }

    private static string FormatCountProgress(int current, int target, string label)
    {
        return $"{FormatCount(current)}/{FormatCount(target)} {label}";
    }

    private static string FormatFundsProgress(double current, double target, string label)
    {
        return $"{FormatFunds(current)}/{FormatFunds(target)} {label}";
    }

    private static string FormatCount(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture);
    }

    private static string FormatFunds(double value)
    {
        return $"${value:0}";
    }
}
