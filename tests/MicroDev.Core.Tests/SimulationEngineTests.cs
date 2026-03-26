using MicroDev.Core.Portfolio;
using MicroDev.Core.Simulation;

namespace MicroDev.Core.Tests;

public sealed class SimulationEngineTests
{
    private readonly SimulationEngine _engine = new(SimulationConfig.Default);
    private readonly IncidentScheduler _scheduler = new();

    [Fact]
    public void AdvanceTime_AppliesPassiveFocusDrain()
    {
        var state = _engine.CreateNewRun();

        _engine.AdvanceTime(state, 120);

        Assert.Equal(67, state.Focus, 3);
        Assert.Equal(1, state.Day);
        Assert.Equal("10:00", state.ClockText);
    }

    [Fact]
    public void AdvanceTime_WithFirstCoin_PassivelyRegeneratesSanity()
    {
        var state = _engine.CreateNewRun();
        state.Sanity = 50;

        _engine.AdvanceTime(state, 120);

        Assert.Equal(50.72, state.Sanity, 3);
    }

    [Fact]
    public void AdvanceTime_CrossingMidnight_ChargesDailyBills()
    {
        var state = _engine.CreateNewRun();

        _engine.AdvanceTime(state, 16 * 60);

        Assert.Equal(2, state.Day);
        Assert.Equal("00:00", state.ClockText);
        Assert.Equal(35, state.Funds, 3);
    }

    [Fact]
    public void PlaceFoodOrder_WithoutOrderNote_AppliesSluggishPenalty()
    {
        var state = _engine.CreateNewRun();
        state.Focus = 40;

        var ordered = _engine.PlaceFoodOrder(state, FoodChoice.Burger, doubleCheckOrder: false);

        Assert.True(ordered);
        Assert.NotNull(state.ActiveFoodDelivery);
        Assert.Equal(63, state.Funds, 3);
        Assert.Equal(40, state.Focus, 3);

        _engine.AdvanceTime(state, _engine.Config.FoodDeliveryDurationMinutes);

        Assert.Null(state.ActiveFoodDelivery);
        Assert.Equal(100, state.Focus, 3);
        Assert.Equal(_engine.Config.BurgerSluggishDurationMinutes, state.SluggishMinutesRemaining, 3);
        Assert.True(_engine.IsSluggish(state));
    }

    [Fact]
    public void PlaceFoodOrder_WithOrderNote_AvoidsSluggishPenalty_AndCanRaiseSanity()
    {
        var state = _engine.CreateNewRun();

        var ordered = _engine.PlaceFoodOrder(state, FoodChoice.Burrito, doubleCheckOrder: true);

        Assert.True(ordered);
        Assert.NotNull(state.ActiveFoodDelivery);
        Assert.Equal(65, state.Funds, 3);

        _engine.AdvanceTime(state, _engine.Config.FoodDeliveryDurationMinutes);

        Assert.Null(state.ActiveFoodDelivery);
        Assert.Equal(100, state.Focus, 3);
        Assert.Equal(77.18, state.Sanity, 3);
        Assert.Equal(0, state.SluggishMinutesRemaining, 3);
    }

    [Fact]
    public void PlaceFoodOrder_Dumplings_RaisesSanityMoreThanFocus()
    {
        var state = _engine.CreateNewRun();
        state.Focus = 40;
        state.Sanity = 30;

        var ordered = _engine.PlaceFoodOrder(state, FoodChoice.Dumplings, doubleCheckOrder: true);

        Assert.True(ordered);
        Assert.Equal(58, state.Funds, 3);

        _engine.AdvanceTime(state, _engine.Config.FoodDeliveryDurationMinutes);

        Assert.Equal(97.25, state.Focus, 3);
        Assert.Equal(44.18, state.Sanity, 3);
    }

    [Fact]
    public void PlaceFoodOrder_PartialCustomization_ReducesButDoesNotRemoveSluggishPenalty()
    {
        var state = _engine.CreateNewRun();
        state.Focus = 40;

        var ordered = _engine.PlaceFoodOrder(
            state,
            FoodChoice.Burger,
            [FoodOrderModifier.NoCheese],
            reviewReceipt: false);

        Assert.True(ordered);
        Assert.Equal(63, state.Funds, 3);

        _engine.AdvanceTime(state, _engine.Config.FoodDeliveryDurationMinutes);

        Assert.Equal(100, state.Focus, 3);
        Assert.Equal(112, state.SluggishMinutesRemaining, 3);
    }

    [Fact]
    public void PlaceFoodOrder_ExpeditedDelivery_CostsATipAndArrivesSooner()
    {
        var state = _engine.CreateNewRun();
        state.Focus = 40;

        var ordered = _engine.PlaceFoodOrder(state, FoodChoice.Burger, doubleCheckOrder: true, expeditedDelivery: true);

        Assert.True(ordered);
        Assert.NotNull(state.ActiveFoodDelivery);
        Assert.Equal(57, state.Funds, 3);

        _engine.AdvanceTime(state, _engine.Config.ExpeditedFoodDeliveryDurationMinutes - 1);
        Assert.NotNull(state.ActiveFoodDelivery);

        _engine.AdvanceTime(state, 1);

        Assert.Null(state.ActiveFoodDelivery);
        Assert.Equal(100, state.Focus, 3);
        Assert.Equal(0, state.SluggishMinutesRemaining, 3);
    }

    [Fact]
    public void AdvanceTime_WithoutFood_DrainsSanityAfterHungerThreshold()
    {
        var state = _engine.CreateNewRun();
        state.HasFirstCoin = false;
        state.Sanity = 60;
        state.MinutesSinceLastMeal = _engine.Config.HungryAfterMinutes;

        _engine.AdvanceTime(state, 120);

        Assert.Equal(59.04, state.Sanity, 3);
    }

    [Fact]
    public void AdvanceTime_WhileSleepDeprived_DrainsCodeQuality()
    {
        var state = _engine.CreateNewRun();
        state.HasFirstCoin = false;
        state.CodeQuality = 80;
        state.MinutesSinceLastSleep = _engine.Config.SleepDeprivationAfterMinutes;

        _engine.AdvanceTime(state, 120);

        Assert.Equal(78.2, state.CodeQuality, 3);
    }

    [Fact]
    public void AdvanceTime_AfterTwoDaysAwake_RequiresSleep()
    {
        var state = _engine.CreateNewRun();
        state.MinutesSinceLastSleep = _engine.Config.SleepForcedAfterMinutes - 60;

        _engine.AdvanceTime(state, 60);

        Assert.True(_engine.RequiresSleep(state));
        Assert.False(_engine.CanApplyAction(state, PlayerAction.WriteCode));
        Assert.True(_engine.CanApplyAction(state, PlayerAction.Sleep));
        Assert.Contains(state.EventLog, entry => entry.Contains("Two straight days without sleep", StringComparison.Ordinal));
    }

    [Fact]
    public void Sleep_RefillsFocusToFull_AndClearsSleepDebt()
    {
        var state = _engine.CreateNewRun();
        state.HasFirstCoin = false;
        state.Focus = 12;
        state.Sanity = 40;
        state.MinutesSinceLastMeal = 0;
        state.MinutesSinceLastSleep = 30 * 60;

        var slept = _engine.ApplyAction(state, PlayerAction.Sleep);

        Assert.True(slept);
        Assert.Equal(_engine.Config.MaxFocus, state.Focus, 3);
        Assert.Equal(58, state.Sanity, 3);
        Assert.Equal(0, state.MinutesSinceLastSleep, 3);
    }

    [Fact]
    public void CreateNewRun_StartsOnBlankFirstPortfolioFile()
    {
        var state = _engine.CreateNewRun();
        var currentProgram = PortfolioWorkspace.GetCurrentProgram(state);

        Assert.True(state.RunSeed > 0);
        Assert.EndsWith(".cs", currentProgram.FileName);
        Assert.True(currentProgram.TotalLinesOfCode > 0);
        Assert.Equal(0, state.CurrentProgramIndex);
        Assert.Equal(0, state.CurrentProgramVisibleLineCount);
        Assert.Empty(PortfolioWorkspace.GetVisibleLines(state));
    }

    [Fact]
    public void WriteCode_RevealsRealPortfolioCode()
    {
        var state = _engine.CreateNewRun();

        var applied = _engine.ApplyAction(state, PlayerAction.WriteCode);

        var visibleLines = PortfolioWorkspace.GetVisibleLines(state);
        Assert.True(applied);
        Assert.Equal(1, state.LinesOfCode);
        Assert.Equal(68, state.Focus, 3);
        Assert.NotEmpty(visibleLines);
        Assert.Contains(visibleLines, line => line.StartsWith("using ", StringComparison.Ordinal));
    }

    [Fact]
    public void WriteCode_IsReducedWhileSluggish()
    {
        var state = _engine.CreateNewRun();
        _engine.PlaceFoodOrder(state, FoodChoice.Burger, doubleCheckOrder: false);
        _engine.AdvanceTime(state, _engine.Config.FoodDeliveryDurationMinutes);

        var applied = _engine.ApplyAction(state, PlayerAction.WriteCode);

        Assert.True(applied);
        Assert.Equal(1, state.LinesOfCode);
        Assert.Equal(97.25, state.Focus, 3);
        Assert.Equal(100, state.CodeQuality, 3);
    }

    [Fact]
    public void WriteCode_AdvancesIntoNextPortfolioFile()
    {
        var state = _engine.CreateNewRun();
        var firstFileName = PortfolioWorkspace.GetCurrentProgram(state).FileName;
        state.Focus = 200;
        var safety = 0;

        while (state.CurrentProgramIndex == 0 && safety < 60)
        {
            Assert.True(_engine.ApplyAction(state, PlayerAction.WriteCode));
            safety++;
        }

        Assert.True(safety < 60);
        var secondFileName = PortfolioWorkspace.GetCurrentProgram(state).FileName;
        Assert.NotEqual(firstFileName, secondFileName);
        Assert.Equal(0, state.CurrentProgramVisibleLineCount);
        Assert.Equal(firstFileName, state.RecentCompletedFileName);
        Assert.True(state.FileCompletionCelebrationMinutesRemaining > 0);

        Assert.True(_engine.ApplyAction(state, PlayerAction.WriteCode));
        Assert.NotEmpty(PortfolioWorkspace.GetVisibleLines(state));
    }

    [Fact]
    public void PublishApp_CompletingPortfolioPaysAndRollsToANewBatch()
    {
        var state = _engine.CreateNewRun();
        state.Focus = 2000;
        var firstFileName = PortfolioWorkspace.GetCurrentProgram(state).FileName;

        CompleteCurrentPortfolioBatch(_engine, state);
        var fundsBeforePublish = state.Funds;

        Assert.True(_engine.IsPortfolioPublishReady(state));
        Assert.True(_engine.ApplyAction(state, PlayerAction.PublishApp));

        Assert.Equal(1, state.PublishedAppCount);
        Assert.True(state.Funds > fundsBeforePublish);
        Assert.Equal(0, state.CurrentPortfolioLinesOfCode);
        Assert.Equal(0, state.CurrentProgramIndex);
        Assert.Equal(0, state.CurrentProgramVisibleLineCount);
        Assert.NotNull(state.LastPublishedAppName);
        Assert.NotEqual(firstFileName, PortfolioWorkspace.GetCurrentProgram(state).FileName);
    }

    [Fact]
    public void PublishedApps_CanTriggerStorefrontPayouts()
    {
        var state = _engine.CreateNewRun();
        state.Focus = 2000;

        CompleteCurrentPortfolioBatch(_engine, state);
        Assert.True(_engine.ApplyAction(state, PlayerAction.PublishApp));

        state.NextPublishedAppSaleDeskMinute = 1;
        var fundsBeforeSale = state.Funds;
        var incidents = _scheduler.Update(state, 2, _engine.Config);

        Assert.Contains(incidents, incident => incident.Type == IncidentType.PublishedAppSale);

        _engine.QueueIncidents(state, incidents);

        Assert.True(state.Funds > fundsBeforeSale);
    }

    [Fact]
    public void PurchaseUpgrade_ImprovesWriteEfficiency()
    {
        var state = _engine.CreateNewRun();
        state.Funds = 200;

        var purchased = _engine.PurchaseUpgrade(state, EfficiencyUpgradeType.MechanicalKeyboard);

        Assert.True(purchased);
        Assert.Equal(1, _engine.GetUpgradeTier(state, EfficiencyUpgradeType.MechanicalKeyboard));
        Assert.Equal(165, state.Funds, 3);
        Assert.Equal(2, _engine.GetCurrentWriteLinesPerClick(state));
    }

    [Fact]
    public void WriteCode_WithEfficiencyUpgrade_AddsMoreLines()
    {
        var state = _engine.CreateNewRun();
        state.Funds = 200;
        _engine.PurchaseUpgrade(state, EfficiencyUpgradeType.MechanicalKeyboard);

        var applied = _engine.ApplyAction(state, PlayerAction.WriteCode);

        Assert.True(applied);
        Assert.Equal(2, state.LinesOfCode);
    }

    [Fact]
    public void PurchaseUpgrade_CanStackToTierFive()
    {
        var state = _engine.CreateNewRun();
        state.Funds = 2000;

        for (var tier = 1; tier <= 5; tier++)
        {
            Assert.True(_engine.PurchaseUpgrade(state, EfficiencyUpgradeType.MechanicalKeyboard));
            Assert.Equal(tier, _engine.GetUpgradeTier(state, EfficiencyUpgradeType.MechanicalKeyboard));
        }

        Assert.False(_engine.PurchaseUpgrade(state, EfficiencyUpgradeType.MechanicalKeyboard));
        Assert.Equal(6, _engine.GetCurrentWriteLinesPerClick(state));
    }

    [Fact]
    public void TakeFreelanceGig_PipelineRescue_PaysMoreButCostsMore_AfterContractCompletes()
    {
        var state = _engine.CreateNewRun();

        var taken = _engine.TakeFreelanceGig(state, FreelanceGigType.PipelineRescue);
        var safety = 0;
        while (state.ActiveFreelanceGig is not null && safety < 80)
        {
            Assert.True(_engine.WorkOnFreelanceGig(state));
            safety++;
        }

        Assert.True(taken);
        Assert.True(safety < 80);
        Assert.Null(state.ActiveFreelanceGig);
        Assert.Equal(143, state.Funds, 3);
        Assert.Equal("09:45", state.ClockText);
        Assert.Equal(51.375, state.Focus, 3);
        Assert.Equal(58.63, state.Sanity, 3);
        Assert.Equal(100, state.CodeQuality, 3);
    }

    [Fact]
    public void TakeFreelanceGig_RequiresMinimumFocus()
    {
        var state = _engine.CreateNewRun();
        state.Focus = _engine.Config.FreelanceMinimumFocusRequired - 1;

        var taken = _engine.TakeFreelanceGig(state, FreelanceGigType.QuickBugfix);

        Assert.False(taken);
        Assert.Equal(_engine.Config.FreelanceMinimumFocusRequired - 1, state.Focus, 3);
        Assert.Contains("needs at least", state.EventLog.Last());
    }

    [Fact]
    public void Scheduler_QueuesIncidents_AsDeskTimeCrossesThresholds()
    {
        var engine = new SimulationEngine(SimulationConfig.Default, () => 4242);
        var state = engine.CreateNewRun();

        var firstWave = _scheduler.Update(state, 220, engine.Config);
        var secondWave = _scheduler.Update(state, 360, engine.Config);
        var thirdWave = _scheduler.Update(state, 420, engine.Config);

        Assert.Contains(firstWave, incident => incident.Id.StartsWith("desk-", StringComparison.Ordinal));
        Assert.Contains(secondWave, incident => incident.Id == "job-1");
        Assert.Contains(secondWave, incident => incident.Id.StartsWith("mod-", StringComparison.Ordinal));
        Assert.Contains(thirdWave, incident => incident.Id == "job-2");
    }

    [Fact]
    public void HardDifficulty_QueuesMoreModifierIncidentsThanEasy()
    {
        var easyEngine = new SimulationEngine(SimulationConfig.ForDifficulty(GameDifficulty.Easy));
        var hardEngine = new SimulationEngine(SimulationConfig.ForDifficulty(GameDifficulty.Hard));
        var easyState = easyEngine.CreateNewRun();
        var hardState = hardEngine.CreateNewRun();

        var easyIncidents = _scheduler.Update(easyState, 700, easyEngine.Config);
        var hardIncidents = _scheduler.Update(hardState, 700, hardEngine.Config);

        var easyModifierCount = easyIncidents.Count(incident => incident.Id.StartsWith("mod-", StringComparison.Ordinal));
        var hardModifierCount = hardIncidents.Count(incident => incident.Id.StartsWith("mod-", StringComparison.Ordinal));

        Assert.True(hardModifierCount > easyModifierCount);
    }

    [Fact]
    public void EasyDifficulty_GeneratesGuaranteedJobOpportunities()
    {
        var engine = new SimulationEngine(SimulationConfig.ForDifficulty(GameDifficulty.Easy));
        var state = engine.CreateNewRun();

        var incidents = _scheduler.Update(state, 181, engine.Config);

        Assert.Contains(incidents, incident => incident.Type == IncidentType.JobListing);
    }

    [Fact]
    public void QueueIncidents_ActivatesTechDebt_AndDecayAppliesOverTime()
    {
        var state = _engine.CreateNewRun();
        state.CodeQuality = 80;

        _engine.QueueIncidents(state, [new QueuedIncident("bug-1", IncidentType.TechDebtBug, "Bug!")]);
        _engine.AdvanceTime(state, 60);

        Assert.NotNull(state.ActiveTechDebtBug);
        Assert.Equal(75.2, state.CodeQuality, 3);
    }

    [Fact]
    public void QueueIncidents_ActivatesTechDebt_WithDebugSnippetMetadata()
    {
        var state = _engine.CreateNewRun(4242);

        _engine.QueueIncidents(state, [new QueuedIncident("bug-1", IncidentType.TechDebtBug, "Bug!")]);

        var bug = Assert.IsType<ActiveTechDebtBug>(state.ActiveTechDebtBug);
        Assert.NotEmpty(bug.CompilerHint);
        Assert.True(bug.CodeLines.Length >= 3);
        Assert.NotEmpty(bug.HighlightToken);
    }

    [Fact]
    public void ApplyAction_SquashBug_ClearsTechDebtAndCostsFocus()
    {
        var state = _engine.CreateNewRun();
        state.Focus = 20;
        state.CodeQuality = 70;
        _engine.QueueIncidents(state, [new QueuedIncident("bug-1", IncidentType.TechDebtBug, "Bug!")]);

        var applied = _engine.ApplyAction(state, PlayerAction.SquashBug);

        Assert.True(applied);
        Assert.Null(state.ActiveTechDebtBug);
        Assert.Equal(14, state.Focus, 3);
        Assert.Equal(74, state.CodeQuality, 3);
    }

    [Fact]
    public void SevereNeeds_CanSpawnNeedDrivenTechDebtBug()
    {
        var engine = new SimulationEngine(SimulationConfig.Create(GameDifficulty.Normal, GameplayLoopMode.Interview, realisticMode: false), () => 4242);
        var state = engine.CreateNewRun();
        var qualityBefore = state.CodeQuality;

        state.MinutesSinceLastMeal = engine.Config.StarvingAfterMinutes;
        state.MinutesSinceLastSleep = engine.Config.SevereSleepDeprivationAfterMinutes;
        state.Sanity = 18;

        engine.AdvanceTime(state, 60);

        var bug = Assert.IsType<ActiveTechDebtBug>(state.ActiveTechDebtBug);
        Assert.True(state.CodeQuality < qualityBefore);
        Assert.True(bug.IsNeedDriven);
        Assert.NotEmpty(bug.CompilerHint);
    }

    [Fact]
    public void HardDifficulty_SpawnsNeedDrivenTechDebtSoonerThanEasy()
    {
        var easyEngine = new SimulationEngine(SimulationConfig.Create(GameDifficulty.Easy, GameplayLoopMode.Interview, realisticMode: false), () => 4242);
        var hardEngine = new SimulationEngine(SimulationConfig.Create(GameDifficulty.Hard, GameplayLoopMode.Interview, realisticMode: false), () => 4242);
        var easyState = easyEngine.CreateNewRun();
        var hardState = hardEngine.CreateNewRun();

        easyState.MinutesSinceLastMeal = easyEngine.Config.VeryHungryAfterMinutes;
        hardState.MinutesSinceLastMeal = hardEngine.Config.VeryHungryAfterMinutes;
        easyState.MinutesSinceLastSleep = easyEngine.Config.SleepDeprivationAfterMinutes;
        hardState.MinutesSinceLastSleep = hardEngine.Config.SleepDeprivationAfterMinutes;
        easyState.Sanity = 32;
        hardState.Sanity = 32;

        easyEngine.AdvanceTime(easyState, 90);
        hardEngine.AdvanceTime(hardState, 90);

        Assert.Null(easyState.ActiveTechDebtBug);
        Assert.NotNull(hardState.ActiveTechDebtBug);
    }

    [Fact]
    public void ComputerFreeze_CanBeResolvedWithASelfRepairChoice()
    {
        var state = _engine.CreateNewRun();

        _engine.QueueIncidents(state, [new QueuedIncident("freeze-1", IncidentType.ComputerFreeze, "Freeze!")]);

        Assert.True(_engine.HasPendingLifeEvent(state));
        Assert.True(_engine.ResolveLifeEventOption(state, 0));

        Assert.False(_engine.HasPendingLifeEvent(state));
        Assert.Equal("09:15", state.ClockText);
        Assert.True(state.Sanity < 70);
        Assert.True(state.Focus < 70);
    }

    [Fact]
    public void ComputerFreeze_RollsBackUncommittedProgressToLastCommit()
    {
        var state = _engine.CreateNewRun();
        state.Focus = 200;
        var safety = 0;
        while (state.RecentCompletedFileName is null && safety < 120)
        {
            Assert.True(_engine.ApplyAction(state, PlayerAction.WriteCode));
            safety++;
        }

        Assert.True(safety < 120);
        var committedLines = state.CurrentPortfolioLinesOfCode;
        Assert.True(_engine.CommitChanges(state));
        Assert.Equal(committedLines, state.VersionControl.CommittedPortfolioLinesOfCode);

        Assert.True(_engine.ApplyAction(state, PlayerAction.WriteCode));
        var linesBeforeFreeze = state.CurrentPortfolioLinesOfCode;
        Assert.True(state.VersionControl.PendingChangeLines > 0);

        _engine.QueueIncidents(state, [new QueuedIncident("freeze-rollback", IncidentType.ComputerFreeze, "Freeze!")]);

        Assert.True(_engine.HasPendingLifeEvent(state));
        Assert.Equal(committedLines, state.CurrentPortfolioLinesOfCode);
        Assert.True(linesBeforeFreeze > committedLines);
        Assert.Equal(0, state.VersionControl.PendingChangeLines);
        Assert.Equal(0, state.VersionControl.PendingCompletedFileCount);
    }

    [Fact]
    public void StreamingBinge_EventTradesTimeForSanity()
    {
        var state = _engine.CreateNewRun();
        state.Sanity = 50;

        _engine.QueueIncidents(state, [new QueuedIncident("show-1", IncidentType.StreamingBinge, "Show!")]);

        Assert.True(_engine.HasPendingLifeEvent(state));
        Assert.True(_engine.ResolveLifeEventOption(state, 0));

        Assert.False(_engine.HasPendingLifeEvent(state));
        Assert.Equal("09:45", state.ClockText);
        Assert.True(state.Sanity > 50);
        Assert.True(state.Focus < 70);
    }

    [Fact]
    public void OnlineMatch_CanBecomeRealRelationshipAfterEnoughPositiveChoices()
    {
        var state = _engine.CreateNewRun();
        state.Funds = 100;
        state.RelationshipCandidateName = "Jordan";
        state.RelationshipCandidateCompatibility = 0;

        _engine.QueueIncidents(state, [new QueuedIncident("match-1", IncidentType.OnlineMatch, "Match!")]);
        Assert.True(_engine.ResolveLifeEventOption(state, 0));
        Assert.True(_engine.HasPendingLifeEvent(state));
        Assert.True(_engine.ResolveLifeEventOption(state, 2));
        Assert.True(_engine.HasPendingLifeEvent(state));
        Assert.True(_engine.ResolveLifeEventOption(state, 0));
        Assert.False(_engine.HasPendingLifeEvent(state));

        _engine.QueueIncidents(state, [new QueuedIncident("match-2", IncidentType.OnlineMatch, "Match!")]);
        Assert.True(_engine.ResolveLifeEventOption(state, 0));
        Assert.True(_engine.ResolveLifeEventOption(state, 2));
        Assert.True(_engine.ResolveLifeEventOption(state, 0));

        Assert.True(state.HasFoundLove);
        Assert.NotNull(state.PartnerName);
        Assert.True(state.RelationshipProgress >= _engine.Config.RelationshipProgressNeededForLove);
    }

    [Fact]
    public void CreateNewRun_StartsWithNoCommunicationContacts()
    {
        var state = _engine.CreateNewRun();

        Assert.Empty(state.KnownContacts);
    }

    [Fact]
    public void WritingCode_UnlocksWorkContactsOverTime()
    {
        var state = _engine.CreateNewRun();
        state.Focus = 200;
        var safety = 0;

        while (safety < 120 && _engine.CanApplyAction(state, PlayerAction.WriteCode))
        {
            Assert.True(_engine.ApplyAction(state, PlayerAction.WriteCode));
            if (state.KnownContacts.Any(contact => contact.Role == SocialContactRole.Friend) &&
                state.KnownContacts.Any(contact => contact.Role == SocialContactRole.Mentor))
            {
                break;
            }

            safety++;
        }

        Assert.Contains(state.KnownContacts, contact => contact.Role == SocialContactRole.Friend);
        Assert.Contains(state.KnownContacts, contact => contact.Role == SocialContactRole.Mentor);
    }

    [Fact]
    public void OnlineMatch_DiscoversADateContact()
    {
        var state = _engine.CreateNewRun();

        _engine.QueueIncidents(state, [new QueuedIncident("match-1", IncidentType.OnlineMatch, "Match!")]);

        Assert.Contains(state.KnownContacts, contact => contact.Role == SocialContactRole.Date);
    }

    [Fact]
    public void CallingAMentor_BuildsBondAndAwardsInterviewPrep()
    {
        var state = _engine.CreateNewRun();
        var mentor = new SocialContact
        {
            Id = "mentor-test",
            Name = "Parker Lane",
            Role = SocialContactRole.Mentor,
        };
        state.KnownContacts.Add(mentor);
        state.ActiveJobApplication = new ActiveJobApplication
        {
            ListingTitle = "Gameplay Programmer",
            ResumeTrack = ResumeTrack.Gameplay,
        };
        state.Focus = 80;
        state.Sanity = 40;

        Assert.True(_engine.CallContact(state, mentor.Id));

        Assert.True(mentor.BondProgress > 0);
        Assert.True(state.ActiveJobApplication.PrepPoints > 0);
        Assert.True(state.Sanity > 40);
        Assert.True(state.Focus < 80);
    }

    [Fact]
    public void LongFormModes_AddAmbientSanityPressure_ThatScalesWithDifficulty()
    {
        var easyEngine = new SimulationEngine(SimulationConfig.Create(GameDifficulty.Easy, GameplayLoopMode.Indie, realisticMode: false));
        var hardEngine = new SimulationEngine(SimulationConfig.Create(GameDifficulty.Hard, GameplayLoopMode.Indie, realisticMode: false));
        var easyState = easyEngine.CreateNewRun();
        var hardState = hardEngine.CreateNewRun();

        easyState.HasFirstCoin = false;
        hardState.HasFirstCoin = false;
        easyState.Sanity = 80;
        hardState.Sanity = 80;
        easyState.MinutesSinceLastMeal = 0;
        hardState.MinutesSinceLastMeal = 0;
        easyState.MinutesSinceLastSleep = 0;
        hardState.MinutesSinceLastSleep = 0;

        easyEngine.AdvanceTime(easyState, 180);
        hardEngine.AdvanceTime(hardState, 180);

        Assert.True(easyState.Sanity < 80);
        Assert.True(hardState.Sanity < easyState.Sanity);
    }

    [Fact]
    public void CatTimeout_RollsBackOnlyUncommittedProgressWhenIgnored()
    {
        var state = _engine.CreateNewRun();
        state.Focus = 200;
        var safety = 0;
        while (state.RecentCompletedFileName is null && safety < 120)
        {
            Assert.True(_engine.ApplyAction(state, PlayerAction.WriteCode));
            safety++;
        }

        Assert.True(safety < 120);
        var committedLines = state.CurrentPortfolioLinesOfCode;
        Assert.True(_engine.CommitChanges(state));

        Assert.True(_engine.ApplyAction(state, PlayerAction.WriteCode));
        var linesBeforeTimeout = state.CurrentPortfolioLinesOfCode;
        _engine.QueueIncidents(state, [new QueuedIncident("cat-1", IncidentType.CatInterruption, "Cat!")]);

        _engine.AdvanceTime(state, _engine.Config.CatStayDurationMinutes);

        Assert.Null(state.ActiveCatInterruption);
        Assert.Equal(committedLines, state.CurrentPortfolioLinesOfCode);
        Assert.True(linesBeforeTimeout > committedLines);
        Assert.Equal(0, state.VersionControl.PendingChangeLines);
        Assert.Equal(state.CurrentPortfolioLinesOfCode, state.LinesOfCode);
    }

    [Fact]
    public void PetCat_RepeatedEnoughTimes_ClearsInterruptionWithoutPenalty()
    {
        var state = _engine.CreateNewRun();
        state.LinesOfCode = 80;
        _engine.QueueIncidents(state, [new QueuedIncident("cat-1", IncidentType.CatInterruption, "Cat!")]);
        var requiredClears = state.ActiveCatInterruption!.PatsRemaining;

        for (var index = 0; index < requiredClears; index++)
        {
            Assert.True(_engine.ApplyAction(state, PlayerAction.PetCat));
        }

        Assert.Null(state.ActiveCatInterruption);
        Assert.Equal(80, state.LinesOfCode);
    }

    [Fact]
    public void CatInterruption_AddsGibberishAndQualityLossWhileActive()
    {
        var state = _engine.CreateNewRun();
        _engine.QueueIncidents(state, [new QueuedIncident("cat-1", IncidentType.CatInterruption, "Cat!")]);

        _engine.AdvanceTime(state, (_engine.Config.CatTypingBurstIntervalMinutes * 2) + 1);

        var cat = Assert.IsType<ActiveCatInterruption>(state.ActiveCatInterruption);
        Assert.True(cat.PhantomBugCount >= 2);
        Assert.True(cat.GibberishLinesTyped >= 4);
        Assert.True(state.CodeQuality < 100);
    }

    [Fact]
    public void QuickResolveDistraction_ClearsTheActiveDeskDistraction()
    {
        var state = _engine.CreateNewRun();
        state.Funds = 100;
        state.Focus = 100;
        _engine.QueueIncidents(state, [new QueuedIncident("cat-quick", IncidentType.CatInterruption, "Desk noise!")]);

        Assert.True(_engine.CanQuickResolveDistraction(state));
        Assert.True(_engine.QuickResolveDistraction(state));
        Assert.Null(state.ActiveCatInterruption);
    }

    [Fact]
    public void InterviewMode_OfferBranchesIntoCareerChoiceInsteadOfEndingImmediately()
    {
        var engine = new SimulationEngine(SimulationConfig.Create(GameDifficulty.Normal, GameplayLoopMode.Interview, realisticMode: false));
        var state = engine.CreateNewRun();
        state.LinesOfCode = 140;
        state.CodeQuality = 85;
        state.Focus = 200;

        engine.QueueIncidents(state, [new QueuedIncident("job-branch", IncidentType.JobListing, "Job!")]);
        GrantResumeProofForListing(state);

        Assert.True(engine.ApplyAction(state, PlayerAction.ApplyForJob));

        while (state.ActiveJobApplication is not null &&
               !state.ActiveJobApplication.TakeHomeComplete)
        {
            Assert.True(engine.WorkOnJobApplication(state));
        }

        while (state.ActiveJobApplication is not null)
        {
            var question = state.ActiveJobApplication.Questions[state.ActiveJobApplication.CurrentQuestionIndex];
            Assert.True(engine.AnswerInterviewQuestion(state, question.CorrectOptionIndex));
        }

        var branchChoice = Assert.IsType<PendingLifeEvent>(state.PendingLifeEvent);
        Assert.Equal(IncidentType.CareerPathChoice, branchChoice.Type);
        Assert.Equal(RunStatus.InProgress, state.Status);
        var expectedRoute = branchChoice.SubjectScore == (int)GameplayLoopMode.Indie
            ? GameplayLoopMode.Indie
            : GameplayLoopMode.Corporate;

        Assert.True(engine.ResolveLifeEventOption(state, 0));
        Assert.Equal(expectedRoute, state.GameplayMode);
        Assert.Equal(RunStatus.InProgress, state.Status);
    }

    [Fact]
    public void InterviewMode_TimesOutAfterSevenDays()
    {
        var engine = new SimulationEngine(SimulationConfig.Create(GameDifficulty.Normal, GameplayLoopMode.Interview, realisticMode: false));
        var state = engine.CreateNewRun();
        state.Day = engine.Config.InterviewDeadlineDays + 1;

        engine.EvaluateLossState(state);

        Assert.Equal(RunStatus.BurnedOut, state.Status);
        Assert.Contains("seven-day interview sprint", state.OutcomeMessage);
    }

    [Fact]
    public void ApplyForJob_StartsChallengeAndConsumesResumeLines()
    {
        var state = _engine.CreateNewRun();
        state.LinesOfCode = 120;
        state.CodeQuality = 80;
        _engine.QueueIncidents(state, [new QueuedIncident("job-1", IncidentType.JobListing, "Job!")]);
        GrantResumeProofForListing(state);

        var applied = _engine.ApplyAction(state, PlayerAction.ApplyForJob);

        Assert.True(applied);
        Assert.Null(state.ActiveJobListing);
        Assert.NotNull(state.ActiveJobApplication);
        Assert.Equal(85, state.LinesOfCode);
    }

    [Fact]
    public void ApplyForJob_RequiresListingThresholdsBeforeTheInterviewLoop()
    {
        var state = _engine.CreateNewRun();
        state.LinesOfCode = 90;
        state.CodeQuality = 80;
        _engine.QueueIncidents(state, [new QueuedIncident("job-1", IncidentType.JobListing, "Job!")]);
        GrantResumeProofForListing(state);

        var applied = _engine.ApplyAction(state, PlayerAction.ApplyForJob);

        Assert.False(applied);
        Assert.NotNull(state.ActiveJobListing);
        Assert.Null(state.ActiveJobApplication);
    }

    [Fact]
    public void ActiveJobApplication_StillAllowsRecoveryActions()
    {
        var state = _engine.CreateNewRun();
        state.LinesOfCode = 140;
        state.CodeQuality = 80;
        state.Funds = 120;
        _engine.QueueIncidents(state, [new QueuedIncident("job-1", IncidentType.JobListing, "Job!")]);
        GrantResumeProofForListing(state);

        Assert.True(_engine.ApplyAction(state, PlayerAction.ApplyForJob));
        Assert.NotNull(state.ActiveJobApplication);
        Assert.True(_engine.CanApplyAction(state, PlayerAction.Sleep));
        Assert.True(_engine.CanPlaceFoodOrder(state, FoodChoice.Burger));

        var slept = _engine.ApplyAction(state, PlayerAction.Sleep);
        var ordered = _engine.PlaceFoodOrder(state, FoodChoice.Burger, doubleCheckOrder: true);

        Assert.True(slept);
        Assert.True(ordered);
    }

    [Fact]
    public void InterviewPrep_RevealsTakeHomeLinesAtApplicationStart()
    {
        var state = _engine.CreateNewRun();
        state.LinesOfCode = 140;
        state.CodeQuality = 80;
        state.Funds = 200;
        Assert.True(_engine.PurchaseUpgrade(state, EfficiencyUpgradeType.InterviewNotebook));
        _engine.QueueIncidents(state, [new QueuedIncident("job-1", IncidentType.JobListing, "Job!")]);
        GrantResumeProofForListing(state);

        Assert.True(_engine.ApplyAction(state, PlayerAction.ApplyForJob));

        Assert.NotNull(state.ActiveJobApplication);
        Assert.True(state.ActiveJobApplication.VisibleLineCount > 0);
    }

    [Fact]
    public void InterviewRequiresPerfectAnswers_EvenWithPrep()
    {
        var state = _engine.CreateNewRun();
        state.LinesOfCode = 140;
        state.CodeQuality = 80;
        state.Focus = 200;
        state.Funds = 200;
        _engine.QueueIncidents(state, [new QueuedIncident("job-1", IncidentType.JobListing, "Job!")]);
        GrantResumeProofForListing(state);

        Assert.True(_engine.ApplyAction(state, PlayerAction.ApplyForJob));
        _engine.QueueIncidents(state, [new QueuedIncident("bug-1", IncidentType.TechDebtBug, "Bug!")]);
        Assert.True(_engine.ApplyAction(state, PlayerAction.SquashBug));
        Assert.True(_engine.PlaceFoodOrder(state, FoodChoice.Burrito, doubleCheckOrder: true));

        while (state.ActiveJobApplication is not null && !state.ActiveJobApplication.TakeHomeComplete)
        {
            Assert.True(_engine.WorkOnJobApplication(state));
        }

        Assert.NotNull(state.ActiveJobApplication);
        Assert.True(state.ActiveJobApplication.PrepPoints >= 2);

        var answeredCorrectly = false;
        while (state.ActiveJobApplication is not null)
        {
            var question = state.ActiveJobApplication.Questions[state.ActiveJobApplication.CurrentQuestionIndex];
            var answer = answeredCorrectly
                ? (question.CorrectOptionIndex + 1) % question.Options.Count
                : question.CorrectOptionIndex;
            answeredCorrectly = true;
            Assert.True(_engine.AnswerInterviewQuestion(state, answer));
        }

        Assert.Equal(RunStatus.InProgress, state.Status);
        Assert.Equal(0, state.SuccessfulApplications);
        Assert.Null(state.OutcomeMessage);
    }

    [Fact]
    public void ModifierIncidents_ChangeWritingStats()
    {
        var state = _engine.CreateNewRun();
        var baseLines = _engine.GetCurrentWriteLinesPerClick(state);
        var baseFocusCost = _engine.GetCurrentWriteFocusCost(state);

        _engine.QueueIncidents(state, [new QueuedIncident("flow-1", IncidentType.DeepWorkWindow, "Deep work.")]);

        Assert.True(_engine.IsDeepWorkActive(state));
        Assert.True(_engine.GetCurrentWriteLinesPerClick(state) > baseLines);

        _engine.QueueIncidents(state, [new QueuedIncident("chaos-1", IncidentType.ContextSwitch, "Chaos.")]);

        Assert.True(_engine.IsContextSwitchActive(state));
        Assert.True(_engine.GetCurrentWriteFocusCost(state) > baseFocusCost);
    }

    [Fact]
    public void JobApplicationChallenge_WinsRunWhenInterviewGoesWell()
    {
        var state = _engine.CreateNewRun();
        state.LinesOfCode = 140;
        state.CodeQuality = 80;
        state.Focus = 200;
        _engine.QueueIncidents(state, [new QueuedIncident("job-1", IncidentType.JobListing, "Job!")]);
        GrantResumeProofForListing(state);

        var applied = _engine.ApplyAction(state, PlayerAction.ApplyForJob);
        Assert.True(applied);
        Assert.NotNull(state.ActiveJobApplication);

        var safety = 0;
        while (state.ActiveJobApplication is not null &&
               !state.ActiveJobApplication.TakeHomeComplete &&
               safety < 100)
        {
            Assert.True(_engine.WorkOnJobApplication(state));
            safety++;
        }

        Assert.NotNull(state.ActiveJobApplication);
        Assert.True(state.ActiveJobApplication.TakeHomeComplete);
        while (state.ActiveJobApplication is not null)
        {
            var question = state.ActiveJobApplication.Questions[state.ActiveJobApplication.CurrentQuestionIndex];
            Assert.True(_engine.AnswerInterviewQuestion(state, question.CorrectOptionIndex));
        }

        var branchChoice = Assert.IsType<PendingLifeEvent>(state.PendingLifeEvent);
        Assert.Equal(IncidentType.CareerPathChoice, branchChoice.Type);
        Assert.Equal(RunStatus.InProgress, state.Status);
        Assert.Equal(1, state.SuccessfulApplications);
        Assert.Null(state.ActiveJobApplication);
        Assert.Null(state.OutcomeMessage);
    }

    [Fact]
    public void JobApplicationQuestions_ShuffleAnswerOrder()
    {
        var state = _engine.CreateNewRun();
        state.RunSeed = 12345;
        state.LinesOfCode = 140;
        state.CodeQuality = 80;
        state.Focus = 200;
        _engine.QueueIncidents(state, [new QueuedIncident("job-1", IncidentType.JobListing, "Job!")]);
        GrantResumeProofForListing(state);

        Assert.True(_engine.ApplyAction(state, PlayerAction.ApplyForJob));

        var application = Assert.IsType<ActiveJobApplication>(state.ActiveJobApplication);
        Assert.Contains(application.Questions, question => question.CorrectOptionIndex != 0);
        Assert.All(application.Questions, question =>
        {
            Assert.InRange(question.CorrectOptionIndex, 0, question.Options.Count - 1);
        });
    }

    [Fact]
    public void JobApplicationChallenge_RejectionKeepsRunAlive()
    {
        var state = _engine.CreateNewRun();
        state.LinesOfCode = 140;
        state.CodeQuality = 80;
        state.Focus = 200;
        _engine.QueueIncidents(state, [new QueuedIncident("job-1", IncidentType.JobListing, "Job!")]);
        GrantResumeProofForListing(state);

        Assert.True(_engine.ApplyAction(state, PlayerAction.ApplyForJob));

        var safety = 0;
        while (state.ActiveJobApplication is not null &&
               !state.ActiveJobApplication.TakeHomeComplete &&
               safety < 100)
        {
            Assert.True(_engine.WorkOnJobApplication(state));
            safety++;
        }

        while (state.ActiveJobApplication is not null)
        {
            var question = state.ActiveJobApplication.Questions[state.ActiveJobApplication.CurrentQuestionIndex];
            var wrongIndex = (question.CorrectOptionIndex + 1) % question.Options.Count;
            Assert.True(_engine.AnswerInterviewQuestion(state, wrongIndex));
        }

        Assert.Equal(RunStatus.InProgress, state.Status);
        Assert.Equal(0, state.SuccessfulApplications);
        Assert.Null(state.ActiveJobApplication);
        Assert.Null(state.OutcomeMessage);
    }

    [Fact]
    public void JobListing_ExpiresWhenIgnored()
    {
        var state = _engine.CreateNewRun();
        _engine.QueueIncidents(state, [new QueuedIncident("job-1", IncidentType.JobListing, "Job!")]);

        _engine.AdvanceTime(state, _engine.Config.JobListingDurationMinutes);

        Assert.Null(state.ActiveJobListing);
        Assert.Contains("expired", state.EventLog.Last());
    }

    [Fact]
    public void AdvanceTime_EvictsWhenBillsPushFundsNegative()
    {
        var state = _engine.CreateNewRun();
        state.HasFirstCoin = false;
        state.Funds = 20;

        _engine.AdvanceTime(state, 16 * 60);

        Assert.Equal(RunStatus.Evicted, state.Status);
        Assert.Contains("Rent", state.OutcomeMessage);
    }

    [Fact]
    public void AdvanceTime_WithFirstCoin_TriggersRescueDecisionInsteadOfImmediateEviction()
    {
        var state = _engine.CreateNewRun();
        state.Funds = 20;

        _engine.AdvanceTime(state, 16 * 60);

        Assert.Equal(RunStatus.InProgress, state.Status);
        Assert.True(state.FirstCoinDecisionPending);
        Assert.Equal(20, state.FirstCoinRescueDeficit, 3);
    }

    [Fact]
    public void UseFirstCoin_CancelsEvictionAndRemovesPassiveBuff()
    {
        var state = _engine.CreateNewRun();
        state.Funds = 20;

        _engine.AdvanceTime(state, 16 * 60);
        var used = _engine.UseFirstCoin(state);

        Assert.True(used);
        Assert.False(state.HasFirstCoin);
        Assert.False(state.FirstCoinDecisionPending);
        Assert.Equal(5, state.Funds, 3);
        Assert.Equal(61.28, state.Sanity, 3);
    }

    [Fact]
    public void DeclineFirstCoin_EndsRunInEviction()
    {
        var state = _engine.CreateNewRun();
        state.Funds = 20;

        _engine.AdvanceTime(state, 16 * 60);
        var declined = _engine.DeclineFirstCoin(state);

        Assert.True(declined);
        Assert.Equal(RunStatus.Evicted, state.Status);
        Assert.Contains("locks", state.OutcomeMessage);
    }

    [Fact]
    public void ApplyAction_Freelance_CanTriggerBurnout()
    {
        var state = _engine.CreateNewRun();
        state.Sanity = 5;

        Assert.True(_engine.ApplyAction(state, PlayerAction.Freelance));
        var safety = 0;
        while (state.ActiveFreelanceGig is not null && safety < 80)
        {
            Assert.True(_engine.WorkOnFreelanceGig(state));
            safety++;
        }

        Assert.Equal(RunStatus.BurnedOut, state.Status);
        Assert.Contains("tank", state.OutcomeMessage);
    }

    [Fact]
    public void EndlessDifficulty_CanAdvanceBeyondTheFinitePortfolio()
    {
        var engine = new SimulationEngine(SimulationConfig.ForDifficulty(GameDifficulty.Endless));
        var state = engine.CreateNewRun();
        state.LinesOfCode = 10000;
        state.CurrentPortfolioLinesOfCode = 10000;

        PortfolioWorkspace.SynchronizeToLinesOfCode(state);

        Assert.Equal(GameDifficulty.Endless, state.Difficulty);
        Assert.True(state.CurrentProgramIndex >= PortfolioWorkspace.GetProgramCount(state));
        Assert.Contains("Pass", PortfolioWorkspace.GetCurrentProgram(state).FileName);
    }

    [Fact]
    public void ContinualUpgradeLoop_KeepsRunAliveAfterSuccessfulApplication()
    {
        var engine = new SimulationEngine(SimulationConfig.ForDifficulty(GameDifficulty.ContinualUpgradeLoop));
        var state = engine.CreateNewRun();
        state.LinesOfCode = 140;
        state.CodeQuality = 80;
        state.Focus = 200;
        var fundsBeforeSuccess = state.Funds;

        engine.QueueIncidents(state, [new QueuedIncident("job-1", IncidentType.JobListing, "Job!")]);
        GrantResumeProofForListing(state);

        Assert.True(engine.ApplyAction(state, PlayerAction.ApplyForJob));

        while (state.ActiveJobApplication is not null &&
               !state.ActiveJobApplication.TakeHomeComplete)
        {
            Assert.True(engine.WorkOnJobApplication(state));
        }

        while (state.ActiveJobApplication is not null)
        {
            var question = state.ActiveJobApplication.Questions[state.ActiveJobApplication.CurrentQuestionIndex];
            Assert.True(engine.AnswerInterviewQuestion(state, question.CorrectOptionIndex));
        }

        Assert.Equal(GameDifficulty.ContinualUpgradeLoop, state.Difficulty);
        Assert.Equal(RunStatus.InProgress, state.Status);
        Assert.Equal(1, state.SuccessfulApplications);
        Assert.Null(state.OutcomeMessage);
        Assert.True(state.Funds > fundsBeforeSuccess);
        Assert.True(PortfolioWorkspace.HasFiniteProgramCount(state));
    }

    [Fact]
    public void EasyDifficulty_UsesEightPortfolioFiles()
    {
        var engine = new SimulationEngine(SimulationConfig.ForDifficulty(GameDifficulty.Easy));
        var state = engine.CreateNewRun();

        Assert.Equal(8, PortfolioWorkspace.GetProgramCount(state));
    }

    [Fact]
    public void MentorNudge_AddsResumeProofForTheActiveListingTrack()
    {
        var state = _engine.CreateNewRun();
        _engine.QueueIncidents(state, [new QueuedIncident("job-1", IncidentType.JobListing, "Job!")]);
        var track = state.ActiveJobListing!.ResumeTrack;

        _engine.QueueIncidents(state, [new QueuedIncident("mentor-1", IncidentType.MentorNudge, "Mentor!")]);

        Assert.Equal(1, GetResumeProof(state, track));
    }

    [Fact]
    public void ProjectBlueprint_CanBeReplannedBeforeTyping_ThenLocksForTheBatch()
    {
        var state = _engine.CreateNewRun();
        var originalTheme = state.CurrentProjectBlueprint.Theme;

        Assert.True(_engine.AdvanceProjectBlueprintField(state, ProjectPlanField.Theme));
        Assert.NotEqual(originalTheme, state.CurrentProjectBlueprint.Theme);

        Assert.True(_engine.ApplyAction(state, PlayerAction.WriteCode));
        Assert.False(_engine.AdvanceProjectBlueprintField(state, ProjectPlanField.Theme));
    }

    [Fact]
    public void CommitingChanges_CleansTheRepoAndRestoresPublishReadiness()
    {
        var state = _engine.CreateNewRun();
        state.Focus = 200;
        CompleteCurrentPortfolioBatch(_engine, state);
        state.VersionControl.PendingChangeLines = 14;

        Assert.False(_engine.IsPortfolioPublishReady(state));
        Assert.True(_engine.CommitChanges(state));
        Assert.Equal(0, state.VersionControl.PendingChangeLines);
        Assert.True(_engine.IsPortfolioPublishReady(state));
    }

    [Fact]
    public void CommitingChanges_ClearsTrackedCompletedFilesAndIncrementsCommitCount()
    {
        var state = _engine.CreateNewRun();
        state.Focus = 200;
        var safety = 0;
        while (state.RecentCompletedFileName is null && safety < 120)
        {
            Assert.True(_engine.ApplyAction(state, PlayerAction.WriteCode));
            safety++;
        }

        Assert.True(safety < 120);
        Assert.True(state.VersionControl.PendingChangeLines > 0);
        Assert.Equal(1, state.VersionControl.PendingCompletedFileCount);

        Assert.True(_engine.CommitChanges(state));
        Assert.Equal(0, state.VersionControl.PendingChangeLines);
        Assert.Equal(0, state.VersionControl.PendingCompletedFileCount);
        Assert.Equal(1, state.VersionControl.CommitCount);
    }

    [Fact]
    public void IndieMode_ProjectFileCompletion_CanPayBeforeTheReleaseShips()
    {
        var engine = new SimulationEngine(SimulationConfig.Create(GameDifficulty.Normal, GameplayLoopMode.Indie, realisticMode: false), () => 4242);
        var state = engine.CreateNewRun();
        state.Focus = 200;
        var startingFunds = state.Funds;
        var firstFileName = PortfolioWorkspace.GetCurrentProgram(state).FileName;
        var safety = 0;

        while (state.RecentCompletedFileName is null && safety < 120)
        {
            Assert.True(engine.ApplyAction(state, PlayerAction.WriteCode));
            safety++;
        }

        Assert.True(safety < 120);
        Assert.Equal(firstFileName, state.RecentCompletedFileName);
        Assert.True(state.Funds > startingFunds);
        Assert.Contains(state.EventLog, entry => entry.Contains("Indie project progress pays out", StringComparison.Ordinal));
    }

    [Fact]
    public void BankingGoals_CanEndTheRunWithHouseAndRetirementVictory()
    {
        var engine = new SimulationEngine(SimulationConfig.Create(GameDifficulty.Normal, GameplayLoopMode.Corporate, realisticMode: false));
        var state = engine.CreateNewRun();
        state.Funds = 1000;

        Assert.True(engine.MoveToApartment(state));
        Assert.True(state.HasApartment);
        Assert.True(engine.BuyHouse(state));
        Assert.True(state.HasHouse);
        Assert.True(engine.Retire(state));
        Assert.True(state.HasRetired);
        Assert.Equal(RunStatus.Won, state.Status);
    }

    [Fact]
    public void CorporateMode_StartsWithStricterBossAndHigherBasePay()
    {
        var corporateConfig = SimulationConfig.Create(GameDifficulty.Normal, GameplayLoopMode.Corporate, realisticMode: false);
        var indieConfig = SimulationConfig.Create(GameDifficulty.Normal, GameplayLoopMode.Indie, realisticMode: false);
        var engine = new SimulationEngine(corporateConfig, () => 4444);
        var state = engine.CreateNewRun();

        Assert.True(state.BossDisposition is BossDisposition.Mean or BossDisposition.Micromanager);
        Assert.True(corporateConfig.CorporateDailySalaryBase > indieConfig.IndieDailyIncomeBase);
    }

    [Fact]
    public void FounderMode_StartsWithNamingEvent_AndResolvesIntoStudioName()
    {
        var engine = new SimulationEngine(SimulationConfig.Create(GameDifficulty.Normal, GameplayLoopMode.Founder, realisticMode: false), () => 4444);
        var state = engine.CreateNewRun();

        var founderNaming = Assert.IsType<PendingLifeEvent>(state.PendingLifeEvent);
        Assert.Equal(IncidentType.FounderNaming, founderNaming.Type);
        Assert.Equal(3, founderNaming.OptionLabels.Length);

        Assert.True(engine.ResolveLifeEventOption(state, 1));
        Assert.Equal(founderNaming.OptionLabels[1], state.StudioName);
        Assert.False(string.IsNullOrWhiteSpace(state.StudioName));
    }

    [Fact]
    public void CorporateBossCheckIn_CanRaiseStanding()
    {
        var engine = new SimulationEngine(SimulationConfig.Create(GameDifficulty.Normal, GameplayLoopMode.Corporate, realisticMode: false));
        var state = engine.CreateNewRun();
        var idealOption = state.BossDisposition switch
        {
            BossDisposition.Supportive => 1,
            BossDisposition.Nice => 0,
            BossDisposition.Mean => 0,
            _ => 2,
        };

        engine.QueueIncidents(state, [new QueuedIncident("boss-1", IncidentType.BossCheckIn, "Boss wants a status check.")]);

        Assert.True(engine.HasPendingLifeEvent(state));
        Assert.True(engine.ResolveLifeEventOption(state, idealOption));
        Assert.True(state.CorporateStanding > 0);
    }

    [Fact]
    public void IndieFundingSwing_ChangesFunds()
    {
        var engine = new SimulationEngine(SimulationConfig.Create(GameDifficulty.Normal, GameplayLoopMode.Indie, realisticMode: false));
        var state = engine.CreateNewRun();
        var fundsBefore = state.Funds;

        engine.QueueIncidents(state, [new QueuedIncident("indie-funding-1", IncidentType.IndieFundingSwing, "Funding shifts.")]);

        Assert.NotEqual(fundsBefore, state.Funds);
    }

    [Fact]
    public void CreateNewRun_UsesProvidedSeed()
    {
        var engine = new SimulationEngine(SimulationConfig.Default, () => 123456);

        var state = engine.CreateNewRun();

        Assert.Equal(123456, state.RunSeed);
    }

    [Fact]
    public void RestartRun_ReusesTheCurrentSeedEvenWhenSeedProviderChanges()
    {
        var nextSeed = 1000;
        var engine = new SimulationEngine(SimulationConfig.Default, () => nextSeed++);
        var state = engine.CreateNewRun();

        Assert.Equal(1000, state.RunSeed);

        Assert.True(engine.ApplyAction(state, PlayerAction.RestartRun));

        Assert.Equal(1000, state.RunSeed);
    }

    [Fact]
    public void HomeCookedMeals_CannotBeExpedited_AndUseLongerPrepTime()
    {
        var state = _engine.CreateNewRun();

        var ordered = _engine.PlaceFoodOrder(state, FoodChoice.SkilletPasta, doubleCheckOrder: true, expeditedDelivery: true);

        Assert.True(ordered);
        var delivery = Assert.IsType<ActiveFoodDelivery>(state.ActiveFoodDelivery);
        Assert.False(delivery.Expedited);
        Assert.True(delivery.RemainingInGameMinutes > _engine.Config.FoodDeliveryDurationMinutes);
    }

    [Fact]
    public void PartnerCheckIn_CanBeResolved_AfterFindingLove()
    {
        var state = _engine.CreateNewRun();
        state.HasFoundLove = true;
        state.PartnerName = "Jordan";
        state.RelationshipProgress = _engine.Config.RelationshipProgressNeededForLove;
        state.Funds = 100;

        _engine.QueueIncidents(state, [new QueuedIncident("partner-1", IncidentType.PartnerCheckIn, "Check in!")]);

        Assert.True(_engine.HasPendingLifeEvent(state));
        Assert.True(_engine.ResolveLifeEventOption(state, 1));
        Assert.False(_engine.HasPendingLifeEvent(state));
        Assert.True(state.RelationshipProgress > _engine.Config.RelationshipProgressNeededForLove);
    }

    private static void GrantResumeProofForListing(RunState state)
    {
        var listing = Assert.IsType<ActiveJobListing>(state.ActiveJobListing);
        SetResumeProof(state, listing.ResumeTrack, listing.RequiredResumeProof);
    }

    private static void CompleteCurrentPortfolioBatch(SimulationEngine engine, RunState state)
    {
        var totalLines = 0;
        for (var index = 0; index < PortfolioWorkspace.GetProgramCount(state); index++)
        {
            state.CurrentProgramIndex = index;
            state.CurrentProgramVisibleLineCount = 0;
            totalLines += PortfolioWorkspace.GetCurrentProgram(state).TotalLinesOfCode;
        }

        state.CurrentPortfolioLinesOfCode = totalLines;
        state.LinesOfCode = Math.Max(state.LinesOfCode, totalLines);
        PortfolioWorkspace.SynchronizeToLinesOfCode(state);

        Assert.True(engine.IsPortfolioPublishReady(state));
    }

    private static int GetResumeProof(RunState state, ResumeTrack track)
    {
        return track switch
        {
            ResumeTrack.UI => state.UiResumeProof,
            ResumeTrack.Tooling => state.ToolingResumeProof,
            _ => state.GameplayResumeProof,
        };
    }

    private static void SetResumeProof(RunState state, ResumeTrack track, int amount)
    {
        switch (track)
        {
            case ResumeTrack.UI:
                state.UiResumeProof = amount;
                break;
            case ResumeTrack.Tooling:
                state.ToolingResumeProof = amount;
                break;
            default:
                state.GameplayResumeProof = amount;
                break;
        }
    }
}
