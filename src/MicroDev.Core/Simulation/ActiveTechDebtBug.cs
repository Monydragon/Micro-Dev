namespace MicroDev.Core.Simulation;

public sealed class ActiveTechDebtBug
{
    public string Summary { get; set; } = string.Empty;

    public string CompilerHint { get; set; } = string.Empty;

    public string[] CodeLines { get; set; } = [];

    public int HighlightLineIndex { get; set; }

    public int HighlightStartIndex { get; set; }

    public int HighlightLength { get; set; }

    public string HighlightToken { get; set; } = string.Empty;

    public bool HighlightIsInsertion { get; set; }

    public bool IsNeedDriven { get; set; }

    public double RemainingInGameMinutes { get; set; }

    public double QualityDrainPerMinute { get; set; }

    public ActiveTechDebtBug Clone()
    {
        return new ActiveTechDebtBug
        {
            Summary = Summary,
            CompilerHint = CompilerHint,
            CodeLines = [.. CodeLines],
            HighlightLineIndex = HighlightLineIndex,
            HighlightStartIndex = HighlightStartIndex,
            HighlightLength = HighlightLength,
            HighlightToken = HighlightToken,
            HighlightIsInsertion = HighlightIsInsertion,
            IsNeedDriven = IsNeedDriven,
            RemainingInGameMinutes = RemainingInGameMinutes,
            QualityDrainPerMinute = QualityDrainPerMinute,
        };
    }
}
