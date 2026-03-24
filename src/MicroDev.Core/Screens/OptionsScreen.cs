using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MicroDev.Core.Audio;
using MicroDev.Core.Input;
using MicroDev.Core.UI;

namespace MicroDev.Core.Screens;

public sealed class OptionsScreen : IScreen
{
    private static readonly Point[] ResolutionOptions =
    [
        new(1280, 720),
        new(1366, 768),
        new(1600, 900),
        new(1920, 1080),
        new(2560, 1440),
    ];

    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;
    private readonly GameAudio _audio;
    private readonly GameSettings _settings;
    private readonly bool _isBrowserPlatform;
    private readonly Point _virtualResolution;
    private readonly Action _goBack;
    private readonly Action _applySettings;
    private readonly UiButton _displayModeButton = new("Windowed");
    private readonly UiButton _resolutionButton = new("1600 x 900");
    private readonly UiButton[] _resolutionOptionButtons =
    [
        new UiButton("1280 x 720"),
        new UiButton("1366 x 768"),
        new UiButton("1600 x 900"),
        new UiButton("1920 x 1080"),
        new UiButton("2560 x 1440"),
    ];
    private readonly UiButton _soundToggleButton = new("Sound Effects: ON");
    private readonly UiButton _musicToggleButton = new("Background Music: ON");
    private readonly UiButton _masterDownButton = new("-");
    private readonly UiButton _masterUpButton = new("+");
    private readonly UiButton _effectsDownButton = new("-");
    private readonly UiButton _effectsUpButton = new("+");
    private readonly UiButton _musicDownButton = new("-");
    private readonly UiButton _musicUpButton = new("+");
    private readonly UiButton _backButton = new("Back");
    private bool _resolutionDropdownOpen;

    public OptionsScreen(
        SpriteFont font,
        Texture2D pixel,
        GameAudio audio,
        GameSettings settings,
        bool isBrowserPlatform,
        Point virtualResolution,
        Action goBack,
        Action applySettings)
    {
        _font = font;
        _pixel = pixel;
        _audio = audio;
        _settings = settings;
        _isBrowserPlatform = isBrowserPlatform;
        _virtualResolution = virtualResolution;
        _goBack = goBack;
        _applySettings = applySettings;

        _displayModeButton.TextScale = 0.78f;
        _resolutionButton.TextScale = 0.78f;
        _soundToggleButton.TextScale = 0.76f;
        _musicToggleButton.TextScale = 0.76f;
        _masterDownButton.TextScale = 0.92f;
        _masterUpButton.TextScale = 0.92f;
        _effectsDownButton.TextScale = 0.92f;
        _effectsUpButton.TextScale = 0.92f;
        _musicDownButton.TextScale = 0.92f;
        _musicUpButton.TextScale = 0.92f;
        _backButton.TextScale = 0.82f;

        foreach (var button in _resolutionOptionButtons)
        {
            button.TextScale = 0.72f;
        }
    }

    public void Update(GameTime gameTime, InputSnapshot input)
    {
        UpdateLayout();
        SyncButtonLabels();

        if (!_isBrowserPlatform &&
            _resolutionDropdownOpen)
        {
            for (var index = 0; index < _resolutionOptionButtons.Length; index++)
            {
                if (!_resolutionOptionButtons[index].Update(input))
                {
                    continue;
                }

                _settings.PreferredResolution = ResolutionOptions[index];
                _resolutionDropdownOpen = false;
                ApplyAndPreview();
                return;
            }

            var dropdownBounds = GetResolutionDropdownBounds();
            if (input.LeftClicked &&
                !dropdownBounds.Contains(input.MousePosition) &&
                !_resolutionButton.Bounds.Contains(input.MousePosition))
            {
                _resolutionDropdownOpen = false;
            }
        }

        if (!_isBrowserPlatform &&
            _displayModeButton.Update(input))
        {
            _settings.WindowMode = GetNextWindowMode(_settings.WindowMode);
            ApplyAndPreview();
            return;
        }

        if (!_isBrowserPlatform &&
            _resolutionButton.Update(input))
        {
            _resolutionDropdownOpen = !_resolutionDropdownOpen;
            _audio.PlayButtonClick();
            return;
        }

        if (_soundToggleButton.Update(input))
        {
            var enabling = !_settings.SoundEffectsEnabled;
            if (!enabling)
            {
                _audio.PlayButtonClick();
            }

            _settings.SoundEffectsEnabled = enabling;
            _applySettings();

            if (enabling)
            {
                _audio.PlayButtonClick();
            }

            return;
        }

        if (_musicToggleButton.Update(input))
        {
            _settings.MusicEnabled = !_settings.MusicEnabled;
            ApplyAndPreview();
            return;
        }

        if (TryAdjustVolume(_masterDownButton, () => _settings.MasterVolume, value => _settings.MasterVolume = value, -0.05f, input) ||
            TryAdjustVolume(_masterUpButton, () => _settings.MasterVolume, value => _settings.MasterVolume = value, 0.05f, input) ||
            TryAdjustVolume(_effectsDownButton, () => _settings.SoundEffectsVolume, value => _settings.SoundEffectsVolume = value, -0.05f, input) ||
            TryAdjustVolume(_effectsUpButton, () => _settings.SoundEffectsVolume, value => _settings.SoundEffectsVolume = value, 0.05f, input) ||
            TryAdjustVolume(_musicDownButton, () => _settings.MusicVolume, value => _settings.MusicVolume = value, -0.05f, input) ||
            TryAdjustVolume(_musicUpButton, () => _settings.MusicVolume, value => _settings.MusicVolume = value, 0.05f, input))
        {
            return;
        }

        if (_backButton.Update(input))
        {
            _audio.PlayButtonClick();
            _goBack();
        }
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y), UiTheme.DesktopBackground);
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, 220), UiTheme.DesktopGlow * 0.22f);
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, 2), UiTheme.Accent);

        var modalBounds = GetModalBounds();
        var displayBounds = GetDisplayPanelBounds();
        var audioBounds = GetAudioPanelBounds();
        var runtimeBounds = GetRuntimePanelBounds();

        UiPanel.Draw(spriteBatch, _pixel, modalBounds, UiTheme.PanelFill, UiTheme.EditorBorder, 3);
        spriteBatch.Draw(_pixel, new Rectangle(modalBounds.X + 1, modalBounds.Y + 1, modalBounds.Width - 2, 5), UiTheme.Accent);
        UiPanel.Draw(spriteBatch, _pixel, displayBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, audioBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, runtimeBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        UiLabel.Draw(spriteBatch, _font, "Options", new Vector2(modalBounds.X + 28, modalBounds.Y + 24), UiTheme.TextPrimary, 1.36f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Adjust display mode and audio live. These changes apply immediately, so you can tune the desk without restarting the run.",
            new Vector2(modalBounds.X + 28, modalBounds.Y + 70),
            modalBounds.Width - 56,
            UiTheme.TextMuted,
            0.82f,
            3f,
            3);

        UiLabel.Draw(spriteBatch, _font, "Display", new Vector2(displayBounds.X + 20, displayBounds.Y + 18), UiTheme.Accent, 0.9f);
        if (_isBrowserPlatform)
        {
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                "The WebGL build always renders the same 1600 x 900 virtual desktop and scales it to the browser canvas. Use the browser window size or browser fullscreen for presentation changes.",
                new Vector2(displayBounds.X + 20, displayBounds.Y + 48),
                displayBounds.Width - 40,
                UiTheme.TextMuted,
                0.7f,
                2f,
                4);
            UiLabel.Draw(spriteBatch, _font, "Browser Canvas", new Vector2(displayBounds.X + 20, displayBounds.Y + 166), UiTheme.TextPrimary, 0.76f);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                "Desktop-only window mode and resolution controls are disabled here so the Web build stays consistent with the browser host.",
                new Vector2(displayBounds.X + 20, displayBounds.Y + 192),
                displayBounds.Width - 40,
                UiTheme.TextMuted,
                0.62f,
                2f,
                4);
        }
        else
        {
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                "Window mode and resolution only change the desktop presentation. The game still renders on the same virtual canvas underneath.",
                new Vector2(displayBounds.X + 20, displayBounds.Y + 48),
                displayBounds.Width - 40,
                UiTheme.TextMuted,
                0.7f,
                2f,
                3);
            DrawDisplayRow(spriteBatch, "Window Mode", _displayModeButton.Bounds, "Choose windowed, borderless, or fullscreen.");
            DrawDisplayRow(spriteBatch, "Resolution", _resolutionButton.Bounds, "Pick the backbuffer size used for the desktop window.");
        }

        UiLabel.Draw(spriteBatch, _font, "Audio", new Vector2(audioBounds.X + 20, audioBounds.Y + 18), UiTheme.Accent, 0.9f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Shape the typing SFX and the lo-fi desk loop separately. Master volume scales both at once.",
            new Vector2(audioBounds.X + 20, audioBounds.Y + 48),
            audioBounds.Width - 40,
            UiTheme.TextMuted,
            0.7f,
            2f,
            3);

        if (!_isBrowserPlatform)
        {
            _displayModeButton.Draw(spriteBatch, _pixel, _font);
            _resolutionButton.Draw(spriteBatch, _pixel, _font);
            if (_resolutionDropdownOpen)
            {
                DrawResolutionDropdown(spriteBatch);
            }
        }

        _soundToggleButton.Draw(spriteBatch, _pixel, _font);
        _musicToggleButton.Draw(spriteBatch, _pixel, _font);

        DrawVolumeRow(
            spriteBatch,
            "Master Volume",
            new Rectangle(audioBounds.X + 20, audioBounds.Y + 148, audioBounds.Width - 40, 40),
            _settings.MasterVolume,
            UiTheme.Accent,
            _masterDownButton,
            _masterUpButton);
        DrawVolumeRow(
            spriteBatch,
            "SFX Volume",
            new Rectangle(audioBounds.X + 20, audioBounds.Y + 196, audioBounds.Width - 40, 40),
            _settings.SoundEffectsVolume,
            UiTheme.Warning,
            _effectsDownButton,
            _effectsUpButton);
        DrawVolumeRow(
            spriteBatch,
            "BGM Volume",
            new Rectangle(audioBounds.X + 20, audioBounds.Y + 244, audioBounds.Width - 40, 40),
            _settings.MusicVolume,
            UiTheme.Success,
            _musicDownButton,
            _musicUpButton);

        UiLabel.Draw(spriteBatch, _font, "Quick Notes", new Vector2(runtimeBounds.X + 20, runtimeBounds.Y + 18), UiTheme.TextPrimary, 0.88f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            _isBrowserPlatform
                ? "Click once to unlock browser audio, then the typing SFX and background loop will behave normally. For fullscreen presentation, use the browser's fullscreen command."
                : "Borderless is best for seamless alt-tab flow. Fullscreen uses the chosen display mode directly. Lower BGM if you want the typing rhythm to lead the mix.",
            new Vector2(runtimeBounds.X + 20, runtimeBounds.Y + 48),
            runtimeBounds.Width - 40,
            UiTheme.TextMuted,
            0.74f,
            3f,
            4);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            _isBrowserPlatform
                ? $"Browser canvas: {FormatResolution(_virtualResolution)} virtual desktop | Master {ToPercent(_settings.MasterVolume)} | SFX {ToPercent(_settings.SoundEffectsVolume)} | BGM {ToPercent(_settings.MusicVolume)}"
                : $"Current setup: {GetWindowModeLabel(_settings.WindowMode)} at {FormatResolution(_settings.PreferredResolution)} | Master {ToPercent(_settings.MasterVolume)} | SFX {ToPercent(_settings.SoundEffectsVolume)} | BGM {ToPercent(_settings.MusicVolume)}",
            new Vector2(runtimeBounds.X + 20, runtimeBounds.Y + 118),
            runtimeBounds.Width - 40,
            UiTheme.TextPrimary,
            0.76f,
            2f,
            3);

        _backButton.Draw(spriteBatch, _pixel, _font);
    }

    private bool TryAdjustVolume(UiButton button, Func<float> getter, Action<float> setter, float delta, InputSnapshot input)
    {
        if (!button.Update(input))
        {
            return false;
        }

        var current = getter();
        var updated = Math.Clamp(current + delta, 0f, 1f);
        if (Math.Abs(updated - current) < 0.001f)
        {
            _audio.PlayFailure();
            return true;
        }

        setter(updated);
        ApplyAndPreview();
        return true;
    }

    private void ApplyAndPreview()
    {
        _applySettings();
        _audio.PlayButtonClick();
    }

    private void DrawDisplayRow(SpriteBatch spriteBatch, string label, Rectangle buttonBounds, string helperText)
    {
        var labelY = buttonBounds.Y - 50;
        UiLabel.Draw(spriteBatch, _font, label, new Vector2(buttonBounds.X, labelY), UiTheme.TextPrimary, 0.76f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            helperText,
            new Vector2(buttonBounds.X, labelY + 20),
            GetDisplayPanelBounds().Width - 40,
            UiTheme.TextMuted,
            0.62f);
    }

    private void DrawResolutionDropdown(SpriteBatch spriteBatch)
    {
        var dropdownBounds = GetResolutionDropdownBounds();
        UiPanel.Draw(spriteBatch, _pixel, dropdownBounds, UiTheme.PanelFill, UiTheme.AccentDim, 2);

        foreach (var button in _resolutionOptionButtons)
        {
            button.Draw(spriteBatch, _pixel, _font);
        }
    }

    private void DrawVolumeRow(
        SpriteBatch spriteBatch,
        string label,
        Rectangle rowBounds,
        float value,
        Color fillColor,
        UiButton downButton,
        UiButton upButton)
    {
        UiLabel.Draw(spriteBatch, _font, label, new Vector2(rowBounds.X, rowBounds.Y + 2), UiTheme.TextPrimary, 0.74f);
        UiLabel.Draw(spriteBatch, _font, ToPercent(value), new Vector2(rowBounds.Right - 62, rowBounds.Y + 2), UiTheme.TextMuted, 0.74f);

        var trackBounds = new Rectangle(rowBounds.X + 116, rowBounds.Y + 4, rowBounds.Width - 220, 18);
        UiPanel.Draw(spriteBatch, _pixel, trackBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        var fillWidth = (int)MathF.Round((trackBounds.Width - 6) * value);
        if (fillWidth > 0)
        {
            spriteBatch.Draw(_pixel, new Rectangle(trackBounds.X + 3, trackBounds.Y + 3, fillWidth, trackBounds.Height - 6), fillColor);
        }

        downButton.Draw(spriteBatch, _pixel, _font);
        upButton.Draw(spriteBatch, _pixel, _font);
    }

    private void SyncButtonLabels()
    {
        _displayModeButton.Text = GetWindowModeLabel(_settings.WindowMode);
        _resolutionButton.Text = $"Resolution: {FormatResolution(_settings.PreferredResolution)}";
        _soundToggleButton.Text = _settings.SoundEffectsEnabled ? "Sound Effects: ON" : "Sound Effects: OFF";
        _musicToggleButton.Text = _settings.MusicEnabled ? "Background Music: ON" : "Background Music: OFF";

        for (var index = 0; index < _resolutionOptionButtons.Length; index++)
        {
            _resolutionOptionButtons[index].Text = FormatResolution(ResolutionOptions[index]);
            _resolutionOptionButtons[index].IsSelected = ResolutionOptions[index] == _settings.PreferredResolution;
        }
    }

    private void UpdateLayout()
    {
        var modalBounds = GetModalBounds();
        var displayBounds = GetDisplayPanelBounds();
        var audioBounds = GetAudioPanelBounds();

        _displayModeButton.Bounds = new Rectangle(displayBounds.X + 20, displayBounds.Y + 144, 220, 38);
        _resolutionButton.Bounds = new Rectangle(displayBounds.X + 20, displayBounds.Y + 222, 264, 38);

        var dropdownBounds = GetResolutionDropdownBounds();
        for (var index = 0; index < _resolutionOptionButtons.Length; index++)
        {
            _resolutionOptionButtons[index].Bounds = new Rectangle(dropdownBounds.X + 10, dropdownBounds.Y + 10 + (index * 36), dropdownBounds.Width - 20, 30);
        }

        _soundToggleButton.Bounds = new Rectangle(audioBounds.X + 20, audioBounds.Y + 104, 240, 34);
        _musicToggleButton.Bounds = new Rectangle(audioBounds.X + 272, audioBounds.Y + 104, 240, 34);

        var volumeButtonSize = new Point(34, 30);
        _masterDownButton.Bounds = new Rectangle(audioBounds.Right - 92, audioBounds.Y + 146, volumeButtonSize.X, volumeButtonSize.Y);
        _masterUpButton.Bounds = new Rectangle(audioBounds.Right - 48, audioBounds.Y + 146, volumeButtonSize.X, volumeButtonSize.Y);
        _effectsDownButton.Bounds = new Rectangle(audioBounds.Right - 92, audioBounds.Y + 194, volumeButtonSize.X, volumeButtonSize.Y);
        _effectsUpButton.Bounds = new Rectangle(audioBounds.Right - 48, audioBounds.Y + 194, volumeButtonSize.X, volumeButtonSize.Y);
        _musicDownButton.Bounds = new Rectangle(audioBounds.Right - 92, audioBounds.Y + 242, volumeButtonSize.X, volumeButtonSize.Y);
        _musicUpButton.Bounds = new Rectangle(audioBounds.Right - 48, audioBounds.Y + 242, volumeButtonSize.X, volumeButtonSize.Y);

        _backButton.Bounds = new Rectangle(modalBounds.X + 28, modalBounds.Bottom - 56, 180, 38);
    }

    private Rectangle GetModalBounds()
    {
        return new Rectangle(236, 88, 1128, 664);
    }

    private Rectangle GetDisplayPanelBounds()
    {
        var modalBounds = GetModalBounds();
        return new Rectangle(modalBounds.X + 28, modalBounds.Y + 136, 472, 292);
    }

    private Rectangle GetAudioPanelBounds()
    {
        var modalBounds = GetModalBounds();
        return new Rectangle(modalBounds.X + 528, modalBounds.Y + 136, 572, 308);
    }

    private Rectangle GetRuntimePanelBounds()
    {
        var modalBounds = GetModalBounds();
        return new Rectangle(modalBounds.X + 28, modalBounds.Y + 462, modalBounds.Width - 56, 156);
    }

    private Rectangle GetResolutionDropdownBounds()
    {
        return new Rectangle(_resolutionButton.Bounds.X, _resolutionButton.Bounds.Bottom + 8, _resolutionButton.Bounds.Width, 10 + (_resolutionOptionButtons.Length * 36) + 10);
    }

    private static WindowModeSetting GetNextWindowMode(WindowModeSetting current)
    {
        return current switch
        {
            WindowModeSetting.Windowed => WindowModeSetting.Borderless,
            WindowModeSetting.Borderless => WindowModeSetting.Fullscreen,
            _ => WindowModeSetting.Windowed,
        };
    }

    private static string GetWindowModeLabel(WindowModeSetting windowMode)
    {
        return windowMode switch
        {
            WindowModeSetting.Borderless => "Borderless",
            WindowModeSetting.Fullscreen => "Fullscreen",
            _ => "Windowed",
        };
    }

    private static string FormatResolution(Point resolution)
    {
        return $"{resolution.X} x {resolution.Y}";
    }

    private static string ToPercent(float value)
    {
        return $"{MathF.Round(value * 100f):0}%";
    }
}
