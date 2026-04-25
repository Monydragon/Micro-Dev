using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MicroDev.Core.Audio;
using MicroDev.Core.Input;
using MicroDev.Core.Simulation;
using MicroDev.Core.UI;

namespace MicroDev.Core.Screens;

public sealed class MainMenuScreen : IScreen, IUiFontAware
{
    private enum MainMenuView
    {
        Home,
        Setup,
    }

    private const string GameplayLoopIntroText = "Choose the kind of run you want to start with.";
    private const string DifficultyIntroText = "Set how much pressure the run should apply.";

    private readonly Texture2D _pixel;
    private readonly GameAudio _audio;
    private readonly GameSettings _settings;
    private readonly Point _virtualResolution;
    private readonly Action _startGame;
    private readonly Action _showOptions;
    private readonly Action _exitGame;
    private readonly UiButton _startButton = new("Start Run");
    private readonly UiButton _backButton = new("Back");
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
    private MainMenuView _view;
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

    internal void PrepareCaptureSetupView()
    {
        _view = MainMenuView.Setup;
        UpdateLayout();
        ConfigureButtons();
    }

    public void Update(GameTime gameTime, InputSnapshot input)
    {
        UpdateLayout();
        ConfigureButtons();
        AdvanceButtonAnimations((float)gameTime.ElapsedGameTime.TotalSeconds);

        if (_startButton.Update(input))
        {
            _audio.PlayButtonClick();
            if (_view == MainMenuView.Home)
            {
                _view = MainMenuView.Setup;
            }
            else
            {
                _startGame();
            }

            return;
        }

        if (_backButton.Update(input))
        {
            _audio.PlayButtonClick();
            _view = MainMenuView.Home;
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

        if (_view != MainMenuView.Setup)
        {
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

        if (_view == MainMenuView.Setup)
        {
            UiPanel.Draw(spriteBatch, _pixel, _briefBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);
            UiPanel.Draw(spriteBatch, _pixel, _modeBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);
            UiPanel.Draw(spriteBatch, _pixel, _difficultyBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);
        }

        DrawHeroPanel(spriteBatch);
        DrawActionPanel(spriteBatch);

        if (_view == MainMenuView.Setup)
        {
            DrawBriefPanel(spriteBatch);
            DrawModePanel(spriteBatch);
            DrawDifficultyPanel(spriteBatch);
        }
    }

    private void ConfigureButtons()
    {
        _startButton.TextScale = UiTypography.Button;
        _backButton.TextScale = UiTypography.Button;
        _optionsButton.TextScale = UiTypography.Button;
        _exitButton.TextScale = UiTypography.Button;
        _startButton.Text = _view == MainMenuView.Home ? "Set Up Run" : "Start Run";

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

        _startButton.AccentColor = _view == MainMenuView.Home ? UiTheme.Accent : UiTheme.Success;
        _backButton.AccentColor = UiTheme.Warning;
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
        _backButton.AdvanceAnimation(elapsedSeconds);
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
        if (_view == MainMenuView.Home)
        {
            var left = _heroBounds.X + 30;
            var top = _heroBounds.Y + 30;
            var bodyWidth = _heroBounds.Width - 60;
            var featureLines = new (string Number, string Heading, string Body)[]
            {
                ("01", "Interview", "A short seven-day sprint that teaches the loop quickly."),
                ("02", "Career Runs", "Corporate, Indie, and Founder keep going after the opener."),
                ("03", "Readable Pace", "Run choices now live on a second screen instead of one crowded wall."),
            };

            UiLabel.Draw(spriteBatch, _font, "Micro Dev", new Vector2(left, top), UiTheme.TextPrimary, UiTypography.Hero);
            UiLabel.Draw(spriteBatch, _font, "Code, survive, and shape the kind of dev life you want.", new Vector2(left, top + 56), UiTheme.Accent, UiTypography.Section);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                "Start with a quick setup, then jump into the run. The menu now keeps the first choice simple and moves the deeper run options to their own screen.",
                new Vector2(left, top + 104),
                bodyWidth - 24,
                UiTheme.TextMuted,
                UiTypography.Body,
                3f,
                4);

            var stripY = _heroBounds.Y + 232;
            var stripWidth = bodyWidth - 28;
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

            var snapshotBounds = new Rectangle(left, _heroBounds.Bottom - 150, stripWidth, 110);
            UiPanel.Draw(spriteBatch, _pixel, snapshotBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);
            spriteBatch.Draw(_pixel, new Rectangle(snapshotBounds.X + 1, snapshotBounds.Y + 1, snapshotBounds.Width - 2, 3), GetGameplayAccent(_settings.SelectedGameplayMode));
            UiLabel.Draw(spriteBatch, _font, "Current Run Snapshot", new Vector2(snapshotBounds.X + 14, snapshotBounds.Y + 14), UiTheme.TextPrimary, UiTypography.Section);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                $"Route: {GetGameplayLabel(_settings.SelectedGameplayMode)}. {GetGameplayDurationLabel(_settings.SelectedGameplayMode)}. Difficulty: {GetDifficultyLabel(_settings.SelectedDifficulty)}. Realistic+: {(_settings.RealisticSubModeEnabled ? "On" : "Off")}.",
                new Vector2(snapshotBounds.X + 14, snapshotBounds.Y + 48),
                snapshotBounds.Width - 28,
                UiTheme.TextMuted,
                UiTypography.Body,
                2f,
                2);
            return;
        }

        var setupLeft = _heroBounds.X + 28;
        var setupTop = _heroBounds.Y + 20;
        var setupBodyWidth = _heroBounds.Width - 56;

        UiLabel.Draw(spriteBatch, _font, "Run Setup", new Vector2(setupLeft, setupTop), UiTheme.TextPrimary, UiTypography.Title);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Choose the route, set the pressure, and launch when the run summary feels right.",
            new Vector2(setupLeft, setupTop + 44),
            setupBodyWidth,
            UiTheme.Accent,
            UiTypography.Body,
            2f,
            2);

        UiPanel.Draw(spriteBatch, _pixel, new Rectangle(setupLeft, _heroBounds.Bottom - 48, setupBodyWidth, 28), UiTheme.PanelFill, UiTheme.PanelBorder, 1);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"Selected: {GetGameplayLabel(_settings.SelectedGameplayMode)}  |  {GetDifficultyLabel(_settings.SelectedDifficulty)}  |  Realistic+ {(_settings.RealisticSubModeEnabled ? "On" : "Off")}",
            new Vector2(setupLeft + 12, _heroBounds.Bottom - 42),
            setupBodyWidth - 24,
            UiTheme.TextPrimary,
            UiTypography.Caption,
            2f,
            1);
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
        var introText = GetActionPanelIntroText();
        var introMaxLines = _view == MainMenuView.Home ? 4 : 3;
        UiLabel.Draw(spriteBatch, _font, _view == MainMenuView.Home ? "Main Menu" : "Ready", new Vector2(left, _actionBounds.Y + 24), UiTheme.TextPrimary, UiTypography.Title);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            introText,
            new Vector2(left, _actionBounds.Y + 62),
            _actionBounds.Width - 48,
            UiTheme.TextMuted,
            UiTypography.Body,
            3f,
            introMaxLines);

        _startButton.Draw(spriteBatch, _pixel, _font);
        _optionsButton.Draw(spriteBatch, _pixel, _font);
        if (_view == MainMenuView.Home)
        {
            _exitButton.Draw(spriteBatch, _pixel, _font);
        }
        else
        {
            _backButton.Draw(spriteBatch, _pixel, _font);
        }

        var noteHeight = GetActionNoteHeight();
        var noteBounds = new Rectangle(_actionBounds.X + 24, _actionBounds.Bottom - noteHeight - 40, _actionBounds.Width - 48, noteHeight);
        UiPanel.Draw(spriteBatch, _pixel, noteBounds, UiTheme.PanelFill, UiTheme.PanelBorder, 2);
        spriteBatch.Draw(_pixel, new Rectangle(noteBounds.X + 1, noteBounds.Y + 1, noteBounds.Width - 2, 3), _view == MainMenuView.Home ? UiTheme.Warning : UiTheme.Success);
        UiLabel.Draw(
            spriteBatch,
            _font,
            _view == MainMenuView.Home ? "Current Profile" : "Launch Summary",
            new Vector2(noteBounds.X + 14, noteBounds.Y + 14),
            _view == MainMenuView.Home ? UiTheme.Warning : UiTheme.Success,
            UiTypography.Caption);
        if (_view == MainMenuView.Home)
        {
            var profileSummaryY = noteBounds.Y + 38f;
            var profileHeadlineHeight = UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                $"{_settings.ThemeMode}  |  {UiFontCatalog.GetDisplayName(_settings.UiFont)}  |  {GetGameplayLabel(_settings.SelectedGameplayMode)}",
                new Vector2(noteBounds.X + 14, profileSummaryY),
                noteBounds.Width - 28,
                UiTheme.TextPrimary,
                UiTypography.Body,
                2f,
                2);
            profileSummaryY += profileHeadlineHeight + 8f;
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                $"{_settings.WindowMode}  |  {_settings.PreferredResolution.X} x {_settings.PreferredResolution.Y}  |  {GetDifficultyLabel(_settings.SelectedDifficulty)}  |  {GetSeedSummary()}",
                new Vector2(noteBounds.X + 14, profileSummaryY),
                noteBounds.Width - 28,
                UiTheme.TextMuted,
                UiTypography.Caption,
                2f,
                2);
            return;
        }

        var summaryY = noteBounds.Y + 38f;
        var headlineHeight = UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"{GetGameplayLabel(_settings.SelectedGameplayMode)}  |  {GetGameplayDurationLabel(_settings.SelectedGameplayMode)}",
            new Vector2(noteBounds.X + 14, summaryY),
            noteBounds.Width - 28,
            UiTheme.TextPrimary,
            UiTypography.Body,
            2f,
            2);
        summaryY += headlineHeight + 8f;

        var metaHeight = UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"{GetDifficultyLabel(_settings.SelectedDifficulty)}  |  Realistic+ {(_settings.RealisticSubModeEnabled ? "On" : "Off")}  |  {GetSeedSummary()}",
            new Vector2(noteBounds.X + 14, summaryY),
            noteBounds.Width - 28,
            UiTheme.TextMuted,
            UiTypography.Caption,
            2f,
            2);
        summaryY += metaHeight + 8f;

        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            GetGameplayGoalSummary(_settings.SelectedGameplayMode),
            new Vector2(noteBounds.X + 14, summaryY),
            noteBounds.Width - 28,
            UiTheme.TextMuted,
            UiTypography.Caption,
            2f,
            2);
    }

    private void DrawBriefPanel(SpriteBatch spriteBatch)
    {
        var left = _briefBounds.X + 24;
        var contentWidth = _briefBounds.Width - 48;
        var summary = GetGameplaySummary(_settings.SelectedGameplayMode, _settings.RealisticSubModeEnabled);
        var summaryTop = _briefBounds.Y + 54;
        var summaryHeight = UiTextBlock.MeasureWrappedHeight(_font, summary, contentWidth, UiTypography.Body, 3f, 4);
        var nextRowY = (int)MathF.Round(summaryTop + summaryHeight + 18f);

        UiLabel.Draw(spriteBatch, _font, "Run Preview", new Vector2(left, _briefBounds.Y + 20), UiTheme.TextPrimary, UiTypography.Section);
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

    private int GetActionButtonsTop()
    {
        var introHeight = UiTextBlock.MeasureWrappedHeight(
            _font,
            GetActionPanelIntroText(),
            _actionBounds.Width - 48,
            UiTypography.Body,
            3f,
            _view == MainMenuView.Home ? 4 : 3);
        return (int)MathF.Ceiling(_actionBounds.Y + 62f + introHeight + 22f);
    }

    private int GetActionNoteHeight()
    {
        var contentWidth = _actionBounds.Width - 76;

        if (_view == MainMenuView.Home)
        {
            var headlineHeight = UiTextBlock.MeasureWrappedHeight(
                _font,
                $"{_settings.ThemeMode}  |  {UiFontCatalog.GetDisplayName(_settings.UiFont)}  |  {GetGameplayLabel(_settings.SelectedGameplayMode)}",
                contentWidth,
                UiTypography.Body,
                2f,
                2);
            var metaHeight = UiTextBlock.MeasureWrappedHeight(
                _font,
                $"{_settings.WindowMode}  |  {_settings.PreferredResolution.X} x {_settings.PreferredResolution.Y}  |  {GetDifficultyLabel(_settings.SelectedDifficulty)}  |  {GetSeedSummary()}",
                contentWidth,
                UiTypography.Caption,
                2f,
                2);
            return (int)MathF.Ceiling(Math.Max(114f, 38f + headlineHeight + 8f + metaHeight + 16f));
        }

        var summaryHeadlineHeight = UiTextBlock.MeasureWrappedHeight(
            _font,
            $"{GetGameplayLabel(_settings.SelectedGameplayMode)}  |  {GetGameplayDurationLabel(_settings.SelectedGameplayMode)}",
            contentWidth,
            UiTypography.Body,
            2f,
            2);
        var summaryMetaHeight = UiTextBlock.MeasureWrappedHeight(
            _font,
            $"{GetDifficultyLabel(_settings.SelectedDifficulty)}  |  Realistic+ {(_settings.RealisticSubModeEnabled ? "On" : "Off")}  |  {GetSeedSummary()}",
            contentWidth,
            UiTypography.Caption,
            2f,
            2);
        var summaryGoalHeight = UiTextBlock.MeasureWrappedHeight(
            _font,
            GetGameplayGoalSummary(_settings.SelectedGameplayMode),
            contentWidth,
            UiTypography.Caption,
            2f,
            2);
        return (int)MathF.Ceiling(Math.Max(148f, 38f + summaryHeadlineHeight + 8f + summaryMetaHeight + 8f + summaryGoalHeight + 16f));
    }

    private string GetActionPanelIntroText()
    {
        return _view == MainMenuView.Home
            ? "Start with a cleaner first step. Run options live on the next screen, while appearance and audio stay here."
            : "Review the run summary on the left, then launch when the route and pressure feel right.";
    }

    private void UpdateLayout()
    {
        _shellBounds = new Rectangle(70, 28, _virtualResolution.X - 140, _virtualResolution.Y - 56);
        const int shellTopInset = 30;
        const int shellBottomInset = 24;
        const int panelGap = 18;
        var heroX = _shellBounds.X + 24;
        var heroY = _shellBounds.Y + shellTopInset;
        var contentHeight = _shellBounds.Height - shellTopInset - shellBottomInset;

        if (_view == MainMenuView.Home)
        {
            const int actionWidth = 336;
            var heroWidth = _shellBounds.Width - 48 - panelGap - actionWidth;

            _heroBounds = new Rectangle(heroX, heroY, heroWidth, contentHeight);
            _actionBounds = new Rectangle(_heroBounds.Right + panelGap, heroY, actionWidth, contentHeight);
            _briefBounds = Rectangle.Empty;
            _modeBounds = Rectangle.Empty;
            _difficultyBounds = Rectangle.Empty;

            var actionButtonsTop = GetActionButtonsTop();
            _startButton.Bounds = new Rectangle(_actionBounds.X + 24, actionButtonsTop, _actionBounds.Width - 48, 52);
            _optionsButton.Bounds = new Rectangle(_actionBounds.X + 24, _startButton.Bounds.Bottom + 12, _actionBounds.Width - 48, 46);
            _exitButton.Bounds = new Rectangle(_actionBounds.X + 24, _optionsButton.Bounds.Bottom + 12, _actionBounds.Width - 48, 46);
            _backButton.Bounds = Rectangle.Empty;

            foreach (var button in GetGameplayButtons())
            {
                button.Bounds = Rectangle.Empty;
            }

            _realisticModeButton.Bounds = Rectangle.Empty;
            foreach (var button in GetDifficultyButtons())
            {
                button.Bounds = Rectangle.Empty;
            }

            return;
        }

        const int actionPanelWidth = 336;
        const int previewWidth = 432;
        const int headerHeight = 128;
        var leftAreaWidth = _shellBounds.Width - 48 - panelGap - actionPanelWidth;
        var configColumnWidth = leftAreaWidth - previewWidth - panelGap;
        var lowerY = heroY + headerHeight + panelGap;
        var lowerHeight = _shellBounds.Bottom - shellBottomInset - lowerY;
        var modeHeight = Math.Max(250, (lowerHeight - panelGap) / 2);
        var difficultyHeight = lowerHeight - modeHeight - panelGap;

        _heroBounds = new Rectangle(heroX, heroY, leftAreaWidth, headerHeight);
        _actionBounds = new Rectangle(_heroBounds.Right + panelGap, heroY, actionPanelWidth, contentHeight);
        _briefBounds = new Rectangle(heroX, lowerY, previewWidth, lowerHeight);
        _modeBounds = new Rectangle(_briefBounds.Right + panelGap, lowerY, configColumnWidth, modeHeight);
        _difficultyBounds = new Rectangle(_modeBounds.X, _modeBounds.Bottom + panelGap, configColumnWidth, difficultyHeight);

        var actionButtonTop = GetActionButtonsTop();
        _startButton.Bounds = new Rectangle(_actionBounds.X + 24, actionButtonTop, _actionBounds.Width - 48, 52);
        _optionsButton.Bounds = new Rectangle(_actionBounds.X + 24, _startButton.Bounds.Bottom + 12, _actionBounds.Width - 48, 46);
        _backButton.Bounds = new Rectangle(_actionBounds.X + 24, _optionsButton.Bounds.Bottom + 12, _actionBounds.Width - 48, 46);
        _exitButton.Bounds = Rectangle.Empty;

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
            GameDifficulty.Easy => "Easy",
            GameDifficulty.Hard => "Hard",
            GameDifficulty.ContinualUpgradeLoop => "Upgrade Loop",
            GameDifficulty.Endless => "Endless",
            _ => "Normal",
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
            GameplayLoopMode.Corporate => "Corporate",
            GameplayLoopMode.Indie => "Indie",
            GameplayLoopMode.Founder => "Founder",
            _ => "Interview",
        };
    }

    private static string GetGameplaySummary(GameplayLoopMode gameplayMode, bool realisticMode)
    {
        var baseSummary = gameplayMode switch
        {
            GameplayLoopMode.Corporate => "Steady salary, stricter bosses, and a longer grind toward stability.",
            GameplayLoopMode.Indie => "Self-directed pacing, lighter structure, and leaner income that rewards discipline.",
            GameplayLoopMode.Founder => "Bootstrap a studio, freelance for rent, and grow into a real business.",
            _ => "A seven-day sprint to build proof, land an offer, and unlock the longer career routes.",
        };

        return realisticMode
            ? $"{baseSummary} Realistic+ keeps money tighter and choices sharper."
            : baseSummary;
    }

    private static string GetGameplayFlow(GameplayLoopMode gameplayMode)
    {
        return gameplayMode switch
        {
            GameplayLoopMode.Corporate => "Office -> Code -> Endure\nSave -> Retire",
            GameplayLoopMode.Indie => "Plan -> Build -> Recover\nShip -> Retire",
            GameplayLoopMode.Founder => "Freelance -> Build -> Sell\nScale -> Retire",
            _ => "Build Proof -> Apply -> Interview\nWin -> Branch",
        };
    }

    private static string GetGameplayGoal(GameplayLoopMode gameplayMode)
    {
        return gameplayMode switch
        {
            GameplayLoopMode.Corporate => "Keep the paycheck alive long enough to buy a house and retire.",
            GameplayLoopMode.Indie => "Stay disciplined, ship work, and retire on your own terms.",
            GameplayLoopMode.Founder => "Turn survival freelancing into a studio that can carry you to retirement.",
            _ => "Win the first offer in seven days, then choose the long-form route.",
        };
    }

    private static string GetGameplayGoalSummary(GameplayLoopMode gameplayMode)
    {
        return gameplayMode switch
        {
            GameplayLoopMode.Corporate => "Survive the office climb.",
            GameplayLoopMode.Indie => "Ship steadily and stay free.",
            GameplayLoopMode.Founder => "Turn gigs into a studio.",
            _ => "Land the offer and branch.",
        };
    }

    private static string GetGameplayDurationLabel(GameplayLoopMode gameplayMode)
    {
        return gameplayMode == GameplayLoopMode.Interview
            ? "7-day sprint"
            : "Long career run";
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
