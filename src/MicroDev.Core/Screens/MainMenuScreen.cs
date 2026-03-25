using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MicroDev.Core.Audio;
using MicroDev.Core.Input;
using MicroDev.Core.Simulation;
using MicroDev.Core.UI;

namespace MicroDev.Core.Screens;

public sealed class MainMenuScreen : IScreen, IUiFontAware
{
    private readonly Texture2D _pixel;
    private readonly GameAudio _audio;
    private readonly GameSettings _settings;
    private readonly Point _virtualResolution;
    private readonly Action _startGame;
    private readonly Action _showOptions;
    private readonly Action _exitGame;
    private readonly UiButton _startButton = new("Start Run");
    private readonly UiButton _optionsButton = new("Appearance + Audio");
    private readonly UiButton _exitButton = new("Exit");
    private readonly UiButton _easyButton = new("Easy");
    private readonly UiButton _normalButton = new("Normal");
    private readonly UiButton _hardButton = new("Hard");
    private readonly UiButton _continualLoopButton = new("Upgrade Loop");
    private readonly UiButton _endlessButton = new("Endless");

    private SpriteFont _font;
    private Rectangle _shellBounds;
    private Rectangle _heroBounds;
    private Rectangle _actionBounds;
    private Rectangle _briefBounds;
    private Rectangle _difficultyBounds;

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

        ConfigureButtons();
        UpdateLayout();
    }

    public void ApplyFont(SpriteFont font)
    {
        _font = font;
    }

    public void Update(GameTime gameTime, InputSnapshot input)
    {
        UpdateLayout();
        ConfigureButtons();
        AdvanceButtonAnimations((float)gameTime.ElapsedGameTime.TotalSeconds);

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
            UpdateDifficultyButton(_continualLoopButton, GameDifficulty.ContinualUpgradeLoop, input) ||
            UpdateDifficultyButton(_endlessButton, GameDifficulty.Endless, input))
        {
            return;
        }
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        ConfigureButtons();
        DrawBackdrop(spriteBatch);

        UiPanel.Draw(spriteBatch, _pixel, _shellBounds, UiTheme.WithOpacity(UiTheme.PanelFill, 0.94f), UiTheme.PanelBorder, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_shellBounds.X + 1, _shellBounds.Y + 1, _shellBounds.Width - 2, 4), UiTheme.Accent);

        UiPanel.Draw(spriteBatch, _pixel, _heroBounds, UiTheme.PanelRaised, UiTheme.EditorBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _actionBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _briefBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _difficultyBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        DrawHeroPanel(spriteBatch);
        DrawActionPanel(spriteBatch);
        DrawBriefPanel(spriteBatch);
        DrawDifficultyPanel(spriteBatch);
    }

    private void ConfigureButtons()
    {
        _startButton.TextScale = UiTypography.Button;
        _optionsButton.TextScale = UiTypography.Button;
        _exitButton.TextScale = UiTypography.Button;

        foreach (var button in GetDifficultyButtons())
        {
            button.TextScale = UiTypography.Button;
            button.TextAlignment = UiTextAlignment.Left;
            button.HorizontalPadding = 14;
        }

        _startButton.AccentColor = UiTheme.Success;
        _optionsButton.AccentColor = UiTheme.Accent;
        _exitButton.AccentColor = UiTheme.Warning;
        _easyButton.AccentColor = UiTheme.GetDifficultyAccent(GameDifficulty.Easy);
        _normalButton.AccentColor = UiTheme.GetDifficultyAccent(GameDifficulty.Normal);
        _hardButton.AccentColor = UiTheme.GetDifficultyAccent(GameDifficulty.Hard);
        _continualLoopButton.AccentColor = UiTheme.GetDifficultyAccent(GameDifficulty.ContinualUpgradeLoop);
        _endlessButton.AccentColor = UiTheme.GetDifficultyAccent(GameDifficulty.Endless);
    }

    private void AdvanceButtonAnimations(float elapsedSeconds)
    {
        _startButton.AdvanceAnimation(elapsedSeconds);
        _optionsButton.AdvanceAnimation(elapsedSeconds);
        _exitButton.AdvanceAnimation(elapsedSeconds);

        foreach (var button in GetDifficultyButtons())
        {
            button.AdvanceAnimation(elapsedSeconds);
        }
    }

    private void DrawBackdrop(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y), UiTheme.DesktopBackground);
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, 262), UiTheme.WithOpacity(UiTheme.DesktopGlow, 0.5f));
        spriteBatch.Draw(_pixel, new Rectangle(78, 26, 448, 740), UiTheme.WithOpacity(UiTheme.AccentDim, 0.1f));
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, 2), UiTheme.Accent);

        for (var index = 0; index < 8; index++)
        {
            var y = 108 + (index * 86);
            spriteBatch.Draw(_pixel, new Rectangle(0, y, _virtualResolution.X, 1), UiTheme.WithOpacity(UiTheme.AccentDim, 0.18f));
        }
    }

    private void DrawHeroPanel(SpriteBatch spriteBatch)
    {
        var left = _heroBounds.X + 28;
        var top = _heroBounds.Y + 28;
        var bodyWidth = _heroBounds.Width - 56;

        UiLabel.Draw(spriteBatch, _font, "Micro Dev", new Vector2(left, top), UiTheme.TextPrimary, UiTypography.Hero);
        UiLabel.Draw(spriteBatch, _font, "Ship a portfolio before the week ships you.", new Vector2(left, top + 54), UiTheme.Accent, UiTypography.Section);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "The desktop shell is now anchored high on the screen so the whole menu reads like a workstation instead of a floating modal. Every control now follows the same typography scale, the same accent language, and the same input animation rhythm.",
            new Vector2(left, top + 102),
            bodyWidth - 52,
            UiTheme.TextMuted,
            UiTypography.Body,
            3f,
            4);

        var stripY = _heroBounds.Y + 218;
        var stripWidth = bodyWidth - 56;
        UiPanel.Draw(spriteBatch, _pixel, new Rectangle(left, stripY, stripWidth, 128), UiTheme.PanelFill, UiTheme.PanelBorder, 2);
        spriteBatch.Draw(_pixel, new Rectangle(left + 1, stripY + 1, stripWidth - 2, 3), UiTheme.Success);

        DrawFeatureLine(spriteBatch, "01", "Unified theme mode", "Dark and light palettes now drive the entire shell.", left + 18, stripY + 16, stripWidth - 36);
        DrawFeatureLine(spriteBatch, "02", "Global font family", $"Current font: {UiFontCatalog.GetDisplayName(_settings.UiFont)}", left + 18, stripY + 54, stripWidth - 36);
        DrawFeatureLine(spriteBatch, "03", "Digital transitions", "Buttons pulse with scanlines and screens fade between states.", left + 18, stripY + 92, stripWidth - 36);

        UiLabel.Draw(spriteBatch, _font, "Run Snapshot", new Vector2(left, _heroBounds.Bottom - 126), UiTheme.TextPrimary, UiTypography.Section);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Type real C# snippets, survive rent, eat before focus collapses, answer recruiter loops, and keep enough sanity left to finish what you started.",
            new Vector2(left, _heroBounds.Bottom - 94),
            bodyWidth - 18,
            UiTheme.TextMuted,
            UiTypography.Body,
            3f,
            3);
    }

    private void DrawFeatureLine(SpriteBatch spriteBatch, string number, string heading, string body, int x, int y, int width)
    {
        UiLabel.Draw(spriteBatch, _font, number, new Vector2(x, y), UiTheme.Accent, UiTypography.Caption);
        UiLabel.Draw(spriteBatch, _font, heading, new Vector2(x + 34, y), UiTheme.TextPrimary, UiTypography.Body);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            body,
            new Vector2(x + 34, y + 18),
            width - 34,
            UiTheme.TextMuted,
            UiTypography.Small,
            2f,
            2);
    }

    private void DrawActionPanel(SpriteBatch spriteBatch)
    {
        var left = _actionBounds.X + 24;
        UiLabel.Draw(spriteBatch, _font, "Control Stack", new Vector2(left, _actionBounds.Y + 24), UiTheme.TextPrimary, UiTypography.Title);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Start the run, adjust appearance and audio live, or back out. Theme and font selection carry through the whole app.",
            new Vector2(left, _actionBounds.Y + 62),
            _actionBounds.Width - 48,
            UiTheme.TextMuted,
            UiTypography.Body,
            3f,
            4);

        _startButton.Draw(spriteBatch, _pixel, _font);
        _optionsButton.Draw(spriteBatch, _pixel, _font);
        _exitButton.Draw(spriteBatch, _pixel, _font);

        var noteBounds = new Rectangle(_actionBounds.X + 24, _actionBounds.Bottom - 136, _actionBounds.Width - 48, 96);
        UiPanel.Draw(spriteBatch, _pixel, noteBounds, UiTheme.PanelFill, UiTheme.PanelBorder, 2);
        spriteBatch.Draw(_pixel, new Rectangle(noteBounds.X + 1, noteBounds.Y + 1, noteBounds.Width - 2, 3), UiTheme.Warning);
        UiLabel.Draw(spriteBatch, _font, "Current Profile", new Vector2(noteBounds.X + 14, noteBounds.Y + 14), UiTheme.Warning, UiTypography.Caption);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"{_settings.ThemeMode} mode  |  {UiFontCatalog.GetDisplayName(_settings.UiFont)}",
            new Vector2(noteBounds.X + 14, noteBounds.Y + 38),
            noteBounds.Width - 28,
            UiTheme.TextPrimary,
            UiTypography.Body,
            2f,
            1);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"{_settings.WindowMode}  |  {_settings.PreferredResolution.X} x {_settings.PreferredResolution.Y}  |  {GetSeedSummary()}",
            new Vector2(noteBounds.X + 14, noteBounds.Y + 66),
            noteBounds.Width - 28,
            UiTheme.TextMuted,
            UiTypography.Caption,
            2f,
            1);
    }

    private void DrawBriefPanel(SpriteBatch spriteBatch)
    {
        var left = _briefBounds.X + 24;
        UiLabel.Draw(spriteBatch, _font, "Build Brief", new Vector2(left, _briefBounds.Y + 20), UiTheme.TextPrimary, UiTypography.Section);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Rent still hits at midnight. Food protects focus, sleep saves sanity, upgrades buy speed back, and published releases can keep paying once they are live.",
            new Vector2(left, _briefBounds.Y + 54),
            _briefBounds.Width - 48,
            UiTheme.TextMuted,
            UiTypography.Body,
            3f,
            4);

        UiLabel.Draw(spriteBatch, _font, "Loop", new Vector2(left, _briefBounds.Bottom - 72), UiTheme.Accent, UiTypography.Caption);
        UiLabel.Draw(spriteBatch, _font, "Code -> Recover -> Apply -> Upgrade -> Ship", new Vector2(left + 54, _briefBounds.Bottom - 72), UiTheme.TextPrimary, UiTypography.Body);
        UiLabel.Draw(spriteBatch, _font, "Keep the desk stable long enough to convert the run into a job.", new Vector2(left, _briefBounds.Bottom - 40), UiTheme.TextMuted, UiTypography.Caption);
    }

    private void DrawDifficultyPanel(SpriteBatch spriteBatch)
    {
        var left = _difficultyBounds.X + 24;
        var tabRailBounds = new Rectangle(left, _difficultyBounds.Y + 90, _difficultyBounds.Width - 48, 52);
        var summaryTop = tabRailBounds.Bottom + 14;

        UiLabel.Draw(spriteBatch, _font, "Difficulty", new Vector2(left, _difficultyBounds.Y + 20), UiTheme.TextPrimary, UiTypography.Section);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Each mode is color coded and left aligned so the labels and the summary block share one clean edge.",
            new Vector2(left, _difficultyBounds.Y + 50),
            _difficultyBounds.Width - 48,
            UiTheme.TextMuted,
            UiTypography.Caption,
            2f,
            2);

        UiPanel.Draw(spriteBatch, _pixel, tabRailBounds, UiTheme.PanelFill, UiTheme.PanelBorder, 2);
        spriteBatch.Draw(_pixel, new Rectangle(tabRailBounds.X + 1, tabRailBounds.Y + 1, tabRailBounds.Width - 2, 3), UiTheme.WithOpacity(UiTheme.AccentDim, 0.9f));

        _easyButton.IsSelected = _settings.SelectedDifficulty == GameDifficulty.Easy;
        _normalButton.IsSelected = _settings.SelectedDifficulty == GameDifficulty.Normal;
        _hardButton.IsSelected = _settings.SelectedDifficulty == GameDifficulty.Hard;
        _continualLoopButton.IsSelected = _settings.SelectedDifficulty == GameDifficulty.ContinualUpgradeLoop;
        _endlessButton.IsSelected = _settings.SelectedDifficulty == GameDifficulty.Endless;

        foreach (var button in GetDifficultyButtons())
        {
            button.Draw(spriteBatch, _pixel, _font);
        }

        var selectedAccent = UiTheme.GetDifficultyAccent(_settings.SelectedDifficulty);
        UiPanel.Draw(spriteBatch, _pixel, new Rectangle(left, summaryTop, _difficultyBounds.Width - 48, 86), UiTheme.PanelFill, UiTheme.Mix(UiTheme.PanelBorder, selectedAccent, 0.4f), 2);
        spriteBatch.Draw(_pixel, new Rectangle(left + 1, summaryTop + 1, _difficultyBounds.Width - 50, 3), selectedAccent);
        UiLabel.Draw(spriteBatch, _font, GetDifficultyLabel(_settings.SelectedDifficulty), new Vector2(left + 14, summaryTop + 12), selectedAccent, UiTypography.Caption);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            GetDifficultySummary(_settings.SelectedDifficulty),
            new Vector2(left + 14, summaryTop + 34),
            _difficultyBounds.Width - 76,
            UiTheme.TextMuted,
            UiTypography.Body,
            2f,
            3);
    }

    private void UpdateLayout()
    {
        _shellBounds = new Rectangle(70, 28, _virtualResolution.X - 140, _virtualResolution.Y - 56);
        _heroBounds = new Rectangle(_shellBounds.X + 24, _shellBounds.Y + 30, 892, 512);
        _actionBounds = new Rectangle(_heroBounds.Right + 18, _shellBounds.Y + 30, _shellBounds.Right - _heroBounds.Right - 42, 512);
        _briefBounds = new Rectangle(_heroBounds.X, _heroBounds.Bottom + 18, 628, 248);
        _difficultyBounds = new Rectangle(_briefBounds.Right + 18, _heroBounds.Bottom + 18, _shellBounds.Right - _briefBounds.Right - 42, 248);

        _startButton.Bounds = new Rectangle(_actionBounds.X + 24, _actionBounds.Y + 132, _actionBounds.Width - 48, 52);
        _optionsButton.Bounds = new Rectangle(_actionBounds.X + 24, _startButton.Bounds.Bottom + 12, _actionBounds.Width - 48, 46);
        _exitButton.Bounds = new Rectangle(_actionBounds.X + 24, _optionsButton.Bounds.Bottom + 12, _actionBounds.Width - 48, 46);

        const int gap = 8;
        var difficultyY = _difficultyBounds.Y + 97;
        var buttonX = _difficultyBounds.X + 32;
        _easyButton.Bounds = new Rectangle(buttonX, difficultyY, 98, 38);
        _normalButton.Bounds = new Rectangle(_easyButton.Bounds.Right + gap, difficultyY, 112, 38);
        _hardButton.Bounds = new Rectangle(_normalButton.Bounds.Right + gap, difficultyY, 98, 38);
        _continualLoopButton.Bounds = new Rectangle(_hardButton.Bounds.Right + gap, difficultyY, 152, 38);
        _endlessButton.Bounds = new Rectangle(_continualLoopButton.Bounds.Right + gap, difficultyY, 114, 38);
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

    private IEnumerable<UiButton> GetDifficultyButtons()
    {
        yield return _easyButton;
        yield return _normalButton;
        yield return _hardButton;
        yield return _continualLoopButton;
        yield return _endlessButton;
    }

    private static string GetDifficultyLabel(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => "Easy Mode",
            GameDifficulty.Hard => "Hard Mode",
            GameDifficulty.ContinualUpgradeLoop => "Upgrade Loop",
            GameDifficulty.Endless => "Endless Mode",
            _ => "Normal Mode",
        };
    }

    private static string GetDifficultySummary(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => "Shorter portfolio route, calmer desk events, and more forgiving recruiter pressure.",
            GameDifficulty.Hard => "Longer queue, tighter bills, stricter recruiter gates, and a much busier desk.",
            GameDifficulty.ContinualUpgradeLoop => "Keep the upgrade economy and recurring published-app income alive after success.",
            GameDifficulty.Endless => "The queue never ends, recruiter loops keep rolling, and the desk becomes a permanent grind.",
            _ => "The balanced default with steady incidents, recurring opportunities, and a full portfolio run.",
        };
    }

    private string GetSeedSummary()
    {
        return _settings.RunSeedMode == RunSeedMode.RandomEachRun
            ? "Seed Random"
            : $"Seed {_settings.ManualRunSeed}";
    }
}
