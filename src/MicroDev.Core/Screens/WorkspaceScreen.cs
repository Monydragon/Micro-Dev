using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MicroDev.Core.Audio;
using MicroDev.Core.Input;
using MicroDev.Core.Portfolio;
using MicroDev.Core.Simulation;
using MicroDev.Core.UI;

namespace MicroDev.Core.Screens;

public sealed class WorkspaceScreen : IScreen
{
    private const float CodeScale = 0.9f;
    private const float CardBodyScale = 0.66f;
    private const float BodyScale = 0.74f;
    private const float SmallScale = 0.68f;

    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;
    private readonly SimulationEngine _simulation;
    private readonly IncidentScheduler _incidentScheduler;
    private readonly GameAudio _audio;
    private readonly Point _virtualResolution;
    private readonly UiButton _foodAppButton = new("Delivery App");
    private readonly UiButton _freelanceButton = new("Freelance Board");
    private readonly UiButton _sleepButton = new("Sleep");
    private readonly UiButton _upgradesButton = new("Upgrades");
    private readonly UiButton _squashBugButton = new("Fix");
    private readonly UiButton _applyForJobButton = new("Apply");
    private readonly UiButton _restartButton = new("Restart Run");
    private readonly UiButton _breakCoinButton = new("Break Frame");
    private readonly UiButton _acceptEvictionButton = new("Let Go");
    private readonly UiButton _burgerButton = new("Burger");
    private readonly UiButton _burritoButton = new("Burrito");
    private readonly UiButton _pizzaButton = new("Pizza");
    private readonly UiButton _dumplingsButton = new("Dumplings");
    private readonly UiButton _doubleCheckOrderButton = new("Double-Check Order: OFF");
    private readonly UiButton _confirmFoodOrderButton = new("Place Order");
    private readonly UiButton _closeFoodAppButton = new("Close");
    private readonly UiButton _closeFreelanceBoardButton = new("Close");
    private readonly UiButton _closeUpgradesButton = new("Close");
    private readonly Dictionary<EfficiencyUpgradeType, UiButton> _upgradeButtons = [];
    private readonly Dictionary<FreelanceGigType, UiButton> _freelanceGigButtons = [];

    private RunState _state;
    private Rectangle _editorPanelBounds;
    private Rectangle _editorViewportBounds;
    private Rectangle _sidebarBounds;
    private Rectangle _logBounds;
    private Rectangle _catOverlayBounds;
    private Rectangle _alertsPanelBounds;
    private Rectangle _techDebtCardBounds;
    private Rectangle _jobListingCardBounds;
    private Rectangle _coinFrameBounds;
    private Rectangle _foodAppBounds;
    private Rectangle _freelanceBoardBounds;
    private Rectangle _upgradesBounds;
    private readonly Dictionary<EfficiencyUpgradeType, Rectangle> _upgradeCardBounds = [];
    private readonly Dictionary<FreelanceGigType, Rectangle> _freelanceGigCardBounds = [];
    private bool _foodAppOpen;
    private bool _freelanceBoardOpen;
    private bool _upgradesOpen;
    private FoodChoice _selectedFood = FoodChoice.Burger;
    private bool _doubleCheckOrder;
    private string? _lastCelebratedFileName;
    private Point _mousePosition;

    public WorkspaceScreen(
        SpriteFont font,
        Texture2D pixel,
        SimulationEngine simulation,
        IncidentScheduler incidentScheduler,
        GameAudio audio,
        Point virtualResolution)
    {
        _font = font;
        _pixel = pixel;
        _simulation = simulation;
        _incidentScheduler = incidentScheduler;
        _audio = audio;
        _virtualResolution = virtualResolution;
        _state = _simulation.CreateNewRun();

        foreach (var definition in EfficiencyUpgradeCatalog.All)
        {
            _upgradeButtons[definition.Type] = new UiButton("Buy");
        }

        foreach (var type in Enum.GetValues<FreelanceGigType>())
        {
            _freelanceGigButtons[type] = new UiButton("Take Gig");
        }

        ConfigureButtons();
        UpdateLayout();
        UpdateButtons();
    }

    public void Update(GameTime gameTime, InputSnapshot input)
    {
        var previousStatus = _state.Status;
        _mousePosition = input.MousePosition;

        if (previousStatus == RunStatus.InProgress)
        {
            var elapsedSeconds = gameTime.ElapsedGameTime.TotalSeconds;
            var elapsedInGameMinutes = elapsedSeconds * _simulation.Config.InGameMinutesPerRealSecond;

            _simulation.AdvanceRealTime(_state, elapsedSeconds);

            var queued = _incidentScheduler.Update(_state, elapsedInGameMinutes);
            _simulation.QueueIncidents(_state, queued);
            if (queued.Count > 0)
            {
                _audio.PlayAlert();
            }
        }

        UpdateLayout();
        UpdateButtons();

        if (_state.Status != RunStatus.InProgress)
        {
            _foodAppOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _restartButton.Enabled = true;
            if (_restartButton.Update(input))
            {
                _audio.PlayButtonClick();
                _simulation.ApplyAction(_state, PlayerAction.RestartRun);
            }
        }
        else if (_state.FirstCoinDecisionPending)
        {
            _foodAppOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            HandleFirstCoinInput(input);
        }
        else if (_foodAppOpen)
        {
            HandleFoodAppInput(input);
        }
        else if (_freelanceBoardOpen)
        {
            HandleFreelanceBoardInput(input);
        }
        else if (_upgradesOpen)
        {
            HandleUpgradesInput(input);
        }
        else
        {
            HandleWorkspaceInput(input);
        }

        PlayFileCompletionAudio();
        PlayOutcomeAudio(previousStatus, _state.Status);
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        DrawWorkspace(spriteBatch);

        if (_foodAppOpen && _state.Status == RunStatus.InProgress)
        {
            DrawFoodAppOverlay(spriteBatch);
        }
        else if (_freelanceBoardOpen && _state.Status == RunStatus.InProgress)
        {
            DrawFreelanceBoardOverlay(spriteBatch);
        }
        else if (_upgradesOpen && _state.Status == RunStatus.InProgress)
        {
            DrawUpgradesOverlay(spriteBatch);
        }

        if (_state.Status != RunStatus.InProgress)
        {
            DrawOutcomeOverlay(spriteBatch);
        }
        else if (_state.FirstCoinDecisionPending)
        {
            DrawFirstCoinOverlay(spriteBatch);
        }
        else
        {
            DrawTooltip(spriteBatch);
        }
    }

    private void ConfigureButtons()
    {
        _foodAppButton.TextScale = 0.8f;
        _freelanceButton.TextScale = 0.8f;
        _sleepButton.TextScale = 0.8f;
        _upgradesButton.TextScale = 0.8f;
        _squashBugButton.TextScale = 0.64f;
        _applyForJobButton.TextScale = 0.64f;
        _restartButton.TextScale = 0.92f;
        _breakCoinButton.TextScale = 0.82f;
        _acceptEvictionButton.TextScale = 0.8f;
        _burgerButton.TextScale = 0.82f;
        _burritoButton.TextScale = 0.82f;
        _pizzaButton.TextScale = 0.82f;
        _dumplingsButton.TextScale = 0.76f;
        _doubleCheckOrderButton.TextScale = 0.74f;
        _confirmFoodOrderButton.TextScale = 0.84f;
        _closeFoodAppButton.TextScale = 0.72f;
        _closeFreelanceBoardButton.TextScale = 0.72f;
        _closeUpgradesButton.TextScale = 0.72f;

        foreach (var button in _upgradeButtons.Values)
        {
            button.TextScale = 0.72f;
        }

        foreach (var button in _freelanceGigButtons.Values)
        {
            button.TextScale = 0.72f;
        }
    }

    private void HandleWorkspaceInput(InputSnapshot input)
    {
        if (_foodAppButton.Update(input))
        {
            _foodAppOpen = true;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_freelanceButton.Update(input))
        {
            _freelanceBoardOpen = true;
            _foodAppOpen = false;
            _upgradesOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_upgradesButton.Update(input))
        {
            _upgradesOpen = true;
            _foodAppOpen = false;
            _freelanceBoardOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (TryApplyButtonAction(input, _sleepButton, PlayerAction.Sleep))
        {
            return;
        }

        if (TryApplyButtonAction(input, _squashBugButton, PlayerAction.SquashBug))
        {
            return;
        }

        if (TryApplyButtonAction(input, _applyForJobButton, PlayerAction.ApplyForJob))
        {
            return;
        }

        if (!input.IsLeftClickInside(_editorViewportBounds))
        {
            return;
        }

        var editorAction = _state.ActiveCatInterruption is not null
            ? PlayerAction.PetCat
            : PlayerAction.WriteCode;

        var applied = _simulation.ApplyAction(_state, editorAction);
        if (applied)
        {
            if (editorAction == PlayerAction.WriteCode)
            {
                _audio.PlayWriteKey();
            }
            else
            {
                _audio.PlayButtonClick();
            }
        }
        else
        {
            _audio.PlayFailure();
        }
    }

    private bool TryApplyButtonAction(InputSnapshot input, UiButton button, PlayerAction action)
    {
        if (!button.Update(input))
        {
            return false;
        }

        var applied = _simulation.ApplyAction(_state, action);
        if (applied)
        {
            _audio.PlayButtonClick();
        }
        else
        {
            _audio.PlayFailure();
        }

        return true;
    }

    private void HandleFoodAppInput(InputSnapshot input)
    {
        if (_closeFoodAppButton.Update(input))
        {
            _foodAppOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_burgerButton.Update(input))
        {
            _selectedFood = FoodChoice.Burger;
            _audio.PlayButtonClick();
        }

        if (_burritoButton.Update(input))
        {
            _selectedFood = FoodChoice.Burrito;
            _audio.PlayButtonClick();
        }

        if (_pizzaButton.Update(input))
        {
            _selectedFood = FoodChoice.Pizza;
            _audio.PlayButtonClick();
        }

        if (_dumplingsButton.Update(input))
        {
            _selectedFood = FoodChoice.Dumplings;
            _audio.PlayButtonClick();
        }

        if (_doubleCheckOrderButton.Update(input))
        {
            _doubleCheckOrder = !_doubleCheckOrder;
            _audio.PlayButtonClick();
        }

        if (_confirmFoodOrderButton.Update(input))
        {
            if (_simulation.PlaceFoodOrder(_state, _selectedFood, _doubleCheckOrder))
            {
                _foodAppOpen = false;
                _audio.PlayButtonClick();
            }
            else
            {
                _audio.PlayFailure();
            }
        }
    }

    private void HandleFreelanceBoardInput(InputSnapshot input)
    {
        if (_closeFreelanceBoardButton.Update(input))
        {
            _freelanceBoardOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        foreach (var type in Enum.GetValues<FreelanceGigType>())
        {
            var button = _freelanceGigButtons[type];
            if (!button.Update(input))
            {
                continue;
            }

            var taken = _simulation.TakeFreelanceGig(_state, type);
            if (taken)
            {
                _freelanceBoardOpen = false;
                _audio.PlayButtonClick();
            }
            else
            {
                _audio.PlayFailure();
            }

            return;
        }
    }

    private void HandleFirstCoinInput(InputSnapshot input)
    {
        if (_breakCoinButton.Update(input))
        {
            if (_simulation.UseFirstCoin(_state))
            {
                _audio.PlaySuccess();
            }
            else
            {
                _audio.PlayFailure();
            }

            return;
        }

        if (_acceptEvictionButton.Update(input))
        {
            if (_simulation.DeclineFirstCoin(_state))
            {
                _audio.PlayFailure();
            }
            else
            {
                _audio.PlayFailure();
            }
        }
    }

    private void HandleUpgradesInput(InputSnapshot input)
    {
        if (_closeUpgradesButton.Update(input))
        {
            _upgradesOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        foreach (var definition in EfficiencyUpgradeCatalog.All)
        {
            var button = _upgradeButtons[definition.Type];
            if (!button.Update(input))
            {
                continue;
            }

            var purchased = _simulation.PurchaseUpgrade(_state, definition.Type);
            if (purchased)
            {
                _audio.PlaySuccess();
            }
            else
            {
                _audio.PlayFailure();
            }

            return;
        }
    }

    private void DrawWorkspace(SpriteBatch spriteBatch)
    {
        DrawBackdrop(spriteBatch);

        UiPanel.Draw(spriteBatch, _pixel, _editorPanelBounds, UiTheme.PanelFill, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _editorViewportBounds, UiTheme.EditorFill, UiTheme.EditorBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _sidebarBounds, UiTheme.PanelFill, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _logBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        spriteBatch.Draw(_pixel, new Rectangle(_editorPanelBounds.X + 1, _editorPanelBounds.Y + 1, _editorPanelBounds.Width - 2, 4), UiTheme.AccentDim);
        spriteBatch.Draw(_pixel, new Rectangle(_sidebarBounds.X + 1, _sidebarBounds.Y + 1, _sidebarBounds.Width - 2, 4), UiTheme.AccentDim);

        UiLabel.Draw(spriteBatch, _font, "Micro Dev Workspace", new Vector2(_editorPanelBounds.X + 18, _editorPanelBounds.Y + 12), UiTheme.TextPrimary, 1.08f);
        UiLabel.Draw(spriteBatch, _font, $"Day {_state.Day}  {_state.ClockText}", new Vector2(_editorPanelBounds.Right - 172, _editorPanelBounds.Y + 14), UiTheme.TextMuted, 0.88f);

        DrawCodeEditor(spriteBatch);
        DrawSidebar(spriteBatch);
        DrawEventLog(spriteBatch);
    }

    private void DrawBackdrop(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y), UiTheme.DesktopBackground);
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, 180), UiTheme.DesktopGlow * 0.18f);
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, 2), UiTheme.AccentDim);

        for (var index = 0; index < 5; index++)
        {
            var y = 108 + (index * 148);
            spriteBatch.Draw(_pixel, new Rectangle(0, y, _virtualResolution.X, 1), UiTheme.AccentDim * 0.18f);
        }
    }

    private void DrawCodeEditor(SpriteBatch spriteBatch)
    {
        var program = PortfolioWorkspace.GetCurrentProgram(_state);
        var visibleLines = PortfolioWorkspace.GetVisibleLines(_state);
        var fileIndex = Math.Clamp(_state.CurrentProgramIndex + 1, 1, PortfolioWorkspace.ProgramCount);

        var tabBounds = new Rectangle(_editorViewportBounds.X, _editorViewportBounds.Y - 38, 300, 34);
        UiPanel.Draw(spriteBatch, _pixel, tabBounds, UiTheme.PanelRaised, UiTheme.EditorBorder, 2);
        UiLabel.Draw(
            spriteBatch,
            _font,
            UiTextBlock.TrimToWidth(_font, program.FileName, tabBounds.Width - 28, 0.8f),
            new Vector2(tabBounds.X + 14, tabBounds.Y + 7),
            UiTheme.TextPrimary,
            0.8f);

        var contentX = _editorViewportBounds.X + 18;
        var contentWidth = _editorViewportBounds.Width - 36;
        var titleY = _editorViewportBounds.Y + 12;
        var projectMeta = $"File {fileIndex}/{PortfolioWorkspace.ProgramCount}";
        var projectMetaSize = _font.MeasureString(projectMeta) * SmallScale;

        UiLabel.Draw(
            spriteBatch,
            _font,
            program.ProjectName,
            new Vector2(contentX, titleY),
            UiTheme.Accent,
            0.92f);

        UiLabel.Draw(
            spriteBatch,
            _font,
            projectMeta,
            new Vector2(_editorViewportBounds.Right - 18 - projectMetaSize.X, titleY + 3),
            UiTheme.TextMuted,
            SmallScale);

        DrawFirstCoinFrame(spriteBatch);

        var descriptionY = titleY + 30;
        var descriptionHeight = UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            program.Description,
            new Vector2(contentX, descriptionY),
            contentWidth - 120,
            UiTheme.TextMuted,
            BodyScale,
            2f,
            2);

        var completedPrograms = PortfolioWorkspace.GetCompletedProgramCount(_state);
        var progressY = descriptionY + descriptionHeight + 14f;
        UiLabel.Draw(
            spriteBatch,
            _font,
            $"Portfolio {completedPrograms}/{PortfolioWorkspace.ProgramCount} files complete",
            new Vector2(contentX, progressY),
            UiTheme.TextMuted,
            0.66f);

        var progressStripX = contentX;
        var progressStripY = progressY + 24f;
        for (var index = 0; index < PortfolioWorkspace.ProgramCount; index++)
        {
            var fill = index < completedPrograms
                ? UiTheme.Success
                : index == _state.CurrentProgramIndex
                    ? UiTheme.Accent
                    : UiTheme.PanelRaised;
            var border = index < completedPrograms ? UiTheme.Success : UiTheme.PanelBorder;
            UiPanel.Draw(
                spriteBatch,
                _pixel,
                new Rectangle(progressStripX + (index * 22), (int)progressStripY, 16, 8),
                fill,
                border,
                1);
        }

        var codeTop = (int)MathF.Ceiling(progressStripY + 20f);

        if (!string.IsNullOrEmpty(_state.RecentCompletedFileName))
        {
            var completionBanner = new Rectangle(contentX, codeTop, contentWidth, 40);
            UiPanel.Draw(spriteBatch, _pixel, completionBanner, new Color(27, 61, 44), UiTheme.Success, 1);
            UiLabel.Draw(
                spriteBatch,
                _font,
                $"Commit Complete: {_state.RecentCompletedFileName}",
                new Vector2(completionBanner.X + 12, completionBanner.Y + 9),
                UiTheme.Success,
                0.76f);
            codeTop = completionBanner.Bottom + 12;
        }

        if (_state.ActiveTechDebtBug is not null)
        {
            var bugBanner = new Rectangle(contentX, codeTop, contentWidth, 44);
            UiPanel.Draw(spriteBatch, _pixel, bugBanner, new Color(94, 42, 42), UiTheme.Danger, 1);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                $"Tech debt alert: {_state.ActiveTechDebtBug.Summary}",
                new Vector2(bugBanner.X + 10, bugBanner.Y + 7),
                bugBanner.Width - 20,
                UiTheme.TextPrimary,
                BodyScale,
                1f,
                2);
            codeTop = bugBanner.Bottom + 16;
        }

        var guidance = GetEditorGuidance(program);
        var guidanceHeight = UiTextBlock.MeasureWrappedHeight(_font, guidance, contentWidth, BodyScale, 2f, 2);
        var guidanceY = _editorViewportBounds.Bottom - 20 - guidanceHeight;

        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            guidance,
            new Vector2(contentX, guidanceY),
            contentWidth,
            UiTheme.TextMuted,
            BodyScale,
            2f,
            2);

        DrawCodeViewport(spriteBatch, visibleLines, codeTop, guidanceY - 18f, contentX, contentWidth);

        if (_state.ActiveCatInterruption is not null)
        {
            UiPanel.Draw(spriteBatch, _pixel, _catOverlayBounds, new Color(80, 45, 24, 232), UiTheme.CatAccent, 3);
            UiLabel.Draw(spriteBatch, _font, "Desk Cat Event", new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 18), UiTheme.CatAccent, 1.0f);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                "The cat decided your keyboard is warmer than any bed. Pet it away before it times out and trashes a chunk of the current draft.",
                new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 56),
                _catOverlayBounds.Width - 48,
                UiTheme.TextPrimary,
                0.76f,
                2f,
                3);
            UiLabel.Draw(spriteBatch, _font, $"Pet clicks remaining: {_state.ActiveCatInterruption.PatsRemaining}", new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 136), UiTheme.TextPrimary, 0.78f);
            UiLabel.Draw(spriteBatch, _font, $"Leaves in: {FormatRemainingTime(_state.ActiveCatInterruption.RemainingInGameMinutes)}", new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 164), UiTheme.TextMuted, 0.76f);
            UiLabel.Draw(spriteBatch, _font, $"Deletion risk: {_state.ActiveCatInterruption.LinesDeletionPenalty} LoC", new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 192), UiTheme.Warning, 0.76f);
        }
    }

    private void DrawFirstCoinFrame(SpriteBatch spriteBatch)
    {
        var frameFill = _state.HasFirstCoin
            ? new Color(47, 43, 30)
            : new Color(36, 37, 42);
        var frameBorder = _state.HasFirstCoin
            ? UiTheme.CoinAccent
            : UiTheme.PanelBorder;
        var coinColor = _state.HasFirstCoin
            ? UiTheme.CoinAccent
            : UiTheme.TextMuted;

        UiPanel.Draw(spriteBatch, _pixel, _coinFrameBounds, frameFill, frameBorder, 2);

        var innerBounds = new Rectangle(_coinFrameBounds.X + 10, _coinFrameBounds.Y + 10, _coinFrameBounds.Width - 20, _coinFrameBounds.Height - 20);
        UiPanel.Draw(spriteBatch, _pixel, innerBounds, new Color(24, 27, 34), frameBorder * 0.65f, 1);

        var coinBounds = new Rectangle(innerBounds.Center.X - 16, innerBounds.Y + 10, 32, 32);
        spriteBatch.Draw(_pixel, coinBounds, coinColor);
        spriteBatch.Draw(_pixel, new Rectangle(coinBounds.X + 4, coinBounds.Y + 4, coinBounds.Width - 8, coinBounds.Height - 8), frameFill);
        UiLabel.Draw(
            spriteBatch,
            _font,
            "$1",
            new Vector2(coinBounds.X + 3, coinBounds.Y + 3),
            coinColor,
            0.56f);

        var frameLabel = _state.HasFirstCoin
            ? $"First Coin  +{_simulation.Config.FirstCoinPassiveSanityRegenPerInGameMinute * 60:0.0}/h sanity"
            : "First Coin  spent";
        UiLabel.Draw(
            spriteBatch,
            _font,
            UiTextBlock.TrimToWidth(_font, frameLabel, _coinFrameBounds.Width - 16, 0.52f),
            new Vector2(_coinFrameBounds.X + 8, _coinFrameBounds.Bottom - 22),
            _state.HasFirstCoin ? UiTheme.TextPrimary : UiTheme.TextMuted,
            0.52f);
    }

    private void DrawCodeViewport(
        SpriteBatch spriteBatch,
        IReadOnlyList<string> visibleLines,
        int codeTop,
        float codeBottom,
        int contentX,
        int contentWidth)
    {
        var lineHeight = (_font.LineSpacing * CodeScale) + 5f;
        var availableHeight = Math.Max(0f, codeBottom - codeTop);
        var maxVisibleLines = Math.Max(1, (int)Math.Floor(availableHeight / lineHeight));
        var displayLines = visibleLines.Count <= maxVisibleLines
            ? visibleLines
            : visibleLines.Skip(visibleLines.Count - maxVisibleLines).ToArray();

        var lineNumberWidth = 42f;
        var codeX = contentX + (int)lineNumberWidth + 18;
        var codeWidth = contentWidth - (int)lineNumberWidth - 18;
        var lineY = (float)codeTop;

        if (displayLines.Count == 0)
        {
            UiLabel.Draw(spriteBatch, _font, "  1", new Vector2(contentX + 2, lineY + 2), UiTheme.TextMuted, CodeScale);
            spriteBatch.Draw(_pixel, new Rectangle(codeX, (int)lineY + 5, 3, 23), UiTheme.Accent);
            return;
        }

        var startingLine = Math.Max(1, visibleLines.Count - displayLines.Count + 1);
        for (var index = 0; index < displayLines.Count; index++)
        {
            var line = displayLines[index];
            var lineNumber = $"{startingLine + index,3}";
            var codeText = string.IsNullOrWhiteSpace(line)
                ? string.Empty
                : UiTextBlock.TrimToWidth(_font, line, codeWidth, CodeScale);

            UiLabel.Draw(spriteBatch, _font, lineNumber, new Vector2(contentX + 2, lineY + 2), UiTheme.TextMuted, CodeScale);

            if (!string.IsNullOrEmpty(codeText))
            {
                spriteBatch.DrawString(
                    _font,
                    codeText,
                    new Vector2(codeX, lineY),
                    GetCodeLineColor(line),
                    0f,
                    Vector2.Zero,
                    CodeScale,
                    SpriteEffects.None,
                    0f);
            }

            lineY += lineHeight;
        }
    }

    private void DrawSidebar(SpriteBatch spriteBatch)
    {
        var contentX = _sidebarBounds.X + 16;
        UiLabel.Draw(spriteBatch, _font, "Desk Dashboard", new Vector2(contentX, _sidebarBounds.Y + 14), UiTheme.TextPrimary, 0.96f);

        DrawSummaryCards(spriteBatch);
        DrawProgressBars(spriteBatch);
        DrawStatusStrip(spriteBatch);
        DrawActionButtons(spriteBatch);
        DrawAlertsPanel(spriteBatch);
    }

    private void DrawSummaryCards(SpriteBatch spriteBatch)
    {
        const int padding = 16;
        const int gap = 10;
        const int cardHeight = 56;
        var cardWidth = (_sidebarBounds.Width - (padding * 2) - gap) / 2;
        var leftX = _sidebarBounds.X + padding;
        var rightX = leftX + cardWidth + gap;
        var firstRowY = _sidebarBounds.Y + 46;
        var secondRowY = firstRowY + cardHeight + gap;

        DrawStatCard(spriteBatch, new Rectangle(leftX, firstRowY, cardWidth, cardHeight), "Funds", $"${_state.Funds:0}", UiTheme.Warning);
        DrawStatCard(spriteBatch, new Rectangle(rightX, firstRowY, cardWidth, cardHeight), "LoC", _state.LinesOfCode.ToString(), UiTheme.Accent);
        DrawStatCard(spriteBatch, new Rectangle(leftX, secondRowY, cardWidth, cardHeight), "Clock", _state.ClockText, UiTheme.TextPrimary);
        DrawStatCard(spriteBatch, new Rectangle(rightX, secondRowY, cardWidth, cardHeight), "Apps", _state.SuccessfulApplications.ToString(), UiTheme.Success);
    }

    private void DrawStatCard(SpriteBatch spriteBatch, Rectangle bounds, string label, string value, Color valueColor)
    {
        UiPanel.Draw(spriteBatch, _pixel, bounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, label, new Vector2(bounds.X + 10, bounds.Y + 8), UiTheme.TextMuted, 0.68f);
        UiLabel.Draw(spriteBatch, _font, value, new Vector2(bounds.X + 10, bounds.Y + 28), valueColor, 0.94f);
    }

    private void DrawProgressBars(SpriteBatch spriteBatch)
    {
        var barWidth = _sidebarBounds.Width - 32;
        var x = _sidebarBounds.X + 16;
        var startY = _sidebarBounds.Y + 184;
        const int spacing = 44;

        var focusBar = new UiProgressBar("Focus", UiTheme.Focus)
        {
            Bounds = new Rectangle(x, startY, barWidth, 16),
            Value = _state.Focus,
            MaxValue = _simulation.Config.MaxFocus,
        };

        var sanityBar = new UiProgressBar("Sanity", UiTheme.Sanity)
        {
            Bounds = new Rectangle(x, startY + spacing, barWidth, 16),
            Value = _state.Sanity,
            MaxValue = _simulation.Config.MaxSanity,
        };

        var qualityBar = new UiProgressBar("Code Quality", UiTheme.Quality)
        {
            Bounds = new Rectangle(x, startY + (spacing * 2), barWidth, 16),
            Value = _state.CodeQuality,
            MaxValue = _simulation.Config.MaxCodeQuality,
        };

        focusBar.Draw(spriteBatch, _pixel, _font);
        sanityBar.Draw(spriteBatch, _pixel, _font);
        qualityBar.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawStatusStrip(SpriteBatch spriteBatch)
    {
        var stripBounds = new Rectangle(_sidebarBounds.X + 16, _sidebarBounds.Y + 330, _sidebarBounds.Width - 32, 54);
        var fill = UiTheme.PanelRaised;
        var border = UiTheme.PanelBorder;
        var message = $"Typing {_simulation.GetCurrentWriteLinesPerClick(_state)} line/click at {_simulation.GetCurrentWriteFocusCost(_state):0.0} focus. Upgrades buy speed back.";
        var textColor = UiTheme.TextMuted;

        if (_state.ActiveCatInterruption is not null)
        {
            fill = new Color(82, 56, 28);
            border = UiTheme.CatAccent;
            textColor = UiTheme.CatAccent;
            message = $"Cat blocking the keyboard for {FormatRemainingTime(_state.ActiveCatInterruption.RemainingInGameMinutes)}.";
        }
        else if (_simulation.IsSluggish(_state))
        {
            fill = new Color(84, 64, 26);
            border = UiTheme.Warning;
            textColor = UiTheme.Warning;
            message = $"Sluggish for {FormatRemainingTime(_state.SluggishMinutesRemaining)}. A messy meal choice is costing output.";
        }
        else if (_state.ActiveTechDebtBug is not null)
        {
            fill = new Color(58, 34, 34);
            border = UiTheme.Danger;
            textColor = UiTheme.Danger;
            message = "Code quality is draining until the current bug gets fixed.";
        }

        UiPanel.Draw(spriteBatch, _pixel, stripBounds, fill, border, 2);

        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            message,
            new Vector2(stripBounds.X + 12, stripBounds.Y + 10),
            stripBounds.Width - 24,
            textColor,
            BodyScale,
            1f,
            2);
    }

    private void DrawActionButtons(SpriteBatch spriteBatch)
    {
        UiLabel.Draw(spriteBatch, _font, "Actions", new Vector2(_sidebarBounds.X + 16, _sidebarBounds.Y + 402), UiTheme.TextPrimary, 0.88f);
        _foodAppButton.Draw(spriteBatch, _pixel, _font);
        _freelanceButton.Draw(spriteBatch, _pixel, _font);
        _upgradesButton.Draw(spriteBatch, _pixel, _font);
        _sleepButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawAlertsPanel(SpriteBatch spriteBatch)
    {
        UiPanel.Draw(spriteBatch, _pixel, _alertsPanelBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, "Alerts & Inbox", new Vector2(_alertsPanelBounds.X + 12, _alertsPanelBounds.Y + 10), UiTheme.TextPrimary, 0.82f);

        if (_state.ActiveTechDebtBug is null && _state.ActiveJobListing is null)
        {
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                "No urgent bug fires or recruiter pings right now. Keep building until the next break or opportunity lands.",
                new Vector2(_alertsPanelBounds.X + 12, _alertsPanelBounds.Y + 38),
                _alertsPanelBounds.Width - 24,
                UiTheme.TextMuted,
                BodyScale,
                2f,
                4);
            return;
        }

        if (_techDebtCardBounds != Rectangle.Empty)
        {
            DrawTechDebtCard(spriteBatch);
        }

        if (_jobListingCardBounds != Rectangle.Empty)
        {
            DrawJobListingCard(spriteBatch);
        }
    }

    private void DrawTechDebtCard(SpriteBatch spriteBatch)
    {
        var bug = _state.ActiveTechDebtBug!;
        UiPanel.Draw(spriteBatch, _pixel, _techDebtCardBounds, new Color(56, 34, 34), UiTheme.Danger, 2);
        UiLabel.Draw(spriteBatch, _font, "Tech Debt", new Vector2(_techDebtCardBounds.X + 10, _techDebtCardBounds.Y + 8), UiTheme.Danger, 0.74f);
        _squashBugButton.Draw(spriteBatch, _pixel, _font);

        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            bug.Summary,
            new Vector2(_techDebtCardBounds.X + 10, _techDebtCardBounds.Y + 28),
            _techDebtCardBounds.Width - 20,
            UiTheme.TextPrimary,
            CardBodyScale,
            1f,
            2);

        var footer = $"Quality drain for {FormatRemainingTime(bug.RemainingInGameMinutes)}";
        UiLabel.Draw(
            spriteBatch,
            _font,
            UiTextBlock.TrimToWidth(_font, footer, _techDebtCardBounds.Width - 20, 0.64f),
            new Vector2(_techDebtCardBounds.X + 10, _techDebtCardBounds.Bottom - 20),
            UiTheme.TextMuted,
            0.64f);
    }

    private void DrawJobListingCard(SpriteBatch spriteBatch)
    {
        var listing = _state.ActiveJobListing!;
        UiPanel.Draw(spriteBatch, _pixel, _jobListingCardBounds, new Color(29, 49, 42), UiTheme.Success, 2);
        _applyForJobButton.Draw(spriteBatch, _pixel, _font);

        var titleWidth = _jobListingCardBounds.Width - 20 - _applyForJobButton.Bounds.Width - 8;
        UiLabel.Draw(
            spriteBatch,
            _font,
            UiTextBlock.TrimToWidth(_font, listing.Title, titleWidth, 0.72f),
            new Vector2(_jobListingCardBounds.X + 10, _jobListingCardBounds.Y + 8),
            UiTheme.Success,
            0.72f);
        UiLabel.Draw(
            spriteBatch,
            _font,
            UiTextBlock.TrimToWidth(_font, listing.TechStack, _jobListingCardBounds.Width - 20, 0.64f),
            new Vector2(_jobListingCardBounds.X + 10, _jobListingCardBounds.Y + 28),
            UiTheme.TextPrimary,
            0.64f);

        var requirements =
            $"Needs {listing.MinimumPortfolioLines} LoC, {listing.MinimumCodeQuality:0} quality, and {listing.ResumeCostLines} LoC to tailor the resume.";
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            requirements,
            new Vector2(_jobListingCardBounds.X + 10, _jobListingCardBounds.Y + 46),
            _jobListingCardBounds.Width - 20,
            UiTheme.TextMuted,
            CardBodyScale,
            1f,
            3);

        var deadline = $"Deadline: {FormatRemainingTime(listing.RemainingInGameMinutes)}";
        UiLabel.Draw(
            spriteBatch,
            _font,
            UiTextBlock.TrimToWidth(_font, deadline, _jobListingCardBounds.Width - 20, 0.64f),
            new Vector2(_jobListingCardBounds.X + 10, _jobListingCardBounds.Bottom - 20),
            UiTheme.TextMuted,
            0.64f);
    }

    private void DrawEventLog(SpriteBatch spriteBatch)
    {
        UiLabel.Draw(spriteBatch, _font, "Status Feed", new Vector2(_logBounds.X + 18, _logBounds.Y + 12), UiTheme.TextPrimary, 0.92f);

        var maxWidth = _logBounds.Width - 36;
        var lineY = _logBounds.Y + 38f;
        var maxBottom = _logBounds.Bottom - 14f;

        for (var index = _state.EventLog.Count - 1; index >= 0; index--)
        {
            var entry = _state.EventLog[index];
            var entryHeight = UiTextBlock.MeasureWrappedHeight(_font, entry, maxWidth, 0.72f, 2f, 2);
            if (lineY + entryHeight > maxBottom)
            {
                break;
            }

            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                entry,
                new Vector2(_logBounds.X + 18, lineY),
                maxWidth,
                UiTheme.TextMuted,
                0.72f,
                2f,
                2);
            lineY += entryHeight + 8;
        }
    }

    private void DrawFoodAppOverlay(SpriteBatch spriteBatch)
    {
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        UiPanel.Draw(spriteBatch, _pixel, _foodAppBounds, UiTheme.PanelFill, UiTheme.Accent, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_foodAppBounds.X + 1, _foodAppBounds.Y + 1, _foodAppBounds.Width - 2, 4), UiTheme.Accent);

        UiLabel.Draw(spriteBatch, _font, "Food Delivery App", new Vector2(_foodAppBounds.X + 24, _foodAppBounds.Y + 18), UiTheme.TextPrimary, 1.0f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Pick a meal, compare the effects, then decide whether to add a careful order note. Skipping the note risks a sluggish coding block.",
            new Vector2(_foodAppBounds.X + 24, _foodAppBounds.Y + 52),
            _foodAppBounds.Width - 48,
            UiTheme.TextMuted,
            0.76f,
            2f,
            3);

        var choiceBounds = new Rectangle(_foodAppBounds.X + 24, _foodAppBounds.Y + 120, _foodAppBounds.Width - 48, 110);
        UiPanel.Draw(spriteBatch, _pixel, choiceBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        _burgerButton.Draw(spriteBatch, _pixel, _font);
        _burritoButton.Draw(spriteBatch, _pixel, _font);
        _pizzaButton.Draw(spriteBatch, _pixel, _font);
        _dumplingsButton.Draw(spriteBatch, _pixel, _font);

        var summaryBounds = new Rectangle(_foodAppBounds.X + 24, _foodAppBounds.Y + 244, _foodAppBounds.Width - 48, 122);
        UiPanel.Draw(spriteBatch, _pixel, summaryBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        _doubleCheckOrderButton.Draw(spriteBatch, _pixel, _font);
        _confirmFoodOrderButton.Draw(spriteBatch, _pixel, _font);
        _closeFoodAppButton.Draw(spriteBatch, _pixel, _font);

        var option = _simulation.GetFoodOption(_selectedFood);
        var penaltyText = _doubleCheckOrder
            ? "Careful note added. The sluggish penalty is avoided."
            : $"Unchecked order risk: sluggish for {FormatRemainingTime(option.SluggishMinutesWhenUnchecked)}.";

        UiLabel.Draw(spriteBatch, _font, $"Selected meal: {option.Name}", new Vector2(summaryBounds.X + 16, summaryBounds.Y + 16), UiTheme.TextPrimary, 0.82f);
        UiLabel.Draw(spriteBatch, _font, $"Cost: ${option.FundsCost:0}   Focus: +{option.FocusGain:0}   Sanity: {option.SanityGain:+0;-0;0}", new Vector2(summaryBounds.X + 16, summaryBounds.Y + 44), UiTheme.Warning, 0.74f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            option.Description,
            new Vector2(summaryBounds.X + 16, summaryBounds.Y + 74),
            summaryBounds.Width - 32,
            UiTheme.TextMuted,
            0.68f,
            2f,
            2);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            penaltyText,
            new Vector2(summaryBounds.X + 16, summaryBounds.Y + 98),
            summaryBounds.Width - 32,
            _doubleCheckOrder ? UiTheme.Success : UiTheme.Warning,
            0.72f,
            2f,
            2);
    }

    private void DrawFreelanceBoardOverlay(SpriteBatch spriteBatch)
    {
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        UiPanel.Draw(spriteBatch, _pixel, _freelanceBoardBounds, UiTheme.PanelFill, UiTheme.Accent, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_freelanceBoardBounds.X + 1, _freelanceBoardBounds.Y + 1, _freelanceBoardBounds.Width - 2, 4), UiTheme.Accent);

        UiLabel.Draw(spriteBatch, _font, "Freelance Board", new Vector2(_freelanceBoardBounds.X + 24, _freelanceBoardBounds.Y + 18), UiTheme.TextPrimary, 1.0f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Each gig trades time, focus, and sanity for cash. Hover the buttons to compare the damage before you accept.",
            new Vector2(_freelanceBoardBounds.X + 24, _freelanceBoardBounds.Y + 52),
            _freelanceBoardBounds.Width - 48,
            UiTheme.TextMuted,
            0.76f,
            2f,
            3);

        foreach (var type in Enum.GetValues<FreelanceGigType>())
        {
            var gig = _simulation.GetFreelanceGig(type);
            var cardBounds = _freelanceGigCardBounds[type];
            UiPanel.Draw(spriteBatch, _pixel, cardBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
            UiLabel.Draw(spriteBatch, _font, gig.Name, new Vector2(cardBounds.X + 14, cardBounds.Y + 14), UiTheme.TextPrimary, 0.8f);
            UiLabel.Draw(
                spriteBatch,
                _font,
                $"{FormatRemainingTime(gig.DurationMinutes)}  |  +${gig.FundsGain:0}  |  -{gig.FocusCost:0} focus  |  -{gig.SanityCost:0} sanity",
                new Vector2(cardBounds.X + 14, cardBounds.Y + 42),
                UiTheme.Warning,
                0.66f);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                gig.Description,
                new Vector2(cardBounds.X + 14, cardBounds.Y + 68),
                cardBounds.Width - 144,
                UiTheme.TextMuted,
                0.68f,
                2f,
                2);
            if (gig.CodeQualityGain > 0)
            {
                UiLabel.Draw(
                    spriteBatch,
                    _font,
                    $"+{gig.CodeQualityGain:0.#} quality",
                    new Vector2(cardBounds.X + 14, cardBounds.Bottom - 26),
                    UiTheme.Success,
                    0.66f);
            }

            _freelanceGigButtons[type].Draw(spriteBatch, _pixel, _font);
        }

        _closeFreelanceBoardButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawUpgradesOverlay(SpriteBatch spriteBatch)
    {
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        UiPanel.Draw(spriteBatch, _pixel, _upgradesBounds, UiTheme.PanelFill, UiTheme.Accent, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_upgradesBounds.X + 1, _upgradesBounds.Y + 1, _upgradesBounds.Width - 2, 4), UiTheme.Accent);

        UiLabel.Draw(spriteBatch, _font, "Rig Upgrades", new Vector2(_upgradesBounds.X + 24, _upgradesBounds.Y + 18), UiTheme.TextPrimary, 1.02f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Base typing is slow on purpose now. Buy your way back to flow with better tools, cleaner habits, and automation.",
            new Vector2(_upgradesBounds.X + 24, _upgradesBounds.Y + 52),
            _upgradesBounds.Width - 48,
            UiTheme.TextMuted,
            0.76f,
            2f,
            3);

        foreach (var definition in EfficiencyUpgradeCatalog.All)
        {
            var cardBounds = _upgradeCardBounds[definition.Type];
            var purchased = _state.PurchasedUpgrades.Contains(definition.Type);
            var fill = purchased ? new Color(26, 56, 43) : UiTheme.PanelRaised;
            var border = purchased ? UiTheme.Success : UiTheme.PanelBorder;

            UiPanel.Draw(spriteBatch, _pixel, cardBounds, fill, border, 2);
            UiLabel.Draw(
                spriteBatch,
                _font,
                UiTextBlock.TrimToWidth(_font, definition.Name, cardBounds.Width - 24, 0.76f),
                new Vector2(cardBounds.X + 12, cardBounds.Y + 12),
                purchased ? UiTheme.Success : UiTheme.TextPrimary,
                0.76f);
            UiLabel.Draw(
                spriteBatch,
                _font,
                definition.SummaryEffect,
                new Vector2(cardBounds.X + 12, cardBounds.Y + 38),
                UiTheme.Warning,
                0.68f);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                definition.Description,
                new Vector2(cardBounds.X + 12, cardBounds.Y + 62),
                cardBounds.Width - 24,
                UiTheme.TextMuted,
                0.68f,
                2f,
                3);

            UiLabel.Draw(
                spriteBatch,
                _font,
                purchased ? "Installed" : $"Cost: ${definition.FundsCost:0}",
                new Vector2(cardBounds.X + 12, cardBounds.Bottom - 34),
                purchased ? UiTheme.Success : UiTheme.TextPrimary,
                0.68f);
            _upgradeButtons[definition.Type].Draw(spriteBatch, _pixel, _font);
        }

        _closeUpgradesButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawFirstCoinOverlay(SpriteBatch spriteBatch)
    {
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        var modalBounds = new Rectangle(452, 198, 696, 364);
        UiPanel.Draw(spriteBatch, _pixel, modalBounds, UiTheme.PanelFill, UiTheme.CoinAccent, 3);
        spriteBatch.Draw(_pixel, new Rectangle(modalBounds.X + 1, modalBounds.Y + 1, modalBounds.Width - 2, 4), UiTheme.CoinAccent);

        UiLabel.Draw(spriteBatch, _font, "Rent Is Due", new Vector2(modalBounds.X + 28, modalBounds.Y + 28), UiTheme.Warning, 1.2f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "The account slipped below zero after bills. In the corner of the desk, the framed first coin still glints back at you.",
            new Vector2(modalBounds.X + 28, modalBounds.Y + 78),
            modalBounds.Width - 56,
            UiTheme.TextPrimary,
            0.82f,
            3f,
            3);

        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"Current deficit: ${_state.FirstCoinRescueDeficit:0}. Breaking the frame grants +${_simulation.Config.FirstCoinEmergencyFundsGain:0} and permanently removes the passive sanity regen buff.",
            new Vector2(modalBounds.X + 28, modalBounds.Y + 154),
            modalBounds.Width - 56,
            UiTheme.TextMuted,
            0.76f,
            3f,
            3);

        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Keep the coin framed and the run ends here. Break it, and the week gets colder from this point forward.",
            new Vector2(modalBounds.X + 28, modalBounds.Y + 232),
            modalBounds.Width - 56,
            UiTheme.TextMuted,
            0.76f,
            3f,
            3);

        _breakCoinButton.Draw(spriteBatch, _pixel, _font);
        _acceptEvictionButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawOutcomeOverlay(SpriteBatch spriteBatch)
    {
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        var modalBounds = new Rectangle(450, 214, 700, 356);
        UiPanel.Draw(spriteBatch, _pixel, modalBounds, UiTheme.PanelFill, UiTheme.PanelBorder, 3);
        spriteBatch.Draw(_pixel, new Rectangle(modalBounds.X + 1, modalBounds.Y + 1, modalBounds.Width - 2, 4), UiTheme.AccentDim);

        var (title, titleColor) = _state.Status switch
        {
            RunStatus.Won => ("Hired", UiTheme.Success),
            RunStatus.Evicted => ("Evicted", UiTheme.Warning),
            _ => ("Burned Out", UiTheme.Danger),
        };

        UiLabel.Draw(spriteBatch, _font, title, new Vector2(modalBounds.X + 30, modalBounds.Y + 28), titleColor, 1.3f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            _state.OutcomeMessage ?? "The run is over.",
            new Vector2(modalBounds.X + 30, modalBounds.Y + 86),
            modalBounds.Width - 60,
            UiTheme.TextPrimary,
            0.86f,
            2f,
            4);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"Final stats: ${_state.Funds:0}, {_state.LinesOfCode} LoC, quality {_state.CodeQuality:0}, sanity {_state.Sanity:0}.",
            new Vector2(modalBounds.X + 30, modalBounds.Y + 180),
            modalBounds.Width - 60,
            UiTheme.TextMuted,
            0.78f,
            2f,
            2);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Restart to begin another week with cleaner timing and a stronger portfolio route.",
            new Vector2(modalBounds.X + 30, modalBounds.Y + 216),
            modalBounds.Width - 60,
            UiTheme.TextMuted,
            0.78f,
            2f,
            2);

        _restartButton.Draw(spriteBatch, _pixel, _font);
    }

    private void UpdateButtons()
    {
        const int sidebarPadding = 16;
        const int gap = 12;
        var contentX = _sidebarBounds.X + sidebarPadding;
        var contentWidth = _sidebarBounds.Width - (sidebarPadding * 2);
        var halfWidth = (contentWidth - gap) / 2;

        _foodAppButton.Enabled = _state.Status == RunStatus.InProgress;
        _freelanceButton.Enabled = _state.Status == RunStatus.InProgress;
        _sleepButton.Enabled = _simulation.CanApplyAction(_state, PlayerAction.Sleep);
        _upgradesButton.Enabled = _state.Status == RunStatus.InProgress;
        _squashBugButton.Enabled = _simulation.CanApplyAction(_state, PlayerAction.SquashBug);
        _applyForJobButton.Enabled = _simulation.CanApplyAction(_state, PlayerAction.ApplyForJob);

        _foodAppButton.Bounds = new Rectangle(contentX, _sidebarBounds.Y + 430, halfWidth, 36);
        _freelanceButton.Bounds = new Rectangle(contentX + halfWidth + gap, _sidebarBounds.Y + 430, halfWidth, 36);
        _upgradesButton.Bounds = new Rectangle(contentX, _sidebarBounds.Y + 474, halfWidth, 36);
        _sleepButton.Bounds = new Rectangle(contentX + halfWidth + gap, _sidebarBounds.Y + 474, halfWidth, 36);

        _coinFrameBounds = new Rectangle(_editorViewportBounds.Right - 148, _editorViewportBounds.Y + 6, 128, 86);

        _alertsPanelBounds = new Rectangle(contentX, _sidebarBounds.Y + 522, contentWidth, _sidebarBounds.Bottom - (_sidebarBounds.Y + 522) - 16);

        _techDebtCardBounds = Rectangle.Empty;
        _jobListingCardBounds = Rectangle.Empty;
        _squashBugButton.Bounds = Rectangle.Empty;
        _applyForJobButton.Bounds = Rectangle.Empty;

        var innerX = _alertsPanelBounds.X + 10;
        var innerY = _alertsPanelBounds.Y + 36;
        var innerWidth = _alertsPanelBounds.Width - 20;
        var availableHeight = _alertsPanelBounds.Height - 46;

        if (_state.ActiveTechDebtBug is not null && _state.ActiveJobListing is not null)
        {
            var techHeight = Math.Min(76, Math.Max(66, availableHeight / 2));
            var jobHeight = Math.Max(68, availableHeight - techHeight - 8);
            _techDebtCardBounds = new Rectangle(innerX, innerY, innerWidth, techHeight);
            _jobListingCardBounds = new Rectangle(innerX, _techDebtCardBounds.Bottom + 8, innerWidth, jobHeight);
        }
        else if (_state.ActiveTechDebtBug is not null)
        {
            _techDebtCardBounds = new Rectangle(innerX, innerY, innerWidth, Math.Max(72, availableHeight));
        }
        else if (_state.ActiveJobListing is not null)
        {
            _jobListingCardBounds = new Rectangle(innerX, innerY, innerWidth, Math.Max(84, availableHeight));
        }

        if (_techDebtCardBounds != Rectangle.Empty)
        {
            _squashBugButton.Bounds = new Rectangle(_techDebtCardBounds.Right - 80, _techDebtCardBounds.Y + 8, 70, 24);
        }

        if (_jobListingCardBounds != Rectangle.Empty)
        {
            _applyForJobButton.Bounds = new Rectangle(_jobListingCardBounds.Right - 94, _jobListingCardBounds.Y + 8, 84, 24);
        }

        _restartButton.Bounds = new Rectangle(628, 494, 344, 54);
        _breakCoinButton.Bounds = new Rectangle(552, 480, 198, 44);
        _acceptEvictionButton.Bounds = new Rectangle(850, 480, 198, 44);

        _foodAppBounds = new Rectangle(_editorViewportBounds.X + 96, _editorViewportBounds.Y + 28, 710, 470);
        _burgerButton.Bounds = new Rectangle(_foodAppBounds.X + 34, _foodAppBounds.Y + 132, 132, 34);
        _burritoButton.Bounds = new Rectangle(_foodAppBounds.X + 182, _foodAppBounds.Y + 132, 132, 34);
        _pizzaButton.Bounds = new Rectangle(_foodAppBounds.X + 34, _foodAppBounds.Y + 180, 132, 34);
        _dumplingsButton.Bounds = new Rectangle(_foodAppBounds.X + 182, _foodAppBounds.Y + 180, 132, 34);
        _doubleCheckOrderButton.Bounds = new Rectangle(_foodAppBounds.X + 24, _foodAppBounds.Bottom - 92, _foodAppBounds.Width - 48, 34);
        _confirmFoodOrderButton.Bounds = new Rectangle(_foodAppBounds.X + 24, _foodAppBounds.Bottom - 46, 220, 34);
        _closeFoodAppButton.Bounds = new Rectangle(_foodAppBounds.Right - 88, _foodAppBounds.Y + 18, 64, 28);

        _freelanceBoardBounds = new Rectangle(_editorViewportBounds.X + 110, _editorViewportBounds.Y + 46, 684, 430);
        _closeFreelanceBoardButton.Bounds = new Rectangle(_freelanceBoardBounds.Right - 88, _freelanceBoardBounds.Y + 18, 64, 28);

        var freelanceCardX = _freelanceBoardBounds.X + 24;
        var freelanceCardY = _freelanceBoardBounds.Y + 116;
        var freelanceCardWidth = _freelanceBoardBounds.Width - 48;
        var freelanceCardHeight = 86;

        _freelanceGigCardBounds[FreelanceGigType.QuickBugfix] = new Rectangle(freelanceCardX, freelanceCardY, freelanceCardWidth, freelanceCardHeight);
        _freelanceGigCardBounds[FreelanceGigType.UIPolishPass] = new Rectangle(freelanceCardX, freelanceCardY + freelanceCardHeight + 14, freelanceCardWidth, freelanceCardHeight);
        _freelanceGigCardBounds[FreelanceGigType.PipelineRescue] = new Rectangle(freelanceCardX, freelanceCardY + ((freelanceCardHeight + 14) * 2), freelanceCardWidth, freelanceCardHeight);

        foreach (var type in Enum.GetValues<FreelanceGigType>())
        {
            var button = _freelanceGigButtons[type];
            button.Enabled = _simulation.CanTakeFreelanceGig(_state, type);
            var cardBounds = _freelanceGigCardBounds[type];
            button.Bounds = new Rectangle(cardBounds.Right - 116, cardBounds.Bottom - 38, 102, 28);
        }

        _upgradesBounds = new Rectangle(_editorViewportBounds.X + 100, _editorViewportBounds.Y + 36, 704, 456);
        _closeUpgradesButton.Bounds = new Rectangle(_upgradesBounds.Right - 88, _upgradesBounds.Y + 18, 64, 28);

        var upgradeCardWidth = (_upgradesBounds.Width - 72) / 2;
        var upgradeCardHeight = 140;
        var firstCardX = _upgradesBounds.X + 24;
        var secondCardX = firstCardX + upgradeCardWidth + 24;
        var firstRowY = _upgradesBounds.Y + 112;
        var secondRowY = firstRowY + upgradeCardHeight + 20;

        _upgradeCardBounds[EfficiencyUpgradeType.MechanicalKeyboard] = new Rectangle(firstCardX, firstRowY, upgradeCardWidth, upgradeCardHeight);
        _upgradeCardBounds[EfficiencyUpgradeType.SnippetLibrary] = new Rectangle(secondCardX, firstRowY, upgradeCardWidth, upgradeCardHeight);
        _upgradeCardBounds[EfficiencyUpgradeType.LintBot] = new Rectangle(firstCardX, secondRowY, upgradeCardWidth, upgradeCardHeight);
        _upgradeCardBounds[EfficiencyUpgradeType.PomodoroTimer] = new Rectangle(secondCardX, secondRowY, upgradeCardWidth, upgradeCardHeight);

        foreach (var definition in EfficiencyUpgradeCatalog.All)
        {
            var button = _upgradeButtons[definition.Type];
            button.Text = _state.PurchasedUpgrades.Contains(definition.Type)
                ? "Installed"
                : $"Buy ${definition.FundsCost:0}";
            button.Enabled = _simulation.CanPurchaseUpgrade(_state, definition.Type);

            var cardBounds = _upgradeCardBounds[definition.Type];
            button.Bounds = new Rectangle(cardBounds.Right - 118, cardBounds.Bottom - 38, 106, 28);
        }

        _burgerButton.IsSelected = _selectedFood == FoodChoice.Burger;
        _burritoButton.IsSelected = _selectedFood == FoodChoice.Burrito;
        _pizzaButton.IsSelected = _selectedFood == FoodChoice.Pizza;
        _dumplingsButton.IsSelected = _selectedFood == FoodChoice.Dumplings;
        _doubleCheckOrderButton.IsSelected = _doubleCheckOrder;
        _doubleCheckOrderButton.Text = _doubleCheckOrder ? "Double-Check Order: ON" : "Double-Check Order: OFF";
        _confirmFoodOrderButton.Enabled = _simulation.CanPlaceFoodOrder(_state, _selectedFood);
    }

    private void UpdateLayout()
    {
        const int margin = 16;
        const int gap = 14;
        const int sidebarWidth = 392;
        const int logHeight = 138;

        var contentHeight = _virtualResolution.Y - (margin * 3) - logHeight;
        var editorWidth = _virtualResolution.X - (margin * 2) - gap - sidebarWidth;

        _editorPanelBounds = new Rectangle(margin, margin, editorWidth, contentHeight);
        _editorViewportBounds = new Rectangle(_editorPanelBounds.X + 16, _editorPanelBounds.Y + 58, _editorPanelBounds.Width - 32, _editorPanelBounds.Height - 76);
        _sidebarBounds = new Rectangle(_editorPanelBounds.Right + gap, margin, sidebarWidth, contentHeight);
        _logBounds = new Rectangle(margin, _editorPanelBounds.Bottom + gap, _virtualResolution.X - (margin * 2), logHeight);
        _catOverlayBounds = new Rectangle(_editorViewportBounds.X + 144, _editorViewportBounds.Y + 124, _editorViewportBounds.Width - 288, 232);
    }

    private void DrawTooltip(SpriteBatch spriteBatch)
    {
        var tooltip = GetHoveredTooltip();
        if (tooltip is null)
        {
            return;
        }

        const float tooltipWidth = 320f;
        const float titleScale = 0.74f;
        const float bodyScale = 0.68f;
        const int padding = 12;

        var bodyHeight = UiTextBlock.MeasureWrappedHeight(_font, tooltip.Value.Body, tooltipWidth - (padding * 2), bodyScale, 2f, 5);
        var titleHeight = _font.LineSpacing * titleScale;
        var height = (int)MathF.Ceiling(padding + titleHeight + 8f + bodyHeight + padding);

        var x = _mousePosition.X + 18;
        var y = _mousePosition.Y + 22;
        if (x + tooltipWidth > _virtualResolution.X - 12)
        {
            x = (int)(MathF.Max(12, _mousePosition.X - tooltipWidth - 18));
        }

        if (y + height > _virtualResolution.Y - 12)
        {
            y = Math.Max(12, _mousePosition.Y - height - 18);
        }

        var bounds = new Rectangle(x, y, (int)tooltipWidth, height);
        UiPanel.Draw(spriteBatch, _pixel, bounds, UiTheme.PanelFill, UiTheme.Accent, 2);
        spriteBatch.Draw(_pixel, new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, 3), UiTheme.Accent);

        UiLabel.Draw(spriteBatch, _font, tooltip.Value.Title, new Vector2(bounds.X + padding, bounds.Y + padding), UiTheme.TextPrimary, titleScale);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            tooltip.Value.Body,
            new Vector2(bounds.X + padding, bounds.Y + padding + titleHeight + 6f),
            tooltipWidth - (padding * 2),
            UiTheme.TextMuted,
            bodyScale,
            2f,
            5);
    }

    private (string Title, string Body)? GetHoveredTooltip()
    {
        if (_foodAppOpen)
        {
            if (_burgerButton.IsHovered)
            {
                return BuildFoodTooltip(FoodChoice.Burger);
            }

            if (_burritoButton.IsHovered)
            {
                return BuildFoodTooltip(FoodChoice.Burrito);
            }

            if (_pizzaButton.IsHovered)
            {
                return BuildFoodTooltip(FoodChoice.Pizza);
            }

            if (_dumplingsButton.IsHovered)
            {
                return BuildFoodTooltip(FoodChoice.Dumplings);
            }

            if (_doubleCheckOrderButton.IsHovered)
            {
                return ("Double-Check Order", "Spend the extra attention to avoid the sluggish penalty from a messy delivery.");
            }

            if (_confirmFoodOrderButton.IsHovered)
            {
                var option = _simulation.GetFoodOption(_selectedFood);
                return (
                    "Place Order",
                    $"Spend ${option.FundsCost:0} for {option.Name}. Focus {FormatSigned(option.FocusGain)}, sanity {FormatSigned(option.SanityGain)}, sluggish risk {(_doubleCheckOrder ? "removed" : FormatRemainingTime(option.SluggishMinutesWhenUnchecked))}.");
            }

            return null;
        }

        if (_freelanceBoardOpen)
        {
            foreach (var type in Enum.GetValues<FreelanceGigType>())
            {
                if (_freelanceGigButtons[type].IsHovered)
                {
                    return BuildFreelanceTooltip(type);
                }
            }

            return null;
        }

        if (_upgradesOpen)
        {
            foreach (var definition in EfficiencyUpgradeCatalog.All)
            {
                if (_upgradeButtons[definition.Type].IsHovered)
                {
                    return (
                        definition.Name,
                        $"{definition.Description} Cost: ${definition.FundsCost:0}. Effect: {definition.SummaryEffect}");
                }
            }

            return null;
        }

        if (_foodAppButton.IsHovered)
        {
            return ("Delivery App", "Browse meals with different focus, sanity, and sluggishness tradeoffs.");
        }

        if (_freelanceButton.IsHovered)
        {
            return ("Freelance Board", "Choose between short bugfixes, medium polish work, or a brutal pipeline rescue for bigger cash.");
        }

        if (_upgradesButton.IsHovered)
        {
            return ("Upgrades", "Spend saved funds on permanent typing throughput, quality, or focus-efficiency improvements.");
        }

        if (_coinFrameBounds.Contains(_mousePosition))
        {
            return _state.HasFirstCoin
                ? ("The First Coin", $"A keepsake from the first game sale. Passive sanity regen: +{_simulation.Config.FirstCoinPassiveSanityRegenPerInGameMinute * 60:0.0} per in-game hour. It can still buy one last rent rescue.")
                : ("Empty Frame", "The first coin is gone. The desk kept the frame, but not the hope it carried.");
        }

        if (_sleepButton.IsHovered)
        {
            return ("Sleep", $"Advance {FormatRemainingTime(_simulation.Config.SleepDurationMinutes)}. Focus {FormatSigned(_simulation.Config.SleepFocusGain)}, sanity {FormatSigned(_simulation.Config.SleepSanityGain)}.");
        }

        if (_squashBugButton.IsHovered && _state.ActiveTechDebtBug is not null)
        {
            return ("Fix Bug", $"Spend {_simulation.Config.SquashBugFocusCost:0} focus to stop the drain and recover +4 code quality.");
        }

        if (_applyForJobButton.IsHovered && _state.ActiveJobListing is not null)
        {
            var listing = _state.ActiveJobListing;
            return (
                listing.Title,
                $"Resume cost: {listing.ResumeCostLines} LoC. Requirements: {listing.MinimumPortfolioLines} LoC and {listing.MinimumCodeQuality:0} quality before the deadline.");
        }

        return null;
    }

    private (string Title, string Body) BuildFoodTooltip(FoodChoice choice)
    {
        var option = _simulation.GetFoodOption(choice);
        return (
            option.Name,
            $"{option.Description} Cost ${option.FundsCost:0}. Focus {FormatSigned(option.FocusGain)}, sanity {FormatSigned(option.SanityGain)}. Unchecked order risk: {FormatRemainingTime(option.SluggishMinutesWhenUnchecked)} sluggish.");
    }

    private (string Title, string Body) BuildFreelanceTooltip(FreelanceGigType type)
    {
        var gig = _simulation.GetFreelanceGig(type);
        return (
            gig.Name,
            $"{gig.Description} Duration {FormatRemainingTime(gig.DurationMinutes)}. Funds +${gig.FundsGain:0}, focus -{gig.FocusCost:0}, sanity -{gig.SanityCost:0}, quality {FormatSigned(gig.CodeQualityGain)}.");
    }

    private void PlayFileCompletionAudio()
    {
        if (string.IsNullOrEmpty(_state.RecentCompletedFileName))
        {
            _lastCelebratedFileName = null;
            return;
        }

        if (string.Equals(_lastCelebratedFileName, _state.RecentCompletedFileName, StringComparison.Ordinal))
        {
            return;
        }

        _lastCelebratedFileName = _state.RecentCompletedFileName;
        _audio.PlaySuccess();
    }

    private void PlayOutcomeAudio(RunStatus previousStatus, RunStatus currentStatus)
    {
        if (previousStatus == currentStatus || currentStatus == RunStatus.InProgress)
        {
            return;
        }

        if (currentStatus == RunStatus.Won)
        {
            _audio.PlaySuccess();
        }
        else
        {
            _audio.PlayFailure();
        }
    }

    private string GetEditorGuidance(PortfolioProgramDefinition program)
    {
        var programComplete = _state.CurrentProgramIndex >= PortfolioWorkspace.ProgramCount - 1 &&
                              _state.CurrentProgramVisibleLineCount >= program.CodeLines.Count;

        if (_state.ActiveCatInterruption is not null)
        {
            return $"Cat on the keyboard. Click the editor {_state.ActiveCatInterruption.PatsRemaining} more times to pet it away.";
        }

        if (programComplete)
        {
            return "All current portfolio files are typed out. Hunt listings, fix debt, or ship the next move.";
        }

        if (_state.CurrentProgramVisibleLineCount == 0)
        {
            return $"Blank editor ready. Click to start typing real C# into {program.FileName}.";
        }

        if (_simulation.IsSluggish(_state))
        {
            return $"Food sluggishness active for {FormatRemainingTime(_state.SluggishMinutesRemaining)}. Typing now costs {_simulation.GetCurrentWriteFocusCost(_state):0.0} focus.";
        }

        if (!_simulation.CanApplyAction(_state, PlayerAction.WriteCode))
        {
            return "Focus is empty. Order food or sleep before coding again.";
        }

        return $"Each click adds {_simulation.GetCurrentWriteLinesPerClick(_state)} LoC. Upgrades can improve throughput and code quality.";
    }

    private static Color GetCodeLineColor(string line)
    {
        var trimmed = line.TrimStart();
        if (trimmed.StartsWith("using ", StringComparison.Ordinal) ||
            trimmed.StartsWith("namespace ", StringComparison.Ordinal))
        {
            return UiTheme.Accent;
        }

        if (trimmed.StartsWith("public ", StringComparison.Ordinal) ||
            trimmed.StartsWith("private ", StringComparison.Ordinal))
        {
            return new Color(255, 214, 125);
        }

        if (trimmed.StartsWith("return ", StringComparison.Ordinal) ||
            trimmed.StartsWith("if ", StringComparison.Ordinal) ||
            trimmed.StartsWith("if (", StringComparison.Ordinal) ||
            trimmed.StartsWith("foreach ", StringComparison.Ordinal) ||
            trimmed.StartsWith("while ", StringComparison.Ordinal))
        {
            return new Color(114, 206, 255);
        }

        if (trimmed == "{" || trimmed == "}")
        {
            return UiTheme.TextMuted;
        }

        return UiTheme.TextPrimary;
    }

    private static string FormatRemainingTime(double totalMinutes)
    {
        var minutes = Math.Max(0, (int)Math.Ceiling(totalMinutes));
        var hours = minutes / 60;
        var remainingMinutes = minutes % 60;

        return hours > 0
            ? $"{hours}h {remainingMinutes:00}m"
            : $"{remainingMinutes}m";
    }

    private static string FormatSigned(double value)
    {
        return value >= 0
            ? $"+{value:0.#}"
            : $"{value:0.#}";
    }
}
