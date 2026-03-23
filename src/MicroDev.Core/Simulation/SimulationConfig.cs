namespace MicroDev.Core.Simulation;

public sealed class SimulationConfig
{
    public const int MinutesPerDay = 24 * 60;

    public static SimulationConfig Default { get; } = ForDifficulty(GameDifficulty.Normal);

    public GameDifficulty Difficulty { get; init; } = GameDifficulty.Normal;

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

    public double DeepWorkDurationMinutes { get; init; } = 90;

    public int DeepWorkBonusLinesPerClick { get; init; } = 1;

    public double DeepWorkBonusQualityGain { get; init; } = 0.15;

    public double ContextSwitchDurationMinutes { get; init; } = 75;

    public double ContextSwitchFocusCostPenalty { get; init; } = 0.6;

    public double ContextSwitchPassiveFocusDrainPerInGameMinute { get; init; } = 0.02;

    public double CoffeeBounceFocusGain { get; init; } = 16;

    public double CoffeeBounceSanityGain { get; init; } = 3;

    public double MentorNudgeQualityGain { get; init; } = 4;

    public double ExpenseSpikeFundsLoss { get; init; } = 14;

    public double RubberDuckInsightQualityGain { get; init; } = 3;

    public int RubberDuckInsightPrepGain { get; init; } = 1;

    public double MicroSaleFundsGain { get; init; } = 10;

    public double MicroSaleSanityGain { get; init; } = 2;

    public double DoomscrollFocusLoss { get; init; } = 9;

    public double DoomscrollSanityLoss { get; init; } = 4;

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

    public int PortfolioTemplateCount { get; init; } = 12;

    public bool EndlessPortfolio { get; init; }

    public double FirstGuaranteedJobDelayMinutes { get; init; } = 360;

    public double GuaranteedJobListingIntervalMinutes { get; init; } = 420;

    public double FirstModifierIncidentDelayMinutes { get; init; } = 150;

    public double ModifierIncidentIntervalMinutes { get; init; } = 210;

    public int ApplicationChallengeRequiredLines { get; init; } = 10;

    public int ApplicationInterviewMinimumCorrectAnswers { get; init; } = 2;

    public int InterviewPrepPointsPerBonus { get; init; } = 2;

    public int MaxInterviewPrepBonus { get; init; } = 2;

    public double SuccessfulApplicationFundsReward { get; init; } = 0;

    public double SuccessfulApplicationSanityReward { get; init; } = 0;

    public bool ContinueAfterSuccessfulApplication { get; init; }

    public double FileCompletionCelebrationMinutes { get; init; } = 90;

    public double FirstCoinPassiveSanityRegenPerInGameMinute { get; init; } = 0.006;

    public double FirstCoinBreakSanityLoss { get; init; } = 8;

    public double FirstCoinEmergencyFundsGain { get; init; } = 25;

    public static SimulationConfig ForDifficulty(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => new SimulationConfig
            {
                Difficulty = difficulty,
                StartingFunds = 90,
                StartingFocus = 78,
                StartingSanity = 78,
                PortfolioTemplateCount = 8,
                JobListingDurationMinutes = 8 * 60,
                JobResumeCostLines = 24,
                JobMinimumPortfolioLines = 70,
                JobMinimumCodeQuality = 45,
                FirstGuaranteedJobDelayMinutes = 180,
                GuaranteedJobListingIntervalMinutes = 240,
                FirstModifierIncidentDelayMinutes = 210,
                ModifierIncidentIntervalMinutes = 300,
                ApplicationChallengeRequiredLines = 8,
                ApplicationInterviewMinimumCorrectAnswers = 1,
            },
            GameDifficulty.Hard => new SimulationConfig
            {
                Difficulty = difficulty,
                StartingFunds = 65,
                StartingFocus = 66,
                StartingSanity = 64,
                PassiveFocusDrainPerInGameMinute = 0.03,
                DailyBillAmount = 45,
                PortfolioTemplateCount = 16,
                JobListingDurationMinutes = 5 * 60,
                JobResumeCostLines = 40,
                JobMinimumPortfolioLines = 120,
                JobMinimumCodeQuality = 62,
                FirstGuaranteedJobDelayMinutes = 420,
                GuaranteedJobListingIntervalMinutes = 480,
                FirstModifierIncidentDelayMinutes = 105,
                ModifierIncidentIntervalMinutes = 150,
                ApplicationChallengeRequiredLines = 12,
                ApplicationInterviewMinimumCorrectAnswers = 2,
            },
            GameDifficulty.Endless => new SimulationConfig
            {
                Difficulty = difficulty,
                PortfolioTemplateCount = 16,
                EndlessPortfolio = true,
                FirstGuaranteedJobDelayMinutes = 240,
                GuaranteedJobListingIntervalMinutes = 300,
                FirstModifierIncidentDelayMinutes = 120,
                ModifierIncidentIntervalMinutes = 180,
                ApplicationChallengeRequiredLines = 10,
                ApplicationInterviewMinimumCorrectAnswers = 2,
                ContinueAfterSuccessfulApplication = true,
                SuccessfulApplicationFundsReward = 55,
                SuccessfulApplicationSanityReward = 8,
            },
            _ => new SimulationConfig
            {
                Difficulty = difficulty,
                PortfolioTemplateCount = 12,
                FirstGuaranteedJobDelayMinutes = 540,
                GuaranteedJobListingIntervalMinutes = 360,
                ApplicationChallengeRequiredLines = 10,
                ApplicationInterviewMinimumCorrectAnswers = 2,
            },
        };
    }
}
