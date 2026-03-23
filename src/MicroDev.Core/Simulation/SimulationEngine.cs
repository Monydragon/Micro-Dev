using MicroDev.Core.Portfolio;

namespace MicroDev.Core.Simulation;

public sealed class SimulationEngine
{
    private const double BoundaryEpsilon = 0.0001;

    public SimulationEngine(SimulationConfig config)
    {
        Config = config;
    }

    public SimulationConfig Config { get; }

    public RunState CreateNewRun()
    {
        var state = new RunState
        {
            Day = Config.StartingDay,
            TimeOfDayMinutes = Config.StartingTimeOfDayMinutes,
            DeskMinutesElapsed = 0,
            Funds = Config.StartingFunds,
            Focus = Config.StartingFocus,
            Sanity = Config.StartingSanity,
            HasFirstCoin = Config.StartWithFirstCoin,
            FirstCoinDecisionPending = false,
            FirstCoinRescueDeficit = 0,
            LinesOfCode = Config.StartingLinesOfCode,
            CodeQuality = Config.StartingCodeQuality,
            SluggishMinutesRemaining = 0,
            SuccessfulApplications = 0,
            CurrentProgramIndex = 0,
            CurrentProgramVisibleLineCount = 0,
            RecentCompletedFileName = null,
            FileCompletionCelebrationMinutesRemaining = 0,
            Status = RunStatus.InProgress,
        };

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

        if (state.Status != RunStatus.InProgress || state.FirstCoinDecisionPending)
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
            PlayerAction.ApplyForJob => state.ActiveJobListing is not null && state.LinesOfCode >= state.ActiveJobListing.ResumeCostLines,
            _ => false,
        };
    }

    public bool CanPlaceFoodOrder(RunState state, FoodChoice choice)
    {
        return state.Status == RunStatus.InProgress &&
               !state.FirstCoinDecisionPending &&
               state.Funds >= GetFoodCost(choice);
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
        return state.Status == RunStatus.InProgress && !state.FirstCoinDecisionPending;
    }

    public bool TakeFreelanceGig(RunState state, FreelanceGigType type)
    {
        if (!CanTakeFreelanceGig(state, type))
        {
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
        EvaluateLossState(state);
        return true;
    }

    public bool CanPurchaseUpgrade(RunState state, EfficiencyUpgradeType type)
    {
        if (state.Status != RunStatus.InProgress || state.PurchasedUpgrades.Contains(type))
        {
            return false;
        }

        return !state.FirstCoinDecisionPending && state.Funds >= EfficiencyUpgradeCatalog.Get(type).FundsCost;
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
        return true;
    }

    public bool IsSluggish(RunState state)
    {
        return state.SluggishMinutesRemaining > BoundaryEpsilon;
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

    public void AdvanceRealTime(RunState state, double elapsedRealSeconds)
    {
        if (state.Status != RunStatus.InProgress || state.FirstCoinDecisionPending || elapsedRealSeconds <= 0)
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
    }

    public void AdvanceTime(RunState state, double elapsedInGameMinutes)
    {
        if (state.Status != RunStatus.InProgress || state.FirstCoinDecisionPending || elapsedInGameMinutes <= 0)
        {
            return;
        }

        ResolveExpiredIncidents(state);

        var remainingMinutes = elapsedInGameMinutes;

        while (remainingMinutes > BoundaryEpsilon && state.Status == RunStatus.InProgress)
        {
            var step = GetStepMinutes(state, remainingMinutes);

            ApplyPassiveFocusDrain(state, step);
            ApplyPassiveSanityRegeneration(state, step);
            ApplyTechDebtDecay(state, step);
            AdvanceTimedEffects(state, step);

            state.TimeOfDayMinutes += step;
            remainingMinutes -= step;

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

    public bool PlaceFoodOrder(RunState state, FoodChoice choice, bool doubleCheckOrder)
    {
        if (!CanPlaceFoodOrder(state, choice))
        {
            AppendLog(state, "Not enough funds to place that delivery order.");
            return false;
        }

        var option = GetFoodOption(choice);
        state.Funds -= option.FundsCost;
        state.Focus = Clamp(state.Focus + option.FocusGain, 0, Config.MaxFocus);
        state.Sanity = Clamp(state.Sanity + option.SanityGain, 0, Config.MaxSanity);

        if (doubleCheckOrder)
        {
            AppendLog(state, $"{option.Name} delivered cleanly. You double-checked the order and kept your momentum.");
        }
        else
        {
            state.SluggishMinutesRemaining = Math.Max(state.SluggishMinutesRemaining, option.SluggishMinutesWhenUnchecked);
            AppendLog(state, $"{option.Name} landed messy and left you sluggish for a while.");
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

        if (state.Status != RunStatus.InProgress || state.FirstCoinDecisionPending)
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
                        state.ActiveCatInterruption is not null
                            ? "The cat is on the keyboard. Pet it before you can type."
                            : "Too exhausted to write. Recover focus first.");
                    return false;
                }

                var requestedLinesGain = GetWriteCodeLinesGain(state);
                var qualityGain = GetWriteCodeQualityGain(state);
                var writeResult = PortfolioWorkspace.RevealLines(state, requestedLinesGain);

                if (writeResult.LinesAdded == 0)
                {
                    AppendLog(state, "The portfolio files are fully typed out. Ship what you have.");
                    return false;
                }

                state.LinesOfCode += writeResult.LinesAdded;
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
                AdvanceTime(state, Config.SleepDurationMinutes);
                if (state.Status != RunStatus.InProgress)
                {
                    return false;
                }

                state.Focus = Clamp(state.Focus + Config.SleepFocusGain, 0, Config.MaxFocus);
                state.Sanity = Clamp(state.Sanity + Config.SleepSanityGain, 0, Config.MaxSanity);
                AppendLog(
                    state,
                    $"Slept for 8 hours: +{Config.SleepFocusGain:0} focus, +{Config.SleepSanityGain:0} sanity.");
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
                    state.ActiveCatInterruption = null;
                    AppendLog(state, "The cat finally settles somewhere that is not your keyboard.");
                }

                return true;

            case PlayerAction.SquashBug:
                if (!CanApplyAction(state, action))
                {
                    AppendLog(state, "You need a live bug and enough focus to squash it.");
                    return false;
                }

                state.Focus = Clamp(state.Focus - Config.SquashBugFocusCost, 0, Config.MaxFocus);
                state.CodeQuality = Clamp(state.CodeQuality + 4, 0, Config.MaxCodeQuality);
                state.ActiveTechDebtBug = null;
                AppendLog(state, "Bug squashed. Code quality stabilizes and the panic subsides.");
                EvaluateLossState(state);
                return true;

            case PlayerAction.ApplyForJob:
                if (!CanApplyAction(state, action))
                {
                    AppendLog(state, "You need an active listing and enough LoC to tailor the resume.");
                    return false;
                }

                ResolveJobApplication(state);
                return true;

            default:
                return false;
        }
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

    private void ResolveJobApplication(RunState state)
    {
        var listing = state.ActiveJobListing!;
        var availableLines = state.LinesOfCode;
        var hasPortfolio = availableLines >= listing.MinimumPortfolioLines;
        var hasQuality = state.CodeQuality >= listing.MinimumCodeQuality;

        state.LinesOfCode -= listing.ResumeCostLines;
        PortfolioWorkspace.SynchronizeToLinesOfCode(state);
        state.ActiveJobListing = null;

        if (hasPortfolio && hasQuality)
        {
            state.SuccessfulApplications += 1;
            state.Status = RunStatus.Won;
            state.OutcomeMessage = $"You tailored the resume for {listing.Title} and finally landed the callback.";
            AppendLog(state, $"Application accepted: {listing.Title}.");
            return;
        }

        state.OutcomeMessage = null;
        AppendLog(
            state,
            $"Application rejected: {listing.Title}. Recruiters wanted stronger portfolio proof or cleaner code.");
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
                    };
                    AppendLog(state, incident.Description);
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
                if (state.ActiveJobListing is null)
                {
                    state.ActiveJobListing = CreateJobListing(incident.Id);
                    AppendLog(state, incident.Description);
                }
                break;
        }
    }

    private ActiveJobListing CreateJobListing(string incidentId)
    {
        return incidentId switch
        {
            "job-2" => new ActiveJobListing
            {
                Title = ".NET Tools Engineer",
                TechStack = "C# / .NET / Tooling",
                RemainingInGameMinutes = Config.JobListingDurationMinutes,
                ResumeCostLines = Config.JobResumeCostLines,
                MinimumPortfolioLines = Config.JobMinimumPortfolioLines + 20,
                MinimumCodeQuality = Config.JobMinimumCodeQuality + 5,
            },
            _ => new ActiveJobListing
            {
                Title = "Gameplay Programmer",
                TechStack = "C# / .NET / MonoGame",
                RemainingInGameMinutes = Config.JobListingDurationMinutes,
                ResumeCostLines = Config.JobResumeCostLines,
                MinimumPortfolioLines = Config.JobMinimumPortfolioLines,
                MinimumCodeQuality = Config.JobMinimumCodeQuality,
            },
        };
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

    private int GetWriteCodeLinesGain(RunState state)
    {
        var lines = Config.WriteCodeLinesGain;
        foreach (var type in state.PurchasedUpgrades)
        {
            lines += EfficiencyUpgradeCatalog.Get(type).BonusLinesPerClick;
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

        return Math.Max(0.5, focusCost);
    }

    private double GetWriteCodeQualityGain(RunState state)
    {
        var qualityGain = Config.WriteCodeQualityGain;
        foreach (var type in state.PurchasedUpgrades)
        {
            qualityGain += EfficiencyUpgradeCatalog.Get(type).BonusQualityGain;
        }

        if (!IsSluggish(state))
        {
            return qualityGain;
        }

        return Math.Max(0.05, qualityGain * Config.SluggishQualityMultiplier);
    }

    private void ApplyPassiveFocusDrain(RunState state, double elapsedInGameMinutes)
    {
        state.Focus = Clamp(
            state.Focus - (elapsedInGameMinutes * Config.PassiveFocusDrainPerInGameMinute),
            0,
            Config.MaxFocus);
    }

    private void ApplyPassiveSanityRegeneration(RunState state, double elapsedInGameMinutes)
    {
        if (!state.HasFirstCoin)
        {
            return;
        }

        state.Sanity = Clamp(
            state.Sanity + (elapsedInGameMinutes * Config.FirstCoinPassiveSanityRegenPerInGameMinute),
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

    private void AdvanceTimedEffects(RunState state, double elapsedInGameMinutes)
    {
        state.SluggishMinutesRemaining = Math.Max(0, state.SluggishMinutesRemaining - elapsedInGameMinutes);
        state.FileCompletionCelebrationMinutesRemaining = Math.Max(0, state.FileCompletionCelebrationMinutesRemaining - elapsedInGameMinutes);
        if (state.FileCompletionCelebrationMinutesRemaining <= BoundaryEpsilon)
        {
            state.RecentCompletedFileName = null;
        }

        if (state.ActiveCatInterruption is not null)
        {
            state.ActiveCatInterruption.RemainingInGameMinutes -= elapsedInGameMinutes;
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
        if (state.ActiveCatInterruption is not null &&
            state.ActiveCatInterruption.RemainingInGameMinutes <= BoundaryEpsilon)
        {
            var deletedLines = Math.Min(state.LinesOfCode, state.ActiveCatInterruption.LinesDeletionPenalty);
            state.LinesOfCode -= deletedLines;
            PortfolioWorkspace.SynchronizeToLinesOfCode(state);
            state.ActiveCatInterruption = null;
            AppendLog(state, $"The cat stomped across the keyboard and deleted {deletedLines} LoC.");
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
    }

    private double GetStepMinutes(RunState state, double remainingMinutes)
    {
        var step = remainingMinutes;

        var minutesUntilMidnight = SimulationConfig.MinutesPerDay - state.TimeOfDayMinutes;
        if (minutesUntilMidnight > BoundaryEpsilon)
        {
            step = Math.Min(step, minutesUntilMidnight);
        }

        step = Math.Min(step, GetPositiveTimer(state.SluggishMinutesRemaining));
        step = Math.Min(step, GetPositiveTimer(state.ActiveCatInterruption?.RemainingInGameMinutes));
        step = Math.Min(step, GetPositiveTimer(state.ActiveTechDebtBug?.RemainingInGameMinutes));
        step = Math.Min(step, GetPositiveTimer(state.ActiveJobListing?.RemainingInGameMinutes));

        return Math.Max(step, BoundaryEpsilon);
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
}
