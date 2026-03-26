namespace MicroDev.Core;

using Microsoft.Xna.Framework;
using MicroDev.Core.Simulation;
using MicroDev.Core.UI;

public sealed class GameSettings
{
    public const int DefaultManualRunSeed = 1729;

    public UiThemeMode ThemeMode { get; set; } = UiThemeMode.Dark;

    public UiFontOption UiFont { get; set; } = UiFontOption.Consolas;

    public bool SoundEffectsEnabled { get; set; } = true;

    public bool MusicEnabled { get; set; } = true;

    public float MasterVolume { get; set; } = 0.9f;

    public float SoundEffectsVolume { get; set; } = 0.85f;

    public float MusicVolume { get; set; } = 0.72f;

    public Point PreferredResolution { get; set; } = new(1600, 900);

    public WindowModeSetting WindowMode { get; set; } = WindowModeSetting.Windowed;

    public GameDifficulty SelectedDifficulty { get; set; } = GameDifficulty.Normal;

    public GameplayLoopMode SelectedGameplayMode { get; set; } = GameplayLoopMode.Interview;

    public bool RealisticSubModeEnabled { get; set; }

    public RunSeedMode RunSeedMode { get; set; } = RunSeedMode.RandomEachRun;

    public int ManualRunSeed { get; set; } = DefaultManualRunSeed;

    public int LastResolvedRunSeed { get; set; }
}
