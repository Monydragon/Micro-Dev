namespace MicroDev.Core.Simulation;

public readonly record struct FreelanceGigDefinition(
    FreelanceGigType Type,
    string Name,
    string Description,
    double DurationMinutes,
    double FundsGain,
    double FocusCost,
    double SanityCost,
    double CodeQualityGain);
