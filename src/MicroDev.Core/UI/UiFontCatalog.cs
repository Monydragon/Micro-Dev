namespace MicroDev.Core.UI;

public static class UiFontCatalog
{
    private static readonly IReadOnlyList<UiFontOption> FontOptions = Array.AsReadOnly(
    [
        UiFontOption.Consolas,
        UiFontOption.CascadiaMono,
        UiFontOption.CascadiaCode,
        UiFontOption.CourierNew,
        UiFontOption.LucidaConsole,
        UiFontOption.Bahnschrift,
    ]);

    public static IReadOnlyList<UiFontOption> All => FontOptions;

    public static string GetDisplayName(UiFontOption option)
    {
        return option switch
        {
            UiFontOption.CascadiaMono => "Cascadia Mono",
            UiFontOption.CascadiaCode => "Cascadia Code",
            UiFontOption.CourierNew => "Courier New",
            UiFontOption.LucidaConsole => "Lucida Console",
            UiFontOption.Bahnschrift => "Bahnschrift",
            _ => "Consolas",
        };
    }

    public static string GetAssetName(UiFontOption option)
    {
        return option switch
        {
            UiFontOption.CascadiaMono => "Fonts/CascadiaMono",
            UiFontOption.CascadiaCode => "Fonts/CascadiaCode",
            UiFontOption.CourierNew => "Fonts/CourierNew",
            UiFontOption.LucidaConsole => "Fonts/LucidaConsole",
            UiFontOption.Bahnschrift => "Fonts/Bahnschrift",
            _ => "Fonts/Consolas",
        };
    }
}
