using MicroDev.Core.Portfolio;

namespace MicroDev.Core.Simulation;

public sealed class SimulationEngine
{
    private const double BoundaryEpsilon = 0.0001;
    private const int MaxUpgradeTier = 5;
    private static readonly (string Title, string TechStack)[] JobProfiles =
    [
        ("Gameplay Programmer", "C# / .NET / MonoGame"),
        (".NET Tools Engineer", "C# / .NET / Tooling"),
        ("UI Systems Developer", "C# / .NET / UI"),
        ("Engine Support Programmer", "C# / .NET / Gameplay Systems"),
        ("Technical Gameplay Engineer", "C# / .NET / MonoGame"),
        ("Developer Experience Programmer", "C# / .NET / Tooling"),
        ("Runtime Interface Engineer", "C# / .NET / UI"),
        ("Build Pipeline Developer", "C# / .NET / Tooling"),
    ];
    private static readonly TechDebtSnippetTemplate[] TechDebtSnippetTemplates =
    [
        CreateInsertionTechDebtSnippet(
            "missing-semicolon",
            "Missing semicolon at the return line.",
            "error CS1002: ; expected",
            1,
            ";",
            "var readinessSummary = BuildReadinessSummary(snapshot);",
            "return readinessSummary",
            "}"),
        CreateReplaceTechDebtSnippet(
            "broken-method",
            "Broken method call in the current pass.",
            "error CS0103: The name 'BuildHudSuface' does not exist in the current context",
            0,
            "BuildHudSuface",
            "var renderPass = BuildHudSuface(widgetState);",
            "return renderPass;",
            "}"),
        CreateReplaceTechDebtSnippet(
            "unknown-variable",
            "Unknown variable slipped into a guard clause.",
            "error CS0103: The name 'sanitySpiralLevel' does not exist in the current context",
            0,
            "sanitySpiralLevel",
            "if (focusReserve <= 0 || sanitySpiralLevel > 2)",
            "{",
            "    return \"ship later\";",
            "}"),
        CreateReplaceTechDebtSnippet(
            "wrong-property",
            "Wrong property name broke the build.",
            "error CS1061: 'ProjectMetrics' does not contain a definition for 'VisibleLinse'",
            0,
            "VisibleLinse",
            "var visibleLines = projectMetrics.VisibleLinse;",
            "return visibleLines;",
            "}"),
        CreateReplaceTechDebtSnippet(
            "broken-append",
            "Broken logging call will not compile.",
            "error CS1061: 'StringBuilder' does not contain a definition for 'AppendLnie'",
            2,
            "AppendLnie",
            "foreach (var message in recentMessages)",
            "{",
            "    builder.AppendLnie(message);",
            "}"),
        CreateReplaceTechDebtSnippet(
            "broken-identifier",
            "A rushed rename left a dead identifier behind.",
            "error CS0103: The name 'sleepDebtBudget' does not exist in the current context",
            0,
            "sleepDebtBudget",
            "if (sleepDebtBudget <= 0)",
            "{",
            "    return BuildFallbackPlan();",
            "}"),
    ];
    private readonly Func<int> _runSeedProvider;

    private enum TechDebtTrigger
    {
        DeskIncident = 0,
        SleepDebt,
        Hunger,
        Sanity,
    }

    private sealed record TechDebtSnippetTemplate(
        string Id,
        string IssueLabel,
        string CompilerHint,
        string[] CodeLines,
        int HighlightLineIndex,
        int HighlightStartIndex,
        int HighlightLength,
        string HighlightToken,
        bool HighlightIsInsertion);

    public SimulationEngine(SimulationConfig config, Func<int>? runSeedProvider = null)
    {
        Config = config;
        _runSeedProvider = runSeedProvider ?? (() => Random.Shared.Next(1, int.MaxValue));
    }

    public SimulationConfig Config { get; }

    public RunState CreateNewRun(int? runSeed = null)
    {
        var resolvedRunSeed = Math.Clamp(runSeed ?? _runSeedProvider(), 1, int.MaxValue - 1);
        var projectBlueprint = ProceduralRunContent.CreateProjectBlueprint(resolvedRunSeed, Config.GameplayMode);
        var state = new RunState
        {
            Difficulty = Config.Difficulty,
            GameplayMode = Config.GameplayMode,
            IsRealisticMode = Config.RealisticMode,
            RunSeed = resolvedRunSeed,
            Day = Config.StartingDay,
            TimeOfDayMinutes = Config.StartingTimeOfDayMinutes,
            DeskMinutesElapsed = 0,
            Funds = Config.StartingFunds,
            Focus = Config.StartingFocus,
            Sanity = Config.StartingSanity,
            GeneratedJobListingCount = 0,
            GeneratedModifierIncidentCount = 0,
            NextGuaranteedJobDeskMinute = Config.FirstGuaranteedJobDelayMinutes,
            NextModifierDeskMinute = Config.FirstModifierIncidentDelayMinutes,
            HasFirstCoin = Config.StartWithFirstCoin,
            FirstCoinDecisionPending = false,
            FirstCoinRescueDeficit = 0,
            LinesOfCode = Config.StartingLinesOfCode,
            CurrentPortfolioLinesOfCode = Config.StartingLinesOfCode,
            CodeQuality = Config.StartingCodeQuality,
            SluggishMinutesRemaining = 0,
            DeepWorkMinutesRemaining = 0,
            ContextSwitchMinutesRemaining = 0,
            MinutesSinceLastMeal = Config.StartingMinutesSinceLastMeal,
            MinutesSinceLastSleep = Config.StartingMinutesSinceLastSleep,
            DebuggingPressure = 0,
            NeedDrivenBugCooldownMinutesRemaining = 0,
            SuccessfulApplications = 0,
            BossDisposition = ProceduralRunContent.GetBossDisposition(resolvedRunSeed, Config.GameplayMode),
            BossName = ProceduralRunContent.GetBossName(resolvedRunSeed),
            BossTitle = ProceduralRunContent.GetBossTitle(resolvedRunSeed),
            CorporateStanding = 0,
            HasApartment = false,
            HasHouse = false,
            HasRetired = false,
            StudioName = string.Empty,
            CurrentProgramIndex = 0,
            CurrentProgramVisibleLineCount = 0,
            CurrentProjectBlueprint = projectBlueprint,
            VersionControl = CreateVersionControlState(projectBlueprint),
            PublishedAppCount = 0,
            PublishedAppSaleCount = 0,
            NextPublishedAppSaleDeskMinute = double.PositiveInfinity,
            LastPublishedAppName = null,
            RecentCompletedFileName = null,
            FileCompletionCelebrationMinutesRemaining = 0,
            ActiveJobApplication = null,
            ActiveFreelanceGig = null,
            Status = RunStatus.InProgress,
        };

        if (state.CurrentPortfolioLinesOfCode > 0)
        {
            PortfolioWorkspace.SynchronizeToLinesOfCode(state);
        }

        state.VersionControl.CommittedPortfolioLinesOfCode = state.CurrentPortfolioLinesOfCode;
        RefreshPendingChangeLines(state);

        AppendLog(state, "Another week begins. Rent hits at midnight and recruiters are watching.");
        AppendLog(state, $"{GetGameplayModeLabel(state)} is active{(state.IsRealisticMode ? " with Realistic+ pressure layered on top." : ".")}");
        AppendLog(state, $"Run seed {state.RunSeed} locked in. Project order, desk incidents, food flavor, and life events will all riff on it.");
        AppendLog(state, $"Current build plan: {state.CurrentProjectBlueprint.Title}. {state.CurrentProjectBlueprint.Pitch}");
        AppendLog(state, $"Opened {PortfolioWorkspace.GetCurrentProgram(state).FileName} in a blank editor.");
        if (state.HasFirstCoin)
        {
            AppendLog(state, "The first coin still hangs on the desk, quietly steadying your nerves.");
        }

        if (state.GameplayMode == GameplayLoopMode.Corporate)
        {
            AppendLog(state, $"{state.BossName}, {state.BossTitle}, is your boss this run. {ProceduralRunContent.GetBossFlavor(state.BossDisposition, state.BossName)}");
        }
        else if (state.GameplayMode == GameplayLoopMode.Founder)
        {
            state.PendingLifeEvent = CreateFounderNamingEvent(state);
            AppendLog(state, "Founder Mode is live. Name the studio, survive the basement, and grow the company from scratch.");
        }

        return state;
    }

    public bool CanApplyAction(RunState state, PlayerAction action)
    {
        if (action == PlayerAction.RestartRun)
        {
            return true;
        }

        if (state.Status != RunStatus.InProgress ||
            state.FirstCoinDecisionPending ||
            state.PendingLifeEvent is not null)
        {
            return false;
        }

        if (IsActionLockedBySleep(state, action))
        {
            return false;
        }

        return action switch
        {
            PlayerAction.WriteCode => state.Focus > 0 &&
                                      state.ActiveCatInterruption is null,
            PlayerAction.Eat => CanPlaceFoodOrder(state, FoodChoice.Burger),
            PlayerAction.Freelance => CanTakeFreelanceGig(state, FreelanceGigType.QuickBugfix),
            PlayerAction.Sleep => true,
            PlayerAction.PetCat => state.ActiveCatInterruption is not null,
            PlayerAction.SquashBug => state.ActiveTechDebtBug is not null && state.Focus >= GetSquashBugFocusCost(state),
            PlayerAction.ApplyForJob => state.ActiveJobListing is not null && CanBeginJobApplication(state, state.ActiveJobListing),
            PlayerAction.PublishApp => CanPublishCurrentApp(state),
            _ => false,
        };
    }

    public bool CanPlaceFoodOrder(RunState state, FoodChoice choice, bool expeditedDelivery = false)
    {
        expeditedDelivery &= AllowsExpeditedDelivery(choice);
        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               state.ActiveFoodDelivery is null &&
               state.Funds >= GetFoodTotalCost(state, choice, expeditedDelivery);
    }

    public double GetFoodTotalCost(RunState state, FoodChoice choice, bool expeditedDelivery = false)
    {
        return GetFoodCost(state, choice) + GetFoodTipAmount(choice, expeditedDelivery);
    }

    public double GetFoodTotalCost(FoodChoice choice, bool expeditedDelivery = false)
    {
        return GetFoodCost(choice) + GetFoodTipAmount(choice, expeditedDelivery);
    }

    public double GetFoodTipAmount(bool expeditedDelivery)
    {
        return expeditedDelivery
            ? Config.ExpeditedFoodDeliveryTipAmount
            : 0;
    }

    public double GetFoodTipAmount(FoodChoice choice, bool expeditedDelivery)
    {
        return expeditedDelivery && AllowsExpeditedDelivery(choice)
            ? Config.ExpeditedFoodDeliveryTipAmount
            : 0;
    }

    public double GetFoodDeliveryDuration(bool expeditedDelivery)
    {
        return expeditedDelivery
            ? Config.ExpeditedFoodDeliveryDurationMinutes
            : Config.FoodDeliveryDurationMinutes;
    }

    public double GetFoodDeliveryDuration(RunState state, FoodChoice choice, bool expeditedDelivery = false)
    {
        var duration = GetBaseFoodDuration(choice);
        duration -= IsHomeCooked(choice)
            ? GetUpgradeBonusTotal(state, definition => definition.HomeCookDurationReductionMinutes)
            : GetUpgradeBonusTotal(state, definition => definition.FoodDeliveryDurationReductionMinutes);

        if (expeditedDelivery && AllowsExpeditedDelivery(choice))
        {
            duration = Math.Min(duration, Config.ExpeditedFoodDeliveryDurationMinutes);
        }

        return Math.Max(IsHomeCooked(choice) ? 20 : 6, duration);
    }

    public FoodOptionDefinition GetFoodOption(RunState state, FoodChoice choice)
    {
        return GetFoodOption(state.RunSeed, choice);
    }

    public FoodOptionDefinition GetFoodOption(FoodChoice choice)
    {
        return GetFoodOption(0, choice);
    }

    public bool AllowsExpeditedDelivery(FoodChoice choice)
    {
        return !IsHomeCooked(choice);
    }

    public bool IsHomeCooked(FoodChoice choice)
    {
        return choice is FoodChoice.SkilletPasta or FoodChoice.MealPrepChili;
    }

    public IReadOnlyList<FoodOrderModifierOption> GetFoodOrderModifiers(RunState state, FoodChoice choice)
    {
        return ProceduralRunContent.GetFoodModifiers(state.RunSeed, choice);
    }

    public IReadOnlyList<FoodOrderModifierOption> GetFoodOrderModifiers(FoodChoice choice)
    {
        return ProceduralRunContent.GetFoodModifiers(0, choice);
    }

    public bool HasActiveFreelanceGig(RunState state)
    {
        return state.ActiveFreelanceGig is not null;
    }

    public IReadOnlyList<string> GetVisibleFreelanceGigLines(RunState state)
    {
        if (state.ActiveFreelanceGig is null)
        {
            return Array.Empty<string>();
        }

        return state.ActiveFreelanceGig.CodeLines
            .Take(state.ActiveFreelanceGig.VisibleLineCount)
            .ToArray();
    }

    public IReadOnlyList<SocialContact> GetKnownContacts(RunState state)
    {
        return state.KnownContacts;
    }

    public bool CanMessageContact(RunState state, string contactId)
    {
        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               state.ActiveCatInterruption is null &&
               FindKnownContact(state, contactId) is not null &&
               state.Focus >= 1;
    }

    public bool CanCallContact(RunState state, string contactId)
    {
        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               state.ActiveCatInterruption is null &&
               FindKnownContact(state, contactId) is not null &&
               state.Focus >= 2;
    }

    public bool MessageContact(RunState state, string contactId)
    {
        return ApplyCommunicationAction(state, contactId, isCall: false);
    }

    public bool CallContact(RunState state, string contactId)
    {
        return ApplyCommunicationAction(state, contactId, isCall: true);
    }

    public double GetCurrentSquashBugFocusCost(RunState state)
    {
        return GetSquashBugFocusCost(state);
    }

    private FoodOptionDefinition GetFoodOption(int runSeed, FoodChoice choice)
    {
        return choice switch
        {
            FoodChoice.Burrito => new FoodOptionDefinition(
                choice,
                ProceduralRunContent.GetFoodName(runSeed, choice),
                ProceduralRunContent.GetFoodDescription(runSeed, choice),
                Config.BurritoFundsCost,
                Config.BurritoFocusGain,
                Config.BurritoSanityGain,
                Config.BurritoSluggishDurationMinutes),
            FoodChoice.Pizza => new FoodOptionDefinition(
                choice,
                ProceduralRunContent.GetFoodName(runSeed, choice),
                ProceduralRunContent.GetFoodDescription(runSeed, choice),
                Config.PizzaFundsCost,
                Config.PizzaFocusGain,
                Config.PizzaSanityGain,
                Config.PizzaSluggishDurationMinutes),
            FoodChoice.Dumplings => new FoodOptionDefinition(
                choice,
                ProceduralRunContent.GetFoodName(runSeed, choice),
                ProceduralRunContent.GetFoodDescription(runSeed, choice),
                Config.DumplingsFundsCost,
                Config.DumplingsFocusGain,
                Config.DumplingsSanityGain,
                Config.DumplingsSluggishDurationMinutes),
            FoodChoice.Ramen => new FoodOptionDefinition(
                choice,
                ProceduralRunContent.GetFoodName(runSeed, choice),
                ProceduralRunContent.GetFoodDescription(runSeed, choice),
                Config.RamenFundsCost,
                Config.RamenFocusGain,
                Config.RamenSanityGain,
                Config.RamenSluggishDurationMinutes),
            FoodChoice.RiceBowl => new FoodOptionDefinition(
                choice,
                ProceduralRunContent.GetFoodName(runSeed, choice),
                ProceduralRunContent.GetFoodDescription(runSeed, choice),
                Config.RiceBowlFundsCost,
                Config.RiceBowlFocusGain,
                Config.RiceBowlSanityGain,
                Config.RiceBowlSluggishDurationMinutes),
            FoodChoice.SkilletPasta => new FoodOptionDefinition(
                choice,
                ProceduralRunContent.GetFoodName(runSeed, choice),
                ProceduralRunContent.GetFoodDescription(runSeed, choice),
                Config.SkilletPastaFundsCost,
                Config.SkilletPastaFocusGain,
                Config.SkilletPastaSanityGain,
                Config.SkilletPastaSluggishDurationMinutes),
            FoodChoice.MealPrepChili => new FoodOptionDefinition(
                choice,
                ProceduralRunContent.GetFoodName(runSeed, choice),
                ProceduralRunContent.GetFoodDescription(runSeed, choice),
                Config.MealPrepChiliFundsCost,
                Config.MealPrepChiliFocusGain,
                Config.MealPrepChiliSanityGain,
                Config.MealPrepChiliSluggishDurationMinutes),
            _ => new FoodOptionDefinition(
                choice,
                ProceduralRunContent.GetFoodName(runSeed, choice),
                ProceduralRunContent.GetFoodDescription(runSeed, choice),
                Config.BurgerFundsCost,
                Config.BurgerFocusGain,
                Config.BurgerSanityGain,
                Config.BurgerSluggishDurationMinutes),
        };
    }

    public double GetFoodOrderPenaltyMinutes(
        FoodChoice choice,
        IReadOnlyCollection<FoodOrderModifier> selectedModifiers,
        bool reviewReceipt)
    {
        var basePenalty = GetFoodOption(choice).SluggishMinutesWhenUnchecked;
        if (basePenalty <= 0)
        {
            return 0;
        }

        var modifierOptions = GetFoodOrderModifiers(choice);
        var requiredCount = modifierOptions.Count(option => option.Recommended);
        var matchedCount = modifierOptions.Count(option => option.Recommended && selectedModifiers.Contains(option.Modifier));

        if (matchedCount >= requiredCount && reviewReceipt)
        {
            return 0;
        }

        if (matchedCount >= requiredCount)
        {
            return Math.Round(basePenalty * 0.25, 0);
        }

        if (reviewReceipt && matchedCount == Math.Max(0, requiredCount - 1))
        {
            return Math.Round(basePenalty * 0.5, 0);
        }

        if (reviewReceipt || matchedCount > 0)
        {
            return Math.Round(basePenalty * 0.75, 0);
        }

        return basePenalty;
    }

    public FreelanceGigDefinition GetFreelanceGig(FreelanceGigType type)
    {
        return GetFreelanceGig(0, new ProjectBlueprint(), type);
    }

    public FreelanceGigDefinition GetFreelanceGig(RunState state, FreelanceGigType type)
    {
        return GetFreelanceGig(state.RunSeed, state.CurrentProjectBlueprint, type);
    }

    private FreelanceGigDefinition GetFreelanceGig(int runSeed, ProjectBlueprint blueprint, FreelanceGigType type)
    {
        var assignment = ProceduralRunContent.CreateFreelanceAssignment(runSeed, blueprint, type);
        return type switch
        {
            FreelanceGigType.UIPolishPass => new FreelanceGigDefinition(
                type,
                assignment.Title,
                assignment.Brief,
                Config.UiPolishDurationMinutes,
                Config.UiPolishFundsGain,
                Config.UiPolishFocusCost,
                Config.UiPolishSanityCost,
                Config.UiPolishQualityGain),
            FreelanceGigType.GameplayTunePass => new FreelanceGigDefinition(
                type,
                assignment.Title,
                assignment.Brief,
                Config.GameplayTuneDurationMinutes,
                Config.GameplayTuneFundsGain,
                Config.GameplayTuneFocusCost,
                Config.GameplayTuneSanityCost,
                Config.GameplayTuneQualityGain),
            FreelanceGigType.DataMigration => new FreelanceGigDefinition(
                type,
                assignment.Title,
                assignment.Brief,
                Config.DataMigrationDurationMinutes,
                Config.DataMigrationFundsGain,
                Config.DataMigrationFocusCost,
                Config.DataMigrationSanityCost,
                Config.DataMigrationQualityGain),
            FreelanceGigType.PipelineRescue => new FreelanceGigDefinition(
                type,
                assignment.Title,
                assignment.Brief,
                Config.PipelineRescueDurationMinutes,
                Config.PipelineRescueFundsGain,
                Config.PipelineRescueFocusCost,
                Config.PipelineRescueSanityCost,
                Config.PipelineRescueQualityGain),
            _ => new FreelanceGigDefinition(
                type,
                assignment.Title,
                assignment.Brief,
                Config.QuickBugfixDurationMinutes,
                Config.QuickBugfixFundsGain,
                Config.QuickBugfixFocusCost,
                Config.QuickBugfixSanityCost,
                Config.QuickBugfixQualityGain),
        };
    }

    public bool CanTakeFreelanceGig(RunState state, FreelanceGigType type)
    {
        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               state.ActiveFreelanceGig is null &&
               !RequiresSleep(state) &&
               state.Focus >= Config.FreelanceMinimumFocusRequired;
    }

    public bool TakeFreelanceGig(RunState state, FreelanceGigType type)
    {
        return BeginFreelanceGig(state, type);
    }

    public bool BeginFreelanceGig(RunState state, FreelanceGigType type)
    {
        if (!CanTakeFreelanceGig(state, type))
        {
            if (RequiresSleep(state))
            {
                AppendLog(state, "No freelance heroics on two days without sleep. Crash first, then take the gig.");
            }
            else if (state.Focus < Config.FreelanceMinimumFocusRequired)
            {
                AppendLog(state, $"Freelance work needs at least {Config.FreelanceMinimumFocusRequired:0} focus or the contract just becomes a new fire.");
            }

            return false;
        }

        state.ActiveFreelanceGig = CreateFreelanceGig(state, type);
        AppendLog(
            state,
            $"{state.ActiveFreelanceGig.Title} starts for {state.ActiveFreelanceGig.ClientName}. Ship the contract code to lock in the payout.");
        return true;
    }

    public bool CanWorkOnFreelanceGig(RunState state)
    {
        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               state.ActiveFreelanceGig is not null &&
               !state.ActiveFreelanceGig.IsComplete &&
               state.ActiveCatInterruption is null &&
               !RequiresSleep(state) &&
               state.Focus > 0;
    }

    public bool WorkOnFreelanceGig(RunState state)
    {
        if (!CanWorkOnFreelanceGig(state))
        {
            if (state.ActiveCatInterruption is not null)
            {
                AppendLog(state, "The desk distraction is still live. Clear it before you keep typing contract code.");
            }
            else if (RequiresSleep(state))
            {
                AppendLog(state, "You are too sleep-deprived to finish contract work cleanly. Sleep first.");
            }
            else if (state.Focus <= 0)
            {
                AppendLog(state, "There is not enough focus left to keep the freelance contract moving.");
            }

            return false;
        }

        var gig = state.ActiveFreelanceGig!;
        var linesAdded = RevealFreelanceLines(gig, GetWriteCodeLinesGain(state));
        if (linesAdded == 0)
        {
            return false;
        }

        AppendLog(state, $"Freelance progress: +{linesAdded} contract LoC for {gig.Title}.");
        if (!gig.IsComplete)
        {
            return true;
        }

        AdvanceTime(state, gig.DurationMinutes);
        var fundsGain = Math.Round(gig.FundsGain * GetFreelanceFundsMultiplier(state, gig.Type), 0);
        if (state.Status == RunStatus.InProgress)
        {
            state.Funds += fundsGain;
            state.Focus = Clamp(state.Focus - gig.FocusCost, 0, Config.MaxFocus);
            state.Sanity = Clamp(state.Sanity - gig.SanityCost, 0, Config.MaxSanity);
            state.CodeQuality = Clamp(state.CodeQuality + gig.CodeQualityGain, 0, Config.MaxCodeQuality);
            AwardResumeProof(state, GetResumeTrackForGig(gig.Type), 1, $"{gig.Title} gave the resume a stronger {GetResumeTrackLabel(GetResumeTrackForGig(gig.Type)).ToLowerInvariant()} story.");
            AppendLog(
                state,
                $"{gig.Title} shipped for {gig.ClientName}: +${fundsGain:0}, -{gig.FocusCost:0} focus, -{gig.SanityCost:0} sanity, +{gig.CodeQualityGain:0.#} quality.");
            EvaluateLossState(state);
        }

        state.ActiveFreelanceGig = null;
        return true;
    }

    public bool CanPurchaseUpgrade(RunState state, EfficiencyUpgradeType type)
    {
        if (state.Status != RunStatus.InProgress ||
            GetUpgradeTier(state, type) >= MaxUpgradeTier)
        {
            return false;
        }

        return !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               state.Funds >= GetUpgradePurchaseCost(state, type);
    }

    public bool PurchaseUpgrade(RunState state, EfficiencyUpgradeType type)
    {
        if (state.Status != RunStatus.InProgress)
        {
            return false;
        }

        var definition = EfficiencyUpgradeCatalog.Get(type);
        var currentTier = GetUpgradeTier(state, type);
        if (currentTier >= MaxUpgradeTier)
        {
            AppendLog(state, $"{definition.Name} is already maxed at tier {MaxUpgradeTier}.");
            return false;
        }

        var purchaseCost = GetUpgradePurchaseCost(state, type);
        if (state.Funds < purchaseCost)
        {
            AppendLog(state, $"Not enough funds for {definition.Name}.");
            return false;
        }

        var newTier = currentTier + 1;
        state.Funds -= purchaseCost;
        state.PurchasedUpgrades[type] = newTier;
        AppendLog(state, $"Installed {definition.Name} tier {newTier}/{MaxUpgradeTier}: {definition.GetTotalEffectSummary(newTier)}");
        if (newTier <= 2)
        {
            AwardInterviewPrep(state, 1, $"{definition.Name} tightened the interview plan.");
        }

        return true;
    }

    public double GetApartmentCost(RunState state)
    {
        var baseCost = Config.ApartmentMoveCost + (state.IsRealisticMode ? 18 : 0);
        baseCost += state.GameplayMode switch
        {
            GameplayLoopMode.Corporate => 12,
            GameplayLoopMode.Founder => -10,
            GameplayLoopMode.Indie => -6,
            _ => 0,
        };
        baseCost -= Math.Min(12, state.CorporateStanding * 2);
        return Math.Round(Math.Max(90, baseCost), 0);
    }

    public double GetHouseCost(RunState state)
    {
        var baseCost = 278d + (state.IsRealisticMode ? 35 : 0);
        baseCost += state.GameplayMode switch
        {
            GameplayLoopMode.Corporate => 30,
            GameplayLoopMode.Founder => -15,
            GameplayLoopMode.Indie => -10,
            _ => 0,
        };
        baseCost -= Math.Min(24, state.CorporateStanding * 4);
        return Math.Round(Math.Max(180, baseCost), 0);
    }

    public double GetRetirementCost(RunState state)
    {
        var baseCost = 165d + (state.IsRealisticMode ? 25 : 0);
        baseCost += state.GameplayMode switch
        {
            GameplayLoopMode.Corporate => 20,
            GameplayLoopMode.Founder => -10,
            _ => 0,
        };
        baseCost -= Math.Min(18, state.CorporateStanding * 3);
        return Math.Round(Math.Max(135, baseCost), 0);
    }

    public bool CanMoveToApartment(RunState state)
    {
        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               !state.HasApartment &&
               state.Funds >= GetApartmentCost(state) &&
               (state.GameplayMode != GameplayLoopMode.Interview || state.SuccessfulApplications > 0 || state.PublishedAppCount > 0);
    }

    public bool MoveToApartment(RunState state)
    {
        if (!CanMoveToApartment(state))
        {
            return false;
        }

        var apartmentCost = GetApartmentCost(state);
        state.Funds -= apartmentCost;
        state.HasApartment = true;
        state.Sanity = Clamp(state.Sanity + 5, 0, Config.MaxSanity);
        AppendLog(state, $"You finally move out of the basement and into an apartment. -${apartmentCost:0}, sanity +5. The run feels less temporary.");
        return true;
    }

    public bool CanBuyHouse(RunState state)
    {
        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               state.HasApartment &&
               !state.HasHouse &&
               state.Funds >= GetHouseCost(state);
    }

    public bool BuyHouse(RunState state)
    {
        if (!CanBuyHouse(state))
        {
            return false;
        }

        var houseCost = GetHouseCost(state);
        state.Funds -= houseCost;
        state.HasHouse = true;
        state.Sanity = Clamp(state.Sanity + 8, 0, Config.MaxSanity);
        AppendLog(state, $"You finally lock in a down payment and buy a house. -${houseCost:0}, sanity +8. The run suddenly feels less temporary.");
        return true;
    }

    public bool CanRetire(RunState state)
    {
        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               state.HasHouse &&
               !state.HasRetired &&
               state.Funds >= GetRetirementCost(state);
    }

    public bool Retire(RunState state)
    {
        if (!CanRetire(state))
        {
            return false;
        }

        var retirementCost = GetRetirementCost(state);
        state.Funds -= retirementCost;
        state.HasRetired = true;
        state.Sanity = Clamp(state.Sanity + 16, 0, Config.MaxSanity);
        state.Status = RunStatus.Won;
        state.OutcomeMessage = BuildRetirementOutcomeMessage(state);
        AppendLog(state, $"The long game lands. -${retirementCost:0}, sanity +16, and the run finally closes on house keys, savings, and life away from the desk.");
        return true;
    }

    private static string BuildRetirementOutcomeMessage(RunState state)
    {
        return state.GameplayMode switch
        {
            GameplayLoopMode.Corporate => $"You survived the office long enough to buy a house, bank retirement money, and finally walk away from {state.BossName}'s calendar.",
            GameplayLoopMode.Indie => "You stayed self-directed long enough to buy a house and retire on work you chose to make.",
            GameplayLoopMode.Founder => $"You bootstrapped {state.StudioName switch { { Length: > 0 } studioName => studioName, _ => "your studio" }} into a real business, bought a house, and retired on your own terms.",
            _ => "You built enough stability to buy a house, bank retirement money, and finally step away from the desk.",
        };
    }

    public bool CanCommitChanges(RunState state)
    {
        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               state.VersionControl.PendingChangeLines > 0;
    }

    public bool CommitChanges(RunState state)
    {
        if (!CanCommitChanges(state))
        {
            return false;
        }

        var versionControl = state.VersionControl;
        var pendingLines = versionControl.PendingChangeLines;
        var completedFiles = Math.Max(1, versionControl.PendingCompletedFileCount);
        var commitDuration = Math.Max(6, Math.Min(28, 5 + (pendingLines / 2d) + completedFiles));
        var focusCost = Math.Max(1, Math.Ceiling(pendingLines / 10d));

        AdvanceTime(state, commitDuration);
        if (state.Status != RunStatus.InProgress)
        {
            return false;
        }

        state.Focus = Clamp(state.Focus - focusCost, 0, Config.MaxFocus);
        versionControl.CommitCount += 1;
        versionControl.LastCommitSummary = ProceduralRunContent.GetVersionControlCommitSummary(
            state.RunSeed,
            state.CurrentProjectBlueprint,
            versionControl.CommitCount);
        versionControl.CommittedPortfolioLinesOfCode = state.CurrentPortfolioLinesOfCode;
        versionControl.PendingChangeLines = 0;
        versionControl.PendingCompletedFileCount = 0;

        var qualityBonus = completedFiles switch
        {
            1 => 0.8,
            2 => 0.45,
            3 => 0.2,
            _ => 0,
        };
        if (qualityBonus > 0)
        {
            state.CodeQuality = Clamp(state.CodeQuality + qualityBonus, 0, Config.MaxCodeQuality);
        }

        AppendLog(
            state,
            $"Committed {completedFiles} file{(completedFiles == 1 ? string.Empty : "s")} in \"{versionControl.LastCommitSummary}\" after {FormatMinutesForLog(commitDuration)}. Focus -{focusCost:0}{(qualityBonus > 0 ? $", quality +{qualityBonus:0.##}" : string.Empty)}.");
        if (completedFiles <= 2)
        {
            AwardInterviewPrep(state, 1, completedFiles == 1
                ? "A tight single-file commit made the work easy to explain."
                : "A tidy grouped commit kept the work readable and easy to explain.");
        }

        EvaluateLossState(state);
        return true;
    }

    public bool CanCreateFeatureBranch(RunState state)
    {
        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               !state.VersionControl.HasFeatureBranch &&
               state.VersionControl.ActiveMergeConflict is null &&
               state.VersionControl.PendingChangeLines == 0;
    }

    public bool CanToggleFeatureBranch(RunState state)
    {
        if (CanCreateFeatureBranch(state))
        {
            return true;
        }

        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               state.VersionControl.HasFeatureBranch &&
               state.VersionControl.ActiveMergeConflict is null &&
               state.VersionControl.PendingChangeLines == 0 &&
               state.VersionControl.FeatureBranchCommitCount == 0;
    }

    public bool ToggleFeatureBranch(RunState state)
    {
        if (CanCreateFeatureBranch(state))
        {
            return CreateFeatureBranch(state);
        }

        if (!CanToggleFeatureBranch(state))
        {
            return false;
        }

        state.VersionControl.CurrentBranchName = state.VersionControl.MainBranchName;
        state.VersionControl.FeatureBranchCommitCount = 0;
        AppendLog(state, "Closed the empty feature branch and moved the repo back to main. The release lane is clear again.");
        return true;
    }

    public bool CreateFeatureBranch(RunState state)
    {
        if (!CanCreateFeatureBranch(state))
        {
            return false;
        }

        var versionControl = state.VersionControl;
        versionControl.BranchSerial += 1;
        versionControl.CurrentBranchName = ProceduralRunContent.GetVersionControlBranchName(
            state.RunSeed,
            state.CurrentProjectBlueprint,
            versionControl.BranchSerial);
        versionControl.FeatureBranchCommitCount = 0;
        AppendLog(state, $"Created {versionControl.CurrentBranchName}. This release is now splitting work off main.");
        return true;
    }

    public bool CanMergeFeatureBranch(RunState state)
    {
        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               state.VersionControl.HasFeatureBranch &&
               state.VersionControl.ActiveMergeConflict is null &&
               state.VersionControl.PendingChangeLines == 0 &&
               state.VersionControl.FeatureBranchCommitCount > 0;
    }

    public bool MergeFeatureBranch(RunState state)
    {
        if (!CanMergeFeatureBranch(state))
        {
            return false;
        }

        var versionControl = state.VersionControl;
        var branchName = versionControl.CurrentBranchName;
        var mergeDuration = 12 + (versionControl.FeatureBranchCommitCount * 4);
        AdvanceTime(state, mergeDuration);
        if (state.Status != RunStatus.InProgress)
        {
            return false;
        }

        var conflictThreshold = 28 +
                                (state.CurrentProjectBlueprint.ProductType == ProjectProductType.Game ? 12 : 0) +
                                (state.GameplayMode == GameplayLoopMode.Corporate && state.BossDisposition == BossDisposition.Micromanager ? 10 : 0) +
                                Math.Min(18, versionControl.FeatureBranchCommitCount * 4);
        var conflictRoll = CreateSeed(state.RunSeed, $"merge:{branchName}:{versionControl.CommitCount}:{state.CurrentProjectBlueprint.Signature}") % 100;
        if (conflictRoll < conflictThreshold)
        {
            versionControl.MergeConflictCount += 1;
            versionControl.ActiveMergeConflict = ProceduralRunContent.CreateMergeConflict(
                state.RunSeed,
                state.CurrentProjectBlueprint,
                branchName,
                versionControl.MergeConflictCount);
            AppendLog(
                state,
                $"Merge started for {branchName}, but conflict markers explode in {versionControl.ActiveMergeConflict.FileName}. Resolve it before the release can move.");
            return true;
        }

        versionControl.CurrentBranchName = versionControl.MainBranchName;
        versionControl.FeatureBranchCommitCount = 0;
        state.CodeQuality = Clamp(state.CodeQuality + 2, 0, Config.MaxCodeQuality);
        AppendLog(state, $"Merged {branchName} cleanly back into main. Code quality +2 from the tidy integration.");
        return true;
    }

    public bool CanResolveMergeConflictOption(RunState state, int optionIndex)
    {
        return state.Status == RunStatus.InProgress &&
               state.VersionControl.ActiveMergeConflict is not null &&
               optionIndex is >= 0 and <= 2;
    }

    public bool ResolveMergeConflictOption(RunState state, int optionIndex)
    {
        if (!CanResolveMergeConflictOption(state, optionIndex))
        {
            return false;
        }

        var versionControl = state.VersionControl;
        var conflict = versionControl.ActiveMergeConflict!;
        var resolutionDuration = 18 + (conflict.Severity * 10);
        var focusCost = 2 + conflict.Severity;

        AdvanceTime(state, resolutionDuration);
        if (state.Status != RunStatus.InProgress)
        {
            return false;
        }

        state.Focus = Clamp(state.Focus - focusCost, 0, Config.MaxFocus);
        versionControl.ActiveMergeConflict = null;
        versionControl.CurrentBranchName = versionControl.MainBranchName;
        versionControl.FeatureBranchCommitCount = 0;

        if (optionIndex == conflict.OptimalResolutionOptionIndex)
        {
            state.CodeQuality = Clamp(state.CodeQuality + conflict.Severity, 0, Config.MaxCodeQuality);
            AppendLog(
                state,
                $"You resolved the {conflict.FileName} merge cleanly and carried the good parts forward. Focus -{focusCost:0}, code quality +{conflict.Severity:0}.");
            EvaluateLossState(state);
            return true;
        }

        var cleanupLines = 4 + (conflict.Severity * 3);
        versionControl.PendingChangeLines += cleanupLines;
        state.CodeQuality = Clamp(state.CodeQuality - (2 * conflict.Severity), 0, Config.MaxCodeQuality);
        AppendLog(
            state,
            $"The merge technically lands, but {conflict.FileName} comes out rough. Focus -{focusCost:0}, code quality -{2 * conflict.Severity:0}, and {cleanupLines} lines of cleanup are still dirty.");
        EvaluateLossState(state);
        return true;
    }

    public bool CanEditProjectBlueprint(RunState state)
    {
        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               state.ActiveJobApplication is null &&
               state.CurrentPortfolioLinesOfCode == 0 &&
               state.VersionControl.PendingChangeLines == 0;
    }

    public bool AdvanceProjectBlueprintField(RunState state, ProjectPlanField field)
    {
        if (!CanEditProjectBlueprint(state))
        {
            return false;
        }

        state.CurrentProjectBlueprint = ProceduralRunContent.AdvanceProjectBlueprint(
            state.RunSeed,
            state.GameplayMode,
            state.CurrentProjectBlueprint,
            field);
        AppendLog(state, $"Project studio pivots the next release toward {state.CurrentProjectBlueprint.Title}.");
        return true;
    }

    public bool RerollProjectBlueprint(RunState state)
    {
        if (!CanEditProjectBlueprint(state))
        {
            return false;
        }

        state.CurrentProjectBlueprint = ProceduralRunContent.RerollProjectBlueprint(
            state.RunSeed,
            state.GameplayMode,
            state.CurrentProjectBlueprint);
        AppendLog(state, $"Project studio rerolls the concept. The next release is now {state.CurrentProjectBlueprint.Title}.");
        return true;
    }

    public bool IsSluggish(RunState state)
    {
        return state.SluggishMinutesRemaining > BoundaryEpsilon;
    }

    public bool IsDeepWorkActive(RunState state)
    {
        return state.DeepWorkMinutesRemaining > BoundaryEpsilon;
    }

    public bool IsContextSwitchActive(RunState state)
    {
        return state.ContextSwitchMinutesRemaining > BoundaryEpsilon;
    }

    public int GetHungerStage(RunState state)
    {
        return GetHungerStage(state.MinutesSinceLastMeal);
    }

    public int GetSleepStage(RunState state)
    {
        return GetSleepStage(state.MinutesSinceLastSleep);
    }

    public bool RequiresSleep(RunState state)
    {
        return GetSleepStage(state) >= 4;
    }

    public bool CanUseFirstCoin(RunState state)
    {
        return state.Status == RunStatus.InProgress &&
               state.FirstCoinDecisionPending &&
               state.HasFirstCoin;
    }

    public bool UseFirstCoin(RunState state)
    {
        if (!CanUseFirstCoin(state))
        {
            return false;
        }

        state.HasFirstCoin = false;
        state.FirstCoinDecisionPending = false;
        state.Funds = Math.Max(0, state.Funds + Config.FirstCoinEmergencyFundsGain);
        state.FirstCoinRescueDeficit = 0;
        state.Sanity = Clamp(state.Sanity - Config.FirstCoinBreakSanityLoss, 0, Config.MaxSanity);

        AppendLog(state, $"The frame cracks. The first coin buys +${Config.FirstCoinEmergencyFundsGain:0} and the hope it carried is gone.");
        EvaluateLossState(state);
        return true;
    }

    public bool DeclineFirstCoin(RunState state)
    {
        if (state.Status != RunStatus.InProgress || !state.FirstCoinDecisionPending)
        {
            return false;
        }

        state.FirstCoinDecisionPending = false;
        state.Status = RunStatus.Evicted;
        state.OutcomeMessage = "Rent came due, the first coin stayed framed, and the landlord still changed the locks.";
        AppendLog(state, "You let the coin stay framed. The eviction notice still came.");
        return true;
    }

    public bool HasPendingLifeEvent(RunState state)
    {
        return state.PendingLifeEvent is not null;
    }

    public bool CanResolveLifeEventOption(RunState state, int optionIndex)
    {
        if (state.Status != RunStatus.InProgress ||
            state.FirstCoinDecisionPending ||
            state.PendingLifeEvent is null)
        {
            return false;
        }

        return state.PendingLifeEvent.Type switch
        {
            IncidentType.ComputerFreeze => optionIndex switch
            {
                0 => true,
                1 => state.Funds >= Config.ComputerFreezeTechSupportFundsCost,
                2 => state.Funds >= Config.ComputerFreezeRepairShopFundsCost,
                _ => false,
            },
            IncidentType.StreamingBinge => optionIndex is >= 0 and <= 2,
            IncidentType.OnlineMatch => optionIndex switch
            {
                0 => true,
                1 => true,
                2 => true,
                _ => false,
            },
            IncidentType.PartnerCheckIn => optionIndex switch
            {
                0 => true,
                1 => state.Funds >= Config.PartnerCheckInDinnerFundsCost,
                2 => true,
                _ => false,
            },
            IncidentType.CareerPathChoice => optionIndex is >= 0 and <= 2,
            IncidentType.BossCheckIn => optionIndex is >= 0 and <= 2,
            IncidentType.CoworkerInterruption => optionIndex is >= 0 and <= 2,
            IncidentType.FounderNaming => optionIndex is >= 0 and <= 2,
            _ => false,
        };
    }

    public bool ResolveLifeEventOption(RunState state, int optionIndex)
    {
        if (!CanResolveLifeEventOption(state, optionIndex))
        {
            return false;
        }

        var lifeEvent = state.PendingLifeEvent!;
        state.PendingLifeEvent = null;

        switch (lifeEvent.Type)
        {
            case IncidentType.ComputerFreeze:
                return ResolveComputerFreeze(state, lifeEvent, optionIndex);

            case IncidentType.StreamingBinge:
                return ResolveStreamingBinge(state, lifeEvent, optionIndex);

            case IncidentType.OnlineMatch:
                return ResolveOnlineMatch(state, lifeEvent, optionIndex);

            case IncidentType.PartnerCheckIn:
                return ResolvePartnerCheckIn(state, lifeEvent, optionIndex);

            case IncidentType.CareerPathChoice:
                return ResolveCareerPathChoice(state, lifeEvent, optionIndex);

            case IncidentType.BossCheckIn:
                return ResolveBossCheckIn(state, lifeEvent, optionIndex);

            case IncidentType.CoworkerInterruption:
                return ResolveCoworkerInterruption(state, lifeEvent, optionIndex);

            case IncidentType.FounderNaming:
                return ResolveFounderNaming(state, lifeEvent, optionIndex);

            default:
                return false;
        }
    }

    public int GetCurrentWriteLinesPerClick(RunState state)
    {
        return GetWriteCodeLinesGain(state);
    }

    public double GetCurrentWriteFocusCost(RunState state)
    {
        return GetWriteCodeFocusCost(state);
    }

    public double GetCurrentWriteQualityGain(RunState state)
    {
        return GetWriteCodeQualityGain(state);
    }

    public int GetUpgradeTier(RunState state, EfficiencyUpgradeType type)
    {
        return state.PurchasedUpgrades.TryGetValue(type, out var tier)
            ? tier
            : 0;
    }

    public int GetUpgradeMaxTier()
    {
        return MaxUpgradeTier;
    }

    public double GetUpgradePurchaseCost(RunState state, EfficiencyUpgradeType type)
    {
        var definition = EfficiencyUpgradeCatalog.Get(type);
        var currentTier = GetUpgradeTier(state, type);
        if (currentTier >= MaxUpgradeTier)
        {
            return 0;
        }

        var tierMultiplier = 1d + (currentTier * 0.45d);
        return Math.Round(definition.FundsCost * tierMultiplier, 0);
    }

    public int GetUncommittedCompletedFileCount(RunState state)
    {
        return state.VersionControl.PendingCompletedFileCount;
    }

    public int GetResumeProof(RunState state, ResumeTrack track)
    {
        return track switch
        {
            ResumeTrack.UI => state.UiResumeProof,
            ResumeTrack.Tooling => state.ToolingResumeProof,
            _ => state.GameplayResumeProof,
        };
    }

    public string GetResumeTrackLabel(ResumeTrack track)
    {
        return track switch
        {
            ResumeTrack.UI => "UI",
            ResumeTrack.Tooling => "Tooling",
            _ => "Gameplay",
        };
    }

    public bool HasActiveJobApplication(RunState state)
    {
        return state.ActiveJobApplication is not null;
    }

    public bool HasPublishedApps(RunState state)
    {
        return state.PublishedAppCount > 0;
    }

    public bool IsPortfolioPublishReady(RunState state)
    {
        return CanPublishCurrentApp(state);
    }

    public IReadOnlyList<string> GetVisibleJobApplicationLines(RunState state)
    {
        if (state.ActiveJobApplication is null)
        {
            return Array.Empty<string>();
        }

        return state.ActiveJobApplication.CodeLines
            .Take(state.ActiveJobApplication.VisibleLineCount)
            .ToArray();
    }

    public bool CanWorkOnJobApplication(RunState state)
    {
        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               state.ActiveJobApplication is not null &&
               !state.ActiveJobApplication.TakeHomeComplete &&
               state.ActiveCatInterruption is null &&
               !RequiresSleep(state) &&
               state.Focus > 0;
    }

    public bool WorkOnJobApplication(RunState state)
    {
        if (!CanWorkOnJobApplication(state))
        {
            if (state.ActiveCatInterruption is not null)
            {
                AppendLog(state, "The cat is still on the keyboard. The take-home challenge has to wait.");
            }
            else if (RequiresSleep(state))
            {
                AppendLog(state, "You have been awake for two straight days. Sleep before touching the take-home again.");
            }
            else if (state.Focus <= 0)
            {
                AppendLog(state, "Too exhausted to finish the take-home. Recover focus first.");
            }

            return false;
        }

        var application = state.ActiveJobApplication!;
        var linesAdded = RevealApplicationLines(application, GetWriteCodeLinesGain(state));
        if (linesAdded == 0)
        {
            return false;
        }

        state.Focus = Clamp(state.Focus - GetWriteCodeFocusCost(state), 0, Config.MaxFocus);
        state.CodeQuality = Clamp(state.CodeQuality + (GetWriteCodeQualityGain(state) * 0.5), 0, Config.MaxCodeQuality);
        AppendLog(state, $"Take-home progress: +{linesAdded} challenge LoC for {application.ListingTitle}.");

        if (application.TakeHomeComplete)
        {
            if (!IsSluggish(state) &&
                !IsContextSwitchActive(state) &&
                state.ActiveTechDebtBug is null)
            {
                AwardInterviewPrep(state, 1, "The take-home shipped under clean desk conditions.");
            }

            AppendLog(state, $"Take-home complete for {application.ListingTitle}. The mock interview is ready.");
        }

        EvaluateLossState(state);
        return true;
    }

    public bool CanAnswerInterviewQuestion(RunState state, int optionIndex)
    {
        if (state.Status != RunStatus.InProgress ||
            state.FirstCoinDecisionPending ||
            state.PendingLifeEvent is not null ||
            state.ActiveJobApplication is null ||
            !state.ActiveJobApplication.TakeHomeComplete ||
            RequiresSleep(state) ||
            state.ActiveJobApplication.InterviewComplete)
        {
            return false;
        }

        var question = state.ActiveJobApplication.Questions[state.ActiveJobApplication.CurrentQuestionIndex];
        return optionIndex >= 0 && optionIndex < question.Options.Count;
    }

    public bool AnswerInterviewQuestion(RunState state, int optionIndex)
    {
        if (!CanAnswerInterviewQuestion(state, optionIndex))
        {
            if (RequiresSleep(state))
            {
                AppendLog(state, "No interview answers while the sleep debt is this bad. Sleep first.");
            }

            return false;
        }

        var application = state.ActiveJobApplication!;
        var question = application.Questions[application.CurrentQuestionIndex];
        var correct = optionIndex == question.CorrectOptionIndex;
        if (correct)
        {
            application.CorrectAnswers++;
        }

        application.CurrentQuestionIndex++;
        AppendLog(
            state,
            correct
                ? $"Interview answer landed cleanly for {application.ListingTitle}."
                : $"Interview answer wobbled for {application.ListingTitle}.");

        if (application.InterviewComplete)
        {
            ResolveJobApplicationOutcome(state);
        }

        return true;
    }

    public void AdvanceRealTime(RunState state, double elapsedRealSeconds)
    {
        if (state.Status != RunStatus.InProgress ||
            state.FirstCoinDecisionPending ||
            state.PendingLifeEvent is not null ||
            elapsedRealSeconds <= 0)
        {
            return;
        }

        AdvanceTime(state, elapsedRealSeconds * Config.InGameMinutesPerRealSecond);
    }

    public void QueueIncidents(RunState state, IReadOnlyList<QueuedIncident> incidents)
    {
        if (state.Status != RunStatus.InProgress || incidents.Count == 0)
        {
            return;
        }

        state.QueuedIncidents.AddRange(incidents);

        foreach (var incident in state.QueuedIncidents)
        {
            ActivateIncident(state, incident);
        }

        state.QueuedIncidents.Clear();
        EvaluateLossState(state);
    }

    public void AdvanceTime(RunState state, double elapsedInGameMinutes)
    {
        AdvanceTime(state, elapsedInGameMinutes, isSleeping: false);
    }

    private void AdvanceTime(RunState state, double elapsedInGameMinutes, bool isSleeping)
    {
        if (state.Status != RunStatus.InProgress ||
            state.FirstCoinDecisionPending ||
            state.PendingLifeEvent is not null ||
            elapsedInGameMinutes <= 0)
        {
            return;
        }

        ResolveExpiredIncidents(state);

        var remainingMinutes = elapsedInGameMinutes;

        while (remainingMinutes > BoundaryEpsilon && state.Status == RunStatus.InProgress)
        {
            var hungerStage = GetHungerStage(state);
            var sleepStage = isSleeping ? 0 : GetSleepStage(state);
            var step = GetStepMinutes(state, remainingMinutes, isSleeping);

            if (!isSleeping)
            {
                ApplyPassiveFocusDrain(state, step);
                ApplyModePressure(state, step);
                ApplyAmbientSanityPressure(state, step);
            }

            ApplyPassiveSanityRegeneration(state, step);
            ApplyTechDebtDecay(state, step);
            ApplyNeedPenalties(state, step, hungerStage, sleepStage);
            AdvanceTimedEffects(state, step);
            AdvanceNeedTimers(state, step, isSleeping);
            AdvanceNeedDrivenDebuggingPressure(state, step, hungerStage, sleepStage, isSleeping);

            state.TimeOfDayMinutes += step;
            remainingMinutes -= step;
            AppendNeedThresholdLogs(state, hungerStage, sleepStage, isSleeping);

            if (state.TimeOfDayMinutes >= SimulationConfig.MinutesPerDay - BoundaryEpsilon)
            {
                state.TimeOfDayMinutes = 0;
                state.Day += 1;
                ApplyDailyModeIncome(state);
                var billAmount = GetCurrentDailyBillAmount(state);
                state.Funds -= billAmount;
                AppendLog(state, $"Paid housing + bills: -${billAmount:0}. Funds now ${state.Funds:0}.");

                if (state.Funds < 0)
                {
                    if (state.HasFirstCoin)
                    {
                        state.FirstCoinDecisionPending = true;
                        state.FirstCoinRescueDeficit = Math.Abs(state.Funds);
                        AppendLog(state, "Eviction warning: the first coin could still buy one more chance.");
                    }
                    else
                    {
                        state.Status = RunStatus.Evicted;
                        state.OutcomeMessage = "Rent came due and the account slipped below zero.";
                        AppendLog(state, "Eviction notice posted on the door.");
                    }
                    break;
                }
            }

            ResolveExpiredIncidents(state);
            EvaluateLossState(state);
        }
    }

    public bool PlaceFoodOrder(RunState state, FoodChoice choice, bool doubleCheckOrder, bool expeditedDelivery = false)
    {
        var selectedModifiers = doubleCheckOrder
            ? GetFoodOrderModifiers(choice)
                .Where(static option => option.Recommended)
                .Select(static option => option.Modifier)
                .ToArray()
            : Array.Empty<FoodOrderModifier>();
        return PlaceFoodOrder(state, choice, selectedModifiers, doubleCheckOrder, expeditedDelivery);
    }

    public bool PlaceFoodOrder(
        RunState state,
        FoodChoice choice,
        IReadOnlyCollection<FoodOrderModifier> selectedModifiers,
        bool reviewReceipt,
        bool expeditedDelivery = false)
    {
        expeditedDelivery &= AllowsExpeditedDelivery(choice);
        if (state.ActiveFoodDelivery is not null)
        {
            AppendLog(state, "A delivery order is already on the way. Wait for it to land before ordering again.");
            return false;
        }

        if (!CanPlaceFoodOrder(state, choice, expeditedDelivery))
        {
            AppendLog(state, "Not enough funds to place that delivery order.");
            return false;
        }

        var option = GetFoodOption(state, choice);
        var tipAmount = GetFoodTipAmount(choice, expeditedDelivery);
        var totalCost = GetFoodTotalCost(state, choice, expeditedDelivery);
        var sluggishPenalty = GetFoodOrderPenaltyMinutes(choice, selectedModifiers, reviewReceipt);
        state.Funds -= totalCost;
        state.ActiveFoodDelivery = new ActiveFoodDelivery
        {
            Choice = choice,
            Expedited = expeditedDelivery,
            ReviewReceipt = reviewReceipt,
            TipAmount = tipAmount,
            TotalFundsCost = totalCost,
            RemainingInGameMinutes = GetFoodDeliveryDuration(state, choice, expeditedDelivery),
        };

        foreach (var modifier in selectedModifiers.Where(static modifier => modifier != FoodOrderModifier.None))
        {
            state.ActiveFoodDelivery.SelectedModifiers.Add(modifier);
        }

        var deliveryLine = IsHomeCooked(choice)
            ? $"{option.Name} goes on the stove. Meal ETA {FormatMinutesForLog(state.ActiveFoodDelivery.RemainingInGameMinutes)} before the desk gets the recovery bump."
            : expeditedDelivery
                ? $"{option.Name} ordered with a ${tipAmount:0} expedite tip. ETA {FormatMinutesForLog(state.ActiveFoodDelivery.RemainingInGameMinutes)}."
                : $"{option.Name} ordered. ETA {FormatMinutesForLog(state.ActiveFoodDelivery.RemainingInGameMinutes)} before the food actually hits the desk.";
        AppendLog(state, deliveryLine);

        if (sluggishPenalty <= BoundaryEpsilon)
        {
            AwardInterviewPrep(state, 1, "Careful food planning kept the recruiter loop stable.");
        }

        return true;
    }

    public bool ApplyAction(RunState state, PlayerAction action)
    {
        if (action == PlayerAction.RestartRun)
        {
            state.ResetFrom(CreateNewRun(state.RunSeed));
            return true;
        }

        if (state.Status != RunStatus.InProgress ||
            state.FirstCoinDecisionPending ||
            state.PendingLifeEvent is not null)
        {
            return false;
        }

        switch (action)
        {
            case PlayerAction.WriteCode:
                if (!CanApplyAction(state, action))
                {
                    AppendLog(
                        state,
                        RequiresSleep(state)
                            ? "You have been awake for two days. Sleep before you touch the keyboard again."
                            : state.ActiveCatInterruption is not null
                            ? "The cat is on the keyboard. Pet it before you can type."
                            : "Too exhausted to write. Recover focus first.");
                    return false;
                }

                var currentProgram = PortfolioWorkspace.GetCurrentProgram(state);
                var requestedLinesGain = GetWriteCodeLinesGain(state);
                var qualityGain = GetWriteCodeQualityGain(state);
                var writeResult = PortfolioWorkspace.RevealLines(state, requestedLinesGain);

                if (writeResult.LinesAdded == 0)
                {
                    AppendLog(state, "The portfolio files are fully typed out. Ship what you have.");
                    return false;
                }

                state.LinesOfCode += writeResult.LinesAdded;
                state.CurrentPortfolioLinesOfCode += writeResult.LinesAdded;
                TrackVersionControlWork(state, writeResult.LinesAdded);
                state.Focus = Clamp(state.Focus - GetWriteCodeFocusCost(state), 0, Config.MaxFocus);
                state.CodeQuality = Clamp(state.CodeQuality + qualityGain, 0, Config.MaxCodeQuality);

                var sluggishSuffix = IsSluggish(state) ? " Sluggish food haze is slowing the session." : string.Empty;
                AppendLog(
                    state,
                    $"+{writeResult.LinesAdded} LoC. Focus now {state.Focus:0}. Portfolio quality {state.CodeQuality:0}.{sluggishSuffix}");

                if (!string.IsNullOrEmpty(writeResult.CompletedFileName))
                {
                    state.RecentCompletedFileName = writeResult.CompletedFileName;
                    state.FileCompletionCelebrationMinutesRemaining = Config.FileCompletionCelebrationMinutes;
                    state.VersionControl.PendingCompletedFileCount += 1;
                    AppendLog(state, $"{writeResult.CompletedFileName} is ready for commit.");
                    AwardIndieProjectProgressIncome(state, writeResult.CompletedFileName);
                    AwardResumeProof(
                        state,
                        GetResumeTrackForProgram(currentProgram),
                        1,
                        $"{writeResult.CompletedFileName} gave the portfolio stronger {GetResumeTrackLabel(GetResumeTrackForProgram(currentProgram)).ToLowerInvariant()} proof.");
                }

                if (!string.IsNullOrEmpty(writeResult.StartedFileName))
                {
                    AppendLog(state, $"Started {writeResult.StartedFileName}.");
                }

                RefreshKnownContactsFromProgress(state);
                EvaluateLossState(state);
                return true;

            case PlayerAction.Eat:
                return PlaceFoodOrder(state, FoodChoice.Burger, doubleCheckOrder: true);

            case PlayerAction.Freelance:
                return BeginFreelanceGig(state, FreelanceGigType.QuickBugfix);

            case PlayerAction.Sleep:
                var previousSleepStage = GetSleepStage(state);
                state.MinutesSinceLastSleep = 0;
                AdvanceTime(state, Config.SleepDurationMinutes, isSleeping: true);
                if (state.Status != RunStatus.InProgress)
                {
                    return false;
                }

                state.Focus = Clamp(state.Focus + (Config.SleepFocusGain * GetModeFocusRecoveryMultiplier(state)), 0, Config.MaxFocus);
                state.Sanity = Clamp(state.Sanity + Config.SleepSanityGain, 0, Config.MaxSanity);
                AppendLog(
                    state,
                    previousSleepStage >= 2
                        ? $"Slept for 8 hours: focus refilled to full, sanity +{Config.SleepSanityGain:0}. The sleep debt finally broke."
                        : $"Slept for 8 hours: focus refilled to full, sanity +{Config.SleepSanityGain:0}.");
                AwardInterviewPrep(state, 1, "Resting before the interview made the next answers steadier.");
                EvaluateLossState(state);
                return true;

            case PlayerAction.PetCat:
                if (!CanApplyAction(state, action))
                {
                    return false;
                }

                state.ActiveCatInterruption!.PatsRemaining -= 1;
                ClearDistractionIfResolved(state);

                return true;

            case PlayerAction.SquashBug:
                if (!CanApplyAction(state, action))
                {
                    AppendLog(
                        state,
                        RequiresSleep(state)
                            ? "You are too sleep-deprived to debug safely. Sleep first."
                            : "You need a live bug and enough focus to squash it.");
                    return false;
                }

                state.Focus = Clamp(state.Focus - GetSquashBugFocusCost(state), 0, Config.MaxFocus);
                state.CodeQuality = Clamp(state.CodeQuality + 4, 0, Config.MaxCodeQuality);
                state.ActiveTechDebtBug = null;
                AppendLog(state, "Bug squashed. Code quality stabilizes and the panic subsides.");
                AwardResumeProof(state, ResumeTrack.Tooling, 1, "Squashing a live bug added a real debugging story to the resume.");
                AwardInterviewPrep(state, 1, "Cleaning up a live bug sharpened the interview story.");
                EvaluateLossState(state);
                return true;

            case PlayerAction.ApplyForJob:
                if (!CanApplyAction(state, action))
                {
                    AppendLog(
                        state,
                        RequiresSleep(state)
                            ? "No recruiter flow while you are this sleep-deprived. Sleep first, then apply."
                            : "You need an active listing, enough LoC, the listed quality bar, and matching resume proof for that stack.");
                    return false;
                }

                BeginJobApplication(state);
                return true;

            case PlayerAction.PublishApp:
                if (!CanApplyAction(state, action))
                {
                    AppendLog(
                        state,
                        RequiresSleep(state)
                            ? "Do not ship code on forty-eight hours awake. Sleep first."
                            : state.VersionControl.PendingChangeLines > 0
                            ? "Commit the current code changes before you publish. Uncommitted work does not count as shippable."
                            : "Finish the current portfolio batch before you publish.");
                    return false;
                }

                PublishCurrentApp(state);
                EvaluateLossState(state);
                return true;

            default:
                return false;
        }
    }

    private bool ResolveComputerFreeze(RunState state, PendingLifeEvent lifeEvent, int optionIndex)
    {
        switch (optionIndex)
        {
            case 0:
                AdvanceTime(state, Config.ComputerFreezeSelfRepairDurationMinutes);
                if (state.Status != RunStatus.InProgress)
                {
                    return false;
                }

                state.Sanity = Clamp(state.Sanity - Config.ComputerFreezeSelfRepairSanityLoss, 0, Config.MaxSanity);
                state.Focus = Clamp(state.Focus - Config.ComputerFreezeSelfRepairFocusLoss, 0, Config.MaxFocus);
                AppendLog(
                    state,
                    $"You repaired the freeze yourself. Lost {FormatMinutesForLog(Config.ComputerFreezeSelfRepairDurationMinutes)}, -{Config.ComputerFreezeSelfRepairSanityLoss:0} sanity, and -{Config.ComputerFreezeSelfRepairFocusLoss:0} focus.");
                break;

            case 1:
                state.Funds -= Config.ComputerFreezeTechSupportFundsCost;
                AdvanceTime(state, Config.ComputerFreezeTechSupportDurationMinutes);
                if (state.Status != RunStatus.InProgress)
                {
                    return false;
                }

                state.Sanity = Clamp(state.Sanity - Config.ComputerFreezeTechSupportSanityLoss, 0, Config.MaxSanity);
                AppendLog(
                    state,
                    $"Tech support finally got you unstuck. -${Config.ComputerFreezeTechSupportFundsCost:0}, {FormatMinutesForLog(Config.ComputerFreezeTechSupportDurationMinutes)} lost, sanity -{Config.ComputerFreezeTechSupportSanityLoss:0}.");
                break;

            case 2:
                state.Funds -= Config.ComputerFreezeRepairShopFundsCost;
                AdvanceTime(state, Config.ComputerFreezeRepairShopDurationMinutes);
                if (state.Status != RunStatus.InProgress)
                {
                    return false;
                }

                state.Sanity = Clamp(state.Sanity - Config.ComputerFreezeRepairShopSanityLoss, 0, Config.MaxSanity);
                AppendLog(
                    state,
                    $"The repair shop revived the machine. -${Config.ComputerFreezeRepairShopFundsCost:0}, {FormatMinutesForLog(Config.ComputerFreezeRepairShopDurationMinutes)} gone, sanity -{Config.ComputerFreezeRepairShopSanityLoss:0}.");
                break;

            default:
                return false;
        }

        EvaluateLossState(state);
        return true;
    }

    private bool ResolveStreamingBinge(RunState state, PendingLifeEvent lifeEvent, int optionIndex)
    {
        var showName = lifeEvent.SubjectName ?? "something you have seen before";
        switch (optionIndex)
        {
            case 0:
                AdvanceTime(state, Config.StreamingBingeDurationMinutes);
                if (state.Status != RunStatus.InProgress)
                {
                    return false;
                }

                state.Sanity = Clamp(state.Sanity + Config.StreamingBingeSanityGain, 0, Config.MaxSanity);
                state.Focus = Clamp(state.Focus - Config.StreamingBingeFocusLoss, 0, Config.MaxFocus);
                AppendLog(
                    state,
                    $"You binged {showName}. Lost {FormatMinutesForLog(Config.StreamingBingeDurationMinutes)}, sanity +{Config.StreamingBingeSanityGain:0}, focus -{Config.StreamingBingeFocusLoss:0}.");
                break;

            case 1:
                AdvanceTime(state, Config.StreamingEpisodeDurationMinutes);
                if (state.Status != RunStatus.InProgress)
                {
                    return false;
                }

                state.Sanity = Clamp(state.Sanity + Config.StreamingEpisodeSanityGain, 0, Config.MaxSanity);
                state.Focus = Clamp(state.Focus - Config.StreamingEpisodeFocusLoss, 0, Config.MaxFocus);
                AppendLog(
                    state,
                    $"You watched one episode of {showName} and cut it off there. {FormatMinutesForLog(Config.StreamingEpisodeDurationMinutes)} passed, sanity +{Config.StreamingEpisodeSanityGain:0}, focus -{Config.StreamingEpisodeFocusLoss:0}.");
                break;

            case 2:
                state.Sanity = Clamp(state.Sanity - Config.StreamingTurnOffSanityLoss, 0, Config.MaxSanity);
                AppendLog(state, $"You killed autoplay before {showName} ate the night. Sanity -{Config.StreamingTurnOffSanityLoss:0} from the denial.");
                break;

            default:
                return false;
        }

        EvaluateLossState(state);
        return true;
    }

    private bool ResolveOnlineMatch(RunState state, PendingLifeEvent lifeEvent, int optionIndex)
    {
        EnsureRelationshipCandidate(state);
        var matchName = lifeEvent.SubjectName ?? state.RelationshipCandidateName ?? "someone unexpectedly steady";
        var stageIndex = Math.Clamp(lifeEvent.StageIndex, 0, 2);
        var stageDuration = stageIndex == 2
            ? Math.Max(10, Config.OnlineMatchMessageDurationMinutes * 0.75)
            : Math.Max(8, Config.OnlineMatchMessageDurationMinutes * 0.5);
        AdvanceTime(state, stageDuration);
        if (state.Status != RunStatus.InProgress)
        {
            return false;
        }

        var targetScore = lifeEvent.TargetScore <= 0
            ? (state.IsRealisticMode ? 3 : 2)
            : lifeEvent.TargetScore;
        var updatedScore = lifeEvent.ProgressScore + (optionIndex == GetOnlineMatchIdealOption(lifeEvent) ? 1 : 0);
        var successSanityGain = stageIndex == 2 ? 2 : 1;
        var failureSanityShift = stageIndex == 2 ? -2 : -1;
        var focusLoss = Math.Max(1, Config.OnlineMatchMessageFocusLoss * 0.5);

        state.Focus = Clamp(state.Focus - focusLoss, 0, Config.MaxFocus);
        state.Sanity = Clamp(
            state.Sanity + (optionIndex == GetOnlineMatchIdealOption(lifeEvent) ? successSanityGain : failureSanityShift),
            0,
            Config.MaxSanity);

        if (stageIndex < 2)
        {
            AppendLog(
                state,
                optionIndex == GetOnlineMatchIdealOption(lifeEvent)
                    ? $"{matchName} actually responds to the version of you that showed up. The thread feels warmer."
                    : $"{matchName} answers, but the vibe slips a little. The conversation is still alive if you recover it.");

            state.PendingLifeEvent = CreateOnlineMatchMiniGame(state, matchName, lifeEvent.SubjectScore, stageIndex + 1, updatedScore, targetScore);
            EvaluateLossState(state);
            return true;
        }

        if (updatedScore >= targetScore)
        {
            if (state.Funds >= Config.OnlineDateFundsCost)
            {
                state.Funds -= Config.OnlineDateFundsCost;
                state.Sanity = Clamp(state.Sanity + Config.OnlineDateSanityGain, 0, Config.MaxSanity);
                state.Focus = Clamp(state.Focus - Math.Max(1, Config.OnlineDateFocusLoss - 1), 0, Config.MaxFocus);
                state.RelationshipProgress += Config.OnlineDateRelationshipGain;
                AppendLog(
                    state,
                    $"{matchName} says yes to something real. -${Config.OnlineDateFundsCost:0}, sanity +{Config.OnlineDateSanityGain:0}, focus -{Math.Max(1, Config.OnlineDateFocusLoss - 1):0}.");
            }
            else
            {
                state.Sanity = Clamp(state.Sanity + Config.OnlineMatchMessageSanityGain, 0, Config.MaxSanity);
                state.RelationshipProgress += Config.OnlineMatchMessageRelationshipGain + 1;
                AppendLog(state, $"{matchName} is still in. Money is too tight for a full night out, but the thread survives and turns into something real anyway.");
            }
        }
        else
        {
            state.Sanity = Clamp(state.Sanity - Config.OnlineMatchIgnoreSanityLoss, 0, Config.MaxSanity);
            state.RelationshipCandidateName = null;
            state.RelationshipCandidateCompatibility = 0;
            AppendLog(state, $"The conversation with {matchName} fizzles before it becomes anything stable. Sanity -{Config.OnlineMatchIgnoreSanityLoss:0}.");
        }

        if (!state.HasFoundLove &&
            state.RelationshipProgress >= Config.RelationshipProgressNeededForLove)
        {
            state.HasFoundLove = true;
            state.PartnerName = matchName;
            state.RelationshipCandidateName = matchName;
            state.Sanity = Clamp(state.Sanity + 8, 0, Config.MaxSanity);
            AppendLog(state, $"Somewhere between the backlog and the late-night messages, you found something real with {matchName}. Passive sanity support is now part of the run.");
        }

        SynchronizeRelationshipContact(state);
        EvaluateLossState(state);
        return true;
    }

    private bool ResolvePartnerCheckIn(RunState state, PendingLifeEvent lifeEvent, int optionIndex)
    {
        var partnerName = lifeEvent.SubjectName ?? state.PartnerName ?? "someone steady";
        switch (optionIndex)
        {
            case 0:
                AdvanceTime(state, Config.PartnerCheckInReplyDurationMinutes);
                if (state.Status != RunStatus.InProgress)
                {
                    return false;
                }

                state.Sanity = Clamp(state.Sanity + Config.PartnerCheckInReplySanityGain, 0, Config.MaxSanity);
                state.Focus = Clamp(state.Focus - Config.PartnerCheckInReplyFocusLoss, 0, Config.MaxFocus);
                state.RelationshipProgress += Config.PartnerCheckInReplyRelationshipGain;
                AppendLog(
                    state,
                    $"You sent {partnerName} a real update instead of disappearing into the backlog. {FormatMinutesForLog(Config.PartnerCheckInReplyDurationMinutes)} passed, sanity +{Config.PartnerCheckInReplySanityGain:0}, focus -{Config.PartnerCheckInReplyFocusLoss:0}.");
                break;

            case 1:
                state.Funds -= Config.PartnerCheckInDinnerFundsCost;
                AdvanceTime(state, Config.PartnerCheckInDinnerDurationMinutes);
                if (state.Status != RunStatus.InProgress)
                {
                    return false;
                }

                state.Sanity = Clamp(state.Sanity + Config.PartnerCheckInDinnerSanityGain, 0, Config.MaxSanity);
                state.Focus = Clamp(state.Focus - Config.PartnerCheckInDinnerFocusLoss, 0, Config.MaxFocus);
                state.RelationshipProgress += Config.PartnerCheckInDinnerRelationshipGain;
                AppendLog(
                    state,
                    $"You made room for {partnerName}. -${Config.PartnerCheckInDinnerFundsCost:0}, {FormatMinutesForLog(Config.PartnerCheckInDinnerDurationMinutes)} gone, sanity +{Config.PartnerCheckInDinnerSanityGain:0}, focus -{Config.PartnerCheckInDinnerFocusLoss:0}.");
                break;

            case 2:
                state.Focus = Clamp(state.Focus + Config.PartnerCheckInHeadsDownFocusGain, 0, Config.MaxFocus);
                state.Sanity = Clamp(state.Sanity - Config.PartnerCheckInHeadsDownSanityLoss, 0, Config.MaxSanity);
                AppendLog(
                    state,
                    $"You stayed heads-down and kept the sprint intact. Focus +{Config.PartnerCheckInHeadsDownFocusGain:0}, sanity -{Config.PartnerCheckInHeadsDownSanityLoss:0}.");
                break;

            default:
                return false;
        }

        SynchronizeRelationshipContact(state);
        EvaluateLossState(state);
        return true;
    }

    private bool ResolveCareerPathChoice(RunState state, PendingLifeEvent lifeEvent, int optionIndex)
    {
        var listingTitle = lifeEvent.SubjectName ?? "the offer";
        var offerMode = lifeEvent.SubjectScore == (int)GameplayLoopMode.Indie
            ? GameplayLoopMode.Indie
            : GameplayLoopMode.Corporate;
        switch (optionIndex)
        {
            case 0:
                state.GameplayMode = offerMode;
                if (offerMode == GameplayLoopMode.Indie)
                {
                    state.Funds += 30;
                    state.Sanity = Clamp(state.Sanity + 6, 0, Config.MaxSanity);
                    AppendLog(state, $"You take the indie studio offer from {listingTitle}. Indie Mode is live with looser structure, self-directed pressure, and income that still depends on what you ship.");
                }
                else
                {
                    state.BossDisposition = ProceduralRunContent.GetBossDisposition(state.RunSeed, GameplayLoopMode.Corporate);
                    state.Funds += 70;
                    state.Sanity = Clamp(state.Sanity + 4, 0, Config.MaxSanity);
                    AppendLog(state, $"You take the corporate offer from {listingTitle}. Corporate Mode is live with daily salary, stricter oversight, and {state.BossName}, {state.BossTitle}, policing how survivable the job actually feels.");
                }
                return true;

            case 1:
                state.GameplayMode = GameplayLoopMode.Founder;
                state.Funds += 18;
                state.Sanity = Clamp(state.Sanity + 3, 0, Config.MaxSanity);
                state.PendingLifeEvent = CreateFounderNamingEvent(state);
                AppendLog(state, $"You turn the momentum from {listingTitle} into a founder bet instead of taking the offer. Funds +$18, sanity +3, and the studio now needs a name.");
                return true;

            case 2:
                state.Status = RunStatus.Won;
                state.OutcomeMessage = $"You cleared the take-home, survived the interview, and landed {listingTitle}.";
                AppendLog(state, $"You locked in {listingTitle} and let the run end on the offer.");
                return true;

            default:
                return false;
        }
    }

    private bool ResolveFounderNaming(RunState state, PendingLifeEvent lifeEvent, int optionIndex)
    {
        if (optionIndex < 0 || optionIndex >= lifeEvent.OptionLabels.Length)
        {
            return false;
        }

        state.StudioName = lifeEvent.OptionLabels[optionIndex];
        state.Sanity = Clamp(state.Sanity + 2, 0, Config.MaxSanity);
        AppendLog(state, $"{state.StudioName} is real now. Founder Mode starts with a name, a bill timer, and a studio nobody else is going to save for you.");
        return true;
    }

    private bool ResolveBossCheckIn(RunState state, PendingLifeEvent lifeEvent, int optionIndex)
    {
        var bossName = lifeEvent.SubjectName ?? state.BossName;
        var duration = optionIndex switch
        {
            0 => 24d,
            1 => 18d,
            _ => 42d,
        };

        AdvanceTime(state, duration);
        if (state.Status != RunStatus.InProgress)
        {
            return false;
        }

        var focusLoss = optionIndex switch
        {
            0 => 3d,
            1 => 2d,
            _ => 7d,
        };
        state.Focus = Clamp(state.Focus - focusLoss, 0, Config.MaxFocus);

        if (optionIndex == GetBossCheckInIdealOption(state.BossDisposition))
        {
            var standingGain = state.BossDisposition == BossDisposition.Micromanager ? 1 : 2;
            var fundsGain = state.BossDisposition switch
            {
                BossDisposition.Supportive => 16,
                BossDisposition.Nice => 14,
                BossDisposition.Mean => 18,
                _ => 20,
            };
            var sanityGain = state.BossDisposition switch
            {
                BossDisposition.Supportive => 3,
                BossDisposition.Nice => 2,
                _ => 0,
            };

            state.CorporateStanding += standingGain;
            state.Funds += fundsGain;
            state.Sanity = Clamp(state.Sanity + sanityGain, 0, Config.MaxSanity);
            if (state.BossDisposition == BossDisposition.Supportive && optionIndex == 1)
            {
                state.DeepWorkMinutesRemaining = Math.Max(state.DeepWorkMinutesRemaining, 45);
            }

            AppendLog(
                state,
                $"{bossName} leaves the check-in satisfied. Corporate standing +{standingGain}, funds +${fundsGain:0}, sanity +{sanityGain:0}, focus -{focusLoss:0}.");
            EvaluateLossState(state);
            return true;
        }

        var sanityLoss = state.BossDisposition switch
        {
            BossDisposition.Mean => 4d,
            BossDisposition.Micromanager => 5d,
            _ => 3d,
        };
        state.Sanity = Clamp(state.Sanity - sanityLoss, 0, Config.MaxSanity);
        state.CorporateStanding = Math.Max(0, state.CorporateStanding - 1);
        state.ContextSwitchMinutesRemaining = Math.Max(state.ContextSwitchMinutesRemaining, 60);

        AppendLog(
            state,
            $"{bossName} turns the sync into drag. Focus -{focusLoss:0}, sanity -{sanityLoss:0}, corporate standing slips, and context switching is back on the desk.");
        EvaluateLossState(state);
        return true;
    }

    private bool ResolveCoworkerInterruption(RunState state, PendingLifeEvent lifeEvent, int optionIndex)
    {
        var coworkerName = lifeEvent.SubjectName ?? "a coworker";
        var duration = optionIndex switch
        {
            0 => 34d,
            1 => 12d,
            _ => 48d,
        };

        AdvanceTime(state, duration);
        if (state.Status != RunStatus.InProgress)
        {
            return false;
        }

        switch (optionIndex)
        {
            case 0:
                state.Focus = Clamp(state.Focus - 2, 0, Config.MaxFocus);
                state.Sanity = Clamp(state.Sanity - 1, 0, Config.MaxSanity);
                state.CodeQuality = Clamp(state.CodeQuality + 2, 0, Config.MaxCodeQuality);
                state.CorporateStanding += 1;
                AppendLog(state, $"You pair with {coworkerName} and keep the team moving. Focus -2, sanity -1, quality +2, standing +1.");
                break;

            case 1:
                state.Focus = Clamp(state.Focus - 1, 0, Config.MaxFocus);
                state.Sanity = Clamp(state.Sanity - 2, 0, Config.MaxSanity);
                state.ContextSwitchMinutesRemaining = Math.Max(state.ContextSwitchMinutesRemaining, 30);
                AppendLog(state, $"You deflect {coworkerName} and claw back the block, but the interruption still lingers. Focus -1, sanity -2.");
                break;

            case 2:
                state.Focus = Clamp(state.Focus - 4, 0, Config.MaxFocus);
                state.Sanity = Clamp(state.Sanity - 4, 0, Config.MaxSanity);
                state.Funds += 12;
                state.CorporateStanding += 1;
                AppendLog(state, $"You absorb {coworkerName}'s fire drill and keep the office happy. Focus -4, sanity -4, funds +$12, standing +1.");
                break;

            default:
                return false;
        }

        EvaluateLossState(state);
        return true;
    }

    private void ResolveFoodDelivery(RunState state)
    {
        if (state.ActiveFoodDelivery is null ||
            state.ActiveFoodDelivery.RemainingInGameMinutes > BoundaryEpsilon)
        {
            return;
        }

        var delivery = state.ActiveFoodDelivery;
        state.ActiveFoodDelivery = null;

        var option = GetFoodOption(state, delivery.Choice);
        var previousHungerStage = GetHungerStage(state);
        state.Focus = Clamp(state.Focus + (option.FocusGain * GetModeFocusRecoveryMultiplier(state)), 0, Config.MaxFocus);
        state.Sanity = Clamp(state.Sanity + option.SanityGain, 0, Config.MaxSanity);
        state.MinutesSinceLastMeal = 0;

        var sluggishPenalty = GetFoodOrderPenaltyMinutes(delivery.Choice, delivery.SelectedModifiers, delivery.ReviewReceipt);
        var hungerSuffix = previousHungerStage > 0
            ? " Hunger finally stops chewing through the day."
            : string.Empty;
        var arrivalLead = IsHomeCooked(delivery.Choice)
            ? $"{option.Name} is finally ready and plated."
            : delivery.Expedited
                ? $"{option.Name} arrives fast thanks to the ${delivery.TipAmount:0} expedite tip."
                : $"{option.Name} finally lands on the desk.";

        if (sluggishPenalty <= BoundaryEpsilon)
        {
            AppendLog(state, $"{arrivalLead} The careful order kept your momentum intact.{hungerSuffix}");
            return;
        }

        state.SluggishMinutesRemaining = Math.Max(state.SluggishMinutesRemaining, sluggishPenalty);
        if (sluggishPenalty < option.SluggishMinutesWhenUnchecked)
        {
            AppendLog(state, $"{arrivalLead} There is still a small food haze for {sluggishPenalty:0} in-game minutes.{hungerSuffix}");
            return;
        }

        AppendLog(state, $"{arrivalLead} It landed messy and left you sluggish for a while.{hungerSuffix}");
    }

    public void EvaluateLossState(RunState state)
    {
        if (state.Status != RunStatus.InProgress)
        {
            return;
        }

        if (state.GameplayMode == GameplayLoopMode.Interview &&
            state.Day > Config.InterviewDeadlineDays)
        {
            state.Status = RunStatus.BurnedOut;
            state.OutcomeMessage = "The seven-day interview sprint ended before you could turn the week into an offer.";
            AppendLog(state, "Interview Mode times out after seven days. The sprint closes before the route can branch.");
            return;
        }

        if (state.Sanity <= 0)
        {
            state.Status = RunStatus.BurnedOut;
            state.OutcomeMessage = "There is nothing left in the tank after too many compromises.";
            AppendLog(state, "Burnout hit hard. The screen finally went dark.");
        }
    }

    private void BeginJobApplication(RunState state)
    {
        var listing = state.ActiveJobListing!;
        state.LinesOfCode -= listing.ResumeCostLines;
        state.ActiveJobListing = null;
        state.ActiveJobApplication = CreateJobApplication(listing, state);

        AppendLog(
            state,
            $"Tailored the resume for {listing.Title} at {listing.CompanyName}. The take-home task and mock interview are live.");
        RefreshKnownContactsFromProgress(state);
    }

    private void ResolveJobApplicationOutcome(RunState state)
    {
        var application = state.ActiveJobApplication!;
        var hasInterview = application.CorrectAnswers >= application.MinimumCorrectAnswers;
        state.ActiveJobApplication = null;

        if (hasInterview)
        {
            state.SuccessfulApplications += 1;
            RefreshKnownContactsFromProgress(state);

            if (state.GameplayMode == GameplayLoopMode.Interview &&
                !ShouldContinueAfterSuccessfulApplication(state))
            {
                var companyRouteLabel = application.OfferMode == GameplayLoopMode.Indie
                    ? "an indie studio"
                    : "a corporate team";
                state.PendingLifeEvent = new PendingLifeEvent
                {
                    Type = IncidentType.CareerPathChoice,
                    Title = "Offer Landed",
                    Description = $"You landed {application.ListingTitle} at {application.CompanyName}, which is {companyRouteLabel}. Take the actual offer, start your own studio instead, or end the seven-day sprint cleanly on the win.",
                    SubjectName = application.CompanyName,
                    SubjectScore = (int)application.OfferMode,
                    OptionLabels =
                    [
                        application.OfferMode == GameplayLoopMode.Indie ? "Join Indie Studio" : "Take Corporate Job",
                        "Start Founder Mode",
                        "End Run Here",
                    ],
                };
                AppendLog(
                    state,
                    $"Offer landed: {application.ListingTitle} at {application.CompanyName}. Interview Mode now branches into the actual company route or a founder gamble.");
                return;
            }

            if (ShouldContinueAfterSuccessfulApplication(state))
            {
                var fundsReward = GetSuccessfulApplicationFundsReward(state);
                var sanityReward = GetSuccessfulApplicationSanityReward(state);
                state.Funds += fundsReward;
                state.Sanity = Clamp(state.Sanity + sanityReward, 0, Config.MaxSanity);
                AppendLog(
                    state,
                    $"Offer landed: {application.ListingTitle} at {application.CompanyName}. {GetContinuationModeLabel(state)} keeps rolling with +${fundsReward:0} and +{sanityReward:0} sanity.");
                return;
            }

            state.Status = RunStatus.Won;
            state.OutcomeMessage = $"You cleared the take-home, survived the interview, and landed {application.ListingTitle} at {application.CompanyName}.";
            AppendLog(
                state,
                $"Application accepted after the interview: {application.ListingTitle} at {application.CompanyName}. Every interview answer landed.");
            return;
        }

        state.OutcomeMessage = null;
        AppendLog(
            state,
            $"Application rejected after the interview loop. {application.ListingTitle} required every interview answer to land cleanly.");
    }

    private ActiveJobApplication CreateJobApplication(ActiveJobListing listing, RunState state)
    {
        var resumeProof = GetResumeProof(state, listing.ResumeTrack);
        var excessResumeProof = Math.Max(0, resumeProof - listing.RequiredResumeProof);
        var application = new ActiveJobApplication
        {
            ListingTitle = listing.Title,
            TechStack = listing.TechStack,
            CompanyName = listing.CompanyName,
            OfferMode = listing.OfferMode,
            ResumeLinesSpent = listing.ResumeCostLines,
            PortfolioLinesSnapshot = state.LinesOfCode + listing.ResumeCostLines,
            CodeQualitySnapshot = state.CodeQuality,
            MinimumPortfolioLines = listing.MinimumPortfolioLines,
            MinimumCodeQuality = listing.MinimumCodeQuality,
            ResumeTrack = listing.ResumeTrack,
            ResumeProofSnapshot = resumeProof,
            PrepPoints = GetUpgradeBonusTotal(state, definition => definition.PrepPointsOnApplicationStart) + Math.Min(2, excessResumeProof),
        };

        var (challengeTitle, challengeDescription, codeLines) = JobApplicationContentCatalog.CreateChallenge(listing, state.RunSeed);
        application.ChallengeTitle = challengeTitle;
        application.ChallengeDescription = challengeDescription;
        application.CodeLines.AddRange(codeLines);
        application.Questions.AddRange(JobApplicationContentCatalog.CreateInterviewQuestions(listing, state.RunSeed));
        application.MinimumCorrectAnswers = application.Questions.Count;
        RevealApplicationLines(application, application.PrepPoints);
        return application;
    }

    private void ActivateIncident(RunState state, QueuedIncident incident)
    {
        switch (incident.Type)
        {
            case IncidentType.CatInterruption:
                if (state.ActiveCatInterruption is null)
                {
                    state.ActiveCatInterruption = CreateDeskDistraction(state, incident.Id);
                    AppendLog(state, incident.Description);
                    AppendLog(state, $"{state.ActiveCatInterruption.Title} is live. Clear it manually, spend focus to stabilize it, or pay for a fast cleanup before it chews up the draft.");
                }
                break;

            case IncidentType.TechDebtBug:
                ActivateDeskTechDebtBug(state, incident);
                break;

            case IncidentType.JobListing:
                if (state.ActiveJobListing is null && state.ActiveJobApplication is null)
                {
                    state.ActiveJobListing = CreateJobListing(state, incident.Id);
                    AppendLog(state, incident.Description);
                }
                break;

            case IncidentType.DeepWorkWindow:
                state.DeepWorkMinutesRemaining = Math.Max(state.DeepWorkMinutesRemaining, Config.DeepWorkDurationMinutes);
                AppendLog(state, incident.Description);
                break;

            case IncidentType.ContextSwitch:
                state.ContextSwitchMinutesRemaining = Math.Max(state.ContextSwitchMinutesRemaining, Config.ContextSwitchDurationMinutes);
                AppendLog(state, incident.Description);
                break;

            case IncidentType.CoffeeBounce:
                state.Focus = Clamp(state.Focus + Config.CoffeeBounceFocusGain, 0, Config.MaxFocus);
                state.Sanity = Clamp(state.Sanity + Config.CoffeeBounceSanityGain, 0, Config.MaxSanity);
                AppendLog(
                    state,
                    $"{incident.Description} Focus +{Config.CoffeeBounceFocusGain:0}, sanity +{Config.CoffeeBounceSanityGain:0}.");
                break;

            case IncidentType.MentorNudge:
                state.CodeQuality = Clamp(state.CodeQuality + Config.MentorNudgeQualityGain, 0, Config.MaxCodeQuality);
                AppendLog(state, $"{incident.Description} Code quality +{Config.MentorNudgeQualityGain:0}.");
                AwardResumeProof(state, GetMentorResumeTrack(state), 1, "A timely mentor note sharpened how you present the work.");
                break;

            case IncidentType.ExpenseSpike:
                state.Funds -= Config.ExpenseSpikeFundsLoss;
                AppendLog(state, $"{incident.Description} Funds -${Config.ExpenseSpikeFundsLoss:0}.");
                break;

            case IncidentType.RubberDuckInsight:
                state.CodeQuality = Clamp(state.CodeQuality + Config.RubberDuckInsightQualityGain, 0, Config.MaxCodeQuality);
                AppendLog(state, $"{incident.Description} Code quality +{Config.RubberDuckInsightQualityGain:0}.");
                AwardInterviewPrep(state, Config.RubberDuckInsightPrepGain, "Rubber-ducking the problem sharpened the next interview answer.");
                break;

            case IncidentType.MicroSale:
                state.Funds += Config.MicroSaleFundsGain;
                state.Sanity = Clamp(state.Sanity + Config.MicroSaleSanityGain, 0, Config.MaxSanity);
                AppendLog(
                    state,
                    $"{incident.Description} Funds +${Config.MicroSaleFundsGain:0}, sanity +{Config.MicroSaleSanityGain:0}.");
                break;

            case IncidentType.PublishedAppSale:
                var saleNumber = ParseTrailingNumber(incident.Id, "app-sale-");
                var saleFunds = ApplyRouteIncomeModifier(
                    state,
                    RollBoundedAmount(
                    state.RunSeed,
                    $"published-sale:{saleNumber}",
                    Config.PublishedAppSaleFundsMin,
                    Config.PublishedAppSaleFundsMax),
                    incomeType: "sale");
                state.Funds += saleFunds;
                AppendLog(state, $"{incident.Description} Funds +${saleFunds:0}.");
                break;

            case IncidentType.DoomscrollSpiral:
                state.Focus = Clamp(state.Focus - Config.DoomscrollFocusLoss, 0, Config.MaxFocus);
                state.Sanity = Clamp(state.Sanity - Config.DoomscrollSanityLoss, 0, Config.MaxSanity);
                AppendLog(
                    state,
                    $"{incident.Description} Focus -{Config.DoomscrollFocusLoss:0}, sanity -{Config.DoomscrollSanityLoss:0}.");
                break;

            case IncidentType.ComputerFreeze:
                if (state.PendingLifeEvent is null)
                {
                    var lostLines = LoseUncommittedProgress(state);
                    state.PendingLifeEvent = new PendingLifeEvent
                    {
                        Type = incident.Type,
                        Title = "Computer Freeze",
                        Description = "The cursor is dead, the fans are screaming, and the whole night is now about getting the machine back.",
                    };
                    AppendLog(state, incident.Description);
                    if (lostLines > 0)
                    {
                        AppendLog(state, $"The freeze hits before a commit lands. {lostLines} uncommitted LoC vanish and the editor rolls back to the last commit.");
                    }
                }
                break;

            case IncidentType.StreamingBinge:
                if (state.PendingLifeEvent is null)
                {
                    var showTitle = ProceduralRunContent.GetShowTitle(state.RunSeed, $"{incident.Id}:show");
                    state.PendingLifeEvent = new PendingLifeEvent
                    {
                        Type = incident.Type,
                        Title = "Autoplay Temptation",
                        Description = $"{showTitle} is already lined up and the next episode starts in seconds.",
                        SubjectName = showTitle,
                    };
                    AppendLog(state, incident.Description);
                }
                break;

            case IncidentType.OnlineMatch:
                if (state.PendingLifeEvent is null && !state.HasFoundLove)
                {
                    EnsureRelationshipCandidate(state);
                    var matchName = state.RelationshipCandidateName!;
                    state.PendingLifeEvent = CreateOnlineMatchMiniGame(
                        state,
                        matchName,
                        state.RelationshipCandidateCompatibility,
                        0,
                        0,
                        state.IsRealisticMode ? 3 : 2);
                    AppendLog(state, incident.Description);
                }
                break;

            case IncidentType.PartnerCheckIn:
                if (state.PendingLifeEvent is null && state.HasFoundLove && !string.IsNullOrWhiteSpace(state.PartnerName))
                {
                    state.PendingLifeEvent = new PendingLifeEvent
                    {
                        Type = incident.Type,
                        Title = "Relationship Check-In",
                        Description = ProceduralRunContent.GetPartnerPrompt(state.RunSeed, state.PartnerName!, state.RelationshipProgress),
                        SubjectName = state.PartnerName,
                        SubjectScore = state.RelationshipProgress,
                    };
                    AppendLog(state, incident.Description);
                }
                break;

            case IncidentType.BossCheckIn:
                if (state.PendingLifeEvent is null && state.GameplayMode == GameplayLoopMode.Corporate)
                {
                    state.PendingLifeEvent = new PendingLifeEvent
                    {
                        Type = incident.Type,
                        Title = "Boss Check-In",
                        Description = $"{state.BossName}, {state.BossTitle}, wants a status read before the work is actually done. {ProceduralRunContent.GetBossFlavor(state.BossDisposition, state.BossName)}",
                        SubjectName = state.BossName,
                        SubjectScore = (int)state.BossDisposition,
                    };
                    AppendLog(state, incident.Description);
                }
                break;

            case IncidentType.CoworkerInterruption:
                if (state.PendingLifeEvent is null && state.GameplayMode == GameplayLoopMode.Corporate)
                {
                    var coworkerName = ProceduralRunContent.GetCoworkerName(state.RunSeed, incident.Id);
                    state.PendingLifeEvent = new PendingLifeEvent
                    {
                        Type = incident.Type,
                        Title = "Coworker Drive-By",
                        Description = $"{coworkerName} needs context, help, or cleanup during office hours. The work is real, but so is the drain.",
                        SubjectName = coworkerName,
                    };
                    AppendLog(state, incident.Description);
                }
                break;

            case IncidentType.IndieFundingSwing:
                if (state.GameplayMode == GameplayLoopMode.Indie)
                {
                    var positive = (CreateSeed(state.RunSeed, $"{incident.Id}:indie-funding") % 100) >= (state.PublishedAppCount > 0 ? 38 : 62);
                    var amount = positive
                        ? 12 + (state.PublishedAppCount * 4)
                        : 10 + Math.Max(0, (2 - state.PublishedAppCount) * 4);
                    state.Funds += positive ? amount : -amount;
                    if (positive)
                    {
                        state.Sanity = Clamp(state.Sanity + 2, 0, Config.MaxSanity);
                    }
                    else
                    {
                        state.Sanity = Clamp(state.Sanity - 1, 0, Config.MaxSanity);
                    }

                    AppendLog(state, incident.Description);
                    AppendLog(
                        state,
                        $"{ProceduralRunContent.GetIndieFundingLine(state.RunSeed, incident.Id, state.CurrentProjectBlueprint, positive)} Funds {(positive ? "+" : "-")}${amount:0}{(positive ? ", sanity +2." : ", sanity -1.")}");
                }
                break;
        }
    }

    private ActiveJobListing CreateJobListing(RunState state, string incidentId)
    {
        var profiles = GetJobProfilesForRun(state.RunSeed);

        var listingIndex = incidentId switch
        {
            "job-2" => 1,
            _ when incidentId.StartsWith("job-dyn-", StringComparison.Ordinal) =>
                2 + Math.Max(0, state.GeneratedJobListingCount - 1),
            _ => 0,
        };

        var profile = profiles[listingIndex % profiles.Count];
        var offerMode = state.GameplayMode == GameplayLoopMode.Interview
            ? (CreateSeed(state.RunSeed, $"{incidentId}:offer-mode") % 100) >= 45
                ? GameplayLoopMode.Corporate
                : GameplayLoopMode.Indie
            : state.GameplayMode == GameplayLoopMode.Corporate
                ? GameplayLoopMode.Corporate
                : GameplayLoopMode.Indie;
        var companyName = ProceduralRunContent.GetCompanyName(state.RunSeed, incidentId, offerMode);
        var difficultyPortfolioOffset = state.Difficulty switch
        {
            GameDifficulty.Easy => -10,
            GameDifficulty.Hard => 10,
            _ => 0,
        };
        var difficultyQualityOffset = state.Difficulty switch
        {
            GameDifficulty.Easy => -5,
            GameDifficulty.Hard => 5,
            _ => 0,
        };
        var routePortfolioOffset = state.GameplayMode switch
        {
            GameplayLoopMode.Corporate => -6,
            GameplayLoopMode.Indie => 8,
            _ => 0,
        };
        var routeQualityOffset = state.GameplayMode switch
        {
            GameplayLoopMode.Corporate => -2,
            GameplayLoopMode.Indie => 4,
            _ => 0,
        };
        var tier = Math.Min(4, listingIndex);
        var resumeTrack = GetResumeTrackForTechStack(profile.TechStack);
        var difficultyResumeOffset = state.Difficulty == GameDifficulty.Hard ? 1 : 0;
        var routeResumeOffset = state.GameplayMode == GameplayLoopMode.Indie ? 1 : 0;

        return new ActiveJobListing
        {
            ListingId = incidentId,
            Title = profile.Title,
            TechStack = profile.TechStack,
            CompanyName = companyName,
            OfferMode = offerMode,
            RemainingInGameMinutes = Config.JobListingDurationMinutes +
                                     (state.Difficulty == GameDifficulty.Easy ? 60 : 0) +
                                     (state.GameplayMode == GameplayLoopMode.Corporate ? 45 : 0),
            ResumeCostLines = Config.JobResumeCostLines + (tier * 2),
            MinimumPortfolioLines = Math.Max(40, Config.JobMinimumPortfolioLines + difficultyPortfolioOffset + routePortfolioOffset + (tier * 10)),
            MinimumCodeQuality = Math.Max(35, Config.JobMinimumCodeQuality + difficultyQualityOffset + routeQualityOffset + (tier * 3)),
            ResumeTrack = resumeTrack,
            RequiredResumeProof = Math.Max(1, 1 + (tier / 2) + difficultyResumeOffset + routeResumeOffset),
        };
    }

    private static IReadOnlyList<(string Title, string TechStack)> GetJobProfilesForRun(int runSeed)
    {
        var profiles = JobProfiles.ToList();
        Shuffle(profiles, runSeed == 0 ? 17 : runSeed);
        return profiles;
    }

    private static int RevealApplicationLines(ActiveJobApplication application, int requestedLinesOfCode)
    {
        var linesToReveal = Math.Max(0, requestedLinesOfCode);
        var linesAdded = 0;

        while (linesToReveal > 0 && application.VisibleLineCount < application.CodeLines.Count)
        {
            var nextLine = application.CodeLines[application.VisibleLineCount];
            application.VisibleLineCount++;

            if (string.IsNullOrWhiteSpace(nextLine))
            {
                continue;
            }

            linesAdded++;
            linesToReveal--;
        }

        return linesAdded;
    }

    private static int RevealFreelanceLines(ActiveFreelanceGig gig, int requestedLinesOfCode)
    {
        var linesToReveal = Math.Max(0, requestedLinesOfCode);
        var linesAdded = 0;

        while (linesToReveal > 0 && gig.VisibleLineCount < gig.CodeLines.Count)
        {
            var nextLine = gig.CodeLines[gig.VisibleLineCount];
            gig.VisibleLineCount++;

            if (string.IsNullOrWhiteSpace(nextLine))
            {
                continue;
            }

            linesAdded++;
            linesToReveal--;
        }

        return linesAdded;
    }

    private ActiveFreelanceGig CreateFreelanceGig(RunState state, FreelanceGigType type)
    {
        var gig = GetFreelanceGig(state, type);
        var assignment = ProceduralRunContent.CreateFreelanceAssignment(state.RunSeed, state.CurrentProjectBlueprint, type);
        var activeGig = new ActiveFreelanceGig
        {
            Type = type,
            ClientName = ProceduralRunContent.GetFreelanceClientName(state.RunSeed, type),
            Title = gig.Name,
            Brief = gig.Description,
            FileName = assignment.FileName,
            DurationMinutes = gig.DurationMinutes,
            FundsGain = gig.FundsGain,
            FocusCost = gig.FocusCost,
            SanityCost = gig.SanityCost,
            CodeQualityGain = gig.CodeQualityGain,
        };

        activeGig.CodeLines.AddRange(assignment.CodeLines);
        return activeGig;
    }

    private double GetFoodCost(RunState state, FoodChoice choice)
    {
        var foodCost = GetFoodOption(state, choice).FundsCost;
        foodCost -= GetUpgradeBonusTotal(state, definition => definition.FoodCostReduction);

        return Math.Max(1, foodCost);
    }

    private double GetFoodCost(FoodChoice choice)
    {
        return GetFoodOption(choice).FundsCost;
    }

    private double GetBaseFoodDuration(FoodChoice choice)
    {
        return choice switch
        {
            FoodChoice.SkilletPasta => Config.SkilletPastaCookDurationMinutes,
            FoodChoice.MealPrepChili => Config.MealPrepChiliCookDurationMinutes,
            _ => Config.FoodDeliveryDurationMinutes,
        };
    }

    private double GetSquashBugFocusCost(RunState state)
    {
        var focusCost = Config.SquashBugFocusCost;
        focusCost -= GetUpgradeBonusTotal(state, definition => definition.BugSquashFocusCostReduction);

        return Math.Max(1, focusCost);
    }

    private static string GetFoodLabel(FoodChoice choice)
    {
        return choice switch
        {
            FoodChoice.Burrito => "Burrito",
            FoodChoice.Pizza => "Pizza",
            FoodChoice.Dumplings => "Dumplings",
            FoodChoice.Ramen => "Ramen",
            FoodChoice.RiceBowl => "Rice Bowl",
            FoodChoice.SkilletPasta => "Skillet Pasta",
            FoodChoice.MealPrepChili => "Meal Prep Chili",
            _ => "Burger",
        };
    }

    public bool CanSpendFocusOnDistraction(RunState state)
    {
        return state.ActiveCatInterruption is not null &&
               state.Focus >= state.ActiveCatInterruption.FocusActionFocusCost;
    }

    public bool SpendFocusOnDistraction(RunState state)
    {
        var distraction = state.ActiveCatInterruption;
        if (distraction is null || !CanSpendFocusOnDistraction(state))
        {
            return false;
        }

        state.Focus = Clamp(state.Focus - distraction.FocusActionFocusCost, 0, Config.MaxFocus);
        distraction.PatsRemaining = Math.Max(0, distraction.PatsRemaining - Math.Max(1, distraction.FocusActionPatReduction));
        distraction.MinutesUntilNextTypingBurst = Math.Max(distraction.MinutesUntilNextTypingBurst, Config.CatTypingBurstIntervalMinutes * 0.9);
        AppendLog(state, $"{distraction.FocusActionLabel} buys back a little control. Focus -{distraction.FocusActionFocusCost:0}.");
        ClearDistractionIfResolved(state);
        return true;
    }

    public bool CanQuickResolveDistraction(RunState state)
    {
        return state.ActiveCatInterruption is not null &&
               state.Funds >= state.ActiveCatInterruption.QuickResolveFundsCost &&
               state.Focus >= state.ActiveCatInterruption.QuickResolveFocusCost;
    }

    public bool QuickResolveDistraction(RunState state)
    {
        var distraction = state.ActiveCatInterruption;
        if (distraction is null || !CanQuickResolveDistraction(state))
        {
            return false;
        }

        state.Funds -= distraction.QuickResolveFundsCost;
        state.Focus = Clamp(state.Focus - distraction.QuickResolveFocusCost, 0, Config.MaxFocus);
        state.ActiveCatInterruption = null;
        AppendLog(state, $"{distraction.QuickResolveLabel} clears the distraction fast. Funds -${distraction.QuickResolveFundsCost:0}, focus -{distraction.QuickResolveFocusCost:0}.");
        return true;
    }

    private void ClearDistractionIfResolved(RunState state)
    {
        var distraction = state.ActiveCatInterruption;
        if (distraction is null || distraction.PatsRemaining > 0)
        {
            return;
        }

        state.ActiveCatInterruption = null;
        var chaosSummary = distraction.PhantomBugCount > 0 || distraction.GibberishLinesTyped > 0
            ? $" It already slipped in {distraction.PhantomBugCount} phantom bug burst{(distraction.PhantomBugCount == 1 ? string.Empty : "s")} and {distraction.GibberishLinesTyped} gibberish line{(distraction.GibberishLinesTyped == 1 ? string.Empty : "s")}."
            : string.Empty;
        AppendLog(state, $"{distraction.Title} finally clears.{chaosSummary}");
    }

    private ActiveCatInterruption CreateDeskDistraction(RunState state, string incidentId)
    {
        var kind = (DeskDistractionKind)(CreateSeed(state.RunSeed, $"{incidentId}:desk-kind") % 3);
        var (title, description, manualAction, focusAction, quickResolve) = kind switch
        {
            DeskDistractionKind.PhoneBuzz => (
                "Phone Buzz Storm",
                "Notifications keep cutting across the desk faster than you can swipe them away.",
                "Clear Ping",
                "Hit Focus Mode",
                "Mute Everything"),
            DeskDistractionKind.NeighborNoise => (
                "Neighbor Noise",
                "Wall-shaking noise keeps knocking the run out of rhythm and back into survival mode.",
                "Recenter",
                "Put On Headphones",
                "Fix The Noise Fast"),
            _ => (
                "Desk Cat Event",
                "The cat decided your keyboard is warmer than any bed. Clear it before it times out, trashes part of the draft, and keeps typing nonsense into the editor.",
                "Pet Away",
                "Laser Pointer",
                "Treat Bribe"),
        };

        return new ActiveCatInterruption
        {
            Kind = kind,
            Title = title,
            Description = description,
            ManualActionLabel = manualAction,
            FocusActionLabel = focusAction,
            QuickResolveLabel = quickResolve,
            PatsRemaining = Config.CatPatsRequired + (kind == DeskDistractionKind.NeighborNoise ? 2 : 0),
            RemainingInGameMinutes = Config.CatStayDurationMinutes,
            LinesDeletionPenalty = Config.CatLinesDeletionPenalty + (kind == DeskDistractionKind.PhoneBuzz ? 5 : 0),
            MinutesUntilNextTypingBurst = Config.CatTypingBurstIntervalMinutes,
            VisualSeed = CreateSeed(state.RunSeed, $"{incidentId}:cat"),
            FocusActionFocusCost = kind == DeskDistractionKind.Cat ? 3 : 2,
            FocusActionPatReduction = kind == DeskDistractionKind.NeighborNoise ? 3 : 2,
            QuickResolveFundsCost = kind == DeskDistractionKind.Cat ? 4 : 3,
            QuickResolveFocusCost = kind == DeskDistractionKind.NeighborNoise ? 1 : 2,
        };
    }

    private PendingLifeEvent CreateOnlineMatchMiniGame(
        RunState state,
        string matchName,
        int compatibility,
        int stageIndex,
        int currentScore,
        int targetScore)
    {
        return new PendingLifeEvent
        {
            Type = IncidentType.OnlineMatch,
            Title = stageIndex == 0 ? "Dating Minigame" : $"Dating Minigame  {stageIndex + 1}/3",
            Description = GetOnlineMatchPrompt(matchName, compatibility, stageIndex),
            SubjectName = matchName,
            SubjectScore = compatibility,
            StageIndex = stageIndex,
            ProgressScore = currentScore,
            TargetScore = targetScore,
        };
    }

    private string GetOnlineMatchPrompt(string matchName, int compatibility, int stageIndex)
    {
        var archetype = Math.Abs(compatibility) % 3;
        return (stageIndex, archetype) switch
        {
            (0, 0) => $"{matchName} is clearly proud of what they build. Pick the opener that feels curious instead of performative.",
            (0, 1) => $"{matchName} is giving grounded, real-person energy. Pick the opener that sounds like you actually noticed that.",
            (0, _) => $"{matchName} seems playful and fast. Pick the opener that feels lively without turning fake.",
            (1, 0) => $"{matchName} replies. Now pick the plan that fits their pace instead of forcing a generic date template.",
            (1, 1) => $"{matchName} is in. Choose the plan that feels warm and realistic, not overproduced.",
            (1, _) => $"{matchName} keeps the energy moving. Choose the plan that feels fun without being flaky.",
            (2, 0) => $"{matchName} asks what this week actually looks like for you. Pick the answer that is honest and steady.",
            (2, 1) => $"{matchName} gives you room to be real. Pick the response that sounds present, not optimized.",
            _ => $"{matchName} gives you one last chance to show a real signal. Pick the response that sounds alive.",
        };
    }

    private static int GetOnlineMatchIdealOption(PendingLifeEvent lifeEvent)
    {
        var archetype = Math.Abs(lifeEvent.SubjectScore) % 3;
        return (lifeEvent.StageIndex, archetype) switch
        {
            (0, 0) => 0,
            (0, 1) => 2,
            (0, _) => 1,
            (1, 0) => 2,
            (1, 1) => 0,
            (1, _) => 1,
            (2, 0) => 0,
            (2, 1) => 0,
            _ => 2,
        };
    }

    private void EnsureRelationshipCandidate(RunState state)
    {
        if (!string.IsNullOrWhiteSpace(state.RelationshipCandidateName))
        {
            var existingDate = state.KnownContacts.FirstOrDefault(contact =>
                contact.Role == SocialContactRole.Date &&
                string.Equals(contact.Name, state.RelationshipCandidateName, StringComparison.Ordinal));
            if (existingDate is null)
            {
                var fallbackId = FindKnownContact(state, "date-primary") is null
                    ? "date-primary"
                    : "date-secondary";
                TryDiscoverContact(state, fallbackId, state.RelationshipCandidateName, SocialContactRole.Date, null);
            }

            return;
        }

        var knownPrimaryDate = FindKnownContact(state, "date-primary");
        var knownSecondaryDate = FindKnownContact(state, "date-secondary");
        string contactId;
        string contactName;

        if (knownPrimaryDate is null)
        {
            contactId = "date-primary";
            contactName = ProceduralRunContent.GetRelationshipCandidateName(state.RunSeed);
        }
        else if (!state.HasFoundLove && knownSecondaryDate is null)
        {
            contactId = "date-secondary";
            contactName = ProceduralRunContent.GetSocialContactName(state.RunSeed, "date-secondary");
        }
        else
        {
            var knownDate = state.KnownContacts
                .Where(contact => contact.Role == SocialContactRole.Date &&
                                  !string.Equals(contact.Name, state.PartnerName, StringComparison.Ordinal))
                .OrderBy(contact => string.Equals(contact.Id, "date-secondary", StringComparison.Ordinal) ? 0 : 1)
                .FirstOrDefault();
            contactId = knownDate?.Id ?? "date-primary";
            contactName = knownDate?.Name ?? ProceduralRunContent.GetRelationshipCandidateName(state.RunSeed);
        }

        state.RelationshipCandidateName = contactName;
        state.RelationshipCandidateCompatibility = ProceduralRunContent.GetRelationshipCandidateCompatibility(state.RunSeed);
        TryDiscoverContact(
            state,
            contactId,
            contactName,
            SocialContactRole.Date,
            $"{contactName} lands in Communication. The thread is real enough to keep alive now.");
        SynchronizeRelationshipContact(state);
    }

    private bool ApplyCommunicationAction(RunState state, string contactId, bool isCall)
    {
        var contact = FindKnownContact(state, contactId);
        if (contact is null ||
            (isCall
                ? !CanCallContact(state, contactId)
                : !CanMessageContact(state, contactId)))
        {
            return false;
        }

        var durationMinutes = isCall
            ? contact.Role switch
            {
                SocialContactRole.Date => 22d,
                SocialContactRole.Mentor => 24d,
                _ => 18d,
            }
            : contact.Role switch
            {
                SocialContactRole.Date => 12d,
                SocialContactRole.Mentor => 10d,
                _ => 8d,
            };
        var focusCost = isCall
            ? contact.Role switch
            {
                SocialContactRole.Date => 2.5,
                SocialContactRole.Mentor => 3d,
                _ => 2d,
            }
            : contact.Role switch
            {
                SocialContactRole.Date => 1.5,
                SocialContactRole.Mentor => 1.5,
                _ => 1d,
            };
        var sanityDelta = contact.Role switch
        {
            SocialContactRole.Date => isCall ? 3.5 : 1.5,
            SocialContactRole.Mentor => isCall ? 1.5 : 0.5,
            _ => isCall ? 4d : 1.5,
        };
        var bondGain = contact.Role switch
        {
            SocialContactRole.Date => isCall ? 2 : 1,
            SocialContactRole.Mentor => isCall ? 2 : 1,
            _ => isCall ? 2 : 1,
        };

        AdvanceTime(state, durationMinutes);
        if (state.Status != RunStatus.InProgress)
        {
            return false;
        }

        state.Focus = Clamp(state.Focus - focusCost, 0, Config.MaxFocus);
        state.Sanity = Clamp(state.Sanity + sanityDelta, 0, Config.MaxSanity);
        contact.BondProgress += bondGain;
        if (isCall)
        {
            contact.CallCount += 1;
        }
        else
        {
            contact.MessageCount += 1;
        }

        switch (contact.Role)
        {
            case SocialContactRole.Date:
                state.RelationshipCandidateName = contact.Name;
                state.RelationshipProgress = Math.Max(state.RelationshipProgress, contact.BondProgress);
                if (!state.HasFoundLove &&
                    contact.BondProgress >= Config.RelationshipProgressNeededForLove)
                {
                    state.HasFoundLove = true;
                    state.PartnerName = contact.Name;
                    state.Sanity = Clamp(state.Sanity + 6, 0, Config.MaxSanity);
                    AppendLog(state, $"{contact.Name} stops feeling hypothetical. The relationship is real now, and the run gets a little less lonely.");
                }
                else
                {
                    AppendLog(
                        state,
                        isCall
                            ? $"You called {contact.Name}. {FormatMinutesForLog(durationMinutes)} passed, sanity +{sanityDelta:0.#}, focus -{focusCost:0.#}, relationship +{bondGain}."
                            : $"You texted {contact.Name}. {FormatMinutesForLog(durationMinutes)} passed, sanity +{sanityDelta:0.#}, focus -{focusCost:0.#}, relationship +{bondGain}.");
                }
                break;

            case SocialContactRole.Mentor:
                var prepGain = isCall ? 2 : 1;
                AwardInterviewPrep(state, prepGain, $"{contact.Name} sharpened the interview story.");
                AppendLog(
                    state,
                    isCall
                        ? $"You called {contact.Name} and got real signal back. {FormatMinutesForLog(durationMinutes)} passed, prep +{prepGain}, sanity +{sanityDelta:0.#}, focus -{focusCost:0.#}."
                        : $"You texted {contact.Name}. {FormatMinutesForLog(durationMinutes)} passed, prep +{prepGain}, sanity +{sanityDelta:0.#}, focus -{focusCost:0.#}.");
                break;

            default:
                AppendLog(
                    state,
                    isCall
                        ? $"You called {contact.Name}. {FormatMinutesForLog(durationMinutes)} passed, sanity +{sanityDelta:0.#}, focus -{focusCost:0.#}, friendship +{bondGain}."
                        : $"You texted {contact.Name}. {FormatMinutesForLog(durationMinutes)} passed, sanity +{sanityDelta:0.#}, focus -{focusCost:0.#}, friendship +{bondGain}.");
                break;
        }

        SynchronizeRelationshipContact(state);
        EvaluateLossState(state);
        return true;
    }

    private static SocialContact? FindKnownContact(RunState state, string contactId)
    {
        return state.KnownContacts.FirstOrDefault(contact => string.Equals(contact.Id, contactId, StringComparison.Ordinal));
    }

    private void RefreshKnownContactsFromProgress(RunState state)
    {
        if (state.Status != RunStatus.InProgress)
        {
            return;
        }

        if (state.LinesOfCode >= 16)
        {
            TryDiscoverContact(
                state,
                "friend-one",
                ProceduralRunContent.GetSocialContactName(state.RunSeed, "friend-one"),
                SocialContactRole.Friend,
                "A friend finally cuts through the sprint fog. Communication has one real person in it now.");
        }

        if (state.LinesOfCode >= 48 || state.ActiveJobApplication is not null)
        {
            var mentorName = ProceduralRunContent.GetCoworkerName(state.RunSeed, "mentor-one");
            TryDiscoverContact(
                state,
                "mentor-one",
                mentorName,
                SocialContactRole.Mentor,
                $"{mentorName} starts taking the work seriously. Communication just picked up a mentor contact.");
        }

        if (state.LinesOfCode >= 96 || state.PublishedAppCount > 0)
        {
            var friendName = ProceduralRunContent.GetSocialContactName(state.RunSeed, "friend-two");
            TryDiscoverContact(
                state,
                "friend-two",
                friendName,
                SocialContactRole.Friend,
                $"{friendName} ends up back in the orbit. The contacts list is starting to feel lived in.");
        }

        if (state.LinesOfCode >= 144 || state.PublishedAppCount > 0 || state.SuccessfulApplications > 0)
        {
            var mentorName = ProceduralRunContent.GetCoworkerName(state.RunSeed, "mentor-two");
            TryDiscoverContact(
                state,
                "mentor-two",
                mentorName,
                SocialContactRole.Mentor,
                $"{mentorName} becomes a real senior contact you can actually reach when the run gets noisy.");
        }
    }

    private bool TryDiscoverContact(
        RunState state,
        string contactId,
        string name,
        SocialContactRole role,
        string? logLine)
    {
        if (FindKnownContact(state, contactId) is not null ||
            state.KnownContacts.Any(contact => string.Equals(contact.Name, name, StringComparison.Ordinal)))
        {
            return false;
        }

        state.KnownContacts.Add(new SocialContact
        {
            Id = contactId,
            Name = name,
            Role = role,
        });

        if (!string.IsNullOrWhiteSpace(logLine))
        {
            AppendLog(state, logLine);
        }

        return true;
    }

    private static void SynchronizeRelationshipContact(RunState state)
    {
        var relationshipName = !string.IsNullOrWhiteSpace(state.PartnerName)
            ? state.PartnerName
            : state.RelationshipCandidateName;
        if (string.IsNullOrWhiteSpace(relationshipName))
        {
            return;
        }

        var contact = state.KnownContacts.FirstOrDefault(candidate => string.Equals(candidate.Name, relationshipName, StringComparison.Ordinal));
        if (contact is null)
        {
            return;
        }

        contact.BondProgress = Math.Max(contact.BondProgress, state.RelationshipProgress);
    }

    private PendingLifeEvent CreateFounderNamingEvent(RunState state)
    {
        var options = ProceduralRunContent.GetFounderStudioNameChoices(state.RunSeed);
        return new PendingLifeEvent
        {
            Type = IncidentType.FounderNaming,
            Title = "Name Your Studio",
            Description = "Founder Mode starts by choosing the studio name you are going to ship, pitch, and suffer under.",
            OptionLabels = options,
        };
    }

    private string GetGameplayModeLabel(RunState state)
    {
        return state.GameplayMode switch
        {
            GameplayLoopMode.Corporate => "Corporate Mode",
            GameplayLoopMode.Indie => "Indie Mode",
            GameplayLoopMode.Founder => "Founder Mode",
            _ => "Interview Mode",
        };
    }

    private bool ShouldContinueAfterSuccessfulApplication(RunState state)
    {
        return state.GameplayMode != GameplayLoopMode.Interview || Config.ContinueAfterSuccessfulApplication;
    }

    private double GetSuccessfulApplicationFundsReward(RunState state)
    {
        return state.GameplayMode switch
        {
            GameplayLoopMode.Corporate => Math.Max(Config.SuccessfulApplicationFundsReward, 72),
            GameplayLoopMode.Indie => Math.Max(Config.SuccessfulApplicationFundsReward, 28),
            GameplayLoopMode.Founder => Math.Max(Config.SuccessfulApplicationFundsReward, 18),
            _ => Config.SuccessfulApplicationFundsReward,
        };
    }

    private double GetSuccessfulApplicationSanityReward(RunState state)
    {
        return state.GameplayMode switch
        {
            GameplayLoopMode.Corporate => Math.Max(Config.SuccessfulApplicationSanityReward, 4),
            GameplayLoopMode.Indie => Math.Max(Config.SuccessfulApplicationSanityReward, 5),
            GameplayLoopMode.Founder => Math.Max(Config.SuccessfulApplicationSanityReward, 2),
            _ => Config.SuccessfulApplicationSanityReward,
        };
    }

    private string GetContinuationModeLabel(RunState state)
    {
        return state.GameplayMode switch
        {
            GameplayLoopMode.Corporate => "Corporate Mode",
            GameplayLoopMode.Indie => "Indie Mode",
            GameplayLoopMode.Founder => "Founder Mode",
            _ when Config.EndlessPortfolio => "Endless mode",
            _ => "Continual upgrade loop",
        };
    }

    private static VersionControlState CreateVersionControlState(ProjectBlueprint projectBlueprint)
    {
        return new VersionControlState
        {
            MainBranchName = "main",
            CurrentBranchName = "main",
            CommitCount = 0,
            FeatureBranchCommitCount = 0,
            PendingChangeLines = 0,
            PendingCompletedFileCount = 0,
            CommittedPortfolioLinesOfCode = 0,
            BranchSerial = 0,
            MergeConflictCount = 0,
            LastCommitSummary = $"init {projectBlueprint.Title.ToLowerInvariant()}",
        };
    }

    private void TrackVersionControlWork(RunState state, int linesAdded)
    {
        if (linesAdded <= 0)
        {
            return;
        }

        RefreshPendingChangeLines(state);
    }

    private static int GetUpgradeBonusTotal(RunState state, Func<EfficiencyUpgradeDefinition, int> selector)
    {
        var total = 0;
        foreach (var (type, tier) in state.PurchasedUpgrades)
        {
            total += selector(EfficiencyUpgradeCatalog.Get(type)) * tier;
        }

        return total;
    }

    private static double GetUpgradeBonusTotal(RunState state, Func<EfficiencyUpgradeDefinition, double> selector)
    {
        var total = 0d;
        foreach (var (type, tier) in state.PurchasedUpgrades)
        {
            total += selector(EfficiencyUpgradeCatalog.Get(type)) * tier;
        }

        return total;
    }

    private static void RefreshPendingChangeLines(RunState state)
    {
        var versionControl = state.VersionControl;
        versionControl.PendingChangeLines = Math.Max(0, state.CurrentPortfolioLinesOfCode - versionControl.CommittedPortfolioLinesOfCode);
        if (versionControl.PendingChangeLines == 0)
        {
            versionControl.PendingCompletedFileCount = 0;
        }
    }

    private static int LoseUncommittedProgress(RunState state)
    {
        var versionControl = state.VersionControl;
        var committedLines = Math.Clamp(versionControl.CommittedPortfolioLinesOfCode, 0, state.CurrentPortfolioLinesOfCode);
        var lostLines = Math.Max(0, state.CurrentPortfolioLinesOfCode - committedLines);
        if (lostLines <= 0)
        {
            return 0;
        }

        state.CurrentPortfolioLinesOfCode = committedLines;
        state.LinesOfCode = Math.Max(0, state.LinesOfCode - lostLines);
        versionControl.PendingChangeLines = 0;
        versionControl.PendingCompletedFileCount = 0;
        state.RecentCompletedFileName = null;
        state.FileCompletionCelebrationMinutesRemaining = 0;
        PortfolioWorkspace.SynchronizeToLinesOfCode(state);
        return lostLines;
    }

    private double GetFreelanceFundsMultiplier(RunState state, FreelanceGigType type)
    {
        var multiplier = 1d;
        if (state.GameplayMode == GameplayLoopMode.Corporate)
        {
            multiplier += type == FreelanceGigType.PipelineRescue ? 0.12 : 0.06;
        }
        else if (state.GameplayMode == GameplayLoopMode.Indie &&
                 (state.PublishedAppCount == 0 || state.Funds < (GetCurrentDailyBillAmount(state) * 2)))
        {
            multiplier += 0.12;
        }
        else if (state.GameplayMode == GameplayLoopMode.Founder)
        {
            multiplier += state.PublishedAppCount == 0 ? 0.18 : 0.1;
        }

        if (state.IsRealisticMode)
        {
            multiplier -= 0.03;
        }

        return Math.Max(0.85, multiplier);
    }

    private void AwardIndieProjectProgressIncome(RunState state, string completedFileName)
    {
        if (state.GameplayMode != GameplayLoopMode.Indie || string.IsNullOrWhiteSpace(completedFileName))
        {
            return;
        }

        var baseAmount = RollBoundedAmount(
            state.RunSeed,
            $"indie-progress:{completedFileName}:{state.CurrentProjectBlueprint.Signature}:{state.PublishedAppCount}",
            Config.IndieProjectProgressFundsMin,
            Config.IndieProjectProgressFundsMax);
        var blueprintMultiplier = (state.CurrentProjectBlueprint.PublishIncomeMultiplier + state.CurrentProjectBlueprint.SaleIncomeMultiplier) / 2d;
        var fundsGain = Math.Round(Math.Max(2d, baseAmount * blueprintMultiplier), 0);
        state.Funds += fundsGain;
        AppendLog(state, $"Indie project progress pays out on {completedFileName}. +${fundsGain:0} lands while the build is still in flight.");
    }

    private static int GetBossCheckInIdealOption(BossDisposition disposition)
    {
        return disposition switch
        {
            BossDisposition.Supportive => 1,
            BossDisposition.Nice => 0,
            BossDisposition.Mean => 0,
            _ => 2,
        };
    }

    private double ApplyRouteIncomeModifier(RunState state, double amount, string incomeType)
    {
        var multiplier = state.GameplayMode switch
        {
            GameplayLoopMode.Corporate when incomeType == "publish" => 0.9,
            GameplayLoopMode.Corporate when incomeType == "sale" => 0.88,
            GameplayLoopMode.Indie when incomeType == "publish" => 0.94,
            GameplayLoopMode.Indie when incomeType == "sale" => 0.92,
            GameplayLoopMode.Founder when incomeType == "publish" => 1.08,
            GameplayLoopMode.Founder when incomeType == "sale" => 1.12,
            _ => 1.0,
        };

        if (incomeType == "publish")
        {
            multiplier *= state.CurrentProjectBlueprint.PublishIncomeMultiplier;
        }
        else if (incomeType == "sale")
        {
            multiplier *= state.CurrentProjectBlueprint.SaleIncomeMultiplier;
        }

        if (state.IsRealisticMode && incomeType != "offer")
        {
            multiplier *= 0.96;
        }

        return Math.Round(amount * multiplier, 0);
    }

    public double GetCurrentDailyBillAmount(RunState state)
    {
        var billAmount = Config.DailyBillAmount;
        if (state.HasApartment && !state.HasHouse)
        {
            billAmount += 10;
        }
        else if (state.HasHouse)
        {
            billAmount += 6;
        }

        if (state.GameplayMode == GameplayLoopMode.Founder && !state.HasApartment)
        {
            billAmount += 4;
        }

        return Math.Round(Math.Max(0, billAmount), 0);
    }

    public bool IsInCorporateOfficeHours(RunState state)
    {
        return state.GameplayMode == GameplayLoopMode.Corporate &&
               state.TimeOfDayMinutes >= Config.CorporateOfficeHoursStartMinutes &&
               state.TimeOfDayMinutes < Config.CorporateOfficeHoursEndMinutes;
    }

    private double GetModeFocusRecoveryMultiplier(RunState state)
    {
        return state.GameplayMode switch
        {
            GameplayLoopMode.Indie => Config.IndieFocusRecoveryMultiplier,
            GameplayLoopMode.Founder => Config.FounderFocusRecoveryMultiplier,
            _ => 1d,
        };
    }

    private void ApplyModePressure(RunState state, double elapsedInGameMinutes)
    {
        if (state.GameplayMode != GameplayLoopMode.Corporate)
        {
            return;
        }

        var officeMinutes = GetCorporateOfficeMinutesInStep(state.TimeOfDayMinutes, elapsedInGameMinutes);
        if (officeMinutes <= BoundaryEpsilon)
        {
            return;
        }

        var pressureMultiplier = state.BossDisposition switch
        {
            BossDisposition.Supportive => 0.9,
            BossDisposition.Nice => 1.0,
            BossDisposition.Mean => 1.28,
            _ => 1.45,
        };
        var sanityLoss = officeMinutes * Config.CorporateOfficeSanityLossPerInGameMinute * pressureMultiplier;
        var focusLoss = officeMinutes * Config.CorporateOfficeFocusLossPerInGameMinute * pressureMultiplier;
        state.Sanity = Clamp(state.Sanity - sanityLoss, 0, Config.MaxSanity);
        state.Focus = Clamp(state.Focus - focusLoss, 0, Config.MaxFocus);
    }

    private double GetCorporateOfficeMinutesInStep(double timeOfDayMinutes, double elapsedInGameMinutes)
    {
        var officeStart = Config.CorporateOfficeHoursStartMinutes;
        var officeEnd = Config.CorporateOfficeHoursEndMinutes;
        var stepEnd = Math.Min(SimulationConfig.MinutesPerDay, timeOfDayMinutes + elapsedInGameMinutes);
        var overlapStart = Math.Max(timeOfDayMinutes, officeStart);
        var overlapEnd = Math.Min(stepEnd, officeEnd);
        return Math.Max(0, overlapEnd - overlapStart);
    }

    private void ApplyDailyModeIncome(RunState state)
    {
        var income = state.GameplayMode switch
        {
            GameplayLoopMode.Corporate => Config.CorporateDailySalaryBase + (state.CorporateStanding * 6),
            GameplayLoopMode.Indie => Config.IndieDailyIncomeBase + Math.Min(14, state.PublishedAppCount * 2),
            _ => 0d,
        };

        if (income <= 0)
        {
            return;
        }

        state.Funds += income;
        AppendLog(
            state,
            state.GameplayMode == GameplayLoopMode.Corporate
                ? $"Corporate salary lands for +${income:0}. It pays better, but the office still owns the calendar."
                : $"Indie revenue lands for +${income:0}. It helps, but self-directed runs still need shipping or freelance work to grow.");
    }

    private int GetHungerStage(double minutesSinceLastMeal)
    {
        if (minutesSinceLastMeal >= Config.StarvingAfterMinutes)
        {
            return 3;
        }

        if (minutesSinceLastMeal >= Config.VeryHungryAfterMinutes)
        {
            return 2;
        }

        return minutesSinceLastMeal >= Config.HungryAfterMinutes ? 1 : 0;
    }

    private int GetSleepStage(double minutesSinceLastSleep)
    {
        if (minutesSinceLastSleep >= Config.SleepForcedAfterMinutes)
        {
            return 4;
        }

        if (minutesSinceLastSleep >= Config.SevereSleepDeprivationAfterMinutes)
        {
            return 3;
        }

        if (minutesSinceLastSleep >= Config.SleepDeprivationAfterMinutes)
        {
            return 2;
        }

        return minutesSinceLastSleep >= Config.SleepWarningAfterMinutes ? 1 : 0;
    }

    private double GetHungerSanityLossPerInGameMinute(int hungerStage)
    {
        return hungerStage switch
        {
            1 => Config.HungrySanityLossPerInGameMinute,
            2 => Config.VeryHungrySanityLossPerInGameMinute,
            >= 3 => Config.StarvingSanityLossPerInGameMinute,
            _ => 0,
        };
    }

    private double GetSleepSanityLossPerInGameMinute(int sleepStage)
    {
        return sleepStage switch
        {
            2 => Config.SleepDeprivationSanityLossPerInGameMinute,
            >= 3 => Config.SevereSleepDeprivationSanityLossPerInGameMinute,
            _ => 0,
        };
    }

    private double GetSleepQualityLossPerInGameMinute(int sleepStage)
    {
        return sleepStage switch
        {
            2 => Config.SleepDeprivationQualityLossPerInGameMinute,
            >= 3 => Config.SevereSleepDeprivationQualityLossPerInGameMinute,
            _ => 0,
        };
    }

    private int GetWriteCodeLinesGain(RunState state)
    {
        var lines = Config.WriteCodeLinesGain;
        lines += GetUpgradeBonusTotal(state, definition => definition.BonusLinesPerClick);

        if (IsDeepWorkActive(state))
        {
            lines += Config.DeepWorkBonusLinesPerClick;
        }

        if (state.GameplayMode == GameplayLoopMode.Indie)
        {
            lines += 1;
        }
        else if (state.GameplayMode == GameplayLoopMode.Founder)
        {
            lines += 1;
        }

        if (!IsSluggish(state))
        {
            return Math.Max(1, lines);
        }

        return Math.Max(1, (int)Math.Round(lines * Config.SluggishLinesMultiplier));
    }

    private double GetWriteCodeFocusCost(RunState state)
    {
        var focusCost = Config.WriteCodeFocusCost;
        focusCost -= GetUpgradeBonusTotal(state, definition => definition.FocusCostReduction);

        if (IsSluggish(state))
        {
            focusCost += Config.SluggishFocusCostPenalty;
        }

        if (IsContextSwitchActive(state))
        {
            focusCost += Config.ContextSwitchFocusCostPenalty;
        }

        if (state.GameplayMode == GameplayLoopMode.Corporate)
        {
            focusCost += state.BossDisposition switch
            {
                BossDisposition.Mean => 0.15,
                BossDisposition.Micromanager => 0.45,
                _ => 0,
            };
        }
        else if (state.GameplayMode == GameplayLoopMode.Indie)
        {
            focusCost += 0.25;
        }
        else if (state.GameplayMode == GameplayLoopMode.Founder)
        {
            focusCost += 0.4;
        }

        return Math.Max(0.5, focusCost);
    }

    private double GetWriteCodeQualityGain(RunState state)
    {
        var qualityGain = Config.WriteCodeQualityGain;
        qualityGain += GetUpgradeBonusTotal(state, definition => definition.BonusQualityGain);

        if (IsDeepWorkActive(state))
        {
            qualityGain += Config.DeepWorkBonusQualityGain;
        }

        if (state.GameplayMode == GameplayLoopMode.Corporate)
        {
            qualityGain += state.BossDisposition switch
            {
                BossDisposition.Supportive => 0.08,
                BossDisposition.Nice => 0.04,
                BossDisposition.Mean => -0.03,
                _ => -0.05,
            };
        }
        else if (state.GameplayMode == GameplayLoopMode.Indie)
        {
            qualityGain += 0.04;
        }
        else if (state.GameplayMode == GameplayLoopMode.Founder)
        {
            qualityGain += 0.06;
        }

        if (!IsSluggish(state))
        {
            return Math.Max(0.05, qualityGain);
        }

        return Math.Max(0.05, qualityGain * Config.SluggishQualityMultiplier);
    }

    private void ApplyPassiveFocusDrain(RunState state, double elapsedInGameMinutes)
    {
        var passiveDrain = Config.PassiveFocusDrainPerInGameMinute;
        passiveDrain -= GetUpgradeBonusTotal(state, definition => definition.PassiveFocusDrainReduction);

        if (IsContextSwitchActive(state))
        {
            passiveDrain += Config.ContextSwitchPassiveFocusDrainPerInGameMinute;
        }

        if (state.GameplayMode == GameplayLoopMode.Corporate)
        {
            passiveDrain += state.BossDisposition switch
            {
                BossDisposition.Mean => 0.004,
                BossDisposition.Micromanager => 0.008,
                _ => 0,
            };
        }
        else if (state.GameplayMode == GameplayLoopMode.Indie)
        {
            passiveDrain += 0.002;
        }
        else if (state.GameplayMode == GameplayLoopMode.Founder)
        {
            passiveDrain += 0.004;
        }

        state.Focus = Clamp(
            state.Focus - (elapsedInGameMinutes * Math.Max(0, passiveDrain)),
            0,
            Config.MaxFocus);
    }

    private void ApplyPassiveSanityRegeneration(RunState state, double elapsedInGameMinutes)
    {
        var passiveRegenPerMinute = 0d;
        if (state.HasFirstCoin)
        {
            passiveRegenPerMinute += Config.FirstCoinPassiveSanityRegenPerInGameMinute;
        }

        if (state.HasFoundLove)
        {
            passiveRegenPerMinute += Config.FoundLovePassiveSanityRegenPerInGameMinute;
        }

        if (state.HasHouse)
        {
            passiveRegenPerMinute += 0.002;
        }
        else if (state.HasApartment)
        {
            passiveRegenPerMinute += Config.ApartmentPassiveSanityRegenPerInGameMinute;
        }

        if (state.GameplayMode == GameplayLoopMode.Corporate)
        {
            passiveRegenPerMinute += state.BossDisposition switch
            {
                BossDisposition.Supportive => 0.001,
                BossDisposition.Nice => 0.0005,
                _ => 0,
            };
        }
        else if (state.GameplayMode == GameplayLoopMode.Indie)
        {
            passiveRegenPerMinute += 0.0015;
        }
        else if (state.GameplayMode == GameplayLoopMode.Founder && !string.IsNullOrWhiteSpace(state.StudioName))
        {
            passiveRegenPerMinute += 0.0005;
        }

        passiveRegenPerMinute += GetUpgradeBonusTotal(state, definition => definition.PassiveSanityRegenPerInGameMinute);

        if (passiveRegenPerMinute <= 0)
        {
            return;
        }

        state.Sanity = Clamp(
            state.Sanity + (elapsedInGameMinutes * passiveRegenPerMinute),
            0,
            Config.MaxSanity);
    }

    private void ApplyAmbientSanityPressure(RunState state, double elapsedInGameMinutes)
    {
        var pressure = Config.LongFormAmbientSanityLossPerInGameMinute;
        if (pressure <= 0)
        {
            return;
        }

        state.Sanity = Clamp(
            state.Sanity - (elapsedInGameMinutes * pressure),
            0,
            Config.MaxSanity);
    }

    private void ApplyTechDebtDecay(RunState state, double elapsedInGameMinutes)
    {
        if (state.ActiveTechDebtBug is null)
        {
            return;
        }

        state.CodeQuality = Clamp(
            state.CodeQuality - (elapsedInGameMinutes * state.ActiveTechDebtBug.QualityDrainPerMinute),
            0,
            Config.MaxCodeQuality);
    }

    private void ActivateDeskTechDebtBug(RunState state, QueuedIncident incident)
    {
        var bug = CreateTechDebtBug(
            state,
            incident.Id,
            TechDebtTrigger.DeskIncident,
            severity: 1,
            needDriven: false);

        AppendLog(state, incident.Description);
        ApplyOrIntensifyTechDebtBug(state, bug, immediateQualityLoss: 0, intensifiedLog: "Another compile error piled onto the current debug mess.");
        AppendTechDebtInstructionLog(state, bug);
    }

    private void AdvanceNeedDrivenDebuggingPressure(
        RunState state,
        double elapsedInGameMinutes,
        int hungerStage,
        int sleepStage,
        bool isSleeping)
    {
        state.NeedDrivenBugCooldownMinutesRemaining = Math.Max(
            0,
            state.NeedDrivenBugCooldownMinutesRemaining - elapsedInGameMinutes);

        if (state.ActiveTechDebtBug is not null)
        {
            state.DebuggingPressure = Math.Max(0, state.DebuggingPressure - elapsedInGameMinutes);
            return;
        }

        if (isSleeping)
        {
            state.DebuggingPressure = Math.Max(0, state.DebuggingPressure - (elapsedInGameMinutes * 2.5));
            return;
        }

        var pressurePerMinute = GetNeedDrivenDebuggingPressurePerMinute(state, hungerStage, sleepStage);
        if (pressurePerMinute <= 0)
        {
            state.DebuggingPressure = Math.Max(0, state.DebuggingPressure - (elapsedInGameMinutes * 0.9));
            return;
        }

        state.DebuggingPressure += elapsedInGameMinutes * pressurePerMinute;
        if (state.NeedDrivenBugCooldownMinutesRemaining > BoundaryEpsilon ||
            state.DebuggingPressure < Config.DebuggingPressureThreshold)
        {
            return;
        }

        var trigger = PickNeedDrivenTechDebtTrigger(state, hungerStage, sleepStage);
        var severity = GetNeedDrivenBugSeverity(trigger, hungerStage, sleepStage, state.Sanity);
        var sourceKey =
            $"need-bug:{trigger}:{state.Day}:{(int)Math.Floor(state.TimeOfDayMinutes)}:{(int)Math.Floor(state.DeskMinutesElapsed)}";
        var bug = CreateTechDebtBug(state, sourceKey, trigger, severity, needDriven: true);
        var qualityLoss = GetNeedDrivenBugQualityLoss(severity);

        ApplyOrIntensifyTechDebtBug(state, bug, qualityLoss, intensifiedLog: "The active compile mess deepened under the pressure.");
        AppendLog(state, $"{GetNeedDrivenBugLog(trigger)} Code quality -{qualityLoss:0}.");
        AppendTechDebtInstructionLog(state, bug);

        state.DebuggingPressure = Math.Max(0, state.DebuggingPressure - Config.DebuggingPressureThreshold);
        state.NeedDrivenBugCooldownMinutesRemaining = Config.NeedDrivenBugCooldownMinutes;
    }

    private double GetNeedDrivenDebuggingPressurePerMinute(RunState state, int hungerStage, int sleepStage)
    {
        var pressure = hungerStage switch
        {
            1 => 0.18,
            2 => 0.55,
            >= 3 => 1.05,
            _ => 0,
        };

        pressure += sleepStage switch
        {
            1 => 0.2,
            2 => 0.75,
            >= 3 => 1.3,
            _ => 0,
        };

        pressure += GetSanityDebugSeverity(state.Sanity) switch
        {
            1 => 0.22,
            2 => 0.65,
            >= 3 => 1.15,
            _ => 0,
        };

        return pressure;
    }

    private static int GetSanityDebugSeverity(double sanity)
    {
        if (sanity <= 20)
        {
            return 3;
        }

        if (sanity <= 35)
        {
            return 2;
        }

        return sanity <= 50 ? 1 : 0;
    }

    private static TechDebtTrigger PickNeedDrivenTechDebtTrigger(RunState state, int hungerStage, int sleepStage)
    {
        var hungerScore = hungerStage switch
        {
            1 => 1,
            2 => 3,
            >= 3 => 5,
            _ => 0,
        };
        var sleepScore = sleepStage switch
        {
            1 => 1,
            2 => 4,
            >= 3 => 6,
            _ => 0,
        };
        var sanityScore = GetSanityDebugSeverity(state.Sanity) switch
        {
            1 => 2,
            2 => 4,
            >= 3 => 6,
            _ => 0,
        };

        var strongestScore = Math.Max(hungerScore, Math.Max(sleepScore, sanityScore));
        if (strongestScore <= 0)
        {
            return TechDebtTrigger.DeskIncident;
        }

        var candidates = new List<TechDebtTrigger>();
        if (sleepScore == strongestScore)
        {
            candidates.Add(TechDebtTrigger.SleepDebt);
        }

        if (hungerScore == strongestScore)
        {
            candidates.Add(TechDebtTrigger.Hunger);
        }

        if (sanityScore == strongestScore)
        {
            candidates.Add(TechDebtTrigger.Sanity);
        }

        if (candidates.Count == 1)
        {
            return candidates[0];
        }

        var tieBreakSeed = CreateSeed(
            state.RunSeed,
            $"need-bug-cause:{state.Day}:{(int)Math.Floor(state.TimeOfDayMinutes)}:{(int)Math.Floor(state.DeskMinutesElapsed)}");
        return candidates[tieBreakSeed % candidates.Count];
    }

    private static int GetNeedDrivenBugSeverity(
        TechDebtTrigger trigger,
        int hungerStage,
        int sleepStage,
        double sanity)
    {
        return trigger switch
        {
            TechDebtTrigger.SleepDebt => sleepStage switch
            {
                >= 3 => 3,
                2 => 2,
                _ => 1,
            },
            TechDebtTrigger.Hunger => hungerStage switch
            {
                >= 3 => 3,
                2 => 2,
                _ => 1,
            },
            TechDebtTrigger.Sanity => Math.Max(1, GetSanityDebugSeverity(sanity)),
            _ => 1,
        };
    }

    private double GetNeedDrivenBugQualityLoss(int severity)
    {
        return Config.NeedDrivenBugInitialQualityLoss + Math.Max(0, severity - 1) * 2;
    }

    private ActiveTechDebtBug CreateTechDebtBug(
        RunState state,
        string sourceKey,
        TechDebtTrigger trigger,
        int severity,
        bool needDriven)
    {
        var template = PickTechDebtSnippetTemplate(state.RunSeed, $"{sourceKey}:{trigger}:{severity}:{needDriven}");
        var duration = Config.TechDebtDurationMinutes + (needDriven ? severity * 18 : 0);
        var drainMultiplier = needDriven
            ? Config.NeedDrivenBugDrainMultiplier
            : 1d;
        drainMultiplier *= 1d + (Math.Max(0, severity - 1) * 0.18);

        return new ActiveTechDebtBug
        {
            Summary = $"{GetTechDebtTriggerSummary(trigger)} {template.IssueLabel}",
            CompilerHint = template.CompilerHint,
            CodeLines = [.. template.CodeLines],
            HighlightLineIndex = template.HighlightLineIndex,
            HighlightStartIndex = template.HighlightStartIndex,
            HighlightLength = template.HighlightLength,
            HighlightToken = template.HighlightToken,
            HighlightIsInsertion = template.HighlightIsInsertion,
            IsNeedDriven = needDriven,
            RemainingInGameMinutes = duration,
            QualityDrainPerMinute = Config.TechDebtQualityDrainPerMinute * drainMultiplier,
        };
    }

    private bool ApplyOrIntensifyTechDebtBug(
        RunState state,
        ActiveTechDebtBug bug,
        double immediateQualityLoss,
        string intensifiedLog)
    {
        if (immediateQualityLoss > 0)
        {
            state.CodeQuality = Clamp(state.CodeQuality - immediateQualityLoss, 0, Config.MaxCodeQuality);
        }

        if (state.ActiveTechDebtBug is null)
        {
            state.ActiveTechDebtBug = bug;
            return true;
        }

        var activeBug = state.ActiveTechDebtBug;
        activeBug.Summary = bug.Summary;
        activeBug.CompilerHint = bug.CompilerHint;
        activeBug.CodeLines = [.. bug.CodeLines];
        activeBug.HighlightLineIndex = bug.HighlightLineIndex;
        activeBug.HighlightStartIndex = bug.HighlightStartIndex;
        activeBug.HighlightLength = bug.HighlightLength;
        activeBug.HighlightToken = bug.HighlightToken;
        activeBug.HighlightIsInsertion = bug.HighlightIsInsertion;
        activeBug.IsNeedDriven |= bug.IsNeedDriven;
        activeBug.RemainingInGameMinutes = Math.Max(activeBug.RemainingInGameMinutes, bug.RemainingInGameMinutes);
        activeBug.QualityDrainPerMinute = Math.Min(
            Config.TechDebtQualityDrainPerMinute * 4,
            Math.Max(activeBug.QualityDrainPerMinute, bug.QualityDrainPerMinute) + (bug.QualityDrainPerMinute * 0.2));
        AppendLog(state, intensifiedLog);
        return false;
    }

    private void AppendTechDebtInstructionLog(RunState state, ActiveTechDebtBug bug)
    {
        AppendLog(
            state,
            state.IsRealisticMode
                ? $"{bug.CompilerHint} Click the highlighted token in the editor to debug it before quality keeps bleeding."
                : $"{bug.CompilerHint} Fix it from Alerts before the quality drain snowballs.");
    }

    private static string GetNeedDrivenBugLog(TechDebtTrigger trigger)
    {
        return trigger switch
        {
            TechDebtTrigger.SleepDebt => "Late-night fatigue introduced a compile error.",
            TechDebtTrigger.Hunger => "Coding hungry slipped a fresh compile error into the file.",
            TechDebtTrigger.Sanity => "Stress cracked a safe refactor into a compile error.",
            _ => "A new compile error hit the file.",
        };
    }

    private static string GetTechDebtTriggerSummary(TechDebtTrigger trigger)
    {
        return trigger switch
        {
            TechDebtTrigger.SleepDebt => "Late-night typo spiral.",
            TechDebtTrigger.Hunger => "Hunger bug burst.",
            TechDebtTrigger.Sanity => "Sanity spiral refactor.",
            _ => "Fresh bug on the desk.",
        };
    }

    private void ApplyNeedPenalties(RunState state, double elapsedInGameMinutes, int hungerStage, int sleepStage)
    {
        var hungerSanityLoss = GetHungerSanityLossPerInGameMinute(hungerStage);
        if (hungerSanityLoss > 0)
        {
            state.Sanity = Clamp(
                state.Sanity - (elapsedInGameMinutes * hungerSanityLoss),
                0,
                Config.MaxSanity);
        }

        var sleepSanityLoss = GetSleepSanityLossPerInGameMinute(sleepStage);
        if (sleepSanityLoss > 0)
        {
            state.Sanity = Clamp(
                state.Sanity - (elapsedInGameMinutes * sleepSanityLoss),
                0,
                Config.MaxSanity);
        }

        var sleepQualityLoss = GetSleepQualityLossPerInGameMinute(sleepStage);
        if (sleepQualityLoss > 0)
        {
            state.CodeQuality = Clamp(
                state.CodeQuality - (elapsedInGameMinutes * sleepQualityLoss),
                0,
                Config.MaxCodeQuality);
        }
    }

    private void AdvanceNeedTimers(RunState state, double elapsedInGameMinutes, bool isSleeping)
    {
        state.MinutesSinceLastMeal += elapsedInGameMinutes;

        if (!isSleeping)
        {
            state.MinutesSinceLastSleep += elapsedInGameMinutes;
        }
    }

    private void AppendNeedThresholdLogs(RunState state, int previousHungerStage, int previousSleepStage, bool isSleeping)
    {
        var hungerStage = GetHungerStage(state);
        if (hungerStage > previousHungerStage)
        {
            AppendHungerThresholdLog(state, hungerStage);
        }

        if (isSleeping)
        {
            return;
        }

        var sleepStage = GetSleepStage(state);
        if (sleepStage > previousSleepStage)
        {
            AppendSleepThresholdLog(state, sleepStage);
        }
    }

    private void AppendHungerThresholdLog(RunState state, int hungerStage)
    {
        switch (hungerStage)
        {
            case 1:
                AppendLog(state, "It has been too long since the last meal. Hunger is starting to chip away at sanity.");
                break;
            case 2:
                AppendLog(state, "Running seriously hungry now. Sanity is dropping faster until you eat.");
                break;
            case 3:
                AppendLog(state, "Starving. Order food soon or the rest of the day is going to unravel.");
                break;
        }
    }

    private void AppendSleepThresholdLog(RunState state, int sleepStage)
    {
        switch (sleepStage)
        {
            case 1:
                AppendLog(state, "Still awake deep into the grind. Push much longer and fatigue will start shredding quality.");
                break;
            case 2:
                AppendLog(state, "One full day awake. Fatigue is now draining sanity and code quality.");
                break;
            case 3:
                AppendLog(state, "Thirty-six hours awake. Fatigue is chewing through sanity and code quality fast.");
                break;
            case 4:
                AppendLog(state, "Two straight days without sleep. You must sleep before doing anything serious.");
                break;
        }
    }

    private void AdvanceTimedEffects(RunState state, double elapsedInGameMinutes)
    {
        state.SluggishMinutesRemaining = Math.Max(0, state.SluggishMinutesRemaining - elapsedInGameMinutes);
        state.DeepWorkMinutesRemaining = Math.Max(0, state.DeepWorkMinutesRemaining - elapsedInGameMinutes);
        state.ContextSwitchMinutesRemaining = Math.Max(0, state.ContextSwitchMinutesRemaining - elapsedInGameMinutes);
        state.FileCompletionCelebrationMinutesRemaining = Math.Max(0, state.FileCompletionCelebrationMinutesRemaining - elapsedInGameMinutes);
        if (state.FileCompletionCelebrationMinutesRemaining <= BoundaryEpsilon)
        {
            state.RecentCompletedFileName = null;
        }

        if (state.ActiveFoodDelivery is not null)
        {
            state.ActiveFoodDelivery.RemainingInGameMinutes -= elapsedInGameMinutes;
        }

        if (state.ActiveCatInterruption is not null)
        {
            state.ActiveCatInterruption.RemainingInGameMinutes -= elapsedInGameMinutes;
            state.ActiveCatInterruption.MinutesUntilNextTypingBurst -= elapsedInGameMinutes;

            var burstInterval = Math.Max(BoundaryEpsilon, Config.CatTypingBurstIntervalMinutes);
            while (state.ActiveCatInterruption.MinutesUntilNextTypingBurst <= BoundaryEpsilon &&
                   state.ActiveCatInterruption.RemainingInGameMinutes > BoundaryEpsilon)
            {
                state.ActiveCatInterruption.MinutesUntilNextTypingBurst += burstInterval;
                state.ActiveCatInterruption.PhantomBugCount += Config.CatBugLinesPerBurst;
                state.ActiveCatInterruption.GibberishLinesTyped += Config.CatGibberishLinesPerBurst;
                state.ActiveCatInterruption.TotalQualityLoss += Config.CatBugQualityLossPerBurst;
                state.CodeQuality = Clamp(
                    state.CodeQuality - Config.CatBugQualityLossPerBurst,
                    0,
                    Config.MaxCodeQuality);
            }
        }

        if (state.ActiveTechDebtBug is not null)
        {
            state.ActiveTechDebtBug.RemainingInGameMinutes -= elapsedInGameMinutes;
        }

        if (state.ActiveJobListing is not null)
        {
            state.ActiveJobListing.RemainingInGameMinutes -= elapsedInGameMinutes;
        }
    }

    private void ResolveExpiredIncidents(RunState state)
    {
        ResolveFoodDelivery(state);

        if (state.ActiveCatInterruption is not null &&
            state.ActiveCatInterruption.RemainingInGameMinutes <= BoundaryEpsilon)
        {
            var distraction = state.ActiveCatInterruption;
            var deletedLines = LoseUncommittedProgress(state);
            state.ActiveCatInterruption = null;
            var chaosSummary = distraction.PhantomBugCount > 0 || distraction.GibberishLinesTyped > 0
                ? $" It also left {distraction.PhantomBugCount} phantom bug burst{(distraction.PhantomBugCount == 1 ? string.Empty : "s")} and {distraction.GibberishLinesTyped} gibberish line{(distraction.GibberishLinesTyped == 1 ? string.Empty : "s")} worth of chaos."
                : string.Empty;
            AppendLog(
                state,
                deletedLines > 0
                    ? $"{distraction.Title} times out and wipes {deletedLines} uncommitted LoC back to the last commit.{chaosSummary}"
                    : $"{distraction.Title} times out, but the last commit keeps the actual work intact.{chaosSummary}");
        }

        if (state.ActiveTechDebtBug is not null &&
            state.ActiveTechDebtBug.RemainingInGameMinutes <= BoundaryEpsilon)
        {
            state.ActiveTechDebtBug = null;
            AppendLog(state, "The bug calcified into tech debt. The code survives, but trust in it dropped.");
        }

        if (state.ActiveJobListing is not null &&
            state.ActiveJobListing.RemainingInGameMinutes <= BoundaryEpsilon)
        {
            var title = state.ActiveJobListing.Title;
            state.ActiveJobListing = null;
            AppendLog(state, $"The {title} listing expired before you could tailor the resume.");
        }

        if (state.DeepWorkMinutesRemaining <= BoundaryEpsilon && state.DeepWorkMinutesRemaining > 0)
        {
            state.DeepWorkMinutesRemaining = 0;
        }

        if (state.ContextSwitchMinutesRemaining <= BoundaryEpsilon && state.ContextSwitchMinutesRemaining > 0)
        {
            state.ContextSwitchMinutesRemaining = 0;
        }
    }

    private double GetStepMinutes(RunState state, double remainingMinutes, bool isSleeping)
    {
        var step = remainingMinutes;

        var minutesUntilMidnight = SimulationConfig.MinutesPerDay - state.TimeOfDayMinutes;
        if (minutesUntilMidnight > BoundaryEpsilon)
        {
            step = Math.Min(step, minutesUntilMidnight);
        }

        step = Math.Min(step, GetPositiveTimer(state.SluggishMinutesRemaining));
        step = Math.Min(step, GetPositiveTimer(state.DeepWorkMinutesRemaining));
        step = Math.Min(step, GetPositiveTimer(state.ContextSwitchMinutesRemaining));
        step = Math.Min(step, GetPositiveTimer(state.ActiveFoodDelivery?.RemainingInGameMinutes));
        step = Math.Min(step, GetPositiveTimer(state.ActiveCatInterruption?.RemainingInGameMinutes));
        step = Math.Min(step, GetPositiveTimer(state.ActiveCatInterruption?.MinutesUntilNextTypingBurst));
        step = Math.Min(step, GetPositiveTimer(state.ActiveTechDebtBug?.RemainingInGameMinutes));
        step = Math.Min(step, GetPositiveTimer(state.ActiveJobListing?.RemainingInGameMinutes));
        step = Math.Min(
            step,
            GetMinutesUntilNextThreshold(
                state.MinutesSinceLastMeal,
                Config.HungryAfterMinutes,
                Config.VeryHungryAfterMinutes,
                Config.StarvingAfterMinutes));

        if (!isSleeping)
        {
            step = Math.Min(
                step,
                GetMinutesUntilNextThreshold(
                    state.MinutesSinceLastSleep,
                    Config.SleepWarningAfterMinutes,
                    Config.SleepDeprivationAfterMinutes,
                    Config.SevereSleepDeprivationAfterMinutes,
                    Config.SleepForcedAfterMinutes));
        }

        return Math.Max(step, BoundaryEpsilon);
    }

    private double GetMinutesUntilNextThreshold(double currentMinutes, params double[] thresholds)
    {
        foreach (var threshold in thresholds)
        {
            if (threshold > currentMinutes + BoundaryEpsilon)
            {
                return threshold - currentMinutes;
            }
        }

        return double.PositiveInfinity;
    }

    private static double GetPositiveTimer(double remainingMinutes)
    {
        if (remainingMinutes <= BoundaryEpsilon)
        {
            return double.PositiveInfinity;
        }

        return remainingMinutes;
    }

    private static double GetPositiveTimer(double? remainingMinutes)
    {
        if (remainingMinutes is null || remainingMinutes <= BoundaryEpsilon)
        {
            return double.PositiveInfinity;
        }

        return remainingMinutes.Value;
    }

    private static string FormatMinutesForLog(double totalMinutes)
    {
        var minutes = Math.Max(1, (int)Math.Ceiling(totalMinutes));
        var hours = minutes / 60;
        var remainingMinutes = minutes % 60;
        return hours > 0
            ? $"{hours}h {remainingMinutes:00}m"
            : $"{minutes}m";
    }

    private void AppendLog(RunState state, string message)
    {
        state.EventLog.Add($"Day {state.Day} {state.ClockText} | {message}");

        if (state.EventLog.Count > Config.MaxEventLogEntries)
        {
            state.EventLog.RemoveAt(0);
        }
    }

    private static double Clamp(double value, double min, double max)
    {
        return Math.Clamp(value, min, max);
    }

    private static void Shuffle<T>(IList<T> values, int seed)
    {
        var random = new Random(seed);
        for (var index = values.Count - 1; index > 0; index--)
        {
            var swapIndex = random.Next(index + 1);
            (values[index], values[swapIndex]) = (values[swapIndex], values[index]);
        }
    }

    private bool IsActionLockedBySleep(RunState state, PlayerAction action)
    {
        return RequiresSleep(state) &&
               action is PlayerAction.WriteCode
                   or PlayerAction.Freelance
                   or PlayerAction.SquashBug
                   or PlayerAction.ApplyForJob
                   or PlayerAction.PublishApp;
    }

    private bool CanBeginJobApplication(RunState state, ActiveJobListing listing)
    {
        return state.ActiveJobApplication is null &&
               state.LinesOfCode >= listing.ResumeCostLines &&
               state.LinesOfCode >= listing.MinimumPortfolioLines &&
               state.CodeQuality >= listing.MinimumCodeQuality &&
               GetResumeProof(state, listing.ResumeTrack) >= listing.RequiredResumeProof;
    }

    private bool CanPublishCurrentApp(RunState state)
    {
        return PortfolioWorkspace.HasFiniteProgramCount(state) &&
               state.CurrentPortfolioLinesOfCode > 0 &&
               PortfolioWorkspace.IsCurrentBatchComplete(state) &&
               state.VersionControl.PendingChangeLines == 0;
    }

    private void PublishCurrentApp(RunState state)
    {
        var releaseNumber = state.PublishedAppCount + 1;
        var projectAnchor = state.CurrentProjectBlueprint.Title;
        var publishedName = ProceduralRunContent.GetPublishedAppName(state.RunSeed, releaseNumber, projectAnchor);
        var fundsGained = ApplyRouteIncomeModifier(
            state,
            RollBoundedAmount(
            state.RunSeed,
            $"publish:{releaseNumber}",
            Config.PublishAppFundsMin,
            Config.PublishAppFundsMax),
            incomeType: "publish");

        state.PublishedAppCount = releaseNumber;
        state.LastPublishedAppName = publishedName;
        state.Funds += fundsGained;
        state.CurrentPortfolioLinesOfCode = 0;
        state.CurrentProgramIndex = 0;
        state.CurrentProgramVisibleLineCount = 0;
        state.RecentCompletedFileName = null;
        state.FileCompletionCelebrationMinutesRemaining = 0;
        state.VersionControl.CommittedPortfolioLinesOfCode = 0;
        state.VersionControl.PendingChangeLines = 0;
        state.VersionControl.PendingCompletedFileCount = 0;

        ScheduleNextPublishedSale(state, state.PublishedAppSaleCount + 1, state.DeskMinutesElapsed);

        AppendLog(state, $"{publishedName} shipped. Storefront cash lands for +${fundsGained:0}.");
        AppendLog(state, $"Build plan: {state.CurrentProjectBlueprint.Pitch}");
        RefreshKnownContactsFromProgress(state);
        AppendLog(state, $"A fresh snippet batch is live. Opened {PortfolioWorkspace.GetCurrentProgram(state).FileName}.");
    }

    private void ScheduleNextPublishedSale(RunState state, int saleNumber, double startDeskMinute)
    {
        if (state.PublishedAppCount <= 0)
        {
            state.NextPublishedAppSaleDeskMinute = double.PositiveInfinity;
            return;
        }

        state.NextPublishedAppSaleDeskMinute = startDeskMinute + RollBoundedAmount(
            state.RunSeed,
            $"published-sale-interval:{saleNumber}",
            Config.PublishedAppSaleIntervalMinMinutes,
            Config.PublishedAppSaleIntervalMaxMinutes);
    }

    private static TechDebtSnippetTemplate PickTechDebtSnippetTemplate(int runSeed, string key)
    {
        var seed = CreateSeed(runSeed, key);
        return TechDebtSnippetTemplates[seed % TechDebtSnippetTemplates.Length];
    }

    private static TechDebtSnippetTemplate CreateReplaceTechDebtSnippet(
        string id,
        string issueLabel,
        string compilerHint,
        int highlightLineIndex,
        string highlightToken,
        params string[] codeLines)
    {
        var highlightStartIndex = codeLines[highlightLineIndex].IndexOf(highlightToken, StringComparison.Ordinal);
        return new TechDebtSnippetTemplate(
            id,
            issueLabel,
            compilerHint,
            codeLines,
            highlightLineIndex,
            Math.Max(0, highlightStartIndex),
            highlightToken.Length,
            highlightToken,
            HighlightIsInsertion: false);
    }

    private static TechDebtSnippetTemplate CreateInsertionTechDebtSnippet(
        string id,
        string issueLabel,
        string compilerHint,
        int highlightLineIndex,
        string highlightToken,
        params string[] codeLines)
    {
        return new TechDebtSnippetTemplate(
            id,
            issueLabel,
            compilerHint,
            codeLines,
            highlightLineIndex,
            codeLines[highlightLineIndex].Length,
            HighlightLength: 0,
            highlightToken,
            HighlightIsInsertion: true);
    }

    private static double RollBoundedAmount(int runSeed, string key, double min, double max)
    {
        if (max <= min)
        {
            return Math.Round(min, 0);
        }

        var random = new Random(CreateSeed(runSeed, key));
        var value = min + (random.NextDouble() * (max - min));
        return Math.Round(value, 0);
    }

    private static int CreateSeed(int runSeed, string key)
    {
        unchecked
        {
            var seed = runSeed == 0 ? 17 : runSeed;
            foreach (var character in key)
            {
                seed = (seed * 31) + character;
            }

            return seed & int.MaxValue;
        }
    }

    private static int ParseTrailingNumber(string value, string prefix)
    {
        if (!value.StartsWith(prefix, StringComparison.Ordinal))
        {
            return 0;
        }

        return int.TryParse(value[prefix.Length..], out var parsed)
            ? parsed
            : 0;
    }

    private void AwardInterviewPrep(RunState state, int points, string reason)
    {
        if (points <= 0 || state.ActiveJobApplication is null)
        {
            return;
        }

        var application = state.ActiveJobApplication;
        var prepCap = Math.Max(1, Config.InterviewPrepPointsPerBonus * Math.Max(1, Config.MaxInterviewPrepBonus));
        var previous = application.PrepPoints;
        application.PrepPoints = Math.Min(prepCap, application.PrepPoints + points);
        if (application.PrepPoints > previous)
        {
            var prepGained = application.PrepPoints - previous;
            AppendLog(state, $"Interview prep +{prepGained}: {reason}");

            if (!application.TakeHomeComplete)
            {
                var revealedLines = RevealApplicationLines(application, prepGained);
                if (revealedLines > 0)
                {
                    AppendLog(state, $"Prep notes surfaced {revealedLines} take-home line{(revealedLines == 1 ? string.Empty : "s")} for {application.ListingTitle}.");
                }
            }
        }
    }

    private void AwardResumeProof(RunState state, ResumeTrack track, int points, string reason)
    {
        if (points <= 0)
        {
            return;
        }

        var previous = GetResumeProof(state, track);
        var updated = Math.Min(previous + points, 6);
        switch (track)
        {
            case ResumeTrack.UI:
                state.UiResumeProof = updated;
                break;
            case ResumeTrack.Tooling:
                state.ToolingResumeProof = updated;
                break;
            default:
                state.GameplayResumeProof = updated;
                break;
        }

        if (updated > previous)
        {
            AppendLog(state, $"{GetResumeTrackLabel(track)} resume proof +{updated - previous}: {reason}");
        }
    }

    private ResumeTrack GetMentorResumeTrack(RunState state)
    {
        if (state.ActiveJobListing is not null)
        {
            return state.ActiveJobListing.ResumeTrack;
        }

        if (state.ActiveJobApplication is not null)
        {
            return state.ActiveJobApplication.ResumeTrack;
        }

        var gameplay = state.GameplayResumeProof;
        var ui = state.UiResumeProof;
        var tooling = state.ToolingResumeProof;
        if (ui <= gameplay && ui <= tooling)
        {
            return ResumeTrack.UI;
        }

        return tooling <= gameplay
            ? ResumeTrack.Tooling
            : ResumeTrack.Gameplay;
    }

    private static ResumeTrack GetResumeTrackForGig(FreelanceGigType type)
    {
        return type switch
        {
            FreelanceGigType.UIPolishPass => ResumeTrack.UI,
            FreelanceGigType.DataMigration => ResumeTrack.Tooling,
            FreelanceGigType.PipelineRescue => ResumeTrack.Tooling,
            _ => ResumeTrack.Gameplay,
        };
    }

    private ResumeTrack GetResumeTrackForProgram(PortfolioProgramDefinition program)
    {
        var content = $"{program.ProjectName} {program.FileName} {program.Description}".ToLowerInvariant();
        if (content.Contains("ui", StringComparison.Ordinal) ||
            content.Contains("dialogue", StringComparison.Ordinal) ||
            content.Contains("layout", StringComparison.Ordinal) ||
            content.Contains("palette", StringComparison.Ordinal) ||
            content.Contains("screen", StringComparison.Ordinal) ||
            content.Contains("render", StringComparison.Ordinal))
        {
            return ResumeTrack.UI;
        }

        if (content.Contains("tool", StringComparison.Ordinal) ||
            content.Contains("task", StringComparison.Ordinal) ||
            content.Contains("queue", StringComparison.Ordinal) ||
            content.Contains("save", StringComparison.Ordinal) ||
            content.Contains("pipeline", StringComparison.Ordinal) ||
            content.Contains("index", StringComparison.Ordinal) ||
            content.Contains("cache", StringComparison.Ordinal))
        {
            return ResumeTrack.Tooling;
        }

        return ResumeTrack.Gameplay;
    }

    private static ResumeTrack GetResumeTrackForTechStack(string techStack)
    {
        if (techStack.Contains("UI", StringComparison.OrdinalIgnoreCase))
        {
            return ResumeTrack.UI;
        }

        if (techStack.Contains("Tool", StringComparison.OrdinalIgnoreCase) ||
            techStack.Contains("Build", StringComparison.OrdinalIgnoreCase))
        {
            return ResumeTrack.Tooling;
        }

        return ResumeTrack.Gameplay;
    }
}
