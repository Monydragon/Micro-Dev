using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MicroDev.Core.Audio;
using MicroDev.Core.Input;
using MicroDev.Core.Simulation;
using MicroDev.Core.UI;

namespace MicroDev.Core.Screens;

public sealed class MainMenuScreen : IScreen, IUiFontAware
{
    private const string GameplayLoopIntroText = "Interview is the seven-day job-hunt opener. Corporate, Indie, and Founder are the long-form routes that keep pushing from basement survival toward a house-and-retirement finish line.";
    private const string DifficultyIntroText = "Pick how hard the run pushes back.";

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
    private readonly UiButton _interviewModeButton = new("Interview");
    private readonly UiButton _corporateModeButton = new("Corporate");
    private readonly UiButton _indieModeButton = new("Indie");
    private readonly UiButton _founderModeButton = new("Founder");
    private readonly UiButton _realisticModeButton = new("Realistic+");
    private readonly UiButton _easyDifficultyButton = new("Easy");
    private readonly UiButton _normalDifficultyButton = new("Normal");
    private readonly UiButton _hardDifficultyButton = new("Hard");
    private readonly UiButton _upgradeLoopDifficultyButton = new("Upgrade Loop");
    private readonly UiButton _endlessDifficultyButton = new("Endless");

    private SpriteFont _font;
    private Rectangle _shellBounds;
    private Rectangle _heroBounds;
    private Rectangle _actionBounds;
    private Rectangle _briefBounds;
    private Rectangle _modeBounds;
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

        if (UpdateGameplayModeButton(_interviewModeButton, GameplayLoopMode.Interview, input) ||
            UpdateGameplayModeButton(_corporateModeButton, GameplayLoopMode.Corporate, input) ||
            UpdateGameplayModeButton(_indieModeButton, GameplayLoopMode.Indie, input) ||
            UpdateGameplayModeButton(_founderModeButton, GameplayLoopMode.Founder, input))
        {
            return;
        }

        if (_realisticModeButton.Update(input))
        {
            _settings.RealisticSubModeEnabled = !_settings.RealisticSubModeEnabled;
            _audio.PlayButtonClick();
            return;
        }

        if (UpdateDifficultyButton(_easyDifficultyButton, GameDifficulty.Easy, input) ||
            UpdateDifficultyButton(_normalDifficultyButton, GameDifficulty.Normal, input) ||
            UpdateDifficultyButton(_hardDifficultyButton, GameDifficulty.Hard, input) ||
            UpdateDifficultyButton(_upgradeLoopDifficultyButton, GameDifficulty.ContinualUpgradeLoop, input) ||
            UpdateDifficultyButton(_endlessDifficultyButton, GameDifficulty.Endless, input))
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
        UiPanel.Draw(spriteBatch, _pixel, _modeBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _difficultyBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        DrawHeroPanel(spriteBatch);
        DrawActionPanel(spriteBatch);
        DrawBriefPanel(spriteBatch);
        DrawModePanel(spriteBatch);
        DrawDifficultyPanel(spriteBatch);
    }

    private void ConfigureButtons()
    {
        _startButton.TextScale = UiTypography.Button;
        _optionsButton.TextScale = UiTypography.Button;
        _exitButton.TextScale = UiTypography.Button;

        foreach (var button in GetGameplayButtons())
        {
            button.TextScale = UiTypography.Button;
            button.TextAlignment = UiTextAlignment.Left;
            button.HorizontalPadding = 14;
        }

        foreach (var button in GetDifficultyButtons())
        {
            button.TextScale = UiTypography.Button;
        }

        _realisticModeButton.TextScale = UiTypography.Button;
        _realisticModeButton.TextAlignment = UiTextAlignment.Left;
        _realisticModeButton.HorizontalPadding = 14;

        _startButton.AccentColor = UiTheme.Success;
        _optionsButton.AccentColor = UiTheme.Accent;
        _exitButton.AccentColor = UiTheme.Warning;
        _interviewModeButton.AccentColor = UiTheme.Accent;
        _corporateModeButton.AccentColor = UiTheme.Warning;
        _indieModeButton.AccentColor = UiTheme.Success;
        _founderModeButton.AccentColor = UiTheme.CoinAccent;
        _realisticModeButton.AccentColor = UiTheme.Warning;
        _easyDifficultyButton.AccentColor = UiTheme.GetDifficultyAccent(GameDifficulty.Easy);
        _normalDifficultyButton.AccentColor = UiTheme.GetDifficultyAccent(GameDifficulty.Normal);
        _hardDifficultyButton.AccentColor = UiTheme.GetDifficultyAccent(GameDifficulty.Hard);
        _upgradeLoopDifficultyButton.AccentColor = UiTheme.GetDifficultyAccent(GameDifficulty.ContinualUpgradeLoop);
        _endlessDifficultyButton.AccentColor = UiTheme.GetDifficultyAccent(GameDifficulty.Endless);
    }

    private void AdvanceButtonAnimations(float elapsedSeconds)
    {
        _startButton.AdvanceAnimation(elapsedSeconds);
        _optionsButton.AdvanceAnimation(elapsedSeconds);
        _exitButton.AdvanceAnimation(elapsedSeconds);

        foreach (var button in GetGameplayButtons())
        {
            button.AdvanceAnimation(elapsedSeconds);
        }
        _realisticModeButton.AdvanceAnimation(elapsedSeconds);

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
        var featureLines = new (string Number, string Heading, string Body)[]
        {
            ("01", "Unified theme mode", "Dark and light palettes now drive the entire shell."),
            ("02", "Global font family", $"Current font: {UiFontCatalog.GetDisplayName(_settings.UiFont)}"),
            ("03", "Digital transitions", "Buttons pulse with scanlines and screens fade between states."),
        };

        UiLabel.Draw(spriteBatch, _font, "Micro Dev", new Vector2(left, top), UiTheme.TextPrimary, UiTypography.Hero);
        UiLabel.Draw(spriteBatch, _font, "Survive the sprint. Then decide what kind of dev life you are building.", new Vector2(left, top + 54), UiTheme.Accent, UiTypography.Section);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Interview is the seven-day opening sprint. Corporate, Indie, and Founder all keep running after that, stretching the game into housing, retirement, and long-term studio goals instead of a single one-week finish line.",
            new Vector2(left, top + 102),
            bodyWidth - 52,
            UiTheme.TextMuted,
            UiTypography.Body,
            3f,
            4);

        var stripY = _heroBounds.Y + 218;
        var stripWidth = bodyWidth - 56;
        var featureRowWidth = stripWidth - 36;
        var featureRowsHeight = 0f;
        foreach (var featureLine in featureLines)
        {
            featureRowsHeight += MeasureFeatureLineHeight(featureLine.Body, featureRowWidth);
        }

        var stripHeight = (int)MathF.Ceiling(16f + featureRowsHeight + ((featureLines.Length - 1) * 10f) + 14f);
        UiPanel.Draw(spriteBatch, _pixel, new Rectangle(left, stripY, stripWidth, stripHeight), UiTheme.PanelFill, UiTheme.PanelBorder, 2);
        spriteBatch.Draw(_pixel, new Rectangle(left + 1, stripY + 1, stripWidth - 2, 3), UiTheme.Success);

        var featureY = stripY + 16f;
        foreach (var featureLine in featureLines)
        {
            DrawFeatureLine(spriteBatch, featureLine.Number, featureLine.Heading, featureLine.Body, left + 18, featureY, featureRowWidth);
            featureY += MeasureFeatureLineHeight(featureLine.Body, featureRowWidth) + 10f;
        }

        var snapshotTitleY = stripY + stripHeight + 14f;
        var snapshotBodyY = snapshotTitleY + GetLineHeight(UiTypography.Section) + 6f;
        var snapshotHeight = UiTextBlock.MeasureWrappedHeight(
            _font,
            $"Mode: {GetGameplayLabel(_settings.SelectedGameplayMode)}. {GetGameplayDurationLabel(_settings.SelectedGameplayMode)}. Pressure: {GetDifficultyLabel(_settings.SelectedDifficulty)}. Realistic+: {(_settings.RealisticSubModeEnabled ? "On" : "Off")}.",
            bodyWidth - 18,
            UiTypography.Body,
            3f,
            3);
        var snapshotOverflow = (snapshotBodyY + snapshotHeight) - (_heroBounds.Bottom - 20f);
        if (snapshotOverflow > 0f)
        {
            snapshotTitleY -= snapshotOverflow;
            snapshotBodyY -= snapshotOverflow;
        }

        UiLabel.Draw(spriteBatch, _font, "Run Snapshot", new Vector2(left, snapshotTitleY), UiTheme.TextPrimary, UiTypography.Section);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"Mode: {GetGameplayLabel(_settings.SelectedGameplayMode)}. {GetGameplayDurationLabel(_settings.SelectedGameplayMode)}. Pressure: {GetDifficultyLabel(_settings.SelectedDifficulty)}. Realistic+: {(_settings.RealisticSubModeEnabled ? "On" : "Off")}.",
            new Vector2(left, snapshotBodyY),
            bodyWidth - 18,
            UiTheme.TextMuted,
            UiTypography.Body,
            3f,
            3);
    }

    private void DrawFeatureLine(SpriteBatch spriteBatch, string number, string heading, string body, int x, float y, int width)
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
            $"{_settings.ThemeMode} mode  |  {UiFontCatalog.GetDisplayName(_settings.UiFont)}  |  {GetGameplayLabel(_settings.SelectedGameplayMode)}",
            new Vector2(noteBounds.X + 14, noteBounds.Y + 38),
            noteBounds.Width - 28,
            UiTheme.TextPrimary,
            UiTypography.Body,
            2f,
            1);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"{_settings.WindowMode}  |  {_settings.PreferredResolution.X} x {_settings.PreferredResolution.Y}  |  {GetDifficultyLabel(_settings.SelectedDifficulty)}  |  {GetSeedSummary()}",
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
        var contentWidth = _briefBounds.Width - 48;
        var summary = GetGameplaySummary(_settings.SelectedGameplayMode, _settings.RealisticSubModeEnabled);
        var summaryTop = _briefBounds.Y + 54;
        var summaryHeight = UiTextBlock.MeasureWrappedHeight(_font, summary, contentWidth, UiTypography.Body, 3f, 4);
        var nextRowY = (int)MathF.Round(summaryTop + summaryHeight + 18f);

        UiLabel.Draw(spriteBatch, _font, "Build Brief", new Vector2(left, _briefBounds.Y + 20), UiTheme.TextPrimary, UiTypography.Section);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            summary,
            new Vector2(left, summaryTop),
            contentWidth,
            UiTheme.TextMuted,
            UiTypography.Body,
            3f,
            4);

        nextRowY += DrawBriefDetailRow(spriteBatch, left, nextRowY, contentWidth, "Timeline", GetGameplayDurationLabel(_settings.SelectedGameplayMode), 1);
        nextRowY += DrawBriefDetailRow(spriteBatch, left, nextRowY, contentWidth, "Goal", GetGameplayGoal(_settings.SelectedGameplayMode), 2);
        DrawBriefDetailRow(spriteBatch, left, nextRowY, contentWidth, "Flow", GetGameplayFlow(_settings.SelectedGameplayMode), 2);
    }

    private void DrawModePanel(SpriteBatch spriteBatch)
    {
        var left = _modeBounds.X + 24;
        var buttonRailWidth = _modeBounds.Width - 48;
        var selectedSummaryY = _realisticModeButton.Bounds.Bottom + 18f;

        UiLabel.Draw(spriteBatch, _font, "Gameplay Loop", new Vector2(left, _modeBounds.Y + 20), UiTheme.TextPrimary, UiTypography.Section);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            GameplayLoopIntroText,
            new Vector2(left, _modeBounds.Y + 48),
            buttonRailWidth,
            UiTheme.TextMuted,
            UiTypography.Caption,
            2f,
            3);

        _interviewModeButton.IsSelected = _settings.SelectedGameplayMode == GameplayLoopMode.Interview;
        _corporateModeButton.IsSelected = _settings.SelectedGameplayMode == GameplayLoopMode.Corporate;
        _indieModeButton.IsSelected = _settings.SelectedGameplayMode == GameplayLoopMode.Indie;
        _founderModeButton.IsSelected = _settings.SelectedGameplayMode == GameplayLoopMode.Founder;
        _realisticModeButton.IsSelected = _settings.RealisticSubModeEnabled;
        _realisticModeButton.Text = _settings.RealisticSubModeEnabled ? "Realistic+  ON" : "Realistic+  OFF";

        foreach (var button in GetGameplayButtons())
        {
            button.Draw(spriteBatch, _pixel, _font);
        }

        _realisticModeButton.Draw(spriteBatch, _pixel, _font);

        var selectedAccent = GetGameplayAccent(_settings.SelectedGameplayMode);
        DrawFittedLabel(
            spriteBatch,
            $"Selected: {GetGameplayLabel(_settings.SelectedGameplayMode)}  |  {GetGameplayDurationLabel(_settings.SelectedGameplayMode)}",
            new Vector2(left, selectedSummaryY),
            buttonRailWidth,
            selectedAccent,
            UiTypography.Caption);
    }

    private void DrawDifficultyPanel(SpriteBatch spriteBatch)
    {
        var left = _difficultyBounds.X + 24;
        var buttonWidth = _difficultyBounds.Width - 48;
        var summary = GetDifficultySummary(_settings.SelectedDifficulty);
        var selectedAccent = UiTheme.GetDifficultyAccent(_settings.SelectedDifficulty);
        var summaryLabelHeight = GetLineHeight(UiTypography.Caption);
        var summaryBodyHeight = UiTextBlock.MeasureWrappedHeight(_font, summary, buttonWidth - 24, UiTypography.Caption, 2f, 2);
        var summaryHeight = (int)MathF.Ceiling(Math.Max(64f, 16f + summaryLabelHeight + 6f + summaryBodyHeight + 12f));
        var summaryBounds = new Rectangle(left, _endlessDifficultyButton.Bounds.Bottom + 18, buttonWidth, summaryHeight);
        var summaryBodyY = summaryBounds.Y + 14f + summaryLabelHeight + 4f;

        UiLabel.Draw(spriteBatch, _font, "Difficulty", new Vector2(left, _difficultyBounds.Y + 20), UiTheme.TextPrimary, UiTypography.Section);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            DifficultyIntroText,
            new Vector2(left, _difficultyBounds.Y + 48),
            buttonWidth,
            UiTheme.TextMuted,
            UiTypography.Caption,
            2f,
            1);

        _easyDifficultyButton.IsSelected = _settings.SelectedDifficulty == GameDifficulty.Easy;
        _normalDifficultyButton.IsSelected = _settings.SelectedDifficulty == GameDifficulty.Normal;
        _hardDifficultyButton.IsSelected = _settings.SelectedDifficulty == GameDifficulty.Hard;
        _upgradeLoopDifficultyButton.IsSelected = _settings.SelectedDifficulty == GameDifficulty.ContinualUpgradeLoop;
        _endlessDifficultyButton.IsSelected = _settings.SelectedDifficulty == GameDifficulty.Endless;

        foreach (var button in GetDifficultyButtons())
        {
            button.Draw(spriteBatch, _pixel, _font);
        }

        UiPanel.Draw(spriteBatch, _pixel, summaryBounds, UiTheme.PanelFill, UiTheme.Mix(UiTheme.PanelBorder, selectedAccent, 0.4f), 2);
        spriteBatch.Draw(_pixel, new Rectangle(summaryBounds.X + 1, summaryBounds.Y + 1, summaryBounds.Width - 2, 3), selectedAccent);
        UiLabel.Draw(spriteBatch, _font, GetDifficultyLabel(_settings.SelectedDifficulty), new Vector2(summaryBounds.X + 12, summaryBounds.Y + 11), selectedAccent, UiTypography.Caption);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            summary,
            new Vector2(summaryBounds.X + 12, summaryBodyY),
            summaryBounds.Width - 24,
            UiTheme.TextMuted,
            UiTypography.Caption,
            2f,
            2);
    }

    private float MeasureFeatureLineHeight(string body, int width)
    {
        var bodyHeight = UiTextBlock.MeasureWrappedHeight(
            _font,
            body,
            Math.Max(80, width - 34),
            UiTypography.Small,
            2f,
            2);
        return Math.Max(GetLineHeight(UiTypography.Body), 18f + bodyHeight);
    }

    private void UpdateLayout()
    {
        _shellBounds = new Rectangle(70, 28, _virtualResolution.X - 140, _virtualResolution.Y - 56);
        const int heroWidth = 892;
        const int shellTopInset = 30;
        const int shellBottomInset = 24;
        const int panelGap = 18;
        const int baselineHeroHeight = 484;
        const int minimumHeroHeight = 460;
        const int briefWidth = 520;

        var heroX = _shellBounds.X + 24;
        var heroY = _shellBounds.Y + shellTopInset;
        var actionX = heroX + heroWidth + panelGap;
        var actionWidth = _shellBounds.Right - actionX - 24;
        var lowerRightX = heroX + briefWidth + panelGap;
        var lowerRightWidth = _shellBounds.Right - lowerRightX - 24;
        var lowerRightGap = 18;
        var modeWidth = (int)MathF.Round((lowerRightWidth - lowerRightGap) * 0.55f);
        var difficultyWidth = lowerRightWidth - modeWidth - lowerRightGap;
        var sharedVerticalBudget = _shellBounds.Height - shellTopInset - panelGap - shellBottomInset;
        var requiredLowerPanelHeight = Math.Max(
            MeasureBriefPanelHeight(briefWidth),
            Math.Max(MeasureModePanelHeight(modeWidth), MeasureDifficultyPanelHeight(difficultyWidth)));
        var heroHeight = Math.Clamp(sharedVerticalBudget - requiredLowerPanelHeight, minimumHeroHeight, baselineHeroHeight);
        var lowerPanelHeight = sharedVerticalBudget - heroHeight;

        _heroBounds = new Rectangle(heroX, heroY, heroWidth, heroHeight);
        _actionBounds = new Rectangle(actionX, heroY, actionWidth, heroHeight);
        _briefBounds = new Rectangle(heroX, _heroBounds.Bottom + panelGap, briefWidth, lowerPanelHeight);
        _modeBounds = new Rectangle(lowerRightX, _heroBounds.Bottom + panelGap, modeWidth, lowerPanelHeight);
        _difficultyBounds = new Rectangle(_modeBounds.Right + lowerRightGap, _heroBounds.Bottom + panelGap, difficultyWidth, lowerPanelHeight);

        _startButton.Bounds = new Rectangle(_actionBounds.X + 24, _actionBounds.Y + 132, _actionBounds.Width - 48, 52);
        _optionsButton.Bounds = new Rectangle(_actionBounds.X + 24, _startButton.Bounds.Bottom + 12, _actionBounds.Width - 48, 46);
        _exitButton.Bounds = new Rectangle(_actionBounds.X + 24, _optionsButton.Bounds.Bottom + 12, _actionBounds.Width - 48, 46);

        const int gap = 8;
        var modeButtonX = _modeBounds.X + 24;
        var modeIntroHeight = (int)MathF.Ceiling(UiTextBlock.MeasureWrappedHeight(_font, GameplayLoopIntroText, _modeBounds.Width - 48, UiTypography.Caption, 2f, 3));
        var modeButtonY = _modeBounds.Y + 48 + modeIntroHeight + 18;
        var modeButtonWidth = (_modeBounds.Width - 48 - (gap * 3)) / 4;
        _interviewModeButton.Bounds = new Rectangle(modeButtonX, modeButtonY, modeButtonWidth, 38);
        _corporateModeButton.Bounds = new Rectangle(_interviewModeButton.Bounds.Right + gap, modeButtonY, modeButtonWidth, 38);
        _indieModeButton.Bounds = new Rectangle(_corporateModeButton.Bounds.Right + gap, modeButtonY, modeButtonWidth, 38);
        _founderModeButton.Bounds = new Rectangle(_indieModeButton.Bounds.Right + gap, modeButtonY, modeButtonWidth, 38);
        _realisticModeButton.Bounds = new Rectangle(modeButtonX, modeButtonY + 48, _modeBounds.Width - 48, 34);

        var difficultyButtonX = _difficultyBounds.X + 24;
        var difficultyRowWidth = _difficultyBounds.Width - 48;
        var difficultyIntroHeight = (int)MathF.Ceiling(UiTextBlock.MeasureWrappedHeight(_font, DifficultyIntroText, difficultyRowWidth, UiTypography.Caption, 2f, 2));
        var difficultyButtonY = _difficultyBounds.Y + 48 + difficultyIntroHeight + 16;
        var topButtonWidth = (difficultyRowWidth - (gap * 2)) / 3;
        var bottomButtonWidth = (difficultyRowWidth - gap) / 2;
        _easyDifficultyButton.Bounds = new Rectangle(difficultyButtonX, difficultyButtonY, topButtonWidth, 36);
        _normalDifficultyButton.Bounds = new Rectangle(_easyDifficultyButton.Bounds.Right + gap, difficultyButtonY, topButtonWidth, 36);
        _hardDifficultyButton.Bounds = new Rectangle(_normalDifficultyButton.Bounds.Right + gap, difficultyButtonY, topButtonWidth, 36);
        _upgradeLoopDifficultyButton.Bounds = new Rectangle(difficultyButtonX, difficultyButtonY + 44, bottomButtonWidth, 36);
        _endlessDifficultyButton.Bounds = new Rectangle(_upgradeLoopDifficultyButton.Bounds.Right + gap, difficultyButtonY + 44, bottomButtonWidth, 36);
    }

    private int MeasureBriefPanelHeight(int panelWidth)
    {
        var contentWidth = panelWidth - 48;
        var summaryHeight = UiTextBlock.MeasureWrappedHeight(
            _font,
            GetGameplaySummary(_settings.SelectedGameplayMode, _settings.RealisticSubModeEnabled),
            contentWidth,
            UiTypography.Body,
            3f,
            4);

        var totalHeight = 54f + summaryHeight + 18f;
        totalHeight += MeasureBriefDetailRow(contentWidth, GetGameplayDurationLabel(_settings.SelectedGameplayMode), 1);
        totalHeight += MeasureBriefDetailRow(contentWidth, GetGameplayGoal(_settings.SelectedGameplayMode), 2);
        totalHeight += MeasureBriefDetailRow(contentWidth, GetGameplayFlow(_settings.SelectedGameplayMode), 2);
        return (int)MathF.Ceiling(totalHeight + 20f);
    }

    private int MeasureModePanelHeight(int panelWidth)
    {
        var introHeight = UiTextBlock.MeasureWrappedHeight(
            _font,
            GameplayLoopIntroText,
            panelWidth - 48,
            UiTypography.Caption,
            2f,
            3);
        var selectedSummaryY = 48f + introHeight + 18f + 48f + 34f + 18f;
        return (int)MathF.Ceiling(selectedSummaryY + GetLineHeight(UiTypography.Caption) + 20f);
    }

    private int MeasureDifficultyPanelHeight(int panelWidth)
    {
        var difficultyRowWidth = panelWidth - 48;
        var introHeight = UiTextBlock.MeasureWrappedHeight(
            _font,
            DifficultyIntroText,
            difficultyRowWidth,
            UiTypography.Caption,
            2f,
            2);
        var buttonsBottom = 48f + introHeight + 16f + 44f + 36f;
        var summaryLabelHeight = GetLineHeight(UiTypography.Caption);
        var summaryBodyHeight = UiTextBlock.MeasureWrappedHeight(
            _font,
            GetDifficultySummary(_settings.SelectedDifficulty),
            difficultyRowWidth - 24,
            UiTypography.Caption,
            2f,
            2);
        var summaryHeight = Math.Max(64f, 16f + summaryLabelHeight + 6f + summaryBodyHeight + 12f);
        return (int)MathF.Ceiling(buttonsBottom + 18f + summaryHeight + 20f);
    }

    private int MeasureBriefDetailRow(int width, string value, int maxLines)
    {
        const int labelWidth = 78;

        var bodyHeight = UiTextBlock.MeasureWrappedHeight(
            _font,
            value,
            Math.Max(80, width - labelWidth),
            UiTypography.Body,
            2f,
            maxLines);
        var rowHeight = Math.Max(GetLineHeight(UiTypography.Caption), bodyHeight);
        return (int)MathF.Ceiling(rowHeight + 8f);
    }

    private int DrawBriefDetailRow(SpriteBatch spriteBatch, int left, int top, int width, string label, string value, int maxLines)
    {
        const int labelWidth = 78;

        UiLabel.Draw(spriteBatch, _font, label, new Vector2(left, top), UiTheme.Accent, UiTypography.Caption);

        var bodyHeight = UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            value,
            new Vector2(left + labelWidth, top),
            Math.Max(80, width - labelWidth),
            UiTheme.TextPrimary,
            UiTypography.Body,
            2f,
            maxLines);

        var rowHeight = Math.Max(GetLineHeight(UiTypography.Caption), bodyHeight);
        return (int)MathF.Ceiling(rowHeight + 8f);
    }

    private void DrawFittedLabel(
        SpriteBatch spriteBatch,
        string text,
        Vector2 position,
        float maxWidth,
        Color color,
        float preferredScale,
        float minimumScale = UiTypography.Small)
    {
        var (displayText, fittedScale) = UiTextBlock.FitText(_font, text, maxWidth, preferredScale, minimumScale);
        UiLabel.Draw(spriteBatch, _font, displayText, position, color, fittedScale);
    }

    private float GetLineHeight(float scale, float lineGap = 0f)
    {
        return (_font.LineSpacing * scale) + lineGap;
    }

    private bool UpdateGameplayModeButton(UiButton button, GameplayLoopMode gameplayMode, InputSnapshot input)
    {
        if (!button.Update(input))
        {
            return false;
        }

        _settings.SelectedGameplayMode = gameplayMode;
        _audio.PlayButtonClick();
        return true;
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

    private IEnumerable<UiButton> GetGameplayButtons()
    {
        yield return _interviewModeButton;
        yield return _corporateModeButton;
        yield return _indieModeButton;
        yield return _founderModeButton;
    }

    private IEnumerable<UiButton> GetDifficultyButtons()
    {
        yield return _easyDifficultyButton;
        yield return _normalDifficultyButton;
        yield return _hardDifficultyButton;
        yield return _upgradeLoopDifficultyButton;
        yield return _endlessDifficultyButton;
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
            GameDifficulty.Easy => "Lighter bills and steadier recovery.",
            GameDifficulty.Hard => "Tighter money and faster pressure spikes.",
            GameDifficulty.ContinualUpgradeLoop => "Long-form rig growth.",
            GameDifficulty.Endless => "No fixed ending.",
            _ => "Baseline balance.",
        };
    }

    private static string GetGameplayLabel(GameplayLoopMode gameplayMode)
    {
        return gameplayMode switch
        {
            GameplayLoopMode.Corporate => "Corporate Mode",
            GameplayLoopMode.Indie => "Indie Mode",
            GameplayLoopMode.Founder => "Founder Mode",
            _ => "Interview Mode",
        };
    }

    private static string GetGameplaySummary(GameplayLoopMode gameplayMode, bool realisticMode)
    {
        var baseSummary = gameplayMode switch
        {
            GameplayLoopMode.Corporate => "An indefinite work-life climb built around office hours, stricter bosses, micromanagement, and a reliable salary that pays more if you survive it.",
            GameplayLoopMode.Indie => "An indefinite self-directed route built around shipping, goal-setting, lighter sanity drain, and leaner income that still demands discipline.",
            GameplayLoopMode.Founder => "Start your own studio from the basement, freelance for rent, and grow a grassroots company into a house-and-retirement win.",
            _ => "A seven-day interview sprint. Build proof, survive the week, land an offer, and branch into Corporate, Indie, or Founder after the win.",
        };

        return realisticMode
            ? $"{baseSummary} Realistic+ keeps the money tighter and social choices more consequential."
            : baseSummary;
    }

    private static string GetGameplayFlow(GameplayLoopMode gameplayMode)
    {
        return gameplayMode switch
        {
            GameplayLoopMode.Corporate => "Office -> Check-Ins -> Code\nSalary -> Retire",
            GameplayLoopMode.Indie => "Set Goals -> Ship -> Recover\nPublish -> Retire",
            GameplayLoopMode.Founder => "Name Studio -> Freelance -> Build\nSell -> Scale",
            _ => "Build Proof -> Apply -> Interview\nBranch -> Survive",
        };
    }

    private static string GetGameplayGoal(GameplayLoopMode gameplayMode)
    {
        return gameplayMode switch
        {
            GameplayLoopMode.Corporate => "Keep the paycheck, survive micromanagement, buy a house, and retire.",
            GameplayLoopMode.Indie => "Stay self-motivated, ship enough to buy a house, and retire on your own terms.",
            GameplayLoopMode.Founder => "Bootstrap a company from scratch, grow the business, buy a house, and retire.",
            _ => "Win an offer inside seven days, then choose the long-form route.",
        };
    }

    private static string GetGameplayDurationLabel(GameplayLoopMode gameplayMode)
    {
        return gameplayMode == GameplayLoopMode.Interview
            ? "7-day sprint"
            : "Indefinite career run";
    }

    private static Color GetGameplayAccent(GameplayLoopMode gameplayMode)
    {
        return gameplayMode switch
        {
            GameplayLoopMode.Corporate => UiTheme.Warning,
            GameplayLoopMode.Indie => UiTheme.Success,
            GameplayLoopMode.Founder => UiTheme.CoinAccent,
            _ => UiTheme.Accent,
        };
    }

    private string GetSeedSummary()
    {
        return _settings.RunSeedMode == RunSeedMode.RandomEachRun
            ? "Seed Random"
            : $"Seed {_settings.ManualRunSeed}";
    }
}
