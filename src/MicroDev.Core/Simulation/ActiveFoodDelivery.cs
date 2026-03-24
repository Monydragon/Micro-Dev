namespace MicroDev.Core.Simulation;

public sealed class ActiveFoodDelivery
{
    public FoodChoice Choice { get; set; }

    public bool Expedited { get; set; }

    public bool ReviewReceipt { get; set; }

    public double TipAmount { get; set; }

    public double TotalFundsCost { get; set; }

    public double RemainingInGameMinutes { get; set; }

    public List<FoodOrderModifier> SelectedModifiers { get; } = [];

    public ActiveFoodDelivery Clone()
    {
        var clone = new ActiveFoodDelivery
        {
            Choice = Choice,
            Expedited = Expedited,
            ReviewReceipt = ReviewReceipt,
            TipAmount = TipAmount,
            TotalFundsCost = TotalFundsCost,
            RemainingInGameMinutes = RemainingInGameMinutes,
        };

        clone.SelectedModifiers.AddRange(SelectedModifiers);
        return clone;
    }
}
