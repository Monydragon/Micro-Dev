namespace MicroDev.Core.Simulation;

public sealed class ActiveFreelanceGig
{
    public FreelanceGigType Type { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Brief { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public double DurationMinutes { get; set; }

    public double FundsGain { get; set; }

    public double FocusCost { get; set; }

    public double SanityCost { get; set; }

    public double CodeQualityGain { get; set; }

    public int VisibleLineCount { get; set; }

    public List<string> CodeLines { get; } = [];

    public bool IsComplete => VisibleLineCount >= CodeLines.Count;

    public ActiveFreelanceGig Clone()
    {
        var clone = new ActiveFreelanceGig
        {
            Type = Type,
            ClientName = ClientName,
            Title = Title,
            Brief = Brief,
            FileName = FileName,
            DurationMinutes = DurationMinutes,
            FundsGain = FundsGain,
            FocusCost = FocusCost,
            SanityCost = SanityCost,
            CodeQualityGain = CodeQualityGain,
            VisibleLineCount = VisibleLineCount,
        };

        clone.CodeLines.AddRange(CodeLines);
        return clone;
    }
}
