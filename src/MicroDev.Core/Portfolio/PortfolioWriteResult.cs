namespace MicroDev.Core.Portfolio;

public readonly record struct PortfolioWriteResult(
    int LinesAdded,
    string? CompletedFileName,
    string? StartedFileName);
