namespace MicroDev.Core.Simulation;

public sealed class ActiveMergeConflict
{
    public string FileName { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string IncomingBranchName { get; set; } = string.Empty;

    public int OptimalResolutionOptionIndex { get; set; }

    public int Severity { get; set; }

    public ActiveMergeConflict Clone()
    {
        return new ActiveMergeConflict
        {
            FileName = FileName,
            Summary = Summary,
            IncomingBranchName = IncomingBranchName,
            OptimalResolutionOptionIndex = OptimalResolutionOptionIndex,
            Severity = Severity,
        };
    }
}
