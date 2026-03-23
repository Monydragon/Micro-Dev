namespace MicroDev.Core.Portfolio;

public sealed class PortfolioProgramDefinition
{
    public PortfolioProgramDefinition(
        string projectName,
        string fileName,
        string description,
        IReadOnlyList<string> codeLines)
    {
        ProjectName = projectName;
        FileName = fileName;
        Description = description;
        CodeLines = codeLines;
        TotalLinesOfCode = codeLines.Count(static line => !string.IsNullOrWhiteSpace(line));
    }

    public string ProjectName { get; }

    public string FileName { get; }

    public string Description { get; }

    public IReadOnlyList<string> CodeLines { get; }

    public int TotalLinesOfCode { get; }
}
