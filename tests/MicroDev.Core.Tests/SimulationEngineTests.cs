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
        Assert.Equal(63, state.Funds, 3);
        Assert.Equal(60, state.Focus, 3);
        Assert.Equal(_engine.Config.BurgerSluggishDurationMinutes, state.SluggishMinutesRemaining, 3);
        Assert.True(_engine.IsSluggish(state));
    }

    [Fact]
    public void PlaceFoodOrder_WithOrderNote_AvoidsSluggishPenalty_AndCanRaiseSanity()
    {
        var state = _engine.CreateNewRun();

        var ordered = _engine.PlaceFoodOrder(state, FoodChoice.Burrito, doubleCheckOrder: true);

        Assert.True(ordered);
        Assert.Equal(65, state.Funds, 3);
        Assert.Equal(86, state.Focus, 3);
        Assert.Equal(72, state.Sanity, 3);
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
        Assert.Equal(54, state.Focus, 3);
        Assert.Equal(36, state.Sanity, 3);
    }

    [Fact]
    public void CreateNewRun_StartsOnBlankFirstPortfolioFile()
    {
        var state = _engine.CreateNewRun();

        Assert.Equal("TaskQueue.cs", PortfolioWorkspace.GetCurrentProgram(state).FileName);
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
        Assert.Contains("using System;", visibleLines);
    }

    [Fact]
    public void WriteCode_IsReducedWhileSluggish()
    {
        var state = _engine.CreateNewRun();
        _engine.PlaceFoodOrder(state, FoodChoice.Burger, doubleCheckOrder: false);

        var applied = _engine.ApplyAction(state, PlayerAction.WriteCode);

        Assert.True(applied);
        Assert.Equal(1, state.LinesOfCode);
        Assert.Equal(87.25, state.Focus, 3);
        Assert.Equal(100, state.CodeQuality, 3);
    }

    [Fact]
    public void WriteCode_AdvancesIntoNextPortfolioFile()
    {
        var state = _engine.CreateNewRun();
        state.Focus = 200;
        var safety = 0;

        while (state.CurrentProgramIndex == 0 && safety < 60)
        {
            Assert.True(_engine.ApplyAction(state, PlayerAction.WriteCode));
            safety++;
        }

        Assert.True(safety < 60);
        Assert.Equal("DialogueFormatter.cs", PortfolioWorkspace.GetCurrentProgram(state).FileName);
        Assert.Equal(0, state.CurrentProgramVisibleLineCount);
        Assert.Equal("TaskQueue.cs", state.RecentCompletedFileName);
        Assert.True(state.FileCompletionCelebrationMinutesRemaining > 0);

        Assert.True(_engine.ApplyAction(state, PlayerAction.WriteCode));
        Assert.Contains("using System.Collections.Generic;", PortfolioWorkspace.GetVisibleLines(state));
    }

    [Fact]
    public void PurchaseUpgrade_ImprovesWriteEfficiency()
    {
        var state = _engine.CreateNewRun();
        state.Funds = 200;

        var purchased = _engine.PurchaseUpgrade(state, EfficiencyUpgradeType.MechanicalKeyboard);

        Assert.True(purchased);
        Assert.Contains(EfficiencyUpgradeType.MechanicalKeyboard, state.PurchasedUpgrades);
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
    public void TakeFreelanceGig_PipelineRescue_PaysMoreButCostsMore()
    {
        var state = _engine.CreateNewRun();

        var taken = _engine.TakeFreelanceGig(state, FreelanceGigType.PipelineRescue);

        Assert.True(taken);
        Assert.Equal(143, state.Funds, 3);
        Assert.Equal("09:45", state.ClockText);
        Assert.Equal(51.375, state.Focus, 3);
        Assert.Equal(58.63, state.Sanity, 3);
        Assert.Equal(100, state.CodeQuality, 3);
    }

    [Fact]
    public void Scheduler_QueuesIncidents_AsDeskTimeCrossesThresholds()
    {
        var state = _engine.CreateNewRun();

        var firstWave = _scheduler.Update(state, 130);
        var secondWave = _scheduler.Update(state, 120);
        var thirdWave = _scheduler.Update(state, 200);

        Assert.Single(firstWave);
        Assert.Equal("bug-1", firstWave[0].Id);
        Assert.Single(secondWave);
        Assert.Equal("cat-1", secondWave[0].Id);
        Assert.Single(thirdWave);
        Assert.Equal("job-1", thirdWave[0].Id);
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
    public void CatTimeout_DeletesLinesOfCodeWhenIgnored()
    {
        var state = _engine.CreateNewRun();
        state.Focus = 200;
        while (state.LinesOfCode < 30)
        {
            Assert.True(_engine.ApplyAction(state, PlayerAction.WriteCode));
        }

        _engine.QueueIncidents(state, [new QueuedIncident("cat-1", IncidentType.CatInterruption, "Cat!")]);

        _engine.AdvanceTime(state, _engine.Config.CatStayDurationMinutes);

        Assert.Null(state.ActiveCatInterruption);
        Assert.Equal(0, state.CurrentProgramIndex);
        Assert.Equal(5, state.LinesOfCode);
        Assert.Contains("namespace Portfolio.Timeboxing;", PortfolioWorkspace.GetVisibleLines(state));
    }

    [Fact]
    public void PetCat_RepeatedEnoughTimes_ClearsInterruptionWithoutPenalty()
    {
        var state = _engine.CreateNewRun();
        state.LinesOfCode = 80;
        _engine.QueueIncidents(state, [new QueuedIncident("cat-1", IncidentType.CatInterruption, "Cat!")]);

        for (var index = 0; index < _engine.Config.CatPatsRequired; index++)
        {
            Assert.True(_engine.ApplyAction(state, PlayerAction.PetCat));
        }

        Assert.Null(state.ActiveCatInterruption);
        Assert.Equal(80, state.LinesOfCode);
    }

    [Fact]
    public void ApplyForJob_WinsRunWhenPortfolioAndQualityMeetThresholds()
    {
        var state = _engine.CreateNewRun();
        state.LinesOfCode = 120;
        state.CodeQuality = 80;
        _engine.QueueIncidents(state, [new QueuedIncident("job-1", IncidentType.JobListing, "Job!")]);

        var applied = _engine.ApplyAction(state, PlayerAction.ApplyForJob);

        Assert.True(applied);
        Assert.Equal(RunStatus.Won, state.Status);
        Assert.Equal(1, state.SuccessfulApplications);
        Assert.Null(state.ActiveJobListing);
        Assert.Contains("callback", state.OutcomeMessage);
    }

    [Fact]
    public void ApplyForJob_RejectedWhenRequirementsAreNotMet()
    {
        var state = _engine.CreateNewRun();
        state.LinesOfCode = 90;
        state.CodeQuality = 50;
        _engine.QueueIncidents(state, [new QueuedIncident("job-1", IncidentType.JobListing, "Job!")]);

        var applied = _engine.ApplyAction(state, PlayerAction.ApplyForJob);

        Assert.True(applied);
        Assert.Equal(RunStatus.InProgress, state.Status);
        Assert.Null(state.ActiveJobListing);
        Assert.Equal(55, state.LinesOfCode);
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
        Assert.Equal(67.76, state.Sanity, 3);
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

        _engine.TakeFreelanceGig(state, FreelanceGigType.QuickBugfix);

        Assert.Equal(RunStatus.BurnedOut, state.Status);
        Assert.Contains("tank", state.OutcomeMessage);
    }
}
