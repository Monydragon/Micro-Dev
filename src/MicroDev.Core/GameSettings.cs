namespace MicroDev.Core;

using Microsoft.Xna.Framework;
using MicroDev.Core.Simulation;

public sealed class GameSettings
{
    public bool SoundEffectsEnabled { get; set; } = true;

    public bool MusicEnabled { get; set; } = true;

    public float MasterVolume { get; set; } = 0.9f;

    public float SoundEffectsVolume { get; set; } = 0.85f;

    public float MusicVolume { get; set; } = 0.72f;

    public Point PreferredResolution { get; set; } = new(1600, 900);

    public WindowModeSetting WindowMode { get; set; } = WindowModeSetting.Windowed;

    public GameDifficulty SelectedDifficulty { get; set; } = GameDifficulty.Normal;
}
