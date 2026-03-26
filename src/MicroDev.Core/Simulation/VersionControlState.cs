using System;

namespace MicroDev.Core.Simulation;

public sealed class VersionControlState
{
    public string MainBranchName { get; set; } = "main";

    public string CurrentBranchName { get; set; } = "main";

    public int CommitCount { get; set; }

    public int FeatureBranchCommitCount { get; set; }

    public int PendingChangeLines { get; set; }

    public int PendingCompletedFileCount { get; set; }

    public int CommittedPortfolioLinesOfCode { get; set; }

    public int BranchSerial { get; set; }

    public int MergeConflictCount { get; set; }

    public string? LastCommitSummary { get; set; }

    public ActiveMergeConflict? ActiveMergeConflict { get; set; }

    public bool HasFeatureBranch =>
        !string.Equals(CurrentBranchName, MainBranchName, StringComparison.Ordinal);

    public VersionControlState Clone()
    {
        return new VersionControlState
        {
            MainBranchName = MainBranchName,
            CurrentBranchName = CurrentBranchName,
            CommitCount = CommitCount,
            FeatureBranchCommitCount = FeatureBranchCommitCount,
            PendingChangeLines = PendingChangeLines,
            PendingCompletedFileCount = PendingCompletedFileCount,
            CommittedPortfolioLinesOfCode = CommittedPortfolioLinesOfCode,
            BranchSerial = BranchSerial,
            MergeConflictCount = MergeConflictCount,
            LastCommitSummary = LastCommitSummary,
            ActiveMergeConflict = ActiveMergeConflict?.Clone(),
        };
    }
}
