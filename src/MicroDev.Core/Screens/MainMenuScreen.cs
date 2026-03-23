using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MicroDev.Core.Audio;
using MicroDev.Core.Input;
using MicroDev.Core.Simulation;
using MicroDev.Core.UI;

namespace MicroDev.Core.Screens;

public sealed class MainMenuScreen : IScreen
{
    private readonly Rectangle _heroBounds = new(104, 96, 708, 430);
    private readonly Rectangle _detailBounds = new(848, 130, 620, 292);
    private readonly Rectangle _footerBounds = new(104, 556, 1364, 182);
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;
    private readonly GameAudio _audio;
    private readonly GameSettings _settings;
    private readonly Point _virtualResolution;
    private readonly Action _startGame;
    private readonly Action _showOptions;
    private readonly Action _exitGame;
    private readonly UiButton _startButton = new("Start");
    private readonly UiButton _optionsButton = new("Options");
    private readonly UiButton _exitButton = new("Exit");
    private readonly UiButton _easyButton = new("Easy");
    private readonly UiButton _normalButton = new("Normal");
    private readonly UiButton _hardButton = new("Hard");
    private readonly UiButton _endlessButton = new("Endless");

    public MainMenuScreen(
        SpriteFont font,
        Texture2D pixel,
        GameAudio audio,
        GameSettings settings,
        Point virtualResolution,
        Action startGame,
        Action showOptions,
        Action exitGame)
    {
        _font = font;
        _pixel = pixel;
        _audio = audio;
        _settings = settings;
        _virtualResolution = virtualResolution;
        _startGame = startGame;
        _showOptions = showOptions;
        _exitGame = exitGame;

        _startButton.TextScale = 0.92f;
        _optionsButton.TextScale = 0.86f;
        _exitButton.TextScale = 0.86f;
        _easyButton.TextScale = 0.74f;
        _normalButton.TextScale = 0.74f;
        _hardButton.TextScale = 0.74f;
        _endlessButton.TextScale = 0.74f;
    }

    public void Update(GameTime gameTime, InputSnapshot input)
    {
        UpdateLayout();

        if (_startButton.Update(input))
        {
            _audio.PlayButtonClick();
            _startGame();
            return;
        }

        if (_optionsButton.Update(input))
        {
            _audio.PlayButtonClick();
            _showOptions();
            return;
        }

        if (_exitButton.Update(input))
        {
            _audio.PlayButtonClick();
            _exitGame();
            return;
        }

        if (UpdateDifficultyButton(_easyButton, GameDifficulty.Easy, input) ||
            UpdateDifficultyButton(_normalButton, GameDifficulty.Normal, input) ||
            UpdateDifficultyButton(_hardButton, GameDifficulty.Hard, input) ||
            UpdateDifficultyButton(_endlessButton, GameDifficulty.Endless, input))
        {
            return;
        }
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        DrawBackdrop(spriteBatch);

        UiPanel.Draw(spriteBatch, _pixel, _heroBounds, UiTheme.PanelFill, UiTheme.EditorBorder, 3);
        UiPanel.Draw(spriteBatch, _pixel, _detailBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _footerBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        spriteBatch.Draw(_pixel, new Rectangle(_heroBounds.X + 1, _heroBounds.Y + 1, _heroBounds.Width - 2, 5), UiTheme.Accent);
        spriteBatch.Draw(_pixel, new Rectangle(_detailBounds.X + 1, _detailBounds.Y + 1, _detailBounds.Width - 2, 4), UiTheme.AccentDim);

        var buttonRailX = _heroBounds.Right - 280;
        spriteBatch.Draw(_pixel, new Rectangle(buttonRailX - 28, _heroBounds.Y + 34, 2, _heroBounds.Height - 68), UiTheme.AccentDim * 0.8f);

        UiLabel.Draw(spriteBatch, _font, "Micro Dev", new Vector2(_heroBounds.X + 30, _heroBounds.Y + 34), UiTheme.TextPrimary, 1.96f);
        UiLabel.Draw(spriteBatch, _font, "One more file. One more day. One real shot.", new Vector2(_heroBounds.X + 32, _heroBounds.Y + 96), UiTheme.Accent, 0.72f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Build a real portfolio one file at a time, survive rent, and make the recruiter callback arrive before burnout does.",
            new Vector2(_heroBounds.X + 32, _heroBounds.Y + 136),
            348,
            UiTheme.TextMuted,
            0.86f,
            4f,
            4);

        UiLabel.Draw(spriteBatch, _font, "Current Build Loop", new Vector2(_detailBounds.X + 24, _detailBounds.Y + 26), UiTheme.Accent, 0.96f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Type real C# portfolio files, react to bugs and recruiter pings, buy efficiency upgrades, and keep your focus stable enough to finish the week.",
            new Vector2(_detailBounds.X + 24, _detailBounds.Y + 62),
            _detailBounds.Width - 48,
            UiTheme.TextPrimary,
            0.8f,
            3f,
            4);

        UiLabel.Draw(spriteBatch, _font, "This iteration adds", new Vector2(_detailBounds.X + 24, _detailBounds.Y + 170), UiTheme.TextMuted, 0.76f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Live display/audio settings, the new banking desktop app, and a richer lo-fi desk loop on top of the expanded career run.",
            new Vector2(_detailBounds.X + 24, _detailBounds.Y + 198),
            _detailBounds.Width - 48,
            UiTheme.Success,
            0.78f,
            3f,
            3);

        _startButton.Draw(spriteBatch, _pixel, _font);
        _optionsButton.Draw(spriteBatch, _pixel, _font);
        _exitButton.Draw(spriteBatch, _pixel, _font);

        UiLabel.Draw(spriteBatch, _font, "Week Survival Brief", new Vector2(_footerBounds.X + 24, _footerBounds.Y + 20), UiTheme.TextPrimary, 0.94f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Rent still lands at midnight. Food and sleep protect focus, upgrades buy back efficiency, and every finished file pushes the portfolio toward a real application loop.",
            new Vector2(_footerBounds.X + 24, _footerBounds.Y + 58),
            760,
            UiTheme.TextMuted,
            0.78f,
            3f,
            4);

        UiLabel.Draw(spriteBatch, _font, "Difficulty", new Vector2(_footerBounds.X + 846, _footerBounds.Y + 20), UiTheme.Accent, 0.82f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            GetDifficultySummary(_settings.SelectedDifficulty),
            new Vector2(_footerBounds.X + 846, _footerBounds.Y + 54),
            480,
            UiTheme.TextMuted,
            0.72f,
            2f,
            3);

        _easyButton.IsSelected = _settings.SelectedDifficulty == GameDifficulty.Easy;
        _normalButton.IsSelected = _settings.SelectedDifficulty == GameDifficulty.Normal;
        _hardButton.IsSelected = _settings.SelectedDifficulty == GameDifficulty.Hard;
        _endlessButton.IsSelected = _settings.SelectedDifficulty == GameDifficulty.Endless;
        _easyButton.Draw(spriteBatch, _pixel, _font);
        _normalButton.Draw(spriteBatch, _pixel, _font);
        _hardButton.Draw(spriteBatch, _pixel, _font);
        _endlessButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawBackdrop(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y), UiTheme.DesktopBackground);
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, 220), UiTheme.DesktopGlow * 0.24f);
        spriteBatch.Draw(_pixel, new Rectangle(88, 84, 600, 578), UiTheme.AccentDim * 0.10f);
        spriteBatch.Draw(_pixel, new Rectangle(0, 252, _virtualResolution.X, 1), UiTheme.AccentDim * 0.22f);
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, 2), UiTheme.Accent);
    }

    private void UpdateLayout()
    {
        var buttonX = _heroBounds.Right - 236;
        _startButton.Bounds = new Rectangle(buttonX, _heroBounds.Y + 120, 188, 52);
        _optionsButton.Bounds = new Rectangle(buttonX, _heroBounds.Y + 186, 188, 46);
        _exitButton.Bounds = new Rectangle(buttonX, _heroBounds.Y + 242, 188, 46);
        _easyButton.Bounds = new Rectangle(_footerBounds.X + 846, _footerBounds.Y + 116, 102, 36);
        _normalButton.Bounds = new Rectangle(_footerBounds.X + 958, _footerBounds.Y + 116, 118, 36);
        _hardButton.Bounds = new Rectangle(_footerBounds.X + 1086, _footerBounds.Y + 116, 102, 36);
        _endlessButton.Bounds = new Rectangle(_footerBounds.X + 1198, _footerBounds.Y + 116, 128, 36);
    }

    private bool UpdateDifficultyButton(UiButton button, GameDifficulty difficulty, InputSnapshot input)
    {
        if (!button.Update(input))
        {
            return false;
        }

        _settings.SelectedDifficulty = difficulty;
        _audio.PlayButtonClick();
        return true;
    }

    private static string GetDifficultySummary(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => "8 curated files, easier thresholds, calmer desk events, and guaranteed recruiter shots so every run gets chances to convert.",
            GameDifficulty.Hard => "Longer portfolio route, tighter bills, stricter recruiters, and a busier desk with more random disruptions.",
            GameDifficulty.Endless => "The portfolio feed never ends. Accepted applications pay out, random desk events keep rolling, and the run never hard-stops on a win.",
            _ => "A balanced run with a larger portfolio queue, recurring job listings, and a steady stream of random desk events.",
        };
    }
}
