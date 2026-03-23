using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MicroDev.Core.Audio;
using MicroDev.Core.Input;
using MicroDev.Core.UI;

namespace MicroDev.Core.Screens;

public sealed class OptionsScreen : IScreen
{
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;
    private readonly GameAudio _audio;
    private readonly GameSettings _settings;
    private readonly Point _virtualResolution;
    private readonly Action _goBack;
    private readonly UiButton _soundToggleButton = new("Sound Effects: ON");
    private readonly UiButton _backButton = new("Back");

    public OptionsScreen(
        SpriteFont font,
        Texture2D pixel,
        GameAudio audio,
        GameSettings settings,
        Point virtualResolution,
        Action goBack)
    {
        _font = font;
        _pixel = pixel;
        _audio = audio;
        _settings = settings;
        _virtualResolution = virtualResolution;
        _goBack = goBack;

        _soundToggleButton.TextScale = 0.86f;
        _backButton.TextScale = 0.84f;
    }

    public void Update(GameTime gameTime, InputSnapshot input)
    {
        UpdateLayout();
        _soundToggleButton.Text = _settings.SoundEffectsEnabled ? "Sound Effects: ON" : "Sound Effects: OFF";

        if (_soundToggleButton.Update(input))
        {
            _settings.SoundEffectsEnabled = !_settings.SoundEffectsEnabled;
            _audio.Enabled = _settings.SoundEffectsEnabled;

            if (_settings.SoundEffectsEnabled)
            {
                _audio.PlayButtonClick();
            }

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
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, 180), UiTheme.DesktopGlow * 0.2f);

        var modalBounds = new Rectangle(420, 176, 760, 400);
        UiPanel.Draw(spriteBatch, _pixel, modalBounds, UiTheme.PanelFill, UiTheme.EditorBorder, 3);
        spriteBatch.Draw(_pixel, new Rectangle(modalBounds.X + 1, modalBounds.Y + 1, modalBounds.Width - 2, 5), UiTheme.Accent);

        UiLabel.Draw(spriteBatch, _font, "Options", new Vector2(modalBounds.X + 32, modalBounds.Y + 30), UiTheme.TextPrimary, 1.36f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Keep this screen light for now: sound effects can be toggled here, and more accessibility and pacing settings can slot into the same surface later.",
            new Vector2(modalBounds.X + 32, modalBounds.Y + 84),
            modalBounds.Width - 64,
            UiTheme.TextMuted,
            0.82f,
            3f,
            4);

        var settingBounds = new Rectangle(modalBounds.X + 32, modalBounds.Y + 182, modalBounds.Width - 64, 116);
        UiPanel.Draw(spriteBatch, _pixel, settingBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, "Audio", new Vector2(settingBounds.X + 20, settingBounds.Y + 18), UiTheme.Accent, 0.88f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Procedural key taps, alerts, and win/loss cues all route through this toggle.",
            new Vector2(settingBounds.X + 20, settingBounds.Y + 48),
            settingBounds.Width - 40,
            UiTheme.TextPrimary,
            0.74f,
            2f,
            2);

        _soundToggleButton.Draw(spriteBatch, _pixel, _font);
        _backButton.Draw(spriteBatch, _pixel, _font);
    }

    private void UpdateLayout()
    {
        _soundToggleButton.Bounds = new Rectangle(784, 390, 330, 42);
        _backButton.Bounds = new Rectangle(452, 500, 188, 46);
    }
}
