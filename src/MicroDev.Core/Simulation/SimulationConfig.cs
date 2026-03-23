namespace MicroDev.Core.Simulation;

public sealed class SimulationConfig
{
    public const int MinutesPerDay = 24 * 60;

    public static SimulationConfig Default { get; } = new();

    public int StartingDay { get; init; } = 1;

    public double StartingTimeOfDayMinutes { get; init; } = 8 * 60;

    public double StartingFunds { get; init; } = 75;

    public double StartingFocus { get; init; } = 70;

    public double StartingSanity { get; init; } = 70;

    public bool StartWithFirstCoin { get; init; } = true;

    public int StartingLinesOfCode { get; init; } = 0;

    public double StartingCodeQuality { get; init; } = 100;

    public double MaxFocus { get; init; } = 100;

    public double MaxSanity { get; init; } = 100;

    public double MaxCodeQuality { get; init; } = 100;

    public double InGameMinutesPerRealSecond { get; init; } = 10;

    public double PassiveFocusDrainPerInGameMinute { get; init; } = 0.025;

    public int WriteCodeLinesGain { get; init; } = 1;

    public double WriteCodeFocusCost { get; init; } = 2;

    public double WriteCodeQualityGain { get; init; } = 0.15;

    public double SluggishLinesMultiplier { get; init; } = 0.5;

    public double SluggishQualityMultiplier { get; init; } = 0.5;

    public double SluggishFocusCostPenalty { get; init; } = 0.75;

    public double BurgerFundsCost { get; init; } = 12;

    public double BurgerFocusGain { get; init; } = 20;

    public double BurgerSanityGain { get; init; } = 0;

    public double BurgerSluggishDurationMinutes { get; init; } = 180;

    public double BurritoFundsCost { get; init; } = 10;

    public double BurritoFocusGain { get; init; } = 16;

    public double BurritoSanityGain { get; init; } = 2;

    public double BurritoSluggishDurationMinutes { get; init; } = 120;

    public double PizzaFundsCost { get; init; } = 15;

    public double PizzaFocusGain { get; init; } = 24;

    public double PizzaSanityGain { get; init; } = -2;

    public double PizzaSluggishDurationMinutes { get; init; } = 240;

    public double DumplingsFundsCost { get; init; } = 17;

    public double DumplingsFocusGain { get; init; } = 14;

    public double DumplingsSanityGain { get; init; } = 6;

    public double DumplingsSluggishDurationMinutes { get; init; } = 75;

    public double FreelanceDurationMinutes { get; init; } = 60;

    public double FreelanceFundsGain { get; init; } = 35;

    public double FreelanceFocusCost { get; init; } = 10;

    public double FreelanceSanityCost { get; init; } = 8;

    public double QuickBugfixDurationMinutes { get; init; } = 45;

    public double QuickBugfixFundsGain { get; init; } = 26;

    public double QuickBugfixFocusCost { get; init; } = 7;

    public double QuickBugfixSanityCost { get; init; } = 6;

    public double QuickBugfixQualityGain { get; init; } = 2;

    public double UiPolishDurationMinutes { get; init; } = 70;

    public double UiPolishFundsGain { get; init; } = 42;

    public double UiPolishFocusCost { get; init; } = 10;

    public double UiPolishSanityCost { get; init; } = 7;

    public double UiPolishQualityGain { get; init; } = 0;

    public double PipelineRescueDurationMinutes { get; init; } = 105;

    public double PipelineRescueFundsGain { get; init; } = 68;

    public double PipelineRescueFocusCost { get; init; } = 16;

    public double PipelineRescueSanityCost { get; init; } = 12;

    public double PipelineRescueQualityGain { get; init; } = 4;

    public double SleepDurationMinutes { get; init; } = 8 * 60;

    public double SleepFocusGain { get; init; } = 60;

    public double SleepSanityGain { get; init; } = 25;

    public double DailyBillAmount { get; init; } = 40;

    public int MaxEventLogEntries { get; init; } = 12;

    public int CatPatsRequired { get; init; } = 8;

    public double CatStayDurationMinutes { get; init; } = 150;

    public int CatLinesDeletionPenalty { get; init; } = 25;

    public double TechDebtDurationMinutes { get; init; } = 180;

    public double TechDebtQualityDrainPerMinute { get; init; } = 0.08;

    public double SquashBugFocusCost { get; init; } = 6;

    public double JobListingDurationMinutes { get; init; } = 6 * 60;

    public int JobResumeCostLines { get; init; } = 35;

    public int JobMinimumPortfolioLines { get; init; } = 100;

    public double JobMinimumCodeQuality { get; init; } = 55;

    public double FileCompletionCelebrationMinutes { get; init; } = 90;

    public double FirstCoinPassiveSanityRegenPerInGameMinute { get; init; } = 0.006;

    public double FirstCoinBreakSanityLoss { get; init; } = 8;

    public double FirstCoinEmergencyFundsGain { get; init; } = 25;
}
