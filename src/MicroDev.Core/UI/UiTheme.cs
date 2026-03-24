using Microsoft.Xna.Framework;
using MicroDev.Core.Simulation;

namespace MicroDev.Core.UI;

public static class UiTheme
{
    private static readonly UiPalette DarkPalette = new(
        new Color(9, 16, 28),
        new Color(19, 49, 78),
        new Color(18, 27, 42),
        new Color(26, 38, 58),
        new Color(77, 106, 142),
        new Color(14, 21, 34),
        new Color(8, 14, 24),
        new Color(73, 120, 182),
        new Color(4, 10, 18, 208),
        new Color(233, 240, 249),
        new Color(138, 156, 180),
        new Color(89, 201, 255),
        new Color(44, 97, 143),
        new Color(84, 197, 255),
        new Color(121, 217, 142),
        new Color(184, 149, 255),
        new Color(120, 226, 158),
        new Color(255, 194, 84),
        new Color(255, 112, 112),
        new Color(245, 170, 87),
        new Color(228, 206, 116),
        new Color(36, 49, 70),
        new Color(50, 69, 97),
        new Color(57, 91, 131),
        new Color(35, 39, 49));

    private static readonly UiPalette LightPalette = new(
        new Color(238, 243, 249),
        new Color(207, 225, 243),
        new Color(249, 252, 255),
        new Color(230, 238, 247),
        new Color(145, 169, 198),
        new Color(220, 230, 240),
        new Color(245, 249, 254),
        new Color(94, 146, 206),
        new Color(225, 233, 243, 220),
        new Color(33, 48, 65),
        new Color(94, 111, 132),
        new Color(17, 126, 209),
        new Color(117, 157, 194),
        new Color(28, 129, 224),
        new Color(42, 156, 97),
        new Color(128, 96, 194),
        new Color(38, 156, 95),
        new Color(196, 132, 18),
        new Color(194, 78, 78),
        new Color(185, 110, 35),
        new Color(176, 137, 23),
        new Color(226, 235, 245),
        new Color(213, 227, 241),
        new Color(199, 220, 240),
        new Color(210, 216, 224));

    private static UiThemeMode _mode = UiThemeMode.Dark;
    private static UiPalette _palette = DarkPalette;

    public static UiThemeMode Mode => _mode;

    public static Color DesktopBackground => _palette.DesktopBackground;

    public static Color DesktopGlow => _palette.DesktopGlow;

    public static Color PanelFill => _palette.PanelFill;

    public static Color PanelRaised => _palette.PanelRaised;

    public static Color PanelBorder => _palette.PanelBorder;

    public static Color PanelMuted => _palette.PanelMuted;

    public static Color EditorFill => _palette.EditorFill;

    public static Color EditorBorder => _palette.EditorBorder;

    public static Color Overlay => _palette.Overlay;

    public static Color TextPrimary => _palette.TextPrimary;

    public static Color TextMuted => _palette.TextMuted;

    public static Color Accent => _palette.Accent;

    public static Color AccentDim => _palette.AccentDim;

    public static Color Focus => _palette.Focus;

    public static Color Sanity => _palette.Sanity;

    public static Color Quality => _palette.Quality;

    public static Color Success => _palette.Success;

    public static Color Warning => _palette.Warning;

    public static Color Danger => _palette.Danger;

    public static Color CatAccent => _palette.CatAccent;

    public static Color CoinAccent => _palette.CoinAccent;

    public static Color ButtonFill => _palette.ButtonFill;

    public static Color ButtonHover => _palette.ButtonHover;

    public static Color ButtonSelected => _palette.ButtonSelected;

    public static Color ButtonDisabled => _palette.ButtonDisabled;

    public static void Apply(UiThemeMode mode)
    {
        _mode = mode;
        _palette = mode == UiThemeMode.Light
            ? LightPalette
            : DarkPalette;
    }

    public static Color GetDifficultyAccent(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => new Color(93, 212, 132),
            GameDifficulty.Hard => new Color(255, 112, 112),
            GameDifficulty.ContinualUpgradeLoop => new Color(255, 194, 84),
            GameDifficulty.Endless => new Color(177, 150, 255),
            _ => new Color(87, 187, 255),
        };
    }

    public static Color Mix(Color source, Color target, float amount)
    {
        var clamped = MathHelper.Clamp(amount, 0f, 1f);
        return new Color(
            (byte)MathHelper.Lerp(source.R, target.R, clamped),
            (byte)MathHelper.Lerp(source.G, target.G, clamped),
            (byte)MathHelper.Lerp(source.B, target.B, clamped),
            (byte)MathHelper.Lerp(source.A, target.A, clamped));
    }

    public static Color WithOpacity(Color color, float opacity)
    {
        var clamped = MathHelper.Clamp(opacity, 0f, 1f);
        return new Color(color.R, color.G, color.B, (byte)MathF.Round(color.A * clamped));
    }

    private readonly record struct UiPalette(
        Color DesktopBackground,
        Color DesktopGlow,
        Color PanelFill,
        Color PanelRaised,
        Color PanelBorder,
        Color PanelMuted,
        Color EditorFill,
        Color EditorBorder,
        Color Overlay,
        Color TextPrimary,
        Color TextMuted,
        Color Accent,
        Color AccentDim,
        Color Focus,
        Color Sanity,
        Color Quality,
        Color Success,
        Color Warning,
        Color Danger,
        Color CatAccent,
        Color CoinAccent,
        Color ButtonFill,
        Color ButtonHover,
        Color ButtonSelected,
        Color ButtonDisabled);
}
