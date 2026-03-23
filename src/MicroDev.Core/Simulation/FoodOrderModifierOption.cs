namespace MicroDev.Core.Simulation;

public readonly record struct FoodOrderModifierOption(
    FoodOrderModifier Modifier,
    string Label,
    string Description,
    bool Recommended);
