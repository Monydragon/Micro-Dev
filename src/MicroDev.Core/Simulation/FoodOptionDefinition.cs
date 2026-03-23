namespace MicroDev.Core.Simulation;

public readonly record struct FoodOptionDefinition(
    FoodChoice Choice,
    string Name,
    string Description,
    double FundsCost,
    double FocusGain,
    double SanityGain,
    double SluggishMinutesWhenUnchecked);
