namespace MicroDev.Core.Simulation;

public sealed class ActiveCatInterruption
{
    public int PatsRemaining { get; set; }

    public double RemainingInGameMinutes { get; set; }

    public int LinesDeletionPenalty { get; set; }

    public ActiveCatInterruption Clone()
    {
        return new ActiveCatInterruption
        {
            PatsRemaining = PatsRemaining,
            RemainingInGameMinutes = RemainingInGameMinutes,
            LinesDeletionPenalty = LinesDeletionPenalty,
        };
    }
}
