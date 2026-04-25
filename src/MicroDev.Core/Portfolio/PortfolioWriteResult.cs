namespace MicroDev.Core.Portfolio;

public readonly record struct PortfolioWriteResult(
    int LinesAdded,
    int CharactersAdded,
    string? CompletedFileName,
    string? StartedFileName);
