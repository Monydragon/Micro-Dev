var captureArg = Environment.GetCommandLineArgs()
    .Skip(1)
    .FirstOrDefault(static argument => argument.StartsWith("--capture-ui", StringComparison.OrdinalIgnoreCase));
string? captureDirectory = null;
if (!string.IsNullOrWhiteSpace(captureArg))
{
    var separatorIndex = captureArg.IndexOf('=');
    captureDirectory = separatorIndex >= 0 && separatorIndex < captureArg.Length - 1
        ? captureArg[(separatorIndex + 1)..]
        : Path.Combine("promotional", "ui-overhaul", "2026-03-24");
}

using var game = new MicroDev.Core.MicroDevGame(captureDirectory);
game.Run();
