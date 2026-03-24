using MicroDev.Core.Portfolio;

namespace MicroDev.Core.Simulation;

public sealed class SimulationEngine
{
    private const double BoundaryEpsilon = 0.0001;
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
    private static readonly string[] MatchNames =
    [
        "Alex",
        "Jordan",
        "Sam",
        "Morgan",
        "Taylor",
        "Riley",
        "Casey",
        "Avery",
    ];
    private static readonly string[] ShowTitles =
    [
        "Midnight Refactor",
        "Space Diner",
        "Patch Notes",
        "Cozy Detectives",
        "Neon Borough",
        "After Hours Arcade",
    ];

    public SimulationEngine(SimulationConfig config)
    {
        Config = config;
    }

    public SimulationConfig Config { get; }

    public RunState CreateNewRun()
    {
        var state = new RunState
        {
            Difficulty = Config.Difficulty,
            RunSeed = Random.Shared.Next(1, int.MaxValue),
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
            SuccessfulApplications = 0,
            CurrentProgramIndex = 0,
            CurrentProgramVisibleLineCount = 0,
            PublishedAppCount = 0,
            PublishedAppSaleCount = 0,
            NextPublishedAppSaleDeskMinute = double.PositiveInfinity,
            LastPublishedAppName = null,
            RecentCompletedFileName = null,
            FileCompletionCelebrationMinutesRemaining = 0,
            ActiveJobApplication = null,
            Status = RunStatus.InProgress,
        };

        if (state.CurrentPortfolioLinesOfCode > 0)
        {
            PortfolioWorkspace.SynchronizeToLinesOfCode(state);
        }

        AppendLog(state, "Another week begins. Rent hits at midnight and recruiters are watching.");
        AppendLog(state, $"Opened {PortfolioWorkspace.GetCurrentProgram(state).FileName} in a blank editor.");
        if (state.HasFirstCoin)
        {
            AppendLog(state, "The first coin still hangs on the desk, quietly steadying your nerves.");
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
            PlayerAction.WriteCode => state.Focus > 0 && state.ActiveCatInterruption is null,
            PlayerAction.Eat => CanPlaceFoodOrder(state, FoodChoice.Burger),
            PlayerAction.Freelance => true,
            PlayerAction.Sleep => true,
            PlayerAction.PetCat => state.ActiveCatInterruption is not null,
            PlayerAction.SquashBug => state.ActiveTechDebtBug is not null && state.Focus >= Config.SquashBugFocusCost,
            PlayerAction.ApplyForJob => state.ActiveJobListing is not null && CanBeginJobApplication(state, state.ActiveJobListing),
            PlayerAction.PublishApp => CanPublishCurrentApp(state),
            _ => false,
        };
    }

    public bool CanPlaceFoodOrder(RunState state, FoodChoice choice, bool expeditedDelivery = false)
    {
        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               state.ActiveFoodDelivery is null &&
               state.Funds >= GetFoodTotalCost(choice, expeditedDelivery);
    }

    public double GetFoodTotalCost(FoodChoice choice, bool expeditedDelivery = false)
    {
        return GetFoodCost(choice) + GetFoodTipAmount(expeditedDelivery);
    }

    public double GetFoodTipAmount(bool expeditedDelivery)
    {
        return expeditedDelivery
            ? Config.ExpeditedFoodDeliveryTipAmount
            : 0;
    }

    public double GetFoodDeliveryDuration(bool expeditedDelivery)
    {
        return expeditedDelivery
            ? Config.ExpeditedFoodDeliveryDurationMinutes
            : Config.FoodDeliveryDurationMinutes;
    }

    public FoodOptionDefinition GetFoodOption(FoodChoice choice)
    {
        return choice switch
        {
            FoodChoice.Burrito => new FoodOptionDefinition(
                choice,
                "Burrito",
                "Cheap, filling, and steadier than the greasiest options if you bother to note the order carefully.",
                Config.BurritoFundsCost,
                Config.BurritoFocusGain,
                Config.BurritoSanityGain,
                Config.BurritoSluggishDurationMinutes),
            FoodChoice.Pizza => new FoodOptionDefinition(
                choice,
                "Pizza",
                "Big focus spike, but the post-meal drag can absolutely flatten the rest of the coding block.",
                Config.PizzaFundsCost,
                Config.PizzaFocusGain,
                Config.PizzaSanityGain,
                Config.PizzaSluggishDurationMinutes),
            FoodChoice.Dumplings => new FoodOptionDefinition(
                choice,
                "Dumplings",
                "Comfort food that helps sanity more than raw typing energy. Better for stabilizing a rough day.",
                Config.DumplingsFundsCost,
                Config.DumplingsFocusGain,
                Config.DumplingsSanityGain,
                Config.DumplingsSluggishDurationMinutes),
            _ => new FoodOptionDefinition(
                choice,
                "Burger",
                "Reliable focus recovery at a fair price, but a sloppy order can still turn the next session sluggish.",
                Config.BurgerFundsCost,
                Config.BurgerFocusGain,
                Config.BurgerSanityGain,
                Config.BurgerSluggishDurationMinutes),
        };
    }

    public IReadOnlyList<FoodOrderModifierOption> GetFoodOrderModifiers(FoodChoice choice)
    {
        return choice switch
        {
            FoodChoice.Burrito =>
            [
                new(FoodOrderModifier.NoCheese, "No Cheese", "Avoids the heavy lunch spiral that wrecks the next coding block.", true),
                new(FoodOrderModifier.SauceOnSide, "Sauce On Side", "Keeps the burrito from turning into a total desk disaster.", true),
                new(FoodOrderModifier.SkipSoda, "Skip Soda", "Cuts the sugar crash, but this one is optional.", false),
            ],
            FoodChoice.Pizza =>
            [
                new(FoodOrderModifier.NoCheese, "Light Cheese", "Tones down the greasy post-slice slowdown.", true),
                new(FoodOrderModifier.SauceOnSide, "Hold Ranch", "Skip the extra heaviness unless you really need the comfort.", false),
                new(FoodOrderModifier.SkipSoda, "Skip Soda", "Avoids stacking a sugar crash on top of the pizza drag.", true),
            ],
            FoodChoice.Dumplings =>
            [
                new(FoodOrderModifier.NoCheese, "No Extra Chili", "Keeps the comfort food from becoming a focus tax.", true),
                new(FoodOrderModifier.SauceOnSide, "Sauce On Side", "Lets you control how messy the whole break becomes.", true),
                new(FoodOrderModifier.SkipSoda, "Skip Soda", "Optional. Helpful, but not the core issue here.", false),
            ],
            _ =>
            [
                new(FoodOrderModifier.NoCheese, "No Cheese", "The classic burger mistake. Leaving it on slows the whole desk down.", true),
                new(FoodOrderModifier.SauceOnSide, "Sauce On Side", "Optional, but it keeps the keyboard and your brain cleaner.", false),
                new(FoodOrderModifier.SkipSoda, "Skip Soda", "Avoids the greasy-food sugar crash combo.", true),
            ],
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
        return type switch
        {
            FreelanceGigType.UIPolishPass => new FreelanceGigDefinition(
                type,
                "UI Polish Pass",
                "Client wants last-minute screen cleanup. Middling pay, moderate drain, no extra portfolio growth.",
                Config.UiPolishDurationMinutes,
                Config.UiPolishFundsGain,
                Config.UiPolishFocusCost,
                Config.UiPolishSanityCost,
                Config.UiPolishQualityGain),
            FreelanceGigType.PipelineRescue => new FreelanceGigDefinition(
                type,
                "Pipeline Rescue",
                "A painful build-system emergency. Great money, heavy focus hit, but it sharpens your engineering discipline.",
                Config.PipelineRescueDurationMinutes,
                Config.PipelineRescueFundsGain,
                Config.PipelineRescueFocusCost,
                Config.PipelineRescueSanityCost,
                Config.PipelineRescueQualityGain),
            _ => new FreelanceGigDefinition(
                type,
                "Quick Bugfix",
                "A short contract patch. Lightest time cost and a small code-quality boost from careful debugging.",
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
               !RequiresSleep(state);
    }

    public bool TakeFreelanceGig(RunState state, FreelanceGigType type)
    {
        if (!CanTakeFreelanceGig(state, type))
        {
            if (RequiresSleep(state))
            {
                AppendLog(state, "No freelance heroics on two days without sleep. Crash first, then take the gig.");
            }

            return false;
        }

        var gig = GetFreelanceGig(type);
        AdvanceTime(state, gig.DurationMinutes);
        if (state.Status != RunStatus.InProgress)
        {
            return false;
        }

        state.Funds += gig.FundsGain;
        state.Focus = Clamp(state.Focus - gig.FocusCost, 0, Config.MaxFocus);
        state.Sanity = Clamp(state.Sanity - gig.SanityCost, 0, Config.MaxSanity);
        state.CodeQuality = Clamp(state.CodeQuality + gig.CodeQualityGain, 0, Config.MaxCodeQuality);

        AppendLog(
            state,
            $"{gig.Name}: +${gig.FundsGain:0}, -{gig.FocusCost:0} focus, -{gig.SanityCost:0} sanity, +{gig.CodeQualityGain:0.#} quality.");
        AwardResumeProof(state, GetResumeTrackForGig(type), 1, $"{gig.Name} gave the resume a stronger {GetResumeTrackLabel(GetResumeTrackForGig(type)).ToLowerInvariant()} story.");
        EvaluateLossState(state);
        return true;
    }

    public bool CanPurchaseUpgrade(RunState state, EfficiencyUpgradeType type)
    {
        if (state.Status != RunStatus.InProgress || state.PurchasedUpgrades.Contains(type))
        {
            return false;
        }

        return !state.FirstCoinDecisionPending &&
               state.PendingLifeEvent is null &&
               state.Funds >= EfficiencyUpgradeCatalog.Get(type).FundsCost;
    }

    public bool PurchaseUpgrade(RunState state, EfficiencyUpgradeType type)
    {
        if (state.Status != RunStatus.InProgress)
        {
            return false;
        }

        var definition = EfficiencyUpgradeCatalog.Get(type);
        if (state.PurchasedUpgrades.Contains(type))
        {
            AppendLog(state, $"{definition.Name} is already installed.");
            return false;
        }

        if (state.Funds < definition.FundsCost)
        {
            AppendLog(state, $"Not enough funds for {definition.Name}.");
            return false;
        }

        state.Funds -= definition.FundsCost;
        state.PurchasedUpgrades.Add(type);
        AppendLog(state, $"Installed {definition.Name}: {definition.SummaryEffect}");
        AwardInterviewPrep(state, 1, $"{definition.Name} tightened the interview plan.");
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
                1 => state.Funds >= Config.OnlineDateFundsCost,
                2 => true,
                _ => false,
            },
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
            }

            ApplyPassiveSanityRegeneration(state, step);
            ApplyTechDebtDecay(state, step);
            ApplyNeedPenalties(state, step, hungerStage, sleepStage);
            AdvanceTimedEffects(state, step);
            AdvanceNeedTimers(state, step, isSleeping);

            state.TimeOfDayMinutes += step;
            remainingMinutes -= step;
            AppendNeedThresholdLogs(state, hungerStage, sleepStage, isSleeping);

            if (state.TimeOfDayMinutes >= SimulationConfig.MinutesPerDay - BoundaryEpsilon)
            {
                state.TimeOfDayMinutes = 0;
                state.Day += 1;
                state.Funds -= Config.DailyBillAmount;
                AppendLog(state, $"Paid bills: -${Config.DailyBillAmount:0}. Funds now ${state.Funds:0}.");

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

        var option = GetFoodOption(choice);
        var tipAmount = GetFoodTipAmount(expeditedDelivery);
        var totalCost = GetFoodTotalCost(choice, expeditedDelivery);
        var sluggishPenalty = GetFoodOrderPenaltyMinutes(choice, selectedModifiers, reviewReceipt);
        state.Funds -= totalCost;
        state.ActiveFoodDelivery = new ActiveFoodDelivery
        {
            Choice = choice,
            Expedited = expeditedDelivery,
            ReviewReceipt = reviewReceipt,
            TipAmount = tipAmount,
            TotalFundsCost = totalCost,
            RemainingInGameMinutes = GetFoodDeliveryDuration(expeditedDelivery),
        };

        foreach (var modifier in selectedModifiers.Where(static modifier => modifier != FoodOrderModifier.None))
        {
            state.ActiveFoodDelivery.SelectedModifiers.Add(modifier);
        }

        var deliveryLine = expeditedDelivery
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
            state.ResetFrom(CreateNewRun());
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
                    AppendLog(state, $"{writeResult.CompletedFileName} is ready for commit.");
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

                EvaluateLossState(state);
                return true;

            case PlayerAction.Eat:
                return PlaceFoodOrder(state, FoodChoice.Burger, doubleCheckOrder: true);

            case PlayerAction.Freelance:
                return TakeFreelanceGig(state, FreelanceGigType.QuickBugfix);

            case PlayerAction.Sleep:
                var previousSleepStage = GetSleepStage(state);
                state.MinutesSinceLastSleep = 0;
                AdvanceTime(state, Config.SleepDurationMinutes, isSleeping: true);
                if (state.Status != RunStatus.InProgress)
                {
                    return false;
                }

                state.Focus = Clamp(state.Focus + Config.SleepFocusGain, 0, Config.MaxFocus);
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
                if (state.ActiveCatInterruption.PatsRemaining <= 0)
                {
                    var cat = state.ActiveCatInterruption;
                    state.ActiveCatInterruption = null;
                    var chaosSummary = cat.PhantomBugCount > 0 || cat.GibberishLinesTyped > 0
                        ? $" It already slipped in {cat.PhantomBugCount} phantom bug burst{(cat.PhantomBugCount == 1 ? string.Empty : "s")} and {cat.GibberishLinesTyped} gibberish line{(cat.GibberishLinesTyped == 1 ? string.Empty : "s")}."
                        : string.Empty;
                    AppendLog(state, $"The cat finally settles somewhere that is not your keyboard.{chaosSummary}");
                }

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

                state.Focus = Clamp(state.Focus - Config.SquashBugFocusCost, 0, Config.MaxFocus);
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
        var matchName = lifeEvent.SubjectName ?? "someone unexpectedly steady";
        switch (optionIndex)
        {
            case 0:
                AdvanceTime(state, Config.OnlineMatchMessageDurationMinutes);
                if (state.Status != RunStatus.InProgress)
                {
                    return false;
                }

                state.Sanity = Clamp(state.Sanity + Config.OnlineMatchMessageSanityGain, 0, Config.MaxSanity);
                state.Focus = Clamp(state.Focus - Config.OnlineMatchMessageFocusLoss, 0, Config.MaxFocus);
                state.RelationshipProgress += Config.OnlineMatchMessageRelationshipGain;
                AppendLog(
                    state,
                    $"You sent {matchName} a real message instead of another throwaway opener. {FormatMinutesForLog(Config.OnlineMatchMessageDurationMinutes)} passed, sanity +{Config.OnlineMatchMessageSanityGain:0}, focus -{Config.OnlineMatchMessageFocusLoss:0}.");
                break;

            case 1:
                state.Funds -= Config.OnlineDateFundsCost;
                AdvanceTime(state, Config.OnlineDateDurationMinutes);
                if (state.Status != RunStatus.InProgress)
                {
                    return false;
                }

                state.Sanity = Clamp(state.Sanity + Config.OnlineDateSanityGain, 0, Config.MaxSanity);
                state.Focus = Clamp(state.Focus - Config.OnlineDateFocusLoss, 0, Config.MaxFocus);
                state.RelationshipProgress += Config.OnlineDateRelationshipGain;
                AppendLog(
                    state,
                    $"You actually went out with {matchName}. -${Config.OnlineDateFundsCost:0}, {FormatMinutesForLog(Config.OnlineDateDurationMinutes)} gone, sanity +{Config.OnlineDateSanityGain:0}, focus -{Config.OnlineDateFocusLoss:0}.");
                break;

            case 2:
                state.Sanity = Clamp(state.Sanity - Config.OnlineMatchIgnoreSanityLoss, 0, Config.MaxSanity);
                AppendLog(state, $"You let the match with {matchName} die in the queue. Sanity -{Config.OnlineMatchIgnoreSanityLoss:0}.");
                break;

            default:
                return false;
        }

        if (!state.HasFoundLove &&
            state.RelationshipProgress >= Config.RelationshipProgressNeededForLove)
        {
            state.HasFoundLove = true;
            state.PartnerName = matchName;
            state.Sanity = Clamp(state.Sanity + 8, 0, Config.MaxSanity);
            AppendLog(state, $"Somewhere between the backlog and the late-night messages, you found something real with {matchName}. Passive sanity support is now part of the run.");
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

        var option = GetFoodOption(delivery.Choice);
        var previousHungerStage = GetHungerStage(state);
        state.Focus = Clamp(state.Focus + option.FocusGain, 0, Config.MaxFocus);
        state.Sanity = Clamp(state.Sanity + option.SanityGain, 0, Config.MaxSanity);
        state.MinutesSinceLastMeal = 0;

        var sluggishPenalty = GetFoodOrderPenaltyMinutes(delivery.Choice, delivery.SelectedModifiers, delivery.ReviewReceipt);
        var hungerSuffix = previousHungerStage > 0
            ? " Hunger finally stops chewing through the day."
            : string.Empty;
        var arrivalLead = delivery.Expedited
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
            $"Tailored the resume for {listing.Title}. The take-home task and mock interview are live.");
    }

    private void ResolveJobApplicationOutcome(RunState state)
    {
        var application = state.ActiveJobApplication!;
        var hasInterview = application.CorrectAnswers >= application.MinimumCorrectAnswers;
        state.ActiveJobApplication = null;

        if (hasInterview)
        {
            state.SuccessfulApplications += 1;

            if (Config.ContinueAfterSuccessfulApplication)
            {
                state.Funds += Config.SuccessfulApplicationFundsReward;
                state.Sanity = Clamp(state.Sanity + Config.SuccessfulApplicationSanityReward, 0, Config.MaxSanity);
                AppendLog(
                    state,
                    $"Offer landed: {application.ListingTitle}. {GetContinuationModeLabel()} keeps rolling with +${Config.SuccessfulApplicationFundsReward:0} and +{Config.SuccessfulApplicationSanityReward:0} sanity.");
                return;
            }

            state.Status = RunStatus.Won;
            state.OutcomeMessage = $"You cleared the take-home, survived the interview, and landed {application.ListingTitle}.";
            AppendLog(
                state,
                $"Application accepted after the interview: {application.ListingTitle}. Every interview answer landed.");
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
            ResumeLinesSpent = listing.ResumeCostLines,
            PortfolioLinesSnapshot = state.LinesOfCode + listing.ResumeCostLines,
            CodeQualitySnapshot = state.CodeQuality,
            MinimumPortfolioLines = listing.MinimumPortfolioLines,
            MinimumCodeQuality = listing.MinimumCodeQuality,
            ResumeTrack = listing.ResumeTrack,
            ResumeProofSnapshot = resumeProof,
            PrepPoints = state.PurchasedUpgrades.Sum(type => EfficiencyUpgradeCatalog.Get(type).PrepPointsOnApplicationStart) + Math.Min(2, excessResumeProof),
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
                    state.ActiveCatInterruption = new ActiveCatInterruption
                    {
                        PatsRemaining = Config.CatPatsRequired,
                        RemainingInGameMinutes = Config.CatStayDurationMinutes,
                        LinesDeletionPenalty = Config.CatLinesDeletionPenalty,
                        MinutesUntilNextTypingBurst = Config.CatTypingBurstIntervalMinutes,
                        VisualSeed = CreateSeed(state.RunSeed, $"{incident.Id}:cat"),
                    };
                    AppendLog(state, incident.Description);
                    AppendLog(state, "Paws hit the keys. The cat starts adding phantom bugs and pure screen gibberish until you clear it.");
                }
                break;

            case IncidentType.TechDebtBug:
                if (state.ActiveTechDebtBug is null)
                {
                    state.ActiveTechDebtBug = new ActiveTechDebtBug
                    {
                        Summary = "A fresh bug is quietly wrecking confidence and code quality.",
                        RemainingInGameMinutes = Config.TechDebtDurationMinutes,
                        QualityDrainPerMinute = Config.TechDebtQualityDrainPerMinute,
                    };
                    AppendLog(state, incident.Description);
                }
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
                var saleFunds = RollBoundedAmount(
                    state.RunSeed,
                    $"published-sale:{saleNumber}",
                    Config.PublishedAppSaleFundsMin,
                    Config.PublishedAppSaleFundsMax);
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
                    state.PendingLifeEvent = new PendingLifeEvent
                    {
                        Type = incident.Type,
                        Title = "Computer Freeze",
                        Description = "The cursor is dead, the fans are screaming, and the whole night is now about getting the machine back.",
                    };
                    AppendLog(state, incident.Description);
                }
                break;

            case IncidentType.StreamingBinge:
                if (state.PendingLifeEvent is null)
                {
                    var showTitle = PickLifeFlavor(state.RunSeed, $"{incident.Id}:show", ShowTitles);
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
                    var matchName = PickLifeFlavor(state.RunSeed, $"{incident.Id}:match", MatchNames);
                    var compatibility = 60 + (CreateSeed(state.RunSeed, $"{incident.Id}:score") % 39);
                    state.PendingLifeEvent = new PendingLifeEvent
                    {
                        Type = incident.Type,
                        Title = "New Match",
                        Description = $"{matchName} actually looks promising instead of just another blurry profile and half-baked bio.",
                        SubjectName = matchName,
                        SubjectScore = compatibility,
                    };
                    AppendLog(state, incident.Description);
                }
                break;
        }
    }

    private static string PickLifeFlavor(int runSeed, string key, IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            return string.Empty;
        }

        var index = CreateSeed(runSeed, key) % values.Count;
        return values[index];
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
        var tier = Math.Min(4, listingIndex);
        var resumeTrack = GetResumeTrackForTechStack(profile.TechStack);
        var difficultyResumeOffset = state.Difficulty == GameDifficulty.Hard ? 1 : 0;

        return new ActiveJobListing
        {
            ListingId = incidentId,
            Title = profile.Title,
            TechStack = profile.TechStack,
            RemainingInGameMinutes = Config.JobListingDurationMinutes + (state.Difficulty == GameDifficulty.Easy ? 60 : 0),
            ResumeCostLines = Config.JobResumeCostLines + (tier * 2),
            MinimumPortfolioLines = Math.Max(40, Config.JobMinimumPortfolioLines + difficultyPortfolioOffset + (tier * 10)),
            MinimumCodeQuality = Math.Max(35, Config.JobMinimumCodeQuality + difficultyQualityOffset + (tier * 3)),
            ResumeTrack = resumeTrack,
            RequiredResumeProof = Math.Max(1, 1 + (tier / 2) + difficultyResumeOffset),
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

    private double GetFoodCost(FoodChoice choice)
    {
        return GetFoodOption(choice).FundsCost;
    }

    private double GetFoodFocusGain(FoodChoice choice)
    {
        return GetFoodOption(choice).FocusGain;
    }

    private static string GetFoodLabel(FoodChoice choice)
    {
        return choice switch
        {
            FoodChoice.Burrito => "Burrito",
            FoodChoice.Pizza => "Pizza",
            FoodChoice.Dumplings => "Dumplings",
            _ => "Burger",
        };
    }

    private string GetContinuationModeLabel()
    {
        return Config.EndlessPortfolio
            ? "Endless mode"
            : "Continual upgrade loop";
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
        foreach (var type in state.PurchasedUpgrades)
        {
            lines += EfficiencyUpgradeCatalog.Get(type).BonusLinesPerClick;
        }

        if (IsDeepWorkActive(state))
        {
            lines += Config.DeepWorkBonusLinesPerClick;
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
        foreach (var type in state.PurchasedUpgrades)
        {
            focusCost -= EfficiencyUpgradeCatalog.Get(type).FocusCostReduction;
        }

        if (IsSluggish(state))
        {
            focusCost += Config.SluggishFocusCostPenalty;
        }

        if (IsContextSwitchActive(state))
        {
            focusCost += Config.ContextSwitchFocusCostPenalty;
        }

        return Math.Max(0.5, focusCost);
    }

    private double GetWriteCodeQualityGain(RunState state)
    {
        var qualityGain = Config.WriteCodeQualityGain;
        foreach (var type in state.PurchasedUpgrades)
        {
            qualityGain += EfficiencyUpgradeCatalog.Get(type).BonusQualityGain;
        }

        if (IsDeepWorkActive(state))
        {
            qualityGain += Config.DeepWorkBonusQualityGain;
        }

        if (!IsSluggish(state))
        {
            return qualityGain;
        }

        return Math.Max(0.05, qualityGain * Config.SluggishQualityMultiplier);
    }

    private void ApplyPassiveFocusDrain(RunState state, double elapsedInGameMinutes)
    {
        var passiveDrain = Config.PassiveFocusDrainPerInGameMinute;
        foreach (var type in state.PurchasedUpgrades)
        {
            passiveDrain -= EfficiencyUpgradeCatalog.Get(type).PassiveFocusDrainReduction;
        }

        if (IsContextSwitchActive(state))
        {
            passiveDrain += Config.ContextSwitchPassiveFocusDrainPerInGameMinute;
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

        if (passiveRegenPerMinute <= 0)
        {
            return;
        }

        state.Sanity = Clamp(
            state.Sanity + (elapsedInGameMinutes * passiveRegenPerMinute),
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
            var cat = state.ActiveCatInterruption;
            var deletedLines = Math.Min(state.CurrentPortfolioLinesOfCode, cat.LinesDeletionPenalty);
            state.CurrentPortfolioLinesOfCode -= deletedLines;
            state.LinesOfCode = Math.Max(0, state.LinesOfCode - deletedLines);
            PortfolioWorkspace.SynchronizeToLinesOfCode(state);
            state.ActiveCatInterruption = null;
            var chaosSummary = cat.PhantomBugCount > 0 || cat.GibberishLinesTyped > 0
                ? $" It also left {cat.PhantomBugCount} phantom bug burst{(cat.PhantomBugCount == 1 ? string.Empty : "s")} and {cat.GibberishLinesTyped} gibberish line{(cat.GibberishLinesTyped == 1 ? string.Empty : "s")} worth of chaos."
                : string.Empty;
            AppendLog(state, $"The cat stomped across the keyboard and deleted {deletedLines} draft LoC.{chaosSummary}");
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
               PortfolioWorkspace.IsCurrentBatchComplete(state);
    }

    private void PublishCurrentApp(RunState state)
    {
        var releaseNumber = state.PublishedAppCount + 1;
        var publishedName = $"Release {releaseNumber}";
        var fundsGained = RollBoundedAmount(
            state.RunSeed,
            $"publish:{releaseNumber}",
            Config.PublishAppFundsMin,
            Config.PublishAppFundsMax);

        state.PublishedAppCount = releaseNumber;
        state.LastPublishedAppName = publishedName;
        state.Funds += fundsGained;
        state.CurrentPortfolioLinesOfCode = 0;
        state.CurrentProgramIndex = 0;
        state.CurrentProgramVisibleLineCount = 0;
        state.RecentCompletedFileName = null;
        state.FileCompletionCelebrationMinutesRemaining = 0;

        ScheduleNextPublishedSale(state, state.PublishedAppSaleCount + 1, state.DeskMinutesElapsed);

        AppendLog(state, $"{publishedName} shipped. Storefront cash lands for +${fundsGained:0}.");
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
