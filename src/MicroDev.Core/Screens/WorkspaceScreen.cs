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
    private readonly Action<WorkspaceScreen> _showOptions;
    private readonly UiButton _foodAppButton = new("Delivery App");
    private readonly UiButton _freelanceButton = new("Freelance Board");
    private readonly UiButton _sleepButton = new("Sleep");
    private readonly UiButton _upgradesButton = new("Upgrades");
    private readonly UiButton _bankAppButton = new("Banking App");
    private readonly UiButton _optionsButton = new("Options");
    private readonly UiButton _squashBugButton = new("Fix");
    private readonly UiButton _applyForJobButton = new("Apply");
    private readonly UiButton _publishAppButton = new("Publish");
    private readonly UiButton _restartButton = new("Restart Run");
    private readonly UiButton _breakCoinButton = new("Break Frame");
    private readonly UiButton _acceptEvictionButton = new("Let Go");
    private readonly UiButton _burgerButton = new("Burger");
    private readonly UiButton _burritoButton = new("Burrito");
    private readonly UiButton _pizzaButton = new("Pizza");
    private readonly UiButton _dumplingsButton = new("Dumplings");
    private readonly UiButton _doubleCheckOrderButton = new("Review Receipt: OFF");
    private readonly UiButton _expediteOrderButton = new("Expedite: OFF");
    private readonly UiButton _confirmFoodOrderButton = new("Place Order");
    private readonly UiButton _closeFoodAppButton = new("Close");
    private readonly UiButton _closeBankAppButton = new("Close");
    private readonly UiButton _closeFreelanceBoardButton = new("Close");
    private readonly UiButton _closeUpgradesButton = new("Close");
    private readonly UiButton _openApplicationButton = new("Continue");
    private readonly UiButton _closeApplicationButton = new("Close");
    private readonly UiButton[] _interviewOptionButtons =
    [
        new UiButton(string.Empty),
        new UiButton(string.Empty),
        new UiButton(string.Empty),
    ];
    private readonly UiButton[] _lifeEventOptionButtons =
    [
        new UiButton(string.Empty),
        new UiButton(string.Empty),
        new UiButton(string.Empty),
    ];
    private readonly UiButton[] _foodModifierButtons =
    [
        new UiButton(string.Empty),
        new UiButton(string.Empty),
        new UiButton(string.Empty),
    ];
    private readonly Dictionary<EfficiencyUpgradeType, UiButton> _upgradeButtons = [];
    private readonly Dictionary<FreelanceGigType, UiButton> _freelanceGigButtons = [];
    private readonly HashSet<FoodOrderModifier> _selectedFoodModifiers = [];

    private RunState _state;
    private Rectangle _editorPanelBounds;
    private Rectangle _editorViewportBounds;
    private Rectangle _sidebarBounds;
    private Rectangle _logBounds;
    private Rectangle _catOverlayBounds;
    private Rectangle _alertsPanelBounds;
    private Rectangle _techDebtCardBounds;
    private Rectangle _jobListingCardBounds;
    private Rectangle _applicationCardBounds;
    private Rectangle _publishCardBounds;
    private Rectangle _coinFrameBounds;
    private Rectangle _foodAppBounds;
    private Rectangle _foodChoicePanelBounds;
    private Rectangle _foodModifierPanelBounds;
    private Rectangle _foodSummaryPanelBounds;
    private Rectangle _lifeEventBounds;
    private Rectangle _bankAppBounds;
    private Rectangle _freelanceBoardBounds;
    private Rectangle _upgradesBounds;
    private Rectangle _applicationBounds;
    private Rectangle _applicationEditorBounds;
    private readonly Dictionary<EfficiencyUpgradeType, Rectangle> _upgradeCardBounds = [];
    private readonly Dictionary<FreelanceGigType, Rectangle> _freelanceGigCardBounds = [];
    private bool _foodAppOpen;
    private bool _bankAppOpen;
    private bool _freelanceBoardOpen;
    private bool _upgradesOpen;
    private bool _jobApplicationOpen;
    private FoodChoice _selectedFood = FoodChoice.Burger;
    private bool _doubleCheckOrder;
    private bool _expediteFoodDelivery;
    private string? _lastCelebratedFileName;
    private Point _mousePosition;

    public WorkspaceScreen(
        SpriteFont font,
        Texture2D pixel,
        SimulationEngine simulation,
        IncidentScheduler incidentScheduler,
        GameAudio audio,
        Point virtualResolution,
        Action<WorkspaceScreen> showOptions)
    {
        _font = font;
        _pixel = pixel;
        _simulation = simulation;
        _incidentScheduler = incidentScheduler;
        _audio = audio;
        _virtualResolution = virtualResolution;
        _showOptions = showOptions;
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

            var queued = _incidentScheduler.Update(_state, elapsedInGameMinutes, _simulation.Config);
            _simulation.QueueIncidents(_state, queued);
            if (queued.Count > 0)
            {
                _audio.PlayAlert();
            }
        }

        UpdateLayout();
        UpdateButtons();
        if (_state.ActiveJobApplication is null)
        {
            _jobApplicationOpen = false;
        }

        if (_state.Status != RunStatus.InProgress)
        {
            _foodAppOpen = false;
            _bankAppOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _jobApplicationOpen = false;
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
            _bankAppOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _jobApplicationOpen = false;
            HandleFirstCoinInput(input);
        }
        else if (_simulation.HasPendingLifeEvent(_state))
        {
            _foodAppOpen = false;
            _bankAppOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _jobApplicationOpen = false;
            HandleLifeEventInput(input);
        }
        else if (_jobApplicationOpen && _simulation.HasActiveJobApplication(_state))
        {
            HandleJobApplicationInput(input);
        }
        else if (_foodAppOpen)
        {
            HandleFoodAppInput(input);
        }
        else if (_bankAppOpen)
        {
            HandleBankAppInput(input);
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

        UpdateButtons();
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
        else if (_bankAppOpen && _state.Status == RunStatus.InProgress)
        {
            DrawBankAppOverlay(spriteBatch);
        }
        else if (_freelanceBoardOpen && _state.Status == RunStatus.InProgress)
        {
            DrawFreelanceBoardOverlay(spriteBatch);
        }
        else if (_upgradesOpen && _state.Status == RunStatus.InProgress)
        {
            DrawUpgradesOverlay(spriteBatch);
        }
        else if (_jobApplicationOpen && _state.Status == RunStatus.InProgress && _simulation.HasActiveJobApplication(_state))
        {
            DrawJobApplicationOverlay(spriteBatch);
        }

        if (_state.Status != RunStatus.InProgress)
        {
            DrawOutcomeOverlay(spriteBatch);
        }
        else if (_state.FirstCoinDecisionPending)
        {
            DrawFirstCoinOverlay(spriteBatch);
        }
        else if (_simulation.HasPendingLifeEvent(_state))
        {
            DrawLifeEventOverlay(spriteBatch);
        }
        else if (_jobApplicationOpen && _simulation.HasActiveJobApplication(_state))
        {
            return;
        }
        else
        {
            DrawTooltip(spriteBatch);
        }
    }

    public float GetSanityRatio()
    {
        return MathHelper.Clamp((float)(_state.Sanity / 100d), 0f, 1f);
    }

    private void ConfigureButtons()
    {
        _foodAppButton.TextScale = 0.8f;
        _freelanceButton.TextScale = 0.8f;
        _sleepButton.TextScale = 0.8f;
        _upgradesButton.TextScale = 0.8f;
        _bankAppButton.TextScale = 0.76f;
        _optionsButton.TextScale = 0.66f;
        _squashBugButton.TextScale = 0.64f;
        _applyForJobButton.TextScale = 0.64f;
        _publishAppButton.TextScale = 0.68f;
        _restartButton.TextScale = 0.92f;
        _breakCoinButton.TextScale = 0.82f;
        _acceptEvictionButton.TextScale = 0.8f;
        _burgerButton.TextScale = 0.82f;
        _burritoButton.TextScale = 0.82f;
        _pizzaButton.TextScale = 0.82f;
        _dumplingsButton.TextScale = 0.76f;
        _doubleCheckOrderButton.TextScale = 0.74f;
        _expediteOrderButton.TextScale = 0.74f;
        _confirmFoodOrderButton.TextScale = 0.84f;
        _closeFoodAppButton.TextScale = 0.72f;
        _closeBankAppButton.TextScale = 0.72f;
        _closeFreelanceBoardButton.TextScale = 0.72f;
        _closeUpgradesButton.TextScale = 0.72f;
        _openApplicationButton.TextScale = 0.64f;
        _closeApplicationButton.TextScale = 0.72f;
        foreach (var button in _interviewOptionButtons)
        {
            button.TextScale = 0.72f;
        }

        foreach (var button in _lifeEventOptionButtons)
        {
            button.TextScale = 0.68f;
        }

        foreach (var button in _foodModifierButtons)
        {
            button.TextScale = 0.68f;
        }

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
        if (_optionsButton.Update(input))
        {
            _audio.PlayButtonClick();
            _showOptions(this);
            return;
        }

        if (_openApplicationButton.Update(input))
        {
            _jobApplicationOpen = true;
            _foodAppOpen = false;
            _bankAppOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (TryApplyButtonAction(input, _publishAppButton, PlayerAction.PublishApp))
        {
            return;
        }

        if (_foodAppButton.Update(input))
        {
            _foodAppOpen = true;
            _bankAppOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_freelanceButton.Update(input))
        {
            _freelanceBoardOpen = true;
            _foodAppOpen = false;
            _bankAppOpen = false;
            _upgradesOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_bankAppButton.Update(input))
        {
            _bankAppOpen = true;
            _foodAppOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_upgradesButton.Update(input))
        {
            _upgradesOpen = true;
            _foodAppOpen = false;
            _bankAppOpen = false;
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

        if (_applyForJobButton.Update(input))
        {
            var applicationStarted = _simulation.ApplyAction(_state, PlayerAction.ApplyForJob);
            if (applicationStarted)
            {
                _jobApplicationOpen = true;
                _audio.PlayButtonClick();
            }
            else
            {
                _audio.PlayFailure();
            }

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

        if (_state.ActiveFoodDelivery is not null)
        {
            return;
        }

        if (_burgerButton.Update(input))
        {
            SetSelectedFood(FoodChoice.Burger);
            return;
        }

        if (_burritoButton.Update(input))
        {
            SetSelectedFood(FoodChoice.Burrito);
            return;
        }

        if (_pizzaButton.Update(input))
        {
            SetSelectedFood(FoodChoice.Pizza);
            return;
        }

        if (_dumplingsButton.Update(input))
        {
            SetSelectedFood(FoodChoice.Dumplings);
            return;
        }

        var modifierOptions = _simulation.GetFoodOrderModifiers(_selectedFood);
        for (var index = 0; index < _foodModifierButtons.Length && index < modifierOptions.Count; index++)
        {
            if (!_foodModifierButtons[index].Update(input))
            {
                continue;
            }

            var modifier = modifierOptions[index].Modifier;
            if (!_selectedFoodModifiers.Add(modifier))
            {
                _selectedFoodModifiers.Remove(modifier);
            }

            _audio.PlayButtonClick();
            return;
        }

        if (_doubleCheckOrderButton.Update(input))
        {
            _doubleCheckOrder = !_doubleCheckOrder;
            _audio.PlayButtonClick();
            return;
        }

        if (_expediteOrderButton.Update(input))
        {
            _expediteFoodDelivery = !_expediteFoodDelivery;
            _audio.PlayButtonClick();
            return;
        }

        if (_confirmFoodOrderButton.Update(input))
        {
            if (_simulation.PlaceFoodOrder(_state, _selectedFood, _selectedFoodModifiers, _doubleCheckOrder, _expediteFoodDelivery))
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

    private void HandleBankAppInput(InputSnapshot input)
    {
        if (_closeBankAppButton.Update(input))
        {
            _bankAppOpen = false;
            _audio.PlayButtonClick();
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

    private void HandleLifeEventInput(InputSnapshot input)
    {
        for (var index = 0; index < _lifeEventOptionButtons.Length; index++)
        {
            if (!_lifeEventOptionButtons[index].Update(input))
            {
                continue;
            }

            if (_simulation.ResolveLifeEventOption(_state, index))
            {
                _audio.PlayButtonClick();
            }
            else
            {
                _audio.PlayFailure();
            }

            return;
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

    private void HandleJobApplicationInput(InputSnapshot input)
    {
        var application = _state.ActiveJobApplication;
        if (application is null)
        {
            return;
        }

        if (_closeApplicationButton.Update(input))
        {
            _jobApplicationOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (!application.TakeHomeComplete)
        {
            if (!input.IsLeftClickInside(_applicationEditorBounds))
            {
                return;
            }

            if (_state.ActiveCatInterruption is not null)
            {
                var petted = _simulation.ApplyAction(_state, PlayerAction.PetCat);
                if (petted)
                {
                    _audio.PlayButtonClick();
                }
                else
                {
                    _audio.PlayFailure();
                }

                return;
            }

            var worked = _simulation.WorkOnJobApplication(_state);
            if (worked)
            {
                _audio.PlayWriteKey();
            }
            else
            {
                _audio.PlayFailure();
            }

            return;
        }

        if (application.CurrentQuestionIndex >= application.Questions.Count)
        {
            return;
        }

        for (var index = 0; index < _interviewOptionButtons.Length; index++)
        {
            if (!_interviewOptionButtons[index].Update(input))
            {
                continue;
            }

            var answered = _simulation.AnswerInterviewQuestion(_state, index);
            if (answered)
            {
                _audio.PlayButtonClick();
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
        var displayedLines = BuildDisplayedCodeLines(visibleLines);
        var totalPrograms = PortfolioWorkspace.GetProgramCount(_state);
        var finitePortfolio = PortfolioWorkspace.HasFiniteProgramCount(_state);
        var fileIndex = Math.Max(1, _state.CurrentProgramIndex + 1);
        var releaseNumber = _state.PublishedAppCount + 1;
        var headerRight = _coinFrameBounds.Left - 16;
        var contentX = _editorViewportBounds.X + 18;
        var contentWidth = Math.Max(360, headerRight - contentX);

        var tabBounds = new Rectangle(_editorViewportBounds.X, _editorViewportBounds.Y - 38, Math.Min(468, _editorViewportBounds.Width - 236), 34);
        UiPanel.Draw(spriteBatch, _pixel, tabBounds, UiTheme.PanelRaised, UiTheme.EditorBorder, 2);
        UiLabel.Draw(
            spriteBatch,
            _font,
            UiTextBlock.TrimToWidth(_font, program.FileName, tabBounds.Width - 28, 0.8f),
            new Vector2(tabBounds.X + 14, tabBounds.Y + 7),
            UiTheme.TextPrimary,
            0.8f);

        var titleY = _editorViewportBounds.Y + 12;
        var projectMeta = finitePortfolio
            ? $"Release {releaseNumber}  |  File {Math.Min(fileIndex, totalPrograms)}/{totalPrograms}"
            : $"File {fileIndex}/ENDLESS";
        var projectMetaSize = _font.MeasureString(projectMeta) * SmallScale;
        var projectTitleMaxWidth = Math.Max(220, headerRight - contentX - 18 - projectMetaSize.X);

        UiLabel.Draw(
            spriteBatch,
            _font,
            UiTextBlock.TrimToWidth(_font, program.ProjectName, projectTitleMaxWidth, 0.92f),
            new Vector2(contentX, titleY),
            UiTheme.Accent,
            0.92f);

        UiLabel.Draw(
            spriteBatch,
            _font,
            projectMeta,
            new Vector2(headerRight - projectMetaSize.X, titleY + 3),
            UiTheme.TextMuted,
            SmallScale);

        DrawFirstCoinFrame(spriteBatch);

        var descriptionY = titleY + 30;
        var descriptionHeight = UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            program.Description,
            new Vector2(contentX, descriptionY),
            contentWidth,
            UiTheme.TextMuted,
            BodyScale,
            2f,
            2);

        var completedPrograms = PortfolioWorkspace.GetCompletedProgramCount(_state);
        var progressY = descriptionY + descriptionHeight + 14f;
        UiLabel.Draw(
            spriteBatch,
            _font,
            finitePortfolio
                ? $"Portfolio {completedPrograms}/{totalPrograms} files complete"
                : $"Portfolio {completedPrograms} files complete. Endless queue still flowing.",
            new Vector2(contentX, progressY),
            UiTheme.TextMuted,
            0.66f);

        var progressStripX = contentX;
        var progressStripY = progressY + 24f;
        var progressSlots = finitePortfolio ? totalPrograms : 8;
        var completedInWindow = finitePortfolio
            ? completedPrograms
            : completedPrograms == 0
                ? 0
                : ((completedPrograms - 1) % progressSlots) + 1;
        var currentSlot = finitePortfolio
            ? Math.Min(_state.CurrentProgramIndex, progressSlots - 1)
            : _state.CurrentProgramIndex % progressSlots;
        for (var index = 0; index < progressSlots; index++)
        {
            var fill = index < completedPrograms
                ? UiTheme.Success
                : index == currentSlot
                    ? UiTheme.Accent
                    : UiTheme.PanelRaised;
            if (!finitePortfolio)
            {
                fill = index < completedInWindow
                    ? UiTheme.Success
                    : index == currentSlot
                        ? UiTheme.Accent
                        : UiTheme.PanelRaised;
            }
            var border = index < completedPrograms ? UiTheme.Success : UiTheme.PanelBorder;
            if (!finitePortfolio)
            {
                border = index < completedInWindow ? UiTheme.Success : UiTheme.PanelBorder;
            }
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

        DrawCodeViewport(spriteBatch, displayedLines, codeTop, guidanceY - 18f, contentX, contentWidth);

        if (_state.ActiveCatInterruption is not null)
        {
            UiPanel.Draw(spriteBatch, _pixel, _catOverlayBounds, new Color(80, 45, 24, 232), UiTheme.CatAccent, 3);
            UiLabel.Draw(spriteBatch, _font, "Desk Cat Event", new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 18), UiTheme.CatAccent, 1.0f);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                "The cat decided your keyboard is warmer than any bed. Pet it away before it times out, trashes part of the draft, and keeps typing nonsense into the editor.",
                new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 56),
                _catOverlayBounds.Width - 48,
                UiTheme.TextPrimary,
                0.76f,
                2f,
                3);
            UiLabel.Draw(spriteBatch, _font, $"Pet clicks remaining: {_state.ActiveCatInterruption.PatsRemaining}", new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 136), UiTheme.TextPrimary, 0.78f);
            UiLabel.Draw(spriteBatch, _font, $"Leaves in: {FormatRemainingTime(_state.ActiveCatInterruption.RemainingInGameMinutes)}", new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 164), UiTheme.TextMuted, 0.76f);
            UiLabel.Draw(spriteBatch, _font, $"Deletion risk: {_state.ActiveCatInterruption.LinesDeletionPenalty} LoC", new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 192), UiTheme.Warning, 0.76f);
            UiLabel.Draw(spriteBatch, _font, $"Phantom bugs typed: {_state.ActiveCatInterruption.PhantomBugCount}", new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 220), UiTheme.Danger, 0.72f);
            UiLabel.Draw(spriteBatch, _font, $"Gibberish lines on screen: {_state.ActiveCatInterruption.GibberishLinesTyped}", new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 246), UiTheme.CatAccent, 0.72f);
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
        _optionsButton.Draw(spriteBatch, _pixel, _font);

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
        DrawStatCard(spriteBatch, new Rectangle(rightX, secondRowY, cardWidth, cardHeight), "Shipped", _state.PublishedAppCount.ToString(), UiTheme.Success);
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
        var startY = _sidebarBounds.Y + 192;
        const int spacing = 40;

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
        var stripBounds = new Rectangle(_sidebarBounds.X + 16, _sidebarBounds.Y + 292, _sidebarBounds.Width - 32, 50);
        var fill = UiTheme.PanelRaised;
        var border = UiTheme.PanelBorder;
        var message = $"Typing {_simulation.GetCurrentWriteLinesPerClick(_state)} line/click at {_simulation.GetCurrentWriteFocusCost(_state):0.0} focus. Upgrades buy speed back.";
        var textColor = UiTheme.TextMuted;
        var sleepStage = _simulation.GetSleepStage(_state);
        var hungerStage = _simulation.GetHungerStage(_state);

        if (_state.ActiveCatInterruption is not null)
        {
            fill = new Color(82, 56, 28);
            border = UiTheme.CatAccent;
            textColor = UiTheme.CatAccent;
            message = $"Cat blocking the keyboard for {FormatRemainingTime(_state.ActiveCatInterruption.RemainingInGameMinutes)}. {_state.ActiveCatInterruption.PhantomBugCount} bug bursts and {_state.ActiveCatInterruption.GibberishLinesTyped} gibberish lines are on screen.";
        }
        else if (_state.ActiveFoodDelivery is not null)
        {
            fill = new Color(36, 46, 67);
            border = UiTheme.Accent;
            textColor = UiTheme.Accent;
            message = $"{_simulation.GetFoodOption(_state.ActiveFoodDelivery.Choice).Name} on the way. ETA {FormatRemainingTime(_state.ActiveFoodDelivery.RemainingInGameMinutes)}{(_state.ActiveFoodDelivery.Expedited ? " after the extra tip." : ".")}";
        }
        else if (_simulation.RequiresSleep(_state))
        {
            fill = new Color(82, 34, 34);
            border = UiTheme.Danger;
            textColor = UiTheme.Danger;
            message = $"Awake for {FormatRemainingTime(_state.MinutesSinceLastSleep)}. You have to sleep before coding, contracts, or interviews.";
        }
        else if (sleepStage >= 2)
        {
            fill = new Color(74, 40, 40);
            border = UiTheme.Warning;
            textColor = UiTheme.Warning;
            message = $"Awake for {FormatRemainingTime(_state.MinutesSinceLastSleep)}. Fatigue is draining sanity and code quality until you sleep.";
        }
        else if (hungerStage >= 1)
        {
            fill = new Color(84, 64, 26);
            border = UiTheme.Warning;
            textColor = UiTheme.Warning;
            message = hungerStage >= 3
                ? $"No meal for {FormatRemainingTime(_state.MinutesSinceLastMeal)}. Hunger is tearing through sanity until you eat."
                : $"No meal for {FormatRemainingTime(_state.MinutesSinceLastMeal)}. Hunger is shaving sanity off the day.";
        }
        else if (_simulation.IsSluggish(_state))
        {
            fill = new Color(84, 64, 26);
            border = UiTheme.Warning;
            textColor = UiTheme.Warning;
            message = $"Sluggish for {FormatRemainingTime(_state.SluggishMinutesRemaining)}. A messy meal choice is costing output.";
        }
        else if (_simulation.IsContextSwitchActive(_state))
        {
            fill = new Color(66, 44, 58);
            border = UiTheme.Accent;
            textColor = UiTheme.Accent;
            message = $"Context switching is active for {FormatRemainingTime(_state.ContextSwitchMinutesRemaining)}. Focus costs are temporarily higher.";
        }
        else if (_simulation.IsDeepWorkActive(_state))
        {
            fill = new Color(27, 55, 46);
            border = UiTheme.Success;
            textColor = UiTheme.Success;
            message = $"Deep work window for {FormatRemainingTime(_state.DeepWorkMinutesRemaining)}. Typing and quality are both temporarily stronger.";
        }
        else if (_simulation.IsPortfolioPublishReady(_state))
        {
            fill = new Color(28, 60, 49);
            border = UiTheme.Success;
            textColor = UiTheme.Success;
            message = $"Release ready to ship. Publish for ${_simulation.Config.PublishAppFundsMin:0}-${_simulation.Config.PublishAppFundsMax:0} and roll the next snippet batch.";
        }
        else if (_simulation.HasPublishedApps(_state) &&
                 _state.NextPublishedAppSaleDeskMinute < double.PositiveInfinity)
        {
            fill = new Color(36, 46, 67);
            border = UiTheme.Accent;
            textColor = UiTheme.Accent;
            var remaining = Math.Max(0, _state.NextPublishedAppSaleDeskMinute - _state.DeskMinutesElapsed);
            message = $"{_state.PublishedAppCount} shipped app{(_state.PublishedAppCount == 1 ? string.Empty : "s")} live. Next storefront payout in {FormatRemainingTime(remaining)}.";
        }
        else if (_state.HasFoundLove && !string.IsNullOrWhiteSpace(_state.PartnerName))
        {
            fill = new Color(27, 55, 46);
            border = UiTheme.Success;
            textColor = UiTheme.Success;
            message = $"{_state.PartnerName} is in your corner. Passive sanity support is buying back {(_simulation.Config.FoundLovePassiveSanityRegenPerInGameMinute * 60):0.0} sanity per in-game hour.";
        }
        else if (_state.ActiveJobListing is not null)
        {
            var listing = _state.ActiveJobListing;
            fill = new Color(29, 49, 42);
            border = UiTheme.Success;
            textColor = UiTheme.Success;
            message =
                $"{_simulation.GetResumeTrackLabel(listing.ResumeTrack)} proof {_simulation.GetResumeProof(_state, listing.ResumeTrack)}/{listing.RequiredResumeProof} is the next interview gate.";
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
        UiLabel.Draw(spriteBatch, _font, "Actions", new Vector2(_sidebarBounds.X + 16, _sidebarBounds.Y + 352), UiTheme.TextPrimary, 0.88f);
        _foodAppButton.Draw(spriteBatch, _pixel, _font);
        _freelanceButton.Draw(spriteBatch, _pixel, _font);
        _bankAppButton.Draw(spriteBatch, _pixel, _font);
        _upgradesButton.Draw(spriteBatch, _pixel, _font);
        _sleepButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawAlertsPanel(SpriteBatch spriteBatch)
    {
        UiPanel.Draw(spriteBatch, _pixel, _alertsPanelBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, "Alerts & Inbox", new Vector2(_alertsPanelBounds.X + 12, _alertsPanelBounds.Y + 10), UiTheme.TextPrimary, 0.82f);

        if (_state.ActiveTechDebtBug is null &&
            _state.ActiveJobListing is null &&
            _state.ActiveJobApplication is null &&
            !_simulation.IsPortfolioPublishReady(_state) &&
            !_simulation.HasPublishedApps(_state))
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

        if (_techDebtCardBounds != Rectangle.Empty && _state.ActiveTechDebtBug is not null)
        {
            DrawTechDebtCard(spriteBatch);
        }

        if (_jobListingCardBounds != Rectangle.Empty && _state.ActiveJobListing is not null)
        {
            DrawJobListingCard(spriteBatch);
        }

        if (_applicationCardBounds != Rectangle.Empty && _state.ActiveJobApplication is not null)
        {
            DrawJobApplicationCard(spriteBatch);
        }

        if (_publishCardBounds != Rectangle.Empty &&
            (_simulation.IsPortfolioPublishReady(_state) || _simulation.HasPublishedApps(_state)))
        {
            DrawPublishCard(spriteBatch);
        }
    }

    private void DrawTechDebtCard(SpriteBatch spriteBatch)
    {
        var bug = _state.ActiveTechDebtBug!;
        UiPanel.Draw(spriteBatch, _pixel, _techDebtCardBounds, new Color(56, 34, 34), UiTheme.Danger, 2);
        _squashBugButton.Draw(spriteBatch, _pixel, _font);
        var titleWidth = _techDebtCardBounds.Width - 20 - _squashBugButton.Bounds.Width - 8;
        UiLabel.Draw(
            spriteBatch,
            _font,
            UiTextBlock.TrimToWidth(_font, "Tech Debt", titleWidth, 0.72f),
            new Vector2(_techDebtCardBounds.X + 10, _techDebtCardBounds.Y + 8),
            UiTheme.Danger,
            0.72f);

        var summary = $"{bug.Summary} | {FormatRemainingTime(bug.RemainingInGameMinutes)} left";
        var bodyTop = GetAlertCardBodyTop(_techDebtCardBounds);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            summary,
            new Vector2(_techDebtCardBounds.X + 10, bodyTop),
            GetAlertCardBodyWidth(_techDebtCardBounds, _squashBugButton.Bounds, bodyTop),
            UiTheme.TextPrimary,
            0.62f,
            1f,
            GetAlertCardBodyMaxLines(_techDebtCardBounds, bodyTop, 0.62f, 1f));
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
        var requirements =
            $"{listing.TechStack}  |  {_simulation.GetResumeTrackLabel(listing.ResumeTrack)} proof {_simulation.GetResumeProof(_state, listing.ResumeTrack)}/{listing.RequiredResumeProof}  |  {FormatRemainingTime(listing.RemainingInGameMinutes)} left";
        var bodyTop = GetAlertCardBodyTop(_jobListingCardBounds);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            requirements,
            new Vector2(_jobListingCardBounds.X + 10, bodyTop),
            GetAlertCardBodyWidth(_jobListingCardBounds, _applyForJobButton.Bounds, bodyTop),
            UiTheme.TextMuted,
            0.62f,
            1f,
            GetAlertCardBodyMaxLines(_jobListingCardBounds, bodyTop, 0.62f, 1f));
    }

    private void DrawJobApplicationCard(SpriteBatch spriteBatch)
    {
        var application = _state.ActiveJobApplication!;
        UiPanel.Draw(spriteBatch, _pixel, _applicationCardBounds, new Color(31, 48, 58), UiTheme.Accent, 2);
        _openApplicationButton.Draw(spriteBatch, _pixel, _font);

        var titleWidth = _applicationCardBounds.Width - 20 - _openApplicationButton.Bounds.Width - 8;
        UiLabel.Draw(
            spriteBatch,
            _font,
            UiTextBlock.TrimToWidth(_font, application.ListingTitle, titleWidth, 0.72f),
            new Vector2(_applicationCardBounds.X + 10, _applicationCardBounds.Y + 8),
            UiTheme.Accent,
            0.72f);

        var revealedLines = _simulation.GetVisibleJobApplicationLines(_state)
            .Count(line => !string.IsNullOrWhiteSpace(line));
        var totalLines = application.CodeLines.Count(line => !string.IsNullOrWhiteSpace(line));
        var body = application.TakeHomeComplete
            ? $"Interview {Math.Min(application.CurrentQuestionIndex + 1, application.Questions.Count)}/{application.Questions.Count}. Correct {application.CorrectAnswers}/{application.MinimumCorrectAnswers} needed."
            : $"Take-home {revealedLines}/{totalLines} lines. Prep notes {application.PrepPoints}. Need {application.MinimumCorrectAnswers}/{application.Questions.Count} correct in the interview.";
        var bodyTop = GetAlertCardBodyTop(_applicationCardBounds);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            body,
            new Vector2(_applicationCardBounds.X + 10, bodyTop),
            GetAlertCardBodyWidth(_applicationCardBounds, _openApplicationButton.Bounds, bodyTop),
            UiTheme.TextMuted,
            0.62f,
            1f,
            GetAlertCardBodyMaxLines(_applicationCardBounds, bodyTop, 0.62f, 1f));
    }

    private void DrawPublishCard(SpriteBatch spriteBatch)
    {
        var publishReady = _simulation.IsPortfolioPublishReady(_state);
        var fill = publishReady
            ? new Color(28, 60, 49)
            : new Color(35, 45, 64);
        var border = publishReady
            ? UiTheme.Success
            : UiTheme.Accent;
        var accent = publishReady
            ? UiTheme.Success
            : UiTheme.Accent;

        UiPanel.Draw(spriteBatch, _pixel, _publishCardBounds, fill, border, 2);

        var title = publishReady
            ? $"Release {_state.PublishedAppCount + 1} Ready"
            : "Published Apps";
        var titleWidth = _publishCardBounds.Width - 20 - (_publishAppButton.Bounds == Rectangle.Empty ? 0 : _publishAppButton.Bounds.Width + 8);
        UiLabel.Draw(
            spriteBatch,
            _font,
            UiTextBlock.TrimToWidth(_font, title, titleWidth, 0.72f),
            new Vector2(_publishCardBounds.X + 10, _publishCardBounds.Y + 8),
            accent,
            0.72f);

        if (_publishAppButton.Bounds != Rectangle.Empty)
        {
            _publishAppButton.Draw(spriteBatch, _pixel, _font);
        }

        var nextSaleText = _state.NextPublishedAppSaleDeskMinute < double.PositiveInfinity
            ? $"Next payout in {FormatRemainingTime(Math.Max(0, _state.NextPublishedAppSaleDeskMinute - _state.DeskMinutesElapsed))}."
            : "Storefront payouts will start rolling in once this build is live.";
        var body = publishReady
            ? $"All {PortfolioWorkspace.GetProgramCount(_state)} files for the current release are complete. Publish now for a randomized ${_simulation.Config.PublishAppFundsMin:0}-${_simulation.Config.PublishAppFundsMax:0}, then roll straight into the next snippet pack. {nextSaleText}"
            : $"{_state.PublishedAppCount} shipped app{(_state.PublishedAppCount == 1 ? string.Empty : "s")} are live. {nextSaleText}";
        var bodyTop = GetAlertCardBodyTop(_publishCardBounds);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            body,
            new Vector2(_publishCardBounds.X + 10, bodyTop),
            GetAlertCardBodyWidth(_publishCardBounds, _publishAppButton.Bounds, bodyTop),
            UiTheme.TextMuted,
            0.62f,
            1f,
            GetAlertCardBodyMaxLines(_publishCardBounds, bodyTop, 0.62f, 1f));
    }

    private float GetAlertCardBodyTop(Rectangle cardBounds)
    {
        return cardBounds.Height >= 64
            ? cardBounds.Y + 34f
            : cardBounds.Y + 28f;
    }

    private static float GetAlertCardBodyWidth(Rectangle cardBounds, Rectangle actionBounds, float bodyTop)
    {
        var width = cardBounds.Width - 20f;
        if (actionBounds != Rectangle.Empty && bodyTop < actionBounds.Bottom + 4f)
        {
            width -= actionBounds.Width + 8f;
        }

        return Math.Max(120f, width);
    }

    private int GetAlertCardBodyMaxLines(Rectangle cardBounds, float bodyTop, float scale, float lineGap)
    {
        var availableHeight = cardBounds.Bottom - 8f - bodyTop;
        if (availableHeight <= 0f)
        {
            return 1;
        }

        var lineHeight = (_font.LineSpacing * scale) + lineGap;
        return Math.Max(1, (int)Math.Floor((availableHeight + lineGap) / lineHeight));
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

        var activeDelivery = _state.ActiveFoodDelivery;
        var option = _simulation.GetFoodOption(activeDelivery?.Choice ?? _selectedFood);
        var orderOptions = _simulation.GetFoodOrderModifiers(activeDelivery?.Choice ?? _selectedFood);
        IReadOnlyCollection<FoodOrderModifier> selectedModifiers = activeDelivery is not null
            ? activeDelivery.SelectedModifiers
            : _selectedFoodModifiers;
        var reviewReceipt = activeDelivery?.ReviewReceipt ?? _doubleCheckOrder;
        var expedited = activeDelivery?.Expedited ?? _expediteFoodDelivery;
        var penaltyMinutes = _simulation.GetFoodOrderPenaltyMinutes(option.Choice, selectedModifiers, reviewReceipt);
        var introText = activeDelivery is null
            ? "Pick a meal to nearly refill focus and stop hunger from chewing at sanity. The food only helps once it arrives, so decide whether the expedite tip is worth the shorter ETA."
            : "Your order is already on the road. Keep an eye on the ETA here or from the desk strip while the delivery timer runs down.";
        var orderStateText = activeDelivery is null
            ? penaltyMinutes <= 0
                ? "Receipt reviewed. This order should land clean with no sluggish penalty."
                : penaltyMinutes < option.SluggishMinutesWhenUnchecked
                    ? $"Partial cleanup. Expected sluggishness falls to {FormatRemainingTime(penaltyMinutes)} after delivery."
                    : $"Messy order. Expect the full {FormatRemainingTime(option.SluggishMinutesWhenUnchecked)} sluggish hit when it arrives."
            : $"{option.Name} is {(activeDelivery.Expedited ? "expedited" : "on standard delivery")} with {FormatRemainingTime(activeDelivery.RemainingInGameMinutes)} left before the stats land.";

        UiLabel.Draw(spriteBatch, _font, "Food Delivery App", new Vector2(_foodAppBounds.X + 24, _foodAppBounds.Y + 18), UiTheme.TextPrimary, 1.0f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            introText,
            new Vector2(_foodAppBounds.X + 24, _foodAppBounds.Y + 52),
            _foodAppBounds.Width - 48,
            UiTheme.TextMuted,
            0.76f,
            2f,
            3);

        UiPanel.Draw(spriteBatch, _pixel, _foodChoicePanelBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _foodModifierPanelBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, "Meals", new Vector2(_foodChoicePanelBounds.X + 14, _foodChoicePanelBounds.Y + 14), UiTheme.TextPrimary, 0.8f);
        UiLabel.Draw(
            spriteBatch,
            _font,
            activeDelivery is null ? "Delivery ETA, cash tradeoff." : "Order locked until this driver arrives.",
            new Vector2(_foodChoicePanelBounds.X + 14, _foodChoicePanelBounds.Y + 38),
            UiTheme.TextMuted,
            0.62f);
        _burgerButton.Draw(spriteBatch, _pixel, _font);
        _burritoButton.Draw(spriteBatch, _pixel, _font);
        _pizzaButton.Draw(spriteBatch, _pixel, _font);
        _dumplingsButton.Draw(spriteBatch, _pixel, _font);

        UiPanel.Draw(spriteBatch, _pixel, _foodSummaryPanelBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        _doubleCheckOrderButton.Draw(spriteBatch, _pixel, _font);
        _expediteOrderButton.Draw(spriteBatch, _pixel, _font);
        _confirmFoodOrderButton.Draw(spriteBatch, _pixel, _font);
        _closeFoodAppButton.Draw(spriteBatch, _pixel, _font);

        UiLabel.Draw(spriteBatch, _font, $"Selected meal: {option.Name}", new Vector2(_foodSummaryPanelBounds.X + 16, _foodSummaryPanelBounds.Y + 14), UiTheme.TextPrimary, 0.82f);
        UiLabel.Draw(
            spriteBatch,
            _font,
            activeDelivery is null
                ? $"Cost: ${_simulation.GetFoodTotalCost(option.Choice, expedited):0}   ETA: {FormatRemainingTime(_simulation.GetFoodDeliveryDuration(expedited))}   Focus on arrival: +{option.FocusGain:0}"
                : $"Paid: ${activeDelivery.TotalFundsCost:0}   ETA: {FormatRemainingTime(activeDelivery.RemainingInGameMinutes)}   Delivery: {(activeDelivery.Expedited ? "Expedited" : "Standard")}",
            new Vector2(_foodSummaryPanelBounds.X + 16, _foodSummaryPanelBounds.Y + 42),
            UiTheme.Warning,
            0.74f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            activeDelivery is null
                ? option.Description
                : $"{option.Description} The driver will apply the focus and sanity bump only when the food actually arrives.",
            new Vector2(_foodSummaryPanelBounds.X + 16, _foodSummaryPanelBounds.Y + 72),
            _foodSummaryPanelBounds.Width - 32,
            UiTheme.TextMuted,
            0.68f,
            2f,
            2);
        UiLabel.Draw(
            spriteBatch,
            _font,
            activeDelivery is null
                ? "Meals clear hunger on delivery. Sleep is slower, but it is the only full reset."
                : $"Expected on arrival: sanity {option.SanityGain:+0;-0;0}, hunger reset, and {(penaltyMinutes <= 0 ? "no sluggishness" : $"{FormatRemainingTime(penaltyMinutes)} sluggishness")}.",
            new Vector2(_foodSummaryPanelBounds.X + 16, _foodSummaryPanelBounds.Y + 108),
            UiTheme.TextMuted,
            0.62f);
        UiLabel.Draw(spriteBatch, _font, "Order Details", new Vector2(_foodModifierPanelBounds.X + 14, _foodModifierPanelBounds.Y + 14), UiTheme.TextPrimary, 0.8f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            GetFoodModifierIntroText(activeDelivery),
            new Vector2(_foodModifierPanelBounds.X + 14, _foodModifierPanelBounds.Y + 38),
            _foodModifierPanelBounds.Width - 28,
            UiTheme.TextMuted,
            0.64f,
            2f,
            3);

        for (var index = 0; index < orderOptions.Count && index < _foodModifierButtons.Length; index++)
        {
            _foodModifierButtons[index].Draw(spriteBatch, _pixel, _font);
            UiLabel.Draw(
                spriteBatch,
                _font,
                activeDelivery is null
                    ? orderOptions[index].Recommended ? "Recommended" : "Optional"
                    : selectedModifiers.Contains(orderOptions[index].Modifier) ? "Locked In" : "Skipped",
                new Vector2(_foodModifierButtons[index].Bounds.Right + 10, _foodModifierButtons[index].Bounds.Y + 8),
                activeDelivery is null
                    ? orderOptions[index].Recommended ? UiTheme.Success : UiTheme.TextMuted
                    : selectedModifiers.Contains(orderOptions[index].Modifier) ? UiTheme.Success : UiTheme.TextMuted,
                0.58f);
        }

        var noteBounds = new Rectangle(_foodSummaryPanelBounds.X + 14, _foodSummaryPanelBounds.Bottom - 46, _foodSummaryPanelBounds.Width - 28, 34);
        UiPanel.Draw(
            spriteBatch,
            _pixel,
            noteBounds,
            activeDelivery is null && penaltyMinutes <= 0 ? new Color(29, 58, 42) : new Color(70, 52, 23),
            activeDelivery is null && penaltyMinutes <= 0 ? UiTheme.Success : UiTheme.Warning,
            1);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            orderStateText,
            new Vector2(noteBounds.X + 10, noteBounds.Y + 6),
            noteBounds.Width - 20,
            activeDelivery is null && penaltyMinutes <= 0 ? UiTheme.Success : UiTheme.Warning,
            0.62f,
            1f,
            2);
    }

    private void DrawBankAppOverlay(SpriteBatch spriteBatch)
    {
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        UiPanel.Draw(spriteBatch, _pixel, _bankAppBounds, UiTheme.PanelFill, UiTheme.Accent, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_bankAppBounds.X + 1, _bankAppBounds.Y + 1, _bankAppBounds.Width - 2, 4), UiTheme.Accent);

        UiLabel.Draw(spriteBatch, _font, "Banking App", new Vector2(_bankAppBounds.X + 24, _bankAppBounds.Y + 18), UiTheme.TextPrimary, 1.0f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Track runway, rent pressure, and the emergency value of the first coin without digging through the feed.",
            new Vector2(_bankAppBounds.X + 24, _bankAppBounds.Y + 52),
            _bankAppBounds.Width - 48,
            UiTheme.TextMuted,
            0.76f,
            2f,
            3);

        var cardWidth = (_bankAppBounds.Width - 72) / 2;
        var accountBounds = new Rectangle(_bankAppBounds.X + 24, _bankAppBounds.Y + 118, cardWidth, 128);
        var rentBounds = new Rectangle(accountBounds.Right + 24, _bankAppBounds.Y + 118, cardWidth, 128);
        var coinBounds = new Rectangle(_bankAppBounds.X + 24, _bankAppBounds.Y + 260, _bankAppBounds.Width - 48, 84);
        var ledgerBounds = new Rectangle(_bankAppBounds.X + 24, _bankAppBounds.Y + 360, _bankAppBounds.Width - 48, _bankAppBounds.Height - 384);

        UiPanel.Draw(spriteBatch, _pixel, accountBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, rentBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(
            spriteBatch,
            _pixel,
            coinBounds,
            _state.HasFirstCoin ? new Color(48, 44, 28) : UiTheme.PanelMuted,
            _state.HasFirstCoin ? UiTheme.CoinAccent : UiTheme.PanelBorder,
            2);
        UiPanel.Draw(spriteBatch, _pixel, ledgerBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        var runwayBills = _simulation.Config.DailyBillAmount <= 0
            ? 0
            : Math.Max(0, (int)Math.Floor(_state.Funds / _simulation.Config.DailyBillAmount));
        var billMinutesRemaining = Math.Max(1d, SimulationConfig.MinutesPerDay - _state.TimeOfDayMinutes);

        UiLabel.Draw(spriteBatch, _font, "Available Cash", new Vector2(accountBounds.X + 16, accountBounds.Y + 14), UiTheme.TextMuted, 0.72f);
        UiLabel.Draw(spriteBatch, _font, $"${_state.Funds:0}", new Vector2(accountBounds.X + 16, accountBounds.Y + 40), UiTheme.Warning, 1.16f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"Roughly {runwayBills} rent hit{(runwayBills == 1 ? string.Empty : "s")} of runway at the current bill rate.",
            new Vector2(accountBounds.X + 16, accountBounds.Y + 78),
            accountBounds.Width - 32,
            UiTheme.TextMuted,
            0.68f,
            2f,
            2);

        UiLabel.Draw(spriteBatch, _font, "Rent Pressure", new Vector2(rentBounds.X + 16, rentBounds.Y + 14), UiTheme.TextMuted, 0.72f);
        UiLabel.Draw(
            spriteBatch,
            _font,
            $"Next bill in {FormatRemainingTime(billMinutesRemaining)}",
            new Vector2(rentBounds.X + 16, rentBounds.Y + 40),
            UiTheme.TextPrimary,
            0.88f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"Daily bill: -${_simulation.Config.DailyBillAmount:0}. Difficulty: {_state.Difficulty}.",
            new Vector2(rentBounds.X + 16, rentBounds.Y + 78),
            rentBounds.Width - 32,
            UiTheme.TextMuted,
            0.68f,
            2f,
            2);

        UiLabel.Draw(spriteBatch, _font, "Emergency Buffer", new Vector2(coinBounds.X + 16, coinBounds.Y + 14), _state.HasFirstCoin ? UiTheme.CoinAccent : UiTheme.TextMuted, 0.78f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            _state.HasFirstCoin
                ? $"The framed first coin is still intact. It steadies sanity and can still rescue one missed rent cycle for +${_simulation.Config.FirstCoinEmergencyFundsGain:0}."
                : "The first coin rescue has already been spent. There is no second emergency buffer on this desk.",
            new Vector2(coinBounds.X + 16, coinBounds.Y + 40),
            coinBounds.Width - 32,
            _state.HasFirstCoin ? UiTheme.TextPrimary : UiTheme.TextMuted,
            0.7f,
            2f,
            3);

        UiLabel.Draw(spriteBatch, _font, "Recent Transactions", new Vector2(ledgerBounds.X + 16, ledgerBounds.Y + 14), UiTheme.TextPrimary, 0.82f);
        var transactionY = ledgerBounds.Y + 42f;
        var transactionWidth = ledgerBounds.Width - 32f;
        foreach (var entry in GetRecentMoneyEntries())
        {
            var height = UiTextBlock.MeasureWrappedHeight(_font, entry, transactionWidth, 0.7f, 2f, 2);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                entry,
                new Vector2(ledgerBounds.X + 16, transactionY),
                transactionWidth,
                UiTheme.TextMuted,
                0.7f,
                2f,
                2);
            transactionY += height + 8f;
            if (transactionY > ledgerBounds.Bottom - 26)
            {
                break;
            }
        }

        _closeBankAppButton.Draw(spriteBatch, _pixel, _font);
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
            var contentWidth = cardBounds.Width - 168;
            var railBounds = new Rectangle(cardBounds.Right - 132, cardBounds.Y + 12, 118, cardBounds.Height - 24);
            UiPanel.Draw(spriteBatch, _pixel, cardBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
            UiPanel.Draw(spriteBatch, _pixel, railBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 1);
            UiLabel.Draw(spriteBatch, _font, gig.Name, new Vector2(cardBounds.X + 14, cardBounds.Y + 14), UiTheme.TextPrimary, 0.8f);
            UiLabel.Draw(
                spriteBatch,
                _font,
                UiTextBlock.TrimToWidth(
                    _font,
                    $"{FormatRemainingTime(gig.DurationMinutes)}  |  +${gig.FundsGain:0}  |  -{gig.FocusCost:0} focus  |  -{gig.SanityCost:0} sanity",
                    contentWidth,
                    0.66f),
                new Vector2(cardBounds.X + 14, cardBounds.Y + 42),
                UiTheme.Warning,
                0.66f);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                gig.Description,
                new Vector2(cardBounds.X + 14, cardBounds.Y + 68),
                contentWidth,
                UiTheme.TextMuted,
                0.68f,
                2f,
                3);

            var qualityLabel = gig.CodeQualityGain > 0
                ? $"+{gig.CodeQualityGain:0.#} quality"
                : "No portfolio gain";

            UiLabel.Draw(
                spriteBatch,
                _font,
                UiTextBlock.TrimToWidth(_font, qualityLabel, railBounds.Width - 20, 0.66f),
                new Vector2(railBounds.X + 10, railBounds.Y + 10),
                gig.CodeQualityGain > 0 ? UiTheme.Success : UiTheme.TextMuted,
                0.66f);

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
            var railBounds = new Rectangle(cardBounds.Right - 132, cardBounds.Y + 12, 118, cardBounds.Height - 24);
            var contentLeft = cardBounds.X + 12;
            var contentWidth = railBounds.X - contentLeft - 12;
            var summaryTop = cardBounds.Y + 38;
            var summaryHeight = UiTextBlock.MeasureWrappedHeight(_font, definition.SummaryEffect, contentWidth, 0.68f, 2f, 2);
            var descriptionTop = summaryTop + summaryHeight + 6f;
            var descriptionLineHeight = (_font.LineSpacing * 0.64f) + 2f;
            var availableDescriptionHeight = cardBounds.Bottom - 12 - descriptionTop;
            var descriptionMaxLines = Math.Max(1, (int)Math.Floor((availableDescriptionHeight + 2f) / descriptionLineHeight));
            var railBodyTop = railBounds.Y + 36;
            var railLineHeight = (_font.LineSpacing * 0.62f) + 1f;
            var availableRailHeight = _upgradeButtons[definition.Type].Bounds.Y - 8 - railBodyTop;
            var railMaxLines = Math.Max(1, (int)Math.Floor((availableRailHeight + 1f) / railLineHeight));

            UiPanel.Draw(spriteBatch, _pixel, cardBounds, fill, border, 2);
            UiPanel.Draw(spriteBatch, _pixel, railBounds, UiTheme.PanelMuted, purchased ? UiTheme.Success : UiTheme.PanelBorder, 1);
            UiLabel.Draw(
                spriteBatch,
                _font,
                UiTextBlock.TrimToWidth(_font, definition.Name, contentWidth, 0.76f),
                new Vector2(contentLeft, cardBounds.Y + 12),
                purchased ? UiTheme.Success : UiTheme.TextPrimary,
                0.76f);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                definition.SummaryEffect,
                new Vector2(contentLeft, summaryTop),
                contentWidth,
                UiTheme.Warning,
                0.68f,
                2f,
                2);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                definition.Description,
                new Vector2(contentLeft, descriptionTop),
                contentWidth,
                UiTheme.TextMuted,
                0.64f,
                2f,
                descriptionMaxLines);

            UiLabel.Draw(
                spriteBatch,
                _font,
                purchased ? "Installed" : $"Cost: ${definition.FundsCost:0}",
                new Vector2(railBounds.X + 10, railBounds.Y + 10),
                purchased ? UiTheme.Success : UiTheme.TextPrimary,
                0.68f);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                purchased ? "Permanent throughput boost already wired into the rig." : "Permanent rig upgrade. Funds are spent immediately.",
                new Vector2(railBounds.X + 10, railBodyTop),
                railBounds.Width - 20,
                UiTheme.TextMuted,
                0.62f,
                1f,
                railMaxLines);
            _upgradeButtons[definition.Type].Draw(spriteBatch, _pixel, _font);
        }

        _closeUpgradesButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawJobApplicationOverlay(SpriteBatch spriteBatch)
    {
        var application = _state.ActiveJobApplication!;
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        UiPanel.Draw(spriteBatch, _pixel, _applicationBounds, UiTheme.PanelFill, UiTheme.Success, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_applicationBounds.X + 1, _applicationBounds.Y + 1, _applicationBounds.Width - 2, 4), UiTheme.Success);

        UiLabel.Draw(spriteBatch, _font, application.ListingTitle, new Vector2(_applicationBounds.X + 24, _applicationBounds.Y + 18), UiTheme.TextPrimary, 1.0f);
        UiLabel.Draw(spriteBatch, _font, application.TechStack, new Vector2(_applicationBounds.X + 24, _applicationBounds.Y + 48), UiTheme.Accent, 0.72f);
        _closeApplicationButton.Draw(spriteBatch, _pixel, _font);

        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Application flow: finish the take-home, then work through the interview prompts. You can close this window and return to the rest of the desk whenever you need.",
            new Vector2(_applicationBounds.X + 24, _applicationBounds.Y + 76),
            _applicationBounds.Width - 48,
            UiTheme.TextMuted,
            0.74f,
            2f,
            3);

        var requirementsText =
            $"Qualified on submit with {application.PortfolioLinesSnapshot} LoC, quality {application.CodeQualitySnapshot:0}, and {_simulation.GetResumeTrackLabel(application.ResumeTrack)} proof {application.ResumeProofSnapshot}. Prep notes: {application.PrepPoints}. The recruiter wants {application.MinimumCorrectAnswers} correct answer{(application.MinimumCorrectAnswers == 1 ? string.Empty : "s")} this round.";
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            requirementsText,
            new Vector2(_applicationBounds.X + 24, _applicationBounds.Y + 132),
            _applicationBounds.Width - 48,
            UiTheme.Warning,
            0.68f,
            2f,
            3);

        if (!application.TakeHomeComplete)
        {
            UiPanel.Draw(spriteBatch, _pixel, _applicationEditorBounds, UiTheme.EditorFill, UiTheme.EditorBorder, 2);
            UiLabel.Draw(spriteBatch, _font, application.ChallengeTitle, new Vector2(_applicationEditorBounds.X + 16, _applicationEditorBounds.Y + 12), UiTheme.Success, 0.82f);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                application.ChallengeDescription,
                new Vector2(_applicationEditorBounds.X + 16, _applicationEditorBounds.Y + 40),
                _applicationEditorBounds.Width - 32,
                UiTheme.TextMuted,
                0.68f,
                2f,
                2);

            var visibleLines = BuildDisplayedCodeLines(_simulation.GetVisibleJobApplicationLines(_state));
            var lineHeight = (_font.LineSpacing * CodeScale) + 3f;
            var lineY = _applicationEditorBounds.Y + 92f;
            var codeWidth = _applicationEditorBounds.Width - 64;
            for (var index = 0; index < visibleLines.Count && lineY < _applicationEditorBounds.Bottom - 44; index++)
            {
                var lineNumber = $"{index + 1,2}";
                UiLabel.Draw(spriteBatch, _font, lineNumber, new Vector2(_applicationEditorBounds.X + 14, lineY), UiTheme.TextMuted, CodeScale);
                spriteBatch.DrawString(
                    _font,
                    UiTextBlock.TrimToWidth(_font, visibleLines[index], codeWidth, CodeScale),
                    new Vector2(_applicationEditorBounds.X + 54, lineY),
                    GetCodeLineColor(visibleLines[index]),
                    0f,
                    Vector2.Zero,
                    CodeScale,
                    SpriteEffects.None,
                    0f);
                lineY += lineHeight;
            }

            var footer = _state.ActiveCatInterruption is not null
                ? $"Cat on the take-home keyboard. Click the editor {_state.ActiveCatInterruption.PatsRemaining} more times to clear it while {_state.ActiveCatInterruption.GibberishLinesTyped} gibberish lines stay smeared across the solution."
                : "Click inside the take-home editor to reveal the solution one real line at a time, or close this panel and come back after you recover.";
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                footer,
                new Vector2(_applicationEditorBounds.X + 16, _applicationEditorBounds.Bottom - 28),
                _applicationEditorBounds.Width - 32,
                _state.ActiveCatInterruption is not null ? UiTheme.CatAccent : UiTheme.TextMuted,
                0.66f,
                2f,
                2);
            return;
        }

        if (application.CurrentQuestionIndex >= application.Questions.Count)
        {
            UiLabel.Draw(
                spriteBatch,
                _font,
                "Interview answers are resolving. The recruiter feed is closing out this loop.",
                new Vector2(_applicationEditorBounds.X + 16, _applicationEditorBounds.Y + 24),
                UiTheme.TextMuted,
                0.78f);
            return;
        }

        var question = application.Questions[application.CurrentQuestionIndex];
        var interviewBounds = _applicationEditorBounds;
        UiPanel.Draw(spriteBatch, _pixel, interviewBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, "Mock Coding Interview", new Vector2(interviewBounds.X + 16, interviewBounds.Y + 16), UiTheme.Success, 0.88f);
        UiLabel.Draw(
            spriteBatch,
            _font,
            $"Question {application.CurrentQuestionIndex + 1}/{application.Questions.Count}  |  Correct {application.CorrectAnswers}/{application.MinimumCorrectAnswers} needed",
            new Vector2(interviewBounds.X + 16, interviewBounds.Y + 44),
            UiTheme.TextMuted,
            0.68f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Answer order is shuffled each round. Close returns to the desk without canceling the interview.",
            new Vector2(interviewBounds.X + 16, interviewBounds.Y + 66),
            interviewBounds.Width - 32,
            UiTheme.TextMuted,
            0.66f,
            2f,
            2);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            question.Prompt,
            new Vector2(interviewBounds.X + 16, interviewBounds.Y + 106),
            interviewBounds.Width - 32,
            UiTheme.TextPrimary,
            0.8f,
            3f,
            3);

        foreach (var button in _interviewOptionButtons)
        {
            button.Draw(spriteBatch, _pixel, _font);
        }
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

    private void DrawLifeEventOverlay(SpriteBatch spriteBatch)
    {
        var lifeEvent = _state.PendingLifeEvent!;
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        var borderColor = lifeEvent.Type switch
        {
            IncidentType.ComputerFreeze => UiTheme.Warning,
            IncidentType.OnlineMatch => UiTheme.Success,
            _ => UiTheme.Accent,
        };

        UiPanel.Draw(spriteBatch, _pixel, _lifeEventBounds, UiTheme.PanelFill, borderColor, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_lifeEventBounds.X + 1, _lifeEventBounds.Y + 1, _lifeEventBounds.Width - 2, 4), borderColor);

        UiLabel.Draw(spriteBatch, _font, lifeEvent.Title, new Vector2(_lifeEventBounds.X + 28, _lifeEventBounds.Y + 26), UiTheme.TextPrimary, 1.1f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            lifeEvent.Description,
            new Vector2(_lifeEventBounds.X + 28, _lifeEventBounds.Y + 72),
            _lifeEventBounds.Width - 56,
            UiTheme.TextPrimary,
            0.8f,
            3f,
            3);

        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            GetLifeEventDecisionText(lifeEvent),
            new Vector2(_lifeEventBounds.X + 28, _lifeEventBounds.Y + 150),
            _lifeEventBounds.Width - 56,
            UiTheme.TextMuted,
            0.72f,
            2f,
            5);

        if (lifeEvent.Type == IncidentType.OnlineMatch)
        {
            UiLabel.Draw(
                spriteBatch,
                _font,
                $"Relationship progress: {_state.RelationshipProgress}/{_simulation.Config.RelationshipProgressNeededForLove}",
                new Vector2(_lifeEventBounds.X + 28, _lifeEventBounds.Y + 264),
                UiTheme.Success,
                0.7f);
        }

        foreach (var button in _lifeEventOptionButtons)
        {
            if (button.Bounds != Rectangle.Empty)
            {
                button.Draw(spriteBatch, _pixel, _font);
            }
        }
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
        var activeDelivery = _state.ActiveFoodDelivery;
        var hasActiveFoodDelivery = activeDelivery is not null;
        var reviewReceiptEnabled = activeDelivery?.ReviewReceipt ?? _doubleCheckOrder;
        var expeditedDeliveryEnabled = activeDelivery?.Expedited ?? _expediteFoodDelivery;

        _foodAppButton.Enabled = _state.Status == RunStatus.InProgress;
        _foodAppButton.Text = hasActiveFoodDelivery ? "Track Order" : "Delivery App";
        _freelanceButton.Enabled = _state.Status == RunStatus.InProgress;
        _bankAppButton.Enabled = _state.Status == RunStatus.InProgress;
        _sleepButton.Enabled = _simulation.CanApplyAction(_state, PlayerAction.Sleep);
        _sleepButton.Text = _simulation.RequiresSleep(_state) ? "Sleep Now" : "Sleep";
        _sleepButton.IsSelected = _simulation.RequiresSleep(_state);
        _upgradesButton.Enabled = _state.Status == RunStatus.InProgress;
        _optionsButton.Enabled = true;
        _squashBugButton.Enabled = _simulation.CanApplyAction(_state, PlayerAction.SquashBug);
        _applyForJobButton.Enabled = _simulation.CanApplyAction(_state, PlayerAction.ApplyForJob);
        _applyForJobButton.Text = "Start";
        _publishAppButton.Enabled = _simulation.CanApplyAction(_state, PlayerAction.PublishApp);
        _publishAppButton.Text = "Publish";
        _openApplicationButton.Enabled = _state.ActiveJobApplication is not null;
        _openApplicationButton.Text = _state.ActiveJobApplication is not null && _state.ActiveJobApplication.TakeHomeComplete
            ? "Interview"
            : "Continue";

        _optionsButton.Bounds = new Rectangle(_sidebarBounds.Right - 92, _sidebarBounds.Y + 10, 76, 28);
        _foodAppButton.Bounds = new Rectangle(contentX, _sidebarBounds.Y + 378, halfWidth, 34);
        _freelanceButton.Bounds = new Rectangle(contentX + halfWidth + gap, _sidebarBounds.Y + 378, halfWidth, 34);
        _bankAppButton.Bounds = new Rectangle(contentX, _sidebarBounds.Y + 418, halfWidth, 34);
        _upgradesButton.Bounds = new Rectangle(contentX + halfWidth + gap, _sidebarBounds.Y + 418, halfWidth, 34);
        _sleepButton.Bounds = new Rectangle(contentX, _sidebarBounds.Y + 458, contentWidth, 34);

        _coinFrameBounds = new Rectangle(_editorViewportBounds.Right - 148, _editorViewportBounds.Y + 6, 128, 86);

        _alertsPanelBounds = new Rectangle(contentX, _sidebarBounds.Y + 504, contentWidth, _sidebarBounds.Bottom - (_sidebarBounds.Y + 504) - 16);

        _techDebtCardBounds = Rectangle.Empty;
        _jobListingCardBounds = Rectangle.Empty;
        _applicationCardBounds = Rectangle.Empty;
        _publishCardBounds = Rectangle.Empty;
        _squashBugButton.Bounds = Rectangle.Empty;
        _applyForJobButton.Bounds = Rectangle.Empty;
        _openApplicationButton.Bounds = Rectangle.Empty;
        _publishAppButton.Bounds = Rectangle.Empty;

        var innerX = _alertsPanelBounds.X + 10;
        var innerY = _alertsPanelBounds.Y + 34;
        var innerWidth = _alertsPanelBounds.Width - 20;
        var availableHeight = _alertsPanelBounds.Height - 40;

        var activeCards = new List<string>();
        if (_state.ActiveTechDebtBug is not null)
        {
            activeCards.Add("bug");
        }

        if (_state.ActiveJobListing is not null)
        {
            activeCards.Add("listing");
        }

        if (_state.ActiveJobApplication is not null)
        {
            activeCards.Add("application");
        }

        if (_simulation.IsPortfolioPublishReady(_state) || _simulation.HasPublishedApps(_state))
        {
            activeCards.Add("publish");
        }

        if (activeCards.Count > 0)
        {
            var cardGap = 6;
            var totalCardHeight = Math.Max(0, availableHeight - ((activeCards.Count - 1) * cardGap));
            var sharedHeight = totalCardHeight / activeCards.Count;
            var leftoverHeight = totalCardHeight % activeCards.Count;
            var cardY = innerY;
            for (var index = 0; index < activeCards.Count; index++)
            {
                var height = sharedHeight + (index < leftoverHeight ? 1 : 0);
                var bounds = new Rectangle(innerX, cardY, innerWidth, height);

                switch (activeCards[index])
                {
                    case "bug":
                        _techDebtCardBounds = bounds;
                        break;
                    case "listing":
                        _jobListingCardBounds = bounds;
                        break;
                    case "application":
                        _applicationCardBounds = bounds;
                        break;
                    case "publish":
                        _publishCardBounds = bounds;
                        break;
                }

                cardY = bounds.Bottom + cardGap;
            }
        }

        if (_techDebtCardBounds != Rectangle.Empty)
        {
            _squashBugButton.Bounds = new Rectangle(_techDebtCardBounds.Right - 80, _techDebtCardBounds.Y + 8, 70, 24);
        }

        if (_jobListingCardBounds != Rectangle.Empty)
        {
            _applyForJobButton.Bounds = new Rectangle(_jobListingCardBounds.Right - 94, _jobListingCardBounds.Y + 8, 84, 24);
        }

        if (_applicationCardBounds != Rectangle.Empty)
        {
            _openApplicationButton.Bounds = new Rectangle(_applicationCardBounds.Right - 104, _applicationCardBounds.Y + 8, 94, 24);
        }

        if (_publishCardBounds != Rectangle.Empty && _publishAppButton.Enabled)
        {
            _publishAppButton.Bounds = new Rectangle(_publishCardBounds.Right - 94, _publishCardBounds.Y + 8, 84, 24);
        }

        _restartButton.Bounds = new Rectangle(628, 494, 344, 54);
        _breakCoinButton.Bounds = new Rectangle(552, 480, 198, 44);
        _acceptEvictionButton.Bounds = new Rectangle(850, 480, 198, 44);
        _lifeEventBounds = new Rectangle(420, 184, 760, 392);

        var lifeButtonY = _lifeEventBounds.Bottom - 62;
        var lifeButtonWidth = (_lifeEventBounds.Width - 88) / 3;
        for (var index = 0; index < _lifeEventOptionButtons.Length; index++)
        {
            _lifeEventOptionButtons[index].Bounds = new Rectangle(
                _lifeEventBounds.X + 28 + (index * (lifeButtonWidth + 16)),
                lifeButtonY,
                lifeButtonWidth,
                38);
            _lifeEventOptionButtons[index].Enabled = false;
            _lifeEventOptionButtons[index].Text = string.Empty;
        }

        if (_state.PendingLifeEvent is not null)
        {
            var optionLabels = GetLifeEventOptionLabels(_state.PendingLifeEvent);
            for (var index = 0; index < _lifeEventOptionButtons.Length; index++)
            {
                if (index >= optionLabels.Length)
                {
                    _lifeEventOptionButtons[index].Bounds = Rectangle.Empty;
                    continue;
                }

                _lifeEventOptionButtons[index].Text = optionLabels[index];
                _lifeEventOptionButtons[index].Enabled = _simulation.CanResolveLifeEventOption(_state, index);
            }
        }

        var foodWidth = Math.Min(_editorViewportBounds.Width - 48, 820);
        var topPanelsGap = 20;
        var choiceWidth = 300;
        var foodContentWidth = foodWidth - 48;
        var modifierWidth = foodContentWidth - choiceWidth - topPanelsGap;
        var modifierOptions = _simulation.GetFoodOrderModifiers(activeDelivery?.Choice ?? _selectedFood);
        var modifierIntroHeight = UiTextBlock.MeasureWrappedHeight(
            _font,
            GetFoodModifierIntroText(activeDelivery),
            modifierWidth - 28,
            0.64f,
            2f,
            3);
        const int mealButtonHeight = 40;
        const int mealButtonGap = 12;
        const int modifierButtonHeight = 30;
        const int modifierRowGap = 40;
        const int baseTopPanelsHeight = 182;
        var requiredChoicePanelHeight = 52 + (mealButtonHeight * 2) + mealButtonGap + 18;
        var requiredModifierPanelHeight = 50 + (int)Math.Ceiling(modifierIntroHeight) + 10 + modifierButtonHeight + 18;
        if (modifierOptions.Count > 1)
        {
            requiredModifierPanelHeight += (modifierOptions.Count - 1) * modifierRowGap;
        }

        var topPanelsHeight = Math.Max(baseTopPanelsHeight, Math.Max(requiredChoicePanelHeight, requiredModifierPanelHeight));
        var foodHeight = Math.Min(_editorViewportBounds.Height - 20, 566 + (topPanelsHeight - baseTopPanelsHeight));
        var foodX = _editorViewportBounds.X + ((_editorViewportBounds.Width - foodWidth) / 2);
        var foodY = _editorViewportBounds.Y + 12;
        _foodAppBounds = new Rectangle(foodX, foodY, foodWidth, foodHeight);
        var foodContentX = _foodAppBounds.X + 24;
        var topPanelsY = _foodAppBounds.Y + 122;
        _foodChoicePanelBounds = new Rectangle(foodContentX, topPanelsY, choiceWidth, topPanelsHeight);
        _foodModifierPanelBounds = new Rectangle(_foodChoicePanelBounds.Right + topPanelsGap, topPanelsY, modifierWidth, topPanelsHeight);

        var mealButtonWidth = (_foodChoicePanelBounds.Width - 48) / 2;
        var mealButtonX = _foodChoicePanelBounds.X + 18;
        var mealButtonY = _foodChoicePanelBounds.Y + 52;
        _burgerButton.Bounds = new Rectangle(mealButtonX, mealButtonY, mealButtonWidth, mealButtonHeight);
        _burritoButton.Bounds = new Rectangle(mealButtonX + mealButtonWidth + mealButtonGap, mealButtonY, mealButtonWidth, mealButtonHeight);
        _pizzaButton.Bounds = new Rectangle(mealButtonX, mealButtonY + mealButtonHeight + mealButtonGap, mealButtonWidth, mealButtonHeight);
        _dumplingsButton.Bounds = new Rectangle(mealButtonX + mealButtonWidth + mealButtonGap, mealButtonY + mealButtonHeight + mealButtonGap, mealButtonWidth, mealButtonHeight);

        var foodActionsY = _foodAppBounds.Bottom - 52;
        _doubleCheckOrderButton.Bounds = new Rectangle(_foodAppBounds.X + 24, foodActionsY, 208, 34);
        _expediteOrderButton.Bounds = new Rectangle(_foodAppBounds.X + 246, foodActionsY, 208, 34);
        _confirmFoodOrderButton.Bounds = new Rectangle(_foodAppBounds.X + 468, foodActionsY, 188, 34);
        _closeFoodAppButton.Bounds = new Rectangle(_foodAppBounds.Right - 88, _foodAppBounds.Y + 18, 64, 28);
        _foodSummaryPanelBounds = new Rectangle(
            foodContentX,
            _foodChoicePanelBounds.Bottom + 18,
            foodContentWidth,
            foodActionsY - (_foodChoicePanelBounds.Bottom + 18) - 14);

        var modifierButtonX = _foodModifierPanelBounds.X + 18;
        var modifierButtonY = (int)Math.Ceiling(_foodModifierPanelBounds.Y + 50 + modifierIntroHeight + 10);
        var modifierButtonWidth = Math.Min(210, _foodModifierPanelBounds.Width - 126);
        for (var index = 0; index < _foodModifierButtons.Length; index++)
        {
            if (index >= modifierOptions.Count)
            {
                _foodModifierButtons[index].Bounds = Rectangle.Empty;
                _foodModifierButtons[index].Enabled = false;
                _foodModifierButtons[index].Text = string.Empty;
                _foodModifierButtons[index].IsSelected = false;
                continue;
            }

            var option = modifierOptions[index];
            _foodModifierButtons[index].Bounds = new Rectangle(modifierButtonX, modifierButtonY + (index * modifierRowGap), modifierButtonWidth, 30);
            _foodModifierButtons[index].Enabled = !hasActiveFoodDelivery;
            _foodModifierButtons[index].Text = option.Label;
            _foodModifierButtons[index].IsSelected = hasActiveFoodDelivery
                ? activeDelivery!.SelectedModifiers.Contains(option.Modifier)
                : _selectedFoodModifiers.Contains(option.Modifier);
        }

        _bankAppBounds = new Rectangle(_editorViewportBounds.X + 100, _editorViewportBounds.Y + 34, 700, 468);
        _closeBankAppButton.Bounds = new Rectangle(_bankAppBounds.Right - 88, _bankAppBounds.Y + 18, 64, 28);

        _applicationBounds = new Rectangle(_editorViewportBounds.X + 70, _editorViewportBounds.Y + 16, 774, 566);
        _applicationEditorBounds = new Rectangle(_applicationBounds.X + 24, _applicationBounds.Y + 174, _applicationBounds.Width - 48, _applicationBounds.Height - 198);
        _closeApplicationButton.Bounds = new Rectangle(_applicationBounds.Right - 88, _applicationBounds.Y + 18, 64, 28);
        var optionY = _applicationEditorBounds.Bottom - 144;
        for (var index = 0; index < _interviewOptionButtons.Length; index++)
        {
            _interviewOptionButtons[index].Bounds = new Rectangle(_applicationEditorBounds.X + 24, optionY + (index * 42), _applicationEditorBounds.Width - 48, 34);
            _interviewOptionButtons[index].Enabled = false;
            _interviewOptionButtons[index].Text = string.Empty;
        }

        if (_state.ActiveJobApplication is not null &&
            _state.ActiveJobApplication.TakeHomeComplete &&
            !_state.ActiveJobApplication.InterviewComplete)
        {
            var question = _state.ActiveJobApplication.Questions[_state.ActiveJobApplication.CurrentQuestionIndex];
            for (var index = 0; index < _interviewOptionButtons.Length; index++)
            {
                if (index >= question.Options.Count)
                {
                    _interviewOptionButtons[index].Bounds = Rectangle.Empty;
                    continue;
                }

                _interviewOptionButtons[index].Enabled = _simulation.CanAnswerInterviewQuestion(_state, index);
                _interviewOptionButtons[index].Text = question.Options[index];
            }
        }

        _freelanceBoardBounds = new Rectangle(_editorViewportBounds.X + 92, _editorViewportBounds.Y + 24, 724, 530);
        _closeFreelanceBoardButton.Bounds = new Rectangle(_freelanceBoardBounds.Right - 88, _freelanceBoardBounds.Y + 18, 64, 28);

        var freelanceCardX = _freelanceBoardBounds.X + 24;
        var freelanceCardY = _freelanceBoardBounds.Y + 126;
        var freelanceCardWidth = _freelanceBoardBounds.Width - 48;
        var freelanceCardHeight = 118;

        _freelanceGigCardBounds[FreelanceGigType.QuickBugfix] = new Rectangle(freelanceCardX, freelanceCardY, freelanceCardWidth, freelanceCardHeight);
        _freelanceGigCardBounds[FreelanceGigType.UIPolishPass] = new Rectangle(freelanceCardX, freelanceCardY + freelanceCardHeight + 14, freelanceCardWidth, freelanceCardHeight);
        _freelanceGigCardBounds[FreelanceGigType.PipelineRescue] = new Rectangle(freelanceCardX, freelanceCardY + ((freelanceCardHeight + 14) * 2), freelanceCardWidth, freelanceCardHeight);

        foreach (var type in Enum.GetValues<FreelanceGigType>())
        {
            var button = _freelanceGigButtons[type];
            button.Enabled = _simulation.CanTakeFreelanceGig(_state, type);
            var cardBounds = _freelanceGigCardBounds[type];
            button.Bounds = new Rectangle(cardBounds.Right - 120, cardBounds.Bottom - 42, 110, 30);
        }

        var upgradesWidth = Math.Min(_editorViewportBounds.Width - 48, 992);
        var upgradesX = _editorViewportBounds.X + ((_editorViewportBounds.Width - upgradesWidth) / 2);
        _upgradesBounds = new Rectangle(upgradesX, _editorViewportBounds.Y + 8, upgradesWidth, 636);
        _closeUpgradesButton.Bounds = new Rectangle(_upgradesBounds.Right - 88, _upgradesBounds.Y + 18, 64, 28);

        var upgradeCardWidth = (_upgradesBounds.Width - 72) / 2;
        var upgradeCardHeight = 118;
        var firstCardX = _upgradesBounds.X + 24;
        var secondCardX = firstCardX + upgradeCardWidth + 24;
        var firstRowY = _upgradesBounds.Y + 126;
        const int upgradeRowGap = 12;
        for (var index = 0; index < EfficiencyUpgradeCatalog.All.Count; index++)
        {
            var definition = EfficiencyUpgradeCatalog.All[index];
            var column = index % 2;
            var row = index / 2;
            var x = column == 0 ? firstCardX : secondCardX;
            var y = firstRowY + (row * (upgradeCardHeight + upgradeRowGap));
            _upgradeCardBounds[definition.Type] = new Rectangle(x, y, upgradeCardWidth, upgradeCardHeight);
        }

        foreach (var definition in EfficiencyUpgradeCatalog.All)
        {
            var button = _upgradeButtons[definition.Type];
            button.Text = _state.PurchasedUpgrades.Contains(definition.Type)
                ? "Installed"
                : $"Buy ${definition.FundsCost:0}";
            button.Enabled = _simulation.CanPurchaseUpgrade(_state, definition.Type);

            var cardBounds = _upgradeCardBounds[definition.Type];
            button.Bounds = new Rectangle(cardBounds.Right - 126, cardBounds.Bottom - 42, 112, 30);
        }

        _burgerButton.IsSelected = _selectedFood == FoodChoice.Burger;
        _burgerButton.Enabled = !hasActiveFoodDelivery;
        _burritoButton.IsSelected = _selectedFood == FoodChoice.Burrito;
        _burritoButton.Enabled = !hasActiveFoodDelivery;
        _pizzaButton.IsSelected = _selectedFood == FoodChoice.Pizza;
        _pizzaButton.Enabled = !hasActiveFoodDelivery;
        _dumplingsButton.IsSelected = _selectedFood == FoodChoice.Dumplings;
        _dumplingsButton.Enabled = !hasActiveFoodDelivery;
        _doubleCheckOrderButton.Enabled = !hasActiveFoodDelivery;
        _doubleCheckOrderButton.IsSelected = reviewReceiptEnabled;
        _doubleCheckOrderButton.Text = reviewReceiptEnabled ? "Review Receipt: ON" : "Review Receipt: OFF";
        _expediteOrderButton.Enabled = !hasActiveFoodDelivery;
        _expediteOrderButton.IsSelected = expeditedDeliveryEnabled;
        _expediteOrderButton.Text = expeditedDeliveryEnabled
            ? $"Expedite (+${_simulation.GetFoodTipAmount(true):0}): ON"
            : $"Expedite (+${_simulation.GetFoodTipAmount(true):0}): OFF";
        _confirmFoodOrderButton.Text = hasActiveFoodDelivery ? "Order Active" : "Place Order";
        _confirmFoodOrderButton.Enabled = _simulation.CanPlaceFoodOrder(_state, _selectedFood, _expediteFoodDelivery);
    }

    private string GetFoodModifierIntroText(ActiveFoodDelivery? activeDelivery)
    {
        return activeDelivery is null
            ? "Toggle the details you want fixed before the order leaves the kitchen."
            : $"Kitchen notes locked in. Receipt review: {(activeDelivery.ReviewReceipt ? "ON" : "OFF")}  |  Tip: ${activeDelivery.TipAmount:0}.";
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
        _catOverlayBounds = new Rectangle(_editorViewportBounds.X + 144, _editorViewportBounds.Y + 108, _editorViewportBounds.Width - 288, 284);
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
                return ("Review Receipt", "Check the full order before it goes through. Pair it with the right order details to eliminate the sluggish penalty.");
            }

            if (_expediteOrderButton.IsHovered)
            {
                return (
                    "Expedite Delivery",
                    $"Add a ${_simulation.GetFoodTipAmount(true):0} tip to cut delivery time down to {FormatRemainingTime(_simulation.GetFoodDeliveryDuration(true))} instead of {FormatRemainingTime(_simulation.GetFoodDeliveryDuration(false))}.");
            }

            var orderOptions = _simulation.GetFoodOrderModifiers(_selectedFood);
            for (var index = 0; index < _foodModifierButtons.Length && index < orderOptions.Count; index++)
            {
                if (_foodModifierButtons[index].IsHovered)
                {
                    return (
                        orderOptions[index].Label,
                        $"{orderOptions[index].Description} {(orderOptions[index].Recommended ? "This one is recommended for a clean order." : "This one is optional support, not the main fix.")}");
                }
            }

            if (_confirmFoodOrderButton.IsHovered)
            {
                var option = _simulation.GetFoodOption(_selectedFood);
                var penalty = _simulation.GetFoodOrderPenaltyMinutes(_selectedFood, _selectedFoodModifiers, _doubleCheckOrder);
                return (
                    "Place Order",
                    $"Spend ${_simulation.GetFoodTotalCost(_selectedFood, _expediteFoodDelivery):0} for {option.Name}. ETA {FormatRemainingTime(_simulation.GetFoodDeliveryDuration(_expediteFoodDelivery))}. Focus {FormatSigned(option.FocusGain)}, sanity {FormatSigned(option.SanityGain)}, and expected sluggishness {(penalty <= 0 ? "removed" : FormatRemainingTime(penalty))} on arrival.");
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
            if (_state.ActiveFoodDelivery is not null)
            {
                var delivery = _state.ActiveFoodDelivery;
                return (
                    "Track Order",
                    $"{_simulation.GetFoodOption(delivery.Choice).Name} is already on the road. ETA {FormatRemainingTime(delivery.RemainingInGameMinutes)} before the stats land.");
            }

            return ("Delivery App", "Browse near-full focus refills that also reset hunger, then decide how much sluggishness risk you can tolerate while the delivery timer runs.");
        }

        if (_freelanceButton.IsHovered)
        {
            return ("Freelance Board", "Choose between short bugfixes, medium polish work, or a brutal pipeline rescue for bigger cash.");
        }

        if (_upgradesButton.IsHovered)
        {
            return ("Upgrades", "Spend saved funds on permanent typing throughput, quality, or focus-efficiency improvements.");
        }

        if (_bankAppButton.IsHovered)
        {
            return ("Banking App", "Open a clean finance view with runway, rent countdown, and first-coin emergency status.");
        }

        if (_optionsButton.IsHovered)
        {
            return ("Options", "Adjust resolution, display mode, and live audio levels without leaving the run.");
        }

        if (_coinFrameBounds.Contains(_mousePosition))
        {
            return _state.HasFirstCoin
                ? ("The First Coin", $"A keepsake from the first game sale. Passive sanity regen: +{_simulation.Config.FirstCoinPassiveSanityRegenPerInGameMinute * 60:0.0} per in-game hour. It can still buy one last rent rescue.")
                : ("Empty Frame", "The first coin is gone. The desk kept the frame, but not the hope it carried.");
        }

        if (_sleepButton.IsHovered)
        {
            var urgency = _simulation.RequiresSleep(_state)
                ? " Required right now."
                : string.Empty;
            return ("Sleep", $"Advance {FormatRemainingTime(_simulation.Config.SleepDurationMinutes)}. Focus refills to full, sanity {FormatSigned(_simulation.Config.SleepSanityGain)}, and sleep debt clears.{urgency}");
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
                $"Resume cost: {listing.ResumeCostLines} LoC. Requirements: {listing.MinimumPortfolioLines} LoC, {listing.MinimumCodeQuality:0} quality, and {_simulation.GetResumeTrackLabel(listing.ResumeTrack)} proof {_simulation.GetResumeProof(_state, listing.ResumeTrack)}/{listing.RequiredResumeProof}.");
        }

        if (_publishAppButton.IsHovered)
        {
            var payoutWindow = $"${_simulation.Config.PublishAppFundsMin:0}-${_simulation.Config.PublishAppFundsMax:0}";
            return (
                "Publish App",
                $"Ship the completed release for a randomized {payoutWindow} payout, then roll into the next batch of snippets. Storefront sales will continue landing on a bounded random timer.");
        }

        if (_openApplicationButton.IsHovered && _state.ActiveJobApplication is not null)
        {
            return (
                _state.ActiveJobApplication.ListingTitle,
                _state.ActiveJobApplication.TakeHomeComplete
                    ? "Reopen the recruiter overlay and answer the next interview question without losing access to the rest of the desk."
                    : "Reopen the take-home coding task. You can step away, recover, and come back without canceling the application.");
        }

        return null;
    }

    private (string Title, string Body) BuildFoodTooltip(FoodChoice choice)
    {
        var option = _simulation.GetFoodOption(choice);
        var recommended = _simulation.GetFoodOrderModifiers(choice)
            .Where(static modifier => modifier.Recommended)
            .Select(static modifier => modifier.Label)
            .ToArray();
        return (
            option.Name,
            $"{option.Description} Base cost ${option.FundsCost:0}. Standard ETA {FormatRemainingTime(_simulation.GetFoodDeliveryDuration(false))}, expedited ETA {FormatRemainingTime(_simulation.GetFoodDeliveryDuration(true))}. Clean order notes: {string.Join(", ", recommended)}.");
    }

    private (string Title, string Body) BuildFreelanceTooltip(FreelanceGigType type)
    {
        var gig = _simulation.GetFreelanceGig(type);
        return (
            gig.Name,
            $"{gig.Description} Duration {FormatRemainingTime(gig.DurationMinutes)}. Funds +${gig.FundsGain:0}, focus -{gig.FocusCost:0}, sanity -{gig.SanityCost:0}, quality {FormatSigned(gig.CodeQualityGain)}.");
    }

    private string GetLifeEventDecisionText(PendingLifeEvent lifeEvent)
    {
        return lifeEvent.Type switch
        {
            IncidentType.ComputerFreeze =>
                $"The machine is dead until you deal with it. Fixing it yourself burns {FormatRemainingTime(_simulation.Config.ComputerFreezeSelfRepairDurationMinutes)} and sanity. Tech support is faster but costs ${_simulation.Config.ComputerFreezeTechSupportFundsCost:0}. The repair shop is the slowest and most expensive, but least mentally brutal.",
            IncidentType.OnlineMatch =>
                $"{lifeEvent.SubjectName} looks like an actual human possibility, not just algorithm filler. Message them for a smaller time hit, spend ${_simulation.Config.OnlineDateFundsCost:0} to actually go out, or let the whole thing die and keep grinding. Finding love adds passive sanity support.",
            _ =>
                $"{lifeEvent.SubjectName} is queued and autoplay is ready to steal the rest of the night. Binging buys sanity at the cost of real time, one episode is the compromise line, and shutting it off protects the schedule but feels bad in the moment.",
        };
    }

    private static string[] GetLifeEventOptionLabels(PendingLifeEvent lifeEvent)
    {
        return lifeEvent.Type switch
        {
            IncidentType.ComputerFreeze => ["Repair Myself", "Tech Support", "Repair Shop"],
            IncidentType.OnlineMatch => ["Send Opener", "Go On Date", "Pass"],
            _ => ["Binge", "One Episode", "Turn It Off"],
        };
    }

    private void SetSelectedFood(FoodChoice choice)
    {
        _selectedFood = choice;
        _selectedFoodModifiers.Clear();
        _doubleCheckOrder = false;
        _expediteFoodDelivery = false;
        _audio.PlayButtonClick();
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
        var totalPrograms = PortfolioWorkspace.GetProgramCount(_state);
        var programComplete = PortfolioWorkspace.HasFiniteProgramCount(_state) &&
                              _state.CurrentProgramIndex >= totalPrograms - 1 &&
                              _state.CurrentProgramVisibleLineCount >= program.CodeLines.Count;

        if (_simulation.RequiresSleep(_state))
        {
            return "You have been awake for two straight days. Sleep before you write, freelance, or continue the recruiter loop.";
        }

        if (_simulation.HasActiveJobApplication(_state))
        {
            return "A recruiter loop is active. Keep building or recover on the desk, then reopen the take-home or interview from Alerts and Inbox when you are ready.";
        }

        if (_state.ActiveCatInterruption is not null)
        {
            return $"Cat on the keyboard. Click the editor {_state.ActiveCatInterruption.PatsRemaining} more times to pet it away while {_state.ActiveCatInterruption.GibberishLinesTyped} gibberish lines and {_state.ActiveCatInterruption.PhantomBugCount} bug bursts stay on screen.";
        }

        if (_state.ActiveFoodDelivery is not null)
        {
            return $"{_simulation.GetFoodOption(_state.ActiveFoodDelivery.Choice).Name} is on the way. Delivery ETA {FormatRemainingTime(_state.ActiveFoodDelivery.RemainingInGameMinutes)} before the focus refill lands.";
        }

        if (_simulation.GetSleepStage(_state) >= 2)
        {
            return $"Awake for {FormatRemainingTime(_state.MinutesSinceLastSleep)}. Fatigue is dragging down sanity and code quality until you sleep.";
        }

        if (_simulation.GetHungerStage(_state) >= 1)
        {
            return $"No meal for {FormatRemainingTime(_state.MinutesSinceLastMeal)}. Eat soon or hunger keeps chewing through sanity.";
        }

        if (programComplete)
        {
            return "All current portfolio files are typed out. Publish from Alerts & Inbox to cash out and unlock the next snippet batch.";
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

    private IReadOnlyList<string> BuildDisplayedCodeLines(IReadOnlyList<string> visibleLines)
    {
        if (_state.ActiveCatInterruption is null)
        {
            return visibleLines;
        }

        var noiseLines = BuildCatNoiseLines(_state.ActiveCatInterruption);
        if (noiseLines.Count == 0)
        {
            return visibleLines;
        }

        if (visibleLines.Count == 0)
        {
            return noiseLines;
        }

        var displayLines = new List<string>(visibleLines.Count + noiseLines.Count);
        var insertionGap = Math.Max(1, visibleLines.Count / Math.Max(1, noiseLines.Count));
        var noiseIndex = 0;

        for (var index = 0; index < visibleLines.Count; index++)
        {
            displayLines.Add(visibleLines[index]);

            if (((index + 1) % insertionGap == 0 || index == visibleLines.Count - 1) &&
                noiseIndex < noiseLines.Count)
            {
                displayLines.Add(noiseLines[noiseIndex]);
                noiseIndex++;
            }
        }

        while (noiseIndex < noiseLines.Count)
        {
            displayLines.Add(noiseLines[noiseIndex]);
            noiseIndex++;
        }

        return displayLines;
    }

    private static IReadOnlyList<string> BuildCatNoiseLines(ActiveCatInterruption cat)
    {
        var lines = new List<string>();
        var bugCount = Math.Min(6, Math.Max(1, cat.PhantomBugCount));
        var gibberishCount = Math.Min(8, Math.Max(2, cat.GibberishLinesTyped));

        for (var index = 0; index < bugCount; index++)
        {
            lines.Add(GetCatBugLine(cat.VisualSeed, index));
        }

        for (var index = 0; index < gibberishCount; index++)
        {
            lines.Add(GetCatGibberishLine(cat.VisualSeed, index));
        }

        return lines;
    }

    private static string GetCatBugLine(int seed, int index)
    {
        return ((seed + index) % 6) switch
        {
            0 => "error CS1002: ; expected after paw smash",
            1 => "BUG: desk-cat race condition triggered",
            2 => "if (keyboardOwner == Cat) throw new MeowException();",
            3 => "public void meow_meow_meow(",
            4 => "// TODO: untangle the cat-typed branch",
            _ => "return null; // purring broke the guard clause",
        };
    }

    private static string GetCatGibberishLine(int seed, int index)
    {
        return ((seed + (index * 7)) % 8) switch
        {
            0 => "mrrp;;;;[] [] []",
            1 => "asdfjjjjjjjjjjj",
            2 => "==^..^== => ???",
            3 => "///// paw paw paw /////",
            4 => "klklklkl ;;; meow ;;;",
            5 => ">>>> ??? <<<< ???",
            6 => "[]{}[]{} cat cat cat",
            _ => "zxzxzxzx meeeeeow",
        };
    }

    private static Color GetCodeLineColor(string line)
    {
        var trimmed = line.TrimStart();
        if (trimmed.StartsWith("error ", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("BUG:", StringComparison.OrdinalIgnoreCase))
        {
            return UiTheme.Danger;
        }

        if (trimmed.StartsWith("//", StringComparison.Ordinal) ||
            trimmed.Contains("meow", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("mrrp", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("paw", StringComparison.OrdinalIgnoreCase))
        {
            return UiTheme.CatAccent;
        }

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

    private IReadOnlyList<string> GetRecentMoneyEntries()
    {
        var entries = new List<string>();
        for (var index = _state.EventLog.Count - 1; index >= 0 && entries.Count < 5; index--)
        {
            var entry = _state.EventLog[index];
            if (entry.Contains('$', StringComparison.Ordinal) ||
                entry.Contains("rent", StringComparison.OrdinalIgnoreCase) ||
                entry.Contains("bill", StringComparison.OrdinalIgnoreCase) ||
                entry.Contains("delivery", StringComparison.OrdinalIgnoreCase) ||
                entry.Contains("gig", StringComparison.OrdinalIgnoreCase))
            {
                entries.Add(entry);
            }
        }

        if (entries.Count == 0)
        {
            entries.Add("No money movement yet. Build some code, take a gig, or buy the first meal.");
        }

        return entries;
    }
}
