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

    public double StartingMinutesSinceLastMeal { get; init; } = 4 * 60;

    public double StartingMinutesSinceLastSleep { get; init; } = 0;

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

    public double HungryAfterMinutes { get; init; } = 10 * 60;

    public double VeryHungryAfterMinutes { get; init; } = 16 * 60;

    public double StarvingAfterMinutes { get; init; } = 24 * 60;

    public double HungrySanityLossPerInGameMinute { get; init; } = 0.008;

    public double VeryHungrySanityLossPerInGameMinute { get; init; } = 0.015;

    public double StarvingSanityLossPerInGameMinute { get; init; } = 0.025;

    public double SleepWarningAfterMinutes { get; init; } = 18 * 60;

    public double SleepDeprivationAfterMinutes { get; init; } = 24 * 60;

    public double SevereSleepDeprivationAfterMinutes { get; init; } = 36 * 60;

    public double SleepForcedAfterMinutes { get; init; } = 48 * 60;

    public double SleepDeprivationSanityLossPerInGameMinute { get; init; } = 0.01;

    public double SleepDeprivationQualityLossPerInGameMinute { get; init; } = 0.015;

    public double SevereSleepDeprivationSanityLossPerInGameMinute { get; init; } = 0.02;

    public double SevereSleepDeprivationQualityLossPerInGameMinute { get; init; } = 0.03;

    public double BurgerFundsCost { get; init; } = 12;

    public double BurgerFocusGain { get; init; } = 72;

    public double BurgerSanityGain { get; init; } = 2;

    public double BurgerSluggishDurationMinutes { get; init; } = 150;

    public double BurritoFundsCost { get; init; } = 10;

    public double BurritoFocusGain { get; init; } = 66;

    public double BurritoSanityGain { get; init; } = 7;

    public double BurritoSluggishDurationMinutes { get; init; } = 105;

    public double PizzaFundsCost { get; init; } = 15;

    public double PizzaFocusGain { get; init; } = 82;

    public double PizzaSanityGain { get; init; } = 0;

    public double PizzaSluggishDurationMinutes { get; init; } = 195;

    public double DumplingsFundsCost { get; init; } = 17;

    public double DumplingsFocusGain { get; init; } = 58;

    public double DumplingsSanityGain { get; init; } = 14;

    public double DumplingsSluggishDurationMinutes { get; init; } = 60;

    public double FoodDeliveryDurationMinutes { get; init; } = 30;

    public double ExpeditedFoodDeliveryDurationMinutes { get; init; } = 10;

    public double ExpeditedFoodDeliveryTipAmount { get; init; } = 6;

    public double ComputerFreezeSelfRepairDurationMinutes { get; init; } = 75;

    public double ComputerFreezeSelfRepairSanityLoss { get; init; } = 8;

    public double ComputerFreezeSelfRepairFocusLoss { get; init; } = 4;

    public double ComputerFreezeTechSupportDurationMinutes { get; init; } = 45;

    public double ComputerFreezeTechSupportSanityLoss { get; init; } = 4;

    public double ComputerFreezeTechSupportFundsCost { get; init; } = 18;

    public double ComputerFreezeRepairShopDurationMinutes { get; init; } = 120;

    public double ComputerFreezeRepairShopSanityLoss { get; init; } = 2;

    public double ComputerFreezeRepairShopFundsCost { get; init; } = 36;

    public double StreamingBingeDurationMinutes { get; init; } = 105;

    public double StreamingBingeSanityGain { get; init; } = 8;

    public double StreamingBingeFocusLoss { get; init; } = 6;

    public double StreamingEpisodeDurationMinutes { get; init; } = 42;

    public double StreamingEpisodeSanityGain { get; init; } = 3;

    public double StreamingEpisodeFocusLoss { get; init; } = 2;

    public double StreamingTurnOffSanityLoss { get; init; } = 1;

    public double OnlineMatchMessageDurationMinutes { get; init; } = 20;

    public double OnlineMatchMessageSanityGain { get; init; } = 3;

    public double OnlineMatchMessageFocusLoss { get; init; } = 2;

    public int OnlineMatchMessageRelationshipGain { get; init; } = 1;

    public double OnlineDateDurationMinutes { get; init; } = 120;

    public double OnlineDateFundsCost { get; init; } = 18;

    public double OnlineDateSanityGain { get; init; } = 10;

    public double OnlineDateFocusLoss { get; init; } = 6;

    public int OnlineDateRelationshipGain { get; init; } = 2;

    public double OnlineMatchIgnoreSanityLoss { get; init; } = 1;

    public int RelationshipProgressNeededForLove { get; init; } = 4;

    public double FoundLovePassiveSanityRegenPerInGameMinute { get; init; } = 0.004;

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

    public double SleepFocusGain { get; init; } = 100;

    public double SleepSanityGain { get; init; } = 18;

    public double DailyBillAmount { get; init; } = 40;

    public int MaxEventLogEntries { get; init; } = 12;

    public int CatPatsRequired { get; init; } = 8;

    public double CatStayDurationMinutes { get; init; } = 150;

    public int CatLinesDeletionPenalty { get; init; } = 25;

    public double CatTypingBurstIntervalMinutes { get; init; } = 20;

    public double CatBugQualityLossPerBurst { get; init; } = 1.5;

    public int CatBugLinesPerBurst { get; init; } = 1;

    public int CatGibberishLinesPerBurst { get; init; } = 2;

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

    public double PublishAppFundsMin { get; init; } = 46;

    public double PublishAppFundsMax { get; init; } = 82;

    public double PublishedAppSaleFundsMin { get; init; } = 8;

    public double PublishedAppSaleFundsMax { get; init; } = 22;

    public double PublishedAppSaleIntervalMinMinutes { get; init; } = 180;

    public double PublishedAppSaleIntervalMaxMinutes { get; init; } = 360;

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
                PublishAppFundsMin = 56,
                PublishAppFundsMax = 92,
                PublishedAppSaleFundsMin = 10,
                PublishedAppSaleFundsMax = 24,
                PublishedAppSaleIntervalMinMinutes = 150,
                PublishedAppSaleIntervalMaxMinutes = 300,
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
                PublishAppFundsMin = 38,
                PublishAppFundsMax = 68,
                PublishedAppSaleFundsMin = 6,
                PublishedAppSaleFundsMax = 18,
                PublishedAppSaleIntervalMinMinutes = 210,
                PublishedAppSaleIntervalMaxMinutes = 420,
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
                PublishAppFundsMin = 52,
                PublishAppFundsMax = 88,
                PublishedAppSaleFundsMin = 9,
                PublishedAppSaleFundsMax = 24,
                PublishedAppSaleIntervalMinMinutes = 165,
                PublishedAppSaleIntervalMaxMinutes = 315,
            },
            GameDifficulty.ContinualUpgradeLoop => new SimulationConfig
            {
                Difficulty = difficulty,
                PortfolioTemplateCount = 12,
                ContinueAfterSuccessfulApplication = true,
                SuccessfulApplicationFundsReward = 48,
                SuccessfulApplicationSanityReward = 6,
                FirstGuaranteedJobDelayMinutes = 330,
                GuaranteedJobListingIntervalMinutes = 300,
                FirstModifierIncidentDelayMinutes = 135,
                ModifierIncidentIntervalMinutes = 180,
                PublishAppFundsMin = 52,
                PublishAppFundsMax = 90,
                PublishedAppSaleFundsMin = 10,
                PublishedAppSaleFundsMax = 24,
                PublishedAppSaleIntervalMinMinutes = 150,
                PublishedAppSaleIntervalMaxMinutes = 300,
            },
            _ => new SimulationConfig
            {
                Difficulty = difficulty,
                PortfolioTemplateCount = 12,
                FirstGuaranteedJobDelayMinutes = 540,
                GuaranteedJobListingIntervalMinutes = 360,
                ApplicationChallengeRequiredLines = 10,
                ApplicationInterviewMinimumCorrectAnswers = 2,
                PublishAppFundsMin = 46,
                PublishAppFundsMax = 82,
                PublishedAppSaleFundsMin = 8,
                PublishedAppSaleFundsMax = 22,
                PublishedAppSaleIntervalMinMinutes = 180,
                PublishedAppSaleIntervalMaxMinutes = 360,
            },
        };
    }
}
