using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MicroDev.Core.Audio;
using MicroDev.Core.Input;
using MicroDev.Core.Portfolio;
using MicroDev.Core.Simulation;
using MicroDev.Core.UI;

namespace MicroDev.Core.Screens;

public sealed class WorkspaceScreen : IScreen, IUiFontAware
{
    private enum OverlayScrollArea
    {
        None,
        Communication,
        ProjectStudio,
        FreelanceBoard,
        Upgrades,
        Stats,
    }

    private const float CodeScale = UiTypography.Code;
    private const float CardBodyScale = UiTypography.Caption;
    private const float BodyScale = UiTypography.Body;
    private const float SmallScale = UiTypography.Caption;
    private const int FreelanceCardHeight = 118;
    private const int FreelanceCardGap = 14;
    private const int FreelanceBoardContentPadding = 12;
    private static readonly FoodChoice[] FoodChoices =
    [
        FoodChoice.Burger,
        FoodChoice.Burrito,
        FoodChoice.Pizza,
        FoodChoice.Dumplings,
        FoodChoice.Ramen,
        FoodChoice.RiceBowl,
        FoodChoice.SkilletPasta,
        FoodChoice.MealPrepChili,
    ];
    private static readonly FreelanceGigType[] FreelanceGigOrder = Enum.GetValues<FreelanceGigType>();

    private SpriteFont _font;
    private readonly Texture2D _pixel;
    private readonly SimulationEngine _simulation;
    private readonly IncidentScheduler _incidentScheduler;
    private readonly GameAudio _audio;
    private readonly Point _virtualResolution;
    private readonly Action<WorkspaceScreen> _showOptions;
    private readonly Action _showMainMenu;
    private readonly UiButton _foodAppButton = new("Food + Kitchen");
    private readonly UiButton _freelanceButton = new("Freelance Board");
    private readonly UiButton _sleepButton = new("Sleep");
    private readonly UiButton _upgradesButton = new("Upgrades");
    private readonly UiButton _statsButton = new("Run Stats");
    private readonly UiButton _bankAppButton = new("Banking");
    private readonly UiButton _communicationButton = new("Communication");
    private readonly UiButton _projectStudioButton = new("Build Studio");
    private readonly UiButton _guideButton = new("Guide");
    private readonly UiButton _newRunButton = new("New Run");
    private readonly UiButton _menuButton = new("Menu");
    private readonly UiButton _optionsButton = new("Options");
    private readonly UiButton _squashBugButton = new("Fix");
    private readonly UiButton _applyForJobButton = new("Apply");
    private readonly UiButton _publishAppButton = new("Publish");
    private readonly UiButton _restartButton = new("Restart Run");
    private readonly UiButton _breakCoinButton = new("Break Frame");
    private readonly UiButton _acceptEvictionButton = new("Let Go");
    private readonly UiButton _deskDistractionFocusButton = new("Refocus");
    private readonly UiButton _deskDistractionQuickFixButton = new("Quick Fix");
    private readonly UiButton _burgerButton = new("Burger");
    private readonly UiButton _burritoButton = new("Burrito");
    private readonly UiButton _pizzaButton = new("Pizza");
    private readonly UiButton _dumplingsButton = new("Dumplings");
    private readonly UiButton _ramenButton = new("Ramen");
    private readonly UiButton _riceBowlButton = new("Rice Bowl");
    private readonly UiButton _skilletPastaButton = new("Pasta");
    private readonly UiButton _mealPrepChiliButton = new("Chili");
    private readonly UiButton _doubleCheckOrderButton = new("Check Details: OFF");
    private readonly UiButton _expediteOrderButton = new("Expedite: OFF");
    private readonly UiButton _confirmFoodOrderButton = new("Place Order");
    private readonly UiButton _closeFoodAppButton = new("Close");
    private readonly UiButton _closeBankAppButton = new("Close");
    private readonly UiButton _closeCommunicationButton = new("Close");
    private readonly UiButton _closeFreelanceBoardButton = new("Close");
    private readonly UiButton _closeUpgradesButton = new("Close");
    private readonly UiButton _closeStatsButton = new("Close");
    private readonly UiButton _closeProjectStudioButton = new("Close");
    private readonly UiButton _commitFileButton = new("Commit Now");
    private readonly UiButton _keepCodingButton = new("Keep Coding");
    private readonly UiButton _projectTypeButton = new("Type");
    private readonly UiButton _projectThemeButton = new("Theme");
    private readonly UiButton _projectToneButton = new("Tone");
    private readonly UiButton _projectPlatformButton = new("Platform");
    private readonly UiButton _projectBusinessButton = new("Business");
    private readonly UiButton _projectRerollButton = new("Reroll Title");
    private readonly UiButton _buyHouseButton = new("Buy House");
    private readonly UiButton _retireButton = new("Retire");
    private readonly UiButton _openApplicationButton = new("Continue");
    private readonly UiButton _closeApplicationButton = new("Close");
    private readonly UiButton _tutorialBackButton = new("Back");
    private readonly UiButton _tutorialNextButton = new("Next");
    private readonly UiButton _tutorialCloseButton = new("Close");
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
    private Rectangle _runControlsBounds;
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
    private Rectangle _communicationBounds;
    private Rectangle _communicationViewportBounds;
    private Rectangle _communicationDetailBounds;
    private Rectangle _bankAccountBounds;
    private Rectangle _bankRentBounds;
    private Rectangle _bankHouseBounds;
    private Rectangle _bankRetirementBounds;
    private Rectangle _bankCoinBounds;
    private Rectangle _bankLedgerBounds;
    private Rectangle _commitPromptBounds;
    private Rectangle _projectStudioBounds;
    private Rectangle _freelanceBoardBounds;
    private Rectangle _upgradesBounds;
    private Rectangle _statsBounds;
    private Rectangle _projectStudioViewportBounds;
    private Rectangle _projectStudioScrollbarTrackBounds;
    private Rectangle _projectStudioScrollbarThumbBounds;
    private Rectangle _communicationScrollbarTrackBounds;
    private Rectangle _communicationScrollbarThumbBounds;
    private Rectangle _freelanceBoardViewportBounds;
    private Rectangle _freelanceBoardScrollbarTrackBounds;
    private Rectangle _freelanceBoardScrollbarThumbBounds;
    private Rectangle _freelanceGigSummaryBounds;
    private Rectangle _freelanceGigEditorBounds;
    private Rectangle _upgradesViewportBounds;
    private Rectangle _upgradesScrollbarTrackBounds;
    private Rectangle _upgradesScrollbarThumbBounds;
    private Rectangle _statsViewportBounds;
    private Rectangle _statsScrollbarTrackBounds;
    private Rectangle _statsScrollbarThumbBounds;
    private Rectangle _applicationBounds;
    private Rectangle _applicationEditorBounds;
    private Rectangle _debugSnippetBounds;
    private Rectangle _debugHighlightBounds;
    private Rectangle _tutorialBounds;
    private readonly Dictionary<EfficiencyUpgradeType, Rectangle> _upgradeCardBounds = [];
    private readonly Dictionary<FreelanceGigType, Rectangle> _freelanceGigCardBounds = [];
    private readonly Dictionary<string, Rectangle> _communicationCardBounds = [];
    private readonly Dictionary<string, UiButton> _communicationPickButtons = [];
    private readonly Dictionary<string, UiButton> _communicationMessageButtons = [];
    private readonly Dictionary<string, UiButton> _communicationCallButtons = [];
    private bool _foodAppOpen;
    private bool _bankAppOpen;
    private bool _communicationOpen;
    private bool _commitPromptOpen;
    private bool _projectStudioOpen;
    private bool _freelanceBoardOpen;
    private bool _upgradesOpen;
    private bool _statsOpen;
    private bool _jobApplicationOpen;
    private FoodChoice _selectedFood = FoodChoice.Burger;
    private bool _doubleCheckOrder;
    private bool _expediteFoodDelivery;
    private float _projectStudioScrollOffset;
    private float _projectStudioMaxScrollOffset;
    private float _communicationScrollOffset;
    private float _communicationMaxScrollOffset;
    private float _freelanceBoardScrollOffset;
    private float _freelanceBoardMaxScrollOffset;
    private float _upgradesScrollOffset;
    private float _upgradesMaxScrollOffset;
    private float _statsScrollOffset;
    private float _statsMaxScrollOffset;
    private string? _lastCelebratedFileName;
    private string? _lastCommitPromptedFileName;
    private string? _selectedCommunicationContactId;
    private Point _mousePosition;
    private bool _tutorialOpen;
    private int _tutorialPageIndex;
    private OverlayScrollArea _activeScrollbarDrag;
    private int _scrollbarDragStartMouseY;
    private float _scrollbarDragStartOffset;

    public WorkspaceScreen(
        SpriteFont font,
        Texture2D pixel,
        SimulationEngine simulation,
        IncidentScheduler incidentScheduler,
        GameAudio audio,
        Point virtualResolution,
        Action<WorkspaceScreen> showOptions,
        Action showMainMenu)
    {
        _font = font;
        _pixel = pixel;
        _simulation = simulation;
        _incidentScheduler = incidentScheduler;
        _audio = audio;
        _virtualResolution = virtualResolution;
        _showOptions = showOptions;
        _showMainMenu = showMainMenu;
        _state = _simulation.CreateNewRun();

        foreach (var definition in EfficiencyUpgradeCatalog.All)
        {
            _upgradeButtons[definition.Type] = new UiButton("Buy");
        }

        foreach (var type in FreelanceGigOrder)
        {
            _freelanceGigButtons[type] = new UiButton("Take Gig");
        }

        EnsureCommunicationButtons();
        ConfigureButtons();
        UpdateLayout();
        OpenTutorial();
        UpdateButtons();
    }

    public void ApplyFont(SpriteFont font)
    {
        _font = font;
    }

    private void EnsureCommunicationButtons()
    {
        foreach (var contact in _state.KnownContacts)
        {
            if (!_communicationPickButtons.ContainsKey(contact.Id))
            {
                _communicationPickButtons[contact.Id] = new UiButton(contact.Name);
            }

            if (!_communicationMessageButtons.ContainsKey(contact.Id))
            {
                _communicationMessageButtons[contact.Id] = new UiButton("Text");
            }

            if (!_communicationCallButtons.ContainsKey(contact.Id))
            {
                _communicationCallButtons[contact.Id] = new UiButton("Call");
            }
        }
    }

    public void Update(GameTime gameTime, InputSnapshot input)
    {
        var previousStatus = _state.Status;
        _mousePosition = input.MousePosition;
        ConfigureButtons();
        AdvanceButtonAnimations((float)gameTime.ElapsedGameTime.TotalSeconds);

        if (previousStatus == RunStatus.InProgress && !_tutorialOpen)
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

        EnsureCommunicationButtons();
        UpdateLayout();
        UpdateButtons();
        UpdateDebugSnippetLayout();
        UpdateCommitPromptState();
        if (_state.ActiveJobApplication is null)
        {
            _jobApplicationOpen = false;
        }

        if (_tutorialOpen)
        {
            HandleTutorialInput(input);
        }
        else if (_state.Status != RunStatus.InProgress)
        {
            _foodAppOpen = false;
            _bankAppOpen = false;
            _communicationOpen = false;
            _commitPromptOpen = false;
            _projectStudioOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _jobApplicationOpen = false;
            _restartButton.Enabled = true;
            _newRunButton.Enabled = true;
            if (_statsOpen)
            {
                HandleStatsInput(input);
            }
            else if (_statsButton.Update(input))
            {
                _statsOpen = true;
                _audio.PlayButtonClick();
            }
            else if (_restartButton.Update(input))
            {
                _audio.PlayButtonClick();
                RestartCurrentRun();
            }
            else if (_newRunButton.Update(input))
            {
                _audio.PlayButtonClick();
                StartFreshRun();
            }
        }
        else if (_state.FirstCoinDecisionPending)
        {
            _foodAppOpen = false;
            _bankAppOpen = false;
            _communicationOpen = false;
            _commitPromptOpen = false;
            _projectStudioOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _statsOpen = false;
            _jobApplicationOpen = false;
            HandleFirstCoinInput(input);
        }
        else if (_simulation.HasPendingLifeEvent(_state))
        {
            _foodAppOpen = false;
            _bankAppOpen = false;
            _communicationOpen = false;
            _commitPromptOpen = false;
            _projectStudioOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _statsOpen = false;
            _jobApplicationOpen = false;
            HandleLifeEventInput(input);
        }
        else if (_statsOpen)
        {
            HandleStatsInput(input);
        }
        else if (_commitPromptOpen)
        {
            HandleCommitPromptInput(input);
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
        else if (_communicationOpen)
        {
            HandleCommunicationInput(input);
        }
        else if (_projectStudioOpen)
        {
            HandleProjectStudioInput(input);
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

        EnsureCommunicationButtons();
        UpdateButtons();
        UpdateDebugSnippetLayout();
        PlayFileCompletionAudio();
        PlayOutcomeAudio(previousStatus, _state.Status);
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        ConfigureButtons();
        DrawWorkspace(spriteBatch);

        if (_foodAppOpen && _state.Status == RunStatus.InProgress)
        {
            DrawFoodAppOverlay(spriteBatch);
        }
        else if (_bankAppOpen && _state.Status == RunStatus.InProgress)
        {
            DrawBankAppOverlay(spriteBatch);
        }
        else if (_communicationOpen && _state.Status == RunStatus.InProgress)
        {
            DrawCommunicationOverlay(spriteBatch);
        }
        else if (_commitPromptOpen && _state.Status == RunStatus.InProgress)
        {
            DrawCommitPromptOverlay(spriteBatch);
        }
        else if (_projectStudioOpen && _state.Status == RunStatus.InProgress)
        {
            DrawProjectStudioOverlay(spriteBatch);
        }
        else if (_freelanceBoardOpen && _state.Status == RunStatus.InProgress)
        {
            DrawFreelanceBoardOverlay(spriteBatch);
        }
        else if (_upgradesOpen && _state.Status == RunStatus.InProgress)
        {
            DrawUpgradesOverlay(spriteBatch);
        }
        else if (_statsOpen)
        {
            DrawStatsOverlay(spriteBatch);
        }
        else if (_jobApplicationOpen && _state.Status == RunStatus.InProgress && _simulation.HasActiveJobApplication(_state))
        {
            DrawJobApplicationOverlay(spriteBatch);
        }

        if (_tutorialOpen)
        {
            DrawTutorialOverlay(spriteBatch);
        }
        else if (_state.Status != RunStatus.InProgress && !_statsOpen)
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
        foreach (var button in GetAllButtons())
        {
            button.TextScale = UiTypography.Button;
        }

        foreach (var button in GetSidebarActionButtons())
        {
            button.TextScale = UiTypography.Body;
            button.HorizontalPadding = 10;
        }

        _guideButton.TextScale = UiTypography.Body;
        _guideButton.HorizontalPadding = 10;
        _restartButton.TextScale = _state.Status == RunStatus.InProgress ? UiTypography.Body : UiTypography.Button;
        _restartButton.HorizontalPadding = 10;
        _newRunButton.TextScale = _state.Status == RunStatus.InProgress ? UiTypography.Body : UiTypography.Button;
        _newRunButton.HorizontalPadding = 10;
        _menuButton.TextScale = UiTypography.Body;
        _menuButton.HorizontalPadding = 10;
        _optionsButton.TextScale = UiTypography.Body;
        _optionsButton.HorizontalPadding = 10;

        foreach (var button in GetOverlayCloseButtons())
        {
            button.TextScale = UiTypography.Body;
            button.HorizontalPadding = 10;
        }

        _foodAppButton.AccentColor = UiTheme.Accent;
        _freelanceButton.AccentColor = UiTheme.Warning;
        _sleepButton.AccentColor = UiTheme.Warning;
        _upgradesButton.AccentColor = UiTheme.Success;
        _statsButton.AccentColor = UiTheme.Accent;
        _bankAppButton.AccentColor = UiTheme.Accent;
        _communicationButton.AccentColor = UiTheme.Accent;
        _projectStudioButton.AccentColor = UiTheme.Success;
        _guideButton.AccentColor = UiTheme.Accent;
        _newRunButton.AccentColor = UiTheme.Success;
        _menuButton.AccentColor = UiTheme.Accent;
        _optionsButton.AccentColor = UiTheme.Warning;
        _squashBugButton.AccentColor = UiTheme.Danger;
        _applyForJobButton.AccentColor = UiTheme.Success;
        _publishAppButton.AccentColor = UiTheme.Success;
        _restartButton.AccentColor = UiTheme.Warning;
        _breakCoinButton.AccentColor = UiTheme.Warning;
        _acceptEvictionButton.AccentColor = UiTheme.Danger;
        _deskDistractionFocusButton.AccentColor = UiTheme.Accent;
        _deskDistractionQuickFixButton.AccentColor = UiTheme.Warning;
        _burgerButton.AccentColor = UiTheme.Warning;
        _burritoButton.AccentColor = UiTheme.Warning;
        _pizzaButton.AccentColor = UiTheme.Warning;
        _dumplingsButton.AccentColor = UiTheme.Warning;
        _skilletPastaButton.AccentColor = UiTheme.Success;
        _mealPrepChiliButton.AccentColor = UiTheme.Success;
        _doubleCheckOrderButton.AccentColor = UiTheme.Accent;
        _expediteOrderButton.AccentColor = UiTheme.Warning;
        _confirmFoodOrderButton.AccentColor = UiTheme.Success;
        _closeFoodAppButton.AccentColor = UiTheme.Warning;
        _closeBankAppButton.AccentColor = UiTheme.Warning;
        _closeFreelanceBoardButton.AccentColor = UiTheme.Warning;
        _closeUpgradesButton.AccentColor = UiTheme.Warning;
        _closeStatsButton.AccentColor = UiTheme.Warning;
        _closeProjectStudioButton.AccentColor = UiTheme.Warning;
        _commitFileButton.AccentColor = UiTheme.Success;
        _keepCodingButton.AccentColor = UiTheme.Warning;
        _projectTypeButton.AccentColor = UiTheme.Accent;
        _projectThemeButton.AccentColor = UiTheme.Accent;
        _projectToneButton.AccentColor = UiTheme.Accent;
        _projectPlatformButton.AccentColor = UiTheme.Accent;
        _projectBusinessButton.AccentColor = UiTheme.Accent;
        _projectRerollButton.AccentColor = UiTheme.Warning;
        _buyHouseButton.AccentColor = UiTheme.Success;
        _retireButton.AccentColor = UiTheme.Success;
        _openApplicationButton.AccentColor = UiTheme.Accent;
        _closeApplicationButton.AccentColor = UiTheme.Warning;
        _tutorialBackButton.AccentColor = UiTheme.Warning;
        _tutorialNextButton.AccentColor = UiTheme.Success;
        _tutorialCloseButton.AccentColor = UiTheme.Warning;

        _doubleCheckOrderButton.TextAlignment = UiTextAlignment.Left;
        _expediteOrderButton.TextAlignment = UiTextAlignment.Left;
        ConfigureButtonAlignment();
    }

    private void ConfigureButtonAlignment()
    {
        _doubleCheckOrderButton.HorizontalPadding = 14;
        _expediteOrderButton.HorizontalPadding = 14;

        foreach (var button in _foodModifierButtons)
        {
            button.TextAlignment = UiTextAlignment.Left;
            button.HorizontalPadding = 12;
        }

        foreach (var choice in FoodChoices)
        {
            var button = GetFoodButton(choice);
            button.HorizontalPadding = 10;
            button.TextScale = UiTypography.Body;
            button.WrapText = true;
            button.MaxTextLines = 2;
        }

        foreach (var button in _lifeEventOptionButtons)
        {
            button.TextAlignment = UiTextAlignment.Left;
            button.HorizontalPadding = 12;
        }

        foreach (var button in _interviewOptionButtons)
        {
            button.TextAlignment = UiTextAlignment.Left;
            button.HorizontalPadding = 12;
        }

        foreach (var button in _communicationPickButtons.Values)
        {
            button.TextAlignment = UiTextAlignment.Left;
            button.HorizontalPadding = 12;
        }
    }

    private IEnumerable<UiButton> GetOverlayCloseButtons()
    {
        yield return _closeFoodAppButton;
        yield return _closeBankAppButton;
        yield return _closeCommunicationButton;
        yield return _closeFreelanceBoardButton;
        yield return _closeUpgradesButton;
        yield return _closeStatsButton;
        yield return _closeProjectStudioButton;
        yield return _closeApplicationButton;
        yield return _tutorialCloseButton;
    }

    private void AdvanceButtonAnimations(float elapsedSeconds)
    {
        foreach (var button in GetAllButtons())
        {
            button.AdvanceAnimation(elapsedSeconds);
        }
    }

    private IEnumerable<UiButton> GetAllButtons()
    {
        yield return _foodAppButton;
        yield return _freelanceButton;
        yield return _sleepButton;
        yield return _upgradesButton;
        yield return _statsButton;
        yield return _bankAppButton;
        yield return _communicationButton;
        yield return _projectStudioButton;
        yield return _guideButton;
        yield return _newRunButton;
        yield return _menuButton;
        yield return _optionsButton;
        yield return _squashBugButton;
        yield return _applyForJobButton;
        yield return _publishAppButton;
        yield return _restartButton;
        yield return _breakCoinButton;
        yield return _acceptEvictionButton;
        yield return _deskDistractionFocusButton;
        yield return _deskDistractionQuickFixButton;
        yield return _burgerButton;
        yield return _burritoButton;
        yield return _pizzaButton;
        yield return _dumplingsButton;
        yield return _skilletPastaButton;
        yield return _mealPrepChiliButton;
        yield return _doubleCheckOrderButton;
        yield return _expediteOrderButton;
        yield return _confirmFoodOrderButton;
        yield return _closeFoodAppButton;
        yield return _closeBankAppButton;
        yield return _closeFreelanceBoardButton;
        yield return _closeUpgradesButton;
        yield return _closeStatsButton;
        yield return _closeProjectStudioButton;
        yield return _commitFileButton;
        yield return _keepCodingButton;
        yield return _projectTypeButton;
        yield return _projectThemeButton;
        yield return _projectToneButton;
        yield return _projectPlatformButton;
        yield return _projectBusinessButton;
        yield return _projectRerollButton;
        yield return _buyHouseButton;
        yield return _retireButton;
        yield return _openApplicationButton;
        yield return _closeApplicationButton;
        yield return _tutorialBackButton;
        yield return _tutorialNextButton;
        yield return _tutorialCloseButton;

        foreach (var button in _interviewOptionButtons)
        {
            yield return button;
        }

        foreach (var button in _lifeEventOptionButtons)
        {
            yield return button;
        }

        foreach (var button in _foodModifierButtons)
        {
            yield return button;
        }

        foreach (var button in _upgradeButtons.Values)
        {
            yield return button;
        }

        foreach (var button in _freelanceGigButtons.Values)
        {
            yield return button;
        }

        foreach (var button in _communicationPickButtons.Values)
        {
            yield return button;
        }

        foreach (var button in _communicationMessageButtons.Values)
        {
            yield return button;
        }

        foreach (var button in _communicationCallButtons.Values)
        {
            yield return button;
        }
    }

    private IEnumerable<UiButton> GetSidebarActionButtons()
    {
        yield return _foodAppButton;
        yield return _freelanceButton;
        yield return _bankAppButton;
        yield return _upgradesButton;
        yield return _communicationButton;
        yield return _projectStudioButton;
        yield return _sleepButton;
        yield return _statsButton;
    }

    private void CancelAllButtonInteractions()
    {
        foreach (var button in GetAllButtons())
        {
            button.CancelInteraction();
        }
    }

    private bool HandleOverlayScrollbarInput(
        InputSnapshot input,
        OverlayScrollArea area,
        Rectangle trackBounds,
        Rectangle thumbBounds,
        ref float scrollOffset,
        float maxScrollOffset)
    {
        if (maxScrollOffset <= 0f)
        {
            if (_activeScrollbarDrag == area)
            {
                _activeScrollbarDrag = OverlayScrollArea.None;
            }

            return false;
        }

        if (_activeScrollbarDrag == area)
        {
            if (input.LeftDown)
            {
                var thumbTravel = Math.Max(1, trackBounds.Height - thumbBounds.Height);
                var dragRatio = (input.MousePosition.Y - _scrollbarDragStartMouseY) / (float)thumbTravel;
                scrollOffset = Math.Clamp(
                    _scrollbarDragStartOffset + (dragRatio * maxScrollOffset),
                    0f,
                    maxScrollOffset);
                UpdateButtons();
            }

            if (input.LeftReleased || !input.LeftDown)
            {
                _activeScrollbarDrag = OverlayScrollArea.None;
            }

            return true;
        }

        if (!input.LeftClicked)
        {
            return false;
        }

        if (thumbBounds.Contains(input.MousePosition) || trackBounds.Contains(input.MousePosition))
        {
            var thumbTravel = Math.Max(1, trackBounds.Height - thumbBounds.Height);
            var thumbTop = Math.Clamp(
                input.MousePosition.Y - trackBounds.Y - (thumbBounds.Height / 2f),
                0f,
                thumbTravel);
            var clickRatio = thumbTop / thumbTravel;
            scrollOffset = Math.Clamp(clickRatio * maxScrollOffset, 0f, maxScrollOffset);
            _activeScrollbarDrag = area;
            _scrollbarDragStartMouseY = input.MousePosition.Y;
            _scrollbarDragStartOffset = scrollOffset;
            CancelAllButtonInteractions();
            UpdateButtons();
            return true;
        }

        return false;
    }

    private void HandleWorkspaceInput(InputSnapshot input)
    {
        if (_guideButton.Update(input))
        {
            OpenTutorial();
            _audio.PlayButtonClick();
            return;
        }

        if (_restartButton.Update(input))
        {
            RestartCurrentRun();
            _audio.PlayButtonClick();
            return;
        }

        if (_newRunButton.Update(input))
        {
            StartFreshRun();
            _audio.PlayButtonClick();
            return;
        }

        if (_menuButton.Update(input))
        {
            _audio.PlayButtonClick();
            _showMainMenu();
            return;
        }

        if (_state.ActiveCatInterruption is not null)
        {
            if (_deskDistractionFocusButton.Update(input))
            {
                if (_simulation.SpendFocusOnDistraction(_state))
                {
                    _audio.PlayButtonClick();
                }
                else
                {
                    _audio.PlayFailure();
                }

                return;
            }

            if (_deskDistractionQuickFixButton.Update(input))
            {
                if (_simulation.QuickResolveDistraction(_state))
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
            _communicationOpen = false;
            _commitPromptOpen = false;
            _projectStudioOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _statsOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_publishAppButton.Update(input))
        {
            if (_simulation.ApplyAction(_state, PlayerAction.PublishApp))
            {
                _commitPromptOpen = false;
                _audio.PlayButtonClick();
            }
            else if (PortfolioWorkspace.IsCurrentBatchComplete(_state) &&
                     _state.VersionControl.PendingChangeLines > 0)
            {
                _commitPromptOpen = true;
                _audio.PlayAlert();
            }
            else
            {
                _audio.PlayFailure();
            }

            return;
        }

        if (_foodAppButton.Update(input))
        {
            _foodAppOpen = true;
            _bankAppOpen = false;
            _communicationOpen = false;
            _commitPromptOpen = false;
            _projectStudioOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _statsOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_freelanceButton.Update(input))
        {
            _freelanceBoardOpen = true;
            _foodAppOpen = false;
            _bankAppOpen = false;
            _communicationOpen = false;
            _commitPromptOpen = false;
            _projectStudioOpen = false;
            _upgradesOpen = false;
            _statsOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_bankAppButton.Update(input))
        {
            _bankAppOpen = true;
            _foodAppOpen = false;
            _communicationOpen = false;
            _commitPromptOpen = false;
            _projectStudioOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _statsOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_communicationButton.Update(input))
        {
            _communicationOpen = true;
            _foodAppOpen = false;
            _bankAppOpen = false;
            _commitPromptOpen = false;
            _projectStudioOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _statsOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_projectStudioButton.Update(input))
        {
            _projectStudioOpen = true;
            _foodAppOpen = false;
            _bankAppOpen = false;
            _communicationOpen = false;
            _commitPromptOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _statsOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_upgradesButton.Update(input))
        {
            _upgradesOpen = true;
            _foodAppOpen = false;
            _bankAppOpen = false;
            _communicationOpen = false;
            _commitPromptOpen = false;
            _projectStudioOpen = false;
            _freelanceBoardOpen = false;
            _statsOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_statsButton.Update(input))
        {
            _statsOpen = true;
            _foodAppOpen = false;
            _bankAppOpen = false;
            _communicationOpen = false;
            _commitPromptOpen = false;
            _projectStudioOpen = false;
            _freelanceBoardOpen = false;
            _upgradesOpen = false;
            _jobApplicationOpen = false;
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

        if (_state.ActiveTechDebtBug is not null &&
            _state.IsRealisticMode &&
            _state.ActiveCatInterruption is null &&
            input.IsLeftClickInside(_debugSnippetBounds))
        {
            if (_debugHighlightBounds.Contains(input.MousePosition) &&
                _simulation.ApplyAction(_state, PlayerAction.SquashBug))
            {
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

        foreach (var choice in FoodChoices)
        {
            if (!GetFoodButton(choice).Update(input))
            {
                continue;
            }

            SetSelectedFood(choice);
            return;
        }

        var modifierOptions = _simulation.GetFoodOrderModifiers(_state, _selectedFood);
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
            return;
        }

        if (_buyHouseButton.Update(input))
        {
            var moved = !_state.HasApartment
                ? _simulation.MoveToApartment(_state)
                : _simulation.BuyHouse(_state);
            if (moved)
            {
                _audio.PlaySuccess();
            }
            else
            {
                _audio.PlayFailure();
            }

            return;
        }

        if (_retireButton.Update(input))
        {
            if (_simulation.Retire(_state))
            {
                _bankAppOpen = false;
                _audio.PlaySuccess();
            }
            else
            {
                _audio.PlayFailure();
            }
        }
    }

    private void HandleCommunicationInput(InputSnapshot input)
    {
        if (HandleOverlayScrollbarInput(
                input,
                OverlayScrollArea.Communication,
                _communicationScrollbarTrackBounds,
                _communicationScrollbarThumbBounds,
                ref _communicationScrollOffset,
                _communicationMaxScrollOffset))
        {
            return;
        }

        if (_communicationMaxScrollOffset > 0f &&
            input.ScrollWheelDelta != 0 &&
            _communicationBounds.Contains(input.MousePosition))
        {
            _communicationScrollOffset = Math.Clamp(
                _communicationScrollOffset - (input.ScrollWheelDelta * 0.36f),
                0f,
                _communicationMaxScrollOffset);
            UpdateButtons();
        }

        if (_closeCommunicationButton.Update(input))
        {
            _communicationOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        foreach (var contact in _state.KnownContacts)
        {
            if (_communicationPickButtons[contact.Id].Update(input))
            {
                _selectedCommunicationContactId = contact.Id;
                _audio.PlayButtonClick();
                return;
            }
        }

        var selectedContact = GetSelectedCommunicationContact();
        if (selectedContact is null)
        {
            return;
        }

        if (_communicationMessageButtons[selectedContact.Id].Update(input))
        {
            if (_simulation.MessageContact(_state, selectedContact.Id))
            {
                _audio.PlayButtonClick();
            }
            else
            {
                _audio.PlayFailure();
            }

            return;
        }

        if (_communicationCallButtons[selectedContact.Id].Update(input))
        {
            if (_simulation.CallContact(_state, selectedContact.Id))
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

    private void HandleCommitPromptInput(InputSnapshot input)
    {
        if (_commitFileButton.Update(input))
        {
            if (_simulation.CommitChanges(_state))
            {
                _commitPromptOpen = false;
                _audio.PlayButtonClick();
            }
            else
            {
                _audio.PlayFailure();
            }

            return;
        }

        if (_keepCodingButton.Update(input))
        {
            _commitPromptOpen = false;
            _audio.PlayButtonClick();
        }
    }

    private void UpdateCommitPromptState()
    {
        if (_state.Status != RunStatus.InProgress ||
            _state.FirstCoinDecisionPending ||
            _simulation.HasPendingLifeEvent(_state) ||
            _state.VersionControl.PendingChangeLines <= 0)
        {
            _commitPromptOpen = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(_state.RecentCompletedFileName))
        {
            return;
        }

        if (string.Equals(_lastCommitPromptedFileName, _state.RecentCompletedFileName, StringComparison.Ordinal))
        {
            return;
        }

        _lastCommitPromptedFileName = _state.RecentCompletedFileName;
        _commitPromptOpen = true;
        _foodAppOpen = false;
        _bankAppOpen = false;
        _communicationOpen = false;
        _projectStudioOpen = false;
        _freelanceBoardOpen = false;
        _upgradesOpen = false;
        _jobApplicationOpen = false;
    }

    private void HandleProjectStudioInput(InputSnapshot input)
    {
        if (HandleOverlayScrollbarInput(
                input,
                OverlayScrollArea.ProjectStudio,
                _projectStudioScrollbarTrackBounds,
                _projectStudioScrollbarThumbBounds,
                ref _projectStudioScrollOffset,
                _projectStudioMaxScrollOffset))
        {
            return;
        }

        if (_projectStudioMaxScrollOffset > 0f &&
            input.ScrollWheelDelta != 0 &&
            _projectStudioBounds.Contains(input.MousePosition))
        {
            _projectStudioScrollOffset = Math.Clamp(
                _projectStudioScrollOffset - (input.ScrollWheelDelta * 0.36f),
                0f,
                _projectStudioMaxScrollOffset);
            UpdateButtons();
        }

        if (_closeProjectStudioButton.Update(input))
        {
            _projectStudioOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_projectTypeButton.Update(input))
        {
            PlayProjectStudioResult(_simulation.AdvanceProjectBlueprintField(_state, ProjectPlanField.ProductType));
            return;
        }

        if (_projectThemeButton.Update(input))
        {
            PlayProjectStudioResult(_simulation.AdvanceProjectBlueprintField(_state, ProjectPlanField.Theme));
            return;
        }

        if (_projectToneButton.Update(input))
        {
            PlayProjectStudioResult(_simulation.AdvanceProjectBlueprintField(_state, ProjectPlanField.Tone));
            return;
        }

        if (_projectPlatformButton.Update(input))
        {
            PlayProjectStudioResult(_simulation.AdvanceProjectBlueprintField(_state, ProjectPlanField.Platform));
            return;
        }

        if (_projectBusinessButton.Update(input))
        {
            PlayProjectStudioResult(_simulation.AdvanceProjectBlueprintField(_state, ProjectPlanField.BusinessModel));
            return;
        }

        if (_projectRerollButton.Update(input))
        {
            PlayProjectStudioResult(_simulation.RerollProjectBlueprint(_state));
        }
    }

    private void PlayProjectStudioResult(bool applied)
    {
        if (applied)
        {
            _audio.PlayButtonClick();
        }
        else
        {
            _audio.PlayFailure();
        }
    }

    private void HandleFreelanceBoardInput(InputSnapshot input)
    {
        if (_state.ActiveFreelanceGig is null &&
            HandleOverlayScrollbarInput(
                input,
                OverlayScrollArea.FreelanceBoard,
                _freelanceBoardScrollbarTrackBounds,
                _freelanceBoardScrollbarThumbBounds,
                ref _freelanceBoardScrollOffset,
                _freelanceBoardMaxScrollOffset))
        {
            return;
        }

        if (_state.ActiveFreelanceGig is null &&
            _freelanceBoardMaxScrollOffset > 0f &&
            input.ScrollWheelDelta != 0 &&
            _freelanceBoardBounds.Contains(input.MousePosition))
        {
            var scrollStep = FreelanceCardHeight + FreelanceCardGap;
            _freelanceBoardScrollOffset = Math.Clamp(
                _freelanceBoardScrollOffset - (Math.Sign(input.ScrollWheelDelta) * scrollStep),
                0f,
                _freelanceBoardMaxScrollOffset);
            UpdateButtons();
        }

        if (_closeFreelanceBoardButton.Update(input))
        {
            _freelanceBoardOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_state.ActiveFreelanceGig is not null)
        {
            if (!input.IsLeftClickInside(_freelanceGigEditorBounds))
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

            var worked = _simulation.WorkOnFreelanceGig(_state);
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

        foreach (var type in FreelanceGigOrder)
        {
            var button = _freelanceGigButtons[type];
            if (!button.Update(input))
            {
                continue;
            }

            var taken = _simulation.BeginFreelanceGig(_state, type);
            if (taken)
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

    private void HandleTutorialInput(InputSnapshot input)
    {
        if (_tutorialCloseButton.Update(input))
        {
            _tutorialOpen = false;
            _audio.PlayButtonClick();
            return;
        }

        if (_tutorialBackButton.Update(input))
        {
            _tutorialPageIndex = Math.Max(0, _tutorialPageIndex - 1);
            _audio.PlayButtonClick();
            return;
        }

        if (_tutorialNextButton.Update(input))
        {
            if (_tutorialPageIndex >= GetTutorialPageCount() - 1)
            {
                _tutorialOpen = false;
            }
            else
            {
                _tutorialPageIndex++;
            }

            _audio.PlayButtonClick();
        }
    }

    private void HandleUpgradesInput(InputSnapshot input)
    {
        if (HandleOverlayScrollbarInput(
                input,
                OverlayScrollArea.Upgrades,
                _upgradesScrollbarTrackBounds,
                _upgradesScrollbarThumbBounds,
                ref _upgradesScrollOffset,
                _upgradesMaxScrollOffset))
        {
            return;
        }

        if (_upgradesMaxScrollOffset > 0f &&
            input.ScrollWheelDelta != 0 &&
            _upgradesBounds.Contains(input.MousePosition))
        {
            _upgradesScrollOffset = Math.Clamp(
                _upgradesScrollOffset - (input.ScrollWheelDelta * 0.36f),
                0f,
                _upgradesMaxScrollOffset);
            UpdateButtons();
        }

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

    private void HandleStatsInput(InputSnapshot input)
    {
        if (HandleOverlayScrollbarInput(
                input,
                OverlayScrollArea.Stats,
                _statsScrollbarTrackBounds,
                _statsScrollbarThumbBounds,
                ref _statsScrollOffset,
                _statsMaxScrollOffset))
        {
            return;
        }

        if (_statsMaxScrollOffset > 0f &&
            input.ScrollWheelDelta != 0 &&
            _statsBounds.Contains(input.MousePosition))
        {
            _statsScrollOffset = Math.Clamp(
                _statsScrollOffset - (input.ScrollWheelDelta * 0.36f),
                0f,
                _statsMaxScrollOffset);
            UpdateButtons();
        }

        if (_closeStatsButton.Update(input))
        {
            _statsOpen = false;
            _audio.PlayButtonClick();
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

        UiPanel.Draw(spriteBatch, _pixel, _editorPanelBounds, UiTheme.WithOpacity(UiTheme.PanelFill, 0.97f), UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _editorViewportBounds, UiTheme.EditorFill, UiTheme.EditorBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _sidebarBounds, UiTheme.WithOpacity(UiTheme.PanelFill, 0.97f), UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _logBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        spriteBatch.Draw(_pixel, new Rectangle(_editorPanelBounds.X + 1, _editorPanelBounds.Y + 1, _editorPanelBounds.Width - 2, 4), UiTheme.Accent);
        spriteBatch.Draw(_pixel, new Rectangle(_sidebarBounds.X + 1, _sidebarBounds.Y + 1, _sidebarBounds.Width - 2, 4), UiTheme.WithOpacity(UiTheme.Success, 0.9f));

        UiLabel.Draw(spriteBatch, _font, "Micro Dev Workspace", new Vector2(_editorPanelBounds.X + 18, _editorPanelBounds.Y + 12), UiTheme.TextPrimary, UiTypography.Title);
        var clockText = $"Day {_state.Day}  {_state.ClockText}";
        var clockTextWidth = _font.MeasureString(clockText).X * UiTypography.Section;
        UiLabel.Draw(spriteBatch, _font, clockText, new Vector2(_editorPanelBounds.Right - 18 - clockTextWidth, _editorPanelBounds.Y + 16), UiTheme.TextMuted, UiTypography.Section);

        DrawCodeEditor(spriteBatch);
        DrawSidebar(spriteBatch);
        DrawEventLog(spriteBatch);
    }

    private void DrawBackdrop(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y), UiTheme.DesktopBackground);
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, 216), UiTheme.WithOpacity(UiTheme.DesktopGlow, 0.34f));
        spriteBatch.Draw(_pixel, new Rectangle(54, 36, 520, 692), UiTheme.WithOpacity(UiTheme.AccentDim, 0.1f));
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, 2), UiTheme.Accent);

        for (var index = 0; index < 6; index++)
        {
            var y = 106 + (index * 132);
            spriteBatch.Draw(_pixel, new Rectangle(0, y, _virtualResolution.X, 1), UiTheme.WithOpacity(UiTheme.AccentDim, 0.16f));
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
        DrawFittedLabel(
            spriteBatch,
            program.FileName,
            new Vector2(tabBounds.X + 14, tabBounds.Y + 7),
            tabBounds.Width - 28,
            UiTheme.TextPrimary,
            0.8f,
            0.7f);

        var titleY = _editorViewportBounds.Y + 12;
        var projectMeta = finitePortfolio
            ? $"Release {releaseNumber}  |  File {Math.Min(fileIndex, totalPrograms)}/{totalPrograms}"
            : $"File {fileIndex}/ENDLESS";
        var projectMetaSize = _font.MeasureString(projectMeta) * SmallScale;

        UiLabel.Draw(
            spriteBatch,
            _font,
            projectMeta,
            new Vector2(headerRight - projectMetaSize.X, titleY + 3),
            UiTheme.TextMuted,
            SmallScale);

        DrawFirstCoinFrame(spriteBatch);

        var descriptionY = titleY + 12;
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
            var completionBanner = new Rectangle(contentX, codeTop, contentWidth, 52);
            UiPanel.Draw(spriteBatch, _pixel, completionBanner, new Color(27, 61, 44), UiTheme.Success, 1);
            UiLabel.Draw(
                spriteBatch,
                _font,
                $"File Complete: {_state.RecentCompletedFileName}",
                new Vector2(completionBanner.X + 12, completionBanner.Y + 7),
                UiTheme.Success,
                0.76f);
            UiLabel.Draw(
                spriteBatch,
                _font,
                _state.VersionControl.PendingChangeLines > 0
                    ? $"{_state.VersionControl.PendingChangeLines} dirty LoC waiting for a commit."
                    : "Committed cleanly. Ready for the next file.",
                new Vector2(completionBanner.X + 12, completionBanner.Y + 28),
                UiTheme.TextPrimary,
                0.62f);
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

        if (_debugSnippetBounds != Rectangle.Empty && _state.ActiveTechDebtBug is not null)
        {
            DrawDebugSnippet(spriteBatch, _state.ActiveTechDebtBug);
            codeTop = _debugSnippetBounds.Bottom + 14;
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
            var distraction = _state.ActiveCatInterruption;
            UiPanel.Draw(spriteBatch, _pixel, _catOverlayBounds, new Color(80, 45, 24, 232), UiTheme.CatAccent, 3);
            UiLabel.Draw(spriteBatch, _font, distraction.Title, new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 18), UiTheme.CatAccent, 1.0f);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                distraction.Description,
                new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 56),
                _catOverlayBounds.Width - 48,
                UiTheme.TextPrimary,
                0.76f,
                2f,
                3);
            UiLabel.Draw(spriteBatch, _font, $"Manual clears remaining: {distraction.PatsRemaining}", new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 136), UiTheme.TextPrimary, 0.78f);
            UiLabel.Draw(spriteBatch, _font, $"Leaves in: {FormatRemainingTime(distraction.RemainingInGameMinutes)}", new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 164), UiTheme.TextMuted, 0.76f);
            UiLabel.Draw(spriteBatch, _font, $"Deletion risk: {distraction.LinesDeletionPenalty} LoC", new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 192), UiTheme.Warning, 0.76f);
            UiLabel.Draw(spriteBatch, _font, $"Phantom bugs typed: {distraction.PhantomBugCount}", new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 220), UiTheme.Danger, 0.72f);
            UiLabel.Draw(spriteBatch, _font, $"Gibberish lines on screen: {distraction.GibberishLinesTyped}", new Vector2(_catOverlayBounds.X + 24, _catOverlayBounds.Y + 246), UiTheme.CatAccent, 0.72f);
            _deskDistractionFocusButton.Draw(spriteBatch, _pixel, _font);
            _deskDistractionQuickFixButton.Draw(spriteBatch, _pixel, _font);
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

    private void UpdateDebugSnippetLayout()
    {
        _debugSnippetBounds = Rectangle.Empty;
        _debugHighlightBounds = Rectangle.Empty;

        var bug = _state.ActiveTechDebtBug;
        if (bug is null || bug.CodeLines.Length == 0)
        {
            return;
        }

        var program = PortfolioWorkspace.GetCurrentProgram(_state);
        var headerRight = _coinFrameBounds.Left - 16;
        var contentX = _editorViewportBounds.X + 18;
        var contentWidth = Math.Max(360, headerRight - contentX);
        var titleY = _editorViewportBounds.Y + 12;
        var descriptionY = titleY + 12;
        var descriptionHeight = UiTextBlock.MeasureWrappedHeight(_font, program.Description, contentWidth, BodyScale, 2f, 2);
        var progressY = descriptionY + descriptionHeight + 14f;
        var progressStripY = progressY + 24f;
        var codeTop = (int)MathF.Ceiling(progressStripY + 20f);

        if (!string.IsNullOrEmpty(_state.RecentCompletedFileName))
        {
            codeTop += 64;
        }

        codeTop += 60;

        var lineHeight = (_font.LineSpacing * CodeScale) + 5f;
        var snippetHeight = 56 + (int)MathF.Ceiling(bug.CodeLines.Length * lineHeight);
        _debugSnippetBounds = new Rectangle(contentX, codeTop, contentWidth, snippetHeight);
        _debugHighlightBounds = GetDebugHighlightBounds(bug, _debugSnippetBounds);
    }

    private Rectangle GetDebugHighlightBounds(ActiveTechDebtBug bug, Rectangle snippetBounds)
    {
        if (snippetBounds == Rectangle.Empty ||
            bug.CodeLines.Length == 0 ||
            bug.HighlightLineIndex < 0 ||
            bug.HighlightLineIndex >= bug.CodeLines.Length)
        {
            return Rectangle.Empty;
        }

        const float lineNumberWidth = 36f;
        const float snippetPadding = 12f;
        var lineHeight = (_font.LineSpacing * CodeScale) + 5f;
        var codeX = snippetBounds.X + snippetPadding + lineNumberWidth + 14f;
        var lineY = snippetBounds.Y + 40f + (bug.HighlightLineIndex * lineHeight);
        var line = bug.CodeLines[bug.HighlightLineIndex];
        var safeHighlightIndex = Math.Clamp(bug.HighlightStartIndex, 0, line.Length);
        var prefix = line[..safeHighlightIndex];
        var prefixWidth = _font.MeasureString(prefix).X * CodeScale;
        var highlightWidth = bug.HighlightIsInsertion
            ? Math.Max(10f, (_font.MeasureString(bug.HighlightToken).X * CodeScale) + 8f)
            : Math.Max(12f, (_font.MeasureString(bug.HighlightToken).X * CodeScale) + 6f);
        var highlightHeight = (_font.LineSpacing * CodeScale) + 4f;

        return new Rectangle(
            (int)MathF.Round(codeX + prefixWidth - 2f),
            (int)MathF.Round(lineY - 1f),
            (int)MathF.Round(highlightWidth),
            (int)MathF.Round(highlightHeight));
    }

    private void DrawDebugSnippet(SpriteBatch spriteBatch, ActiveTechDebtBug bug)
    {
        UiPanel.Draw(spriteBatch, _pixel, _debugSnippetBounds, new Color(26, 31, 40), UiTheme.Danger, 1);
        spriteBatch.Draw(
            _pixel,
            new Rectangle(_debugSnippetBounds.X + 1, _debugSnippetBounds.Y + 1, _debugSnippetBounds.Width - 2, 3),
            UiTheme.Danger);

        var title = _state.IsRealisticMode
            ? "Highlighted Compile Error"
            : "Live Compile Error";
        UiLabel.Draw(
            spriteBatch,
            _font,
            title,
            new Vector2(_debugSnippetBounds.X + 12, _debugSnippetBounds.Y + 8),
            UiTheme.Danger,
            0.72f);

        var statusText = _state.IsRealisticMode
            ? $"Click the red token. {FormatRemainingTime(bug.RemainingInGameMinutes)} left"
            : $"{FormatRemainingTime(bug.RemainingInGameMinutes)} left";
        UiLabel.Draw(
            spriteBatch,
            _font,
            statusText,
            new Vector2(_debugSnippetBounds.X + 12, _debugSnippetBounds.Y + 26),
            UiTheme.TextMuted,
            0.56f);

        const float lineNumberWidth = 36f;
        const float snippetPadding = 12f;
        var lineHeight = (_font.LineSpacing * CodeScale) + 5f;
        var lineY = _debugSnippetBounds.Y + 40f;
        var codeX = _debugSnippetBounds.X + snippetPadding + lineNumberWidth + 14f;
        var baseLineNumber = 12;

        for (var index = 0; index < bug.CodeLines.Length; index++)
        {
            var line = bug.CodeLines[index];
            UiLabel.Draw(
                spriteBatch,
                _font,
                $"{baseLineNumber + index,3}",
                new Vector2(_debugSnippetBounds.X + snippetPadding, lineY + 2),
                UiTheme.TextMuted,
                CodeScale);

            spriteBatch.DrawString(
                _font,
                line,
                new Vector2(codeX, lineY),
                GetCodeLineColor(line),
                0f,
                Vector2.Zero,
                CodeScale,
                SpriteEffects.None,
                0f);

            if (index == bug.HighlightLineIndex && _debugHighlightBounds != Rectangle.Empty)
            {
                var fill = _state.IsRealisticMode
                    ? UiTheme.Warning
                    : UiTheme.Danger;
                UiPanel.Draw(spriteBatch, _pixel, _debugHighlightBounds, fill, UiTheme.Danger, 1);

                var safeHighlightIndex = Math.Clamp(bug.HighlightStartIndex, 0, line.Length);
                var prefix = line[..safeHighlightIndex];
                var tokenPosition = new Vector2(
                    _debugHighlightBounds.X + 3,
                    lineY);

                if (bug.HighlightIsInsertion)
                {
                    spriteBatch.DrawString(
                        _font,
                        bug.HighlightToken,
                        tokenPosition,
                        UiTheme.PanelFill,
                        0f,
                        Vector2.Zero,
                        CodeScale,
                        SpriteEffects.None,
                        0f);
                }
                else
                {
                    var prefixWidth = _font.MeasureString(prefix).X * CodeScale;
                    spriteBatch.DrawString(
                        _font,
                        bug.HighlightToken,
                        new Vector2(codeX + prefixWidth, lineY),
                        UiTheme.PanelFill,
                        0f,
                        Vector2.Zero,
                        CodeScale,
                        SpriteEffects.None,
                        0f);
                }
            }

            lineY += lineHeight;
        }
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
        var (displayLines, startingLine) = GetAutoFollowCodeWindow(visibleLines, maxVisibleLines);

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

    private static (IReadOnlyList<string> Lines, int StartingLineNumber) GetAutoFollowCodeWindow(IReadOnlyList<string> visibleLines, int maxVisibleLines)
    {
        if (visibleLines.Count <= maxVisibleLines)
        {
            return (visibleLines, 1);
        }

        var startingIndex = Math.Max(0, visibleLines.Count - maxVisibleLines);
        return (visibleLines.Skip(startingIndex).ToArray(), startingIndex + 1);
    }

    private void DrawSidebar(SpriteBatch spriteBatch)
    {
        var contentX = _sidebarBounds.X + 16;
        UiLabel.Draw(spriteBatch, _font, "Desk Dashboard", new Vector2(contentX, _sidebarBounds.Y + 14), UiTheme.TextPrimary, 0.96f);
        _menuButton.Draw(spriteBatch, _pixel, _font);
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
        DrawFittedLabel(spriteBatch, value, new Vector2(bounds.X + 10, bounds.Y + 28), bounds.Width - 20, valueColor, 0.94f, 0.6f);
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
        var stripBounds = GetSidebarStatusStripBounds();
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
            message = $"{_state.ActiveCatInterruption.Title} is chewing through the desk for {FormatRemainingTime(_state.ActiveCatInterruption.RemainingInGameMinutes)}. {_state.ActiveCatInterruption.PhantomBugCount} bug bursts and {_state.ActiveCatInterruption.GibberishLinesTyped} gibberish lines are already on screen.";
        }
        else if (_state.ActiveFoodDelivery is not null)
        {
            fill = new Color(36, 46, 67);
            border = UiTheme.Accent;
            textColor = UiTheme.Accent;
            var foodChoice = _state.ActiveFoodDelivery.Choice;
            var prepMode = _simulation.IsHomeCooked(foodChoice)
                ? "cooking"
                : _state.ActiveFoodDelivery.Expedited ? "rushing over" : "on the way";
            message = $"{_simulation.GetFoodOption(_state, foodChoice).Name} is {prepMode}. ETA {FormatRemainingTime(_state.ActiveFoodDelivery.RemainingInGameMinutes)}.";
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
        else if (PortfolioWorkspace.IsCurrentBatchComplete(_state))
        {
            fill = new Color(84, 64, 26);
            border = UiTheme.Warning;
            textColor = UiTheme.Warning;
            message = $"The build is done, but {_state.VersionControl.PendingChangeLines} dirty lines still need a commit before the release can ship.";
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
        else if (_simulation.IsInCorporateOfficeHours(_state))
        {
            fill = new Color(72, 46, 58);
            border = UiTheme.Warning;
            textColor = UiTheme.Warning;
            message = $"Office hours are live. { _state.BossName } and the rest of the floor are actively draining sanity while you hold the day together.";
        }
        else if (_state.HasApartment && !_state.HasHouse)
        {
            fill = new Color(27, 55, 46);
            border = UiTheme.Success;
            textColor = UiTheme.Success;
            message = "You made it out of the basement. Banking now points toward the house goal instead of just raw survival.";
        }
        else if (_state.HasHouse && !_state.HasRetired)
        {
            fill = new Color(27, 55, 46);
            border = UiTheme.Success;
            textColor = UiTheme.Success;
            message = "House purchased. Banking now holds the retirement finish line once the savings are there.";
        }
        else if (_state.GameplayMode == GameplayLoopMode.Corporate)
        {
            fill = new Color(66, 44, 58);
            border = UiTheme.Warning;
            textColor = UiTheme.Warning;
            message = $"{_state.BossName} is a {ProceduralRunContent.GetBossDispositionLabel(_state.BossDisposition).ToLowerInvariant()} boss. Corporate standing is {_state.CorporateStanding}, salary is steady, and the micromanagement is the price of it.";
        }
        else if (_state.GameplayMode == GameplayLoopMode.Founder && !string.IsNullOrWhiteSpace(_state.StudioName))
        {
            fill = new Color(56, 44, 24);
            border = UiTheme.CoinAccent;
            textColor = UiTheme.CoinAccent;
            message = $"{_state.StudioName} is still bootstrapping. Founder Mode is pure grassroots business-building: ship work, freelance for cash, and stay solvent long enough to scale.";
        }
        else if (_state.GameplayMode == GameplayLoopMode.Indie)
        {
            fill = new Color(27, 55, 46);
            border = UiTheme.Success;
            textColor = UiTheme.Success;
            message = "Indie studio rhythm is active. It is more self-directed and goal-driven, but lighter structure means food, sleep, and focus discipline matter more.";
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
            message = _state.IsRealisticMode
                ? "Code quality is draining. The highlighted compiler error in the editor is the fix target."
                : "Code quality is draining until the current bug gets fixed.";
        }

        UiPanel.Draw(spriteBatch, _pixel, stripBounds, fill, border, 2);

        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            message,
            new Vector2(stripBounds.X + 12, stripBounds.Y + 9),
            stripBounds.Width - 24,
            textColor,
            BodyScale,
            1f,
            2);
    }

    private void DrawActionButtons(SpriteBatch spriteBatch)
    {
        UiLabel.Draw(spriteBatch, _font, "Actions", new Vector2(_sidebarBounds.X + 16, GetSidebarActionsHeaderY()), UiTheme.TextPrimary, 0.88f);
        _foodAppButton.Draw(spriteBatch, _pixel, _font);
        _freelanceButton.Draw(spriteBatch, _pixel, _font);
        _bankAppButton.Draw(spriteBatch, _pixel, _font);
        _upgradesButton.Draw(spriteBatch, _pixel, _font);
        _communicationButton.Draw(spriteBatch, _pixel, _font);
        _projectStudioButton.Draw(spriteBatch, _pixel, _font);
        _sleepButton.Draw(spriteBatch, _pixel, _font);
        _statsButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawAlertsPanel(SpriteBatch spriteBatch)
    {
        UiPanel.Draw(spriteBatch, _pixel, _alertsPanelBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, "Alerts & Inbox", new Vector2(_alertsPanelBounds.X + 12, _alertsPanelBounds.Y + 10), UiTheme.TextPrimary, 0.82f);

        if (_state.ActiveTechDebtBug is null &&
            _state.ActiveJobListing is null &&
            _state.ActiveJobApplication is null &&
            !PortfolioWorkspace.IsCurrentBatchComplete(_state) &&
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
            (PortfolioWorkspace.IsCurrentBatchComplete(_state) || _simulation.IsPortfolioPublishReady(_state) || _simulation.HasPublishedApps(_state)))
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
        DrawFittedLabel(
            spriteBatch,
            "Tech Debt",
            new Vector2(_techDebtCardBounds.X + 10, _techDebtCardBounds.Y + 8),
            titleWidth,
            UiTheme.Danger,
            0.72f,
            0.66f);

        var summary = _state.IsRealisticMode
            ? $"{bug.Summary} {bug.CompilerHint} Click the highlighted token in the editor. {FormatRemainingTime(bug.RemainingInGameMinutes)} left."
            : $"{bug.Summary} {bug.CompilerHint} {FormatRemainingTime(bug.RemainingInGameMinutes)} left.";
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
        DrawFittedLabel(
            spriteBatch,
            $"{listing.Title}  |  {listing.CompanyName}",
            new Vector2(_jobListingCardBounds.X + 10, _jobListingCardBounds.Y + 8),
            titleWidth,
            UiTheme.Success,
            0.72f,
            0.62f);
        var requirements =
            $"{listing.TechStack}  |  {(listing.OfferMode == GameplayLoopMode.Indie ? "Indie studio" : "Corporate team")}  |  {_simulation.GetResumeTrackLabel(listing.ResumeTrack)} proof {_simulation.GetResumeProof(_state, listing.ResumeTrack)}/{listing.RequiredResumeProof}  |  {FormatRemainingTime(listing.RemainingInGameMinutes)} left";
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
        DrawFittedLabel(
            spriteBatch,
            application.ListingTitle,
            new Vector2(_applicationCardBounds.X + 10, _applicationCardBounds.Y + 8),
            titleWidth,
            UiTheme.Accent,
            0.72f,
            0.62f);

        var revealedLines = _simulation.GetVisibleJobApplicationLines(_state)
            .Count(line => !string.IsNullOrWhiteSpace(line));
        var totalLines = application.CodeLines.Count(line => !string.IsNullOrWhiteSpace(line));
        var body = application.TakeHomeComplete
            ? $"Interview {Math.Min(application.CurrentQuestionIndex + 1, application.Questions.Count)}/{application.Questions.Count}. Correct {application.CorrectAnswers}/{application.MinimumCorrectAnswers} needed."
            : $"Take-home {revealedLines}/{totalLines} lines for {application.CompanyName}. Prep notes {application.PrepPoints}. Need {application.MinimumCorrectAnswers}/{application.Questions.Count} correct in the interview.";
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
        var batchComplete = PortfolioWorkspace.IsCurrentBatchComplete(_state);
        var fill = publishReady
            ? new Color(28, 60, 49)
            : batchComplete
                ? new Color(70, 48, 30)
                : new Color(35, 45, 64);
        var border = publishReady
            ? UiTheme.Success
            : batchComplete ? UiTheme.Warning : UiTheme.Accent;
        var accent = publishReady
            ? UiTheme.Success
            : batchComplete ? UiTheme.Warning : UiTheme.Accent;

        UiPanel.Draw(spriteBatch, _pixel, _publishCardBounds, fill, border, 2);

        var title = publishReady
            ? $"Release {_state.PublishedAppCount + 1} Ready"
            : batchComplete
                ? "Release Blocked"
                : "Published Apps";
        var titleWidth = _publishCardBounds.Width - 20 - (_publishAppButton.Bounds == Rectangle.Empty ? 0 : _publishAppButton.Bounds.Width + 8);
        DrawFittedLabel(
            spriteBatch,
            title,
            new Vector2(_publishCardBounds.X + 10, _publishCardBounds.Y + 8),
            titleWidth,
            accent,
            0.72f,
            0.66f);

        if (_publishAppButton.Bounds != Rectangle.Empty)
        {
            _publishAppButton.Draw(spriteBatch, _pixel, _font);
        }

        var nextSaleText = _state.NextPublishedAppSaleDeskMinute < double.PositiveInfinity
            ? $"Next payout in {FormatRemainingTime(Math.Max(0, _state.NextPublishedAppSaleDeskMinute - _state.DeskMinutesElapsed))}."
            : "Storefront payouts will start rolling in once this build is live.";
        var body = publishReady
            ? $"All {PortfolioWorkspace.GetProgramCount(_state)} files for the current release are complete. Publish now for a randomized ${_simulation.Config.PublishAppFundsMin:0}-${_simulation.Config.PublishAppFundsMax:0}, then roll straight into the next snippet pack. {nextSaleText}"
            : batchComplete
                ? $"The release itself is finished, but {_state.VersionControl.PendingChangeLines} dirty lines are still sitting outside a commit. Lock them in, then ship."
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

        if (_runControlsBounds != Rectangle.Empty)
        {
            DrawRunControlsPanel(spriteBatch);
        }

        var maxWidth = (_runControlsBounds == Rectangle.Empty
                ? _logBounds.Right - 18
                : _runControlsBounds.X - 16) - (_logBounds.X + 18);
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

    private void DrawRunControlsPanel(SpriteBatch spriteBatch)
    {
        UiPanel.Draw(spriteBatch, _pixel, _runControlsBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, "Run Controls", new Vector2(_runControlsBounds.X + 12, _runControlsBounds.Y + 10), UiTheme.TextPrimary, 0.74f);

        var seedModeText = _state.RunSeed > 0
            ? $"Seed {_state.RunSeed}  |  Restart repeats this run  |  New Run rerolls from settings."
            : "Restart repeats this run. New Run rerolls from settings.";
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            seedModeText,
            new Vector2(_runControlsBounds.X + 12, _runControlsBounds.Y + 32),
            _runControlsBounds.Width - 24,
            UiTheme.TextMuted,
            0.54f,
            1f,
            2);

        _guideButton.Draw(spriteBatch, _pixel, _font);
        _restartButton.Draw(spriteBatch, _pixel, _font);
        _newRunButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawFoodAppOverlay(SpriteBatch spriteBatch)
    {
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        UiPanel.Draw(spriteBatch, _pixel, _foodAppBounds, UiTheme.PanelFill, UiTheme.Accent, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_foodAppBounds.X + 1, _foodAppBounds.Y + 1, _foodAppBounds.Width - 2, 4), UiTheme.Accent);

        var activeDelivery = _state.ActiveFoodDelivery;
        var currentFoodChoice = activeDelivery?.Choice ?? _selectedFood;
        var option = _simulation.GetFoodOption(_state, currentFoodChoice);
        var orderOptions = _simulation.GetFoodOrderModifiers(_state, currentFoodChoice);
        IReadOnlyCollection<FoodOrderModifier> selectedModifiers = activeDelivery is not null
            ? activeDelivery.SelectedModifiers
            : _selectedFoodModifiers;
        var reviewReceipt = activeDelivery?.ReviewReceipt ?? _doubleCheckOrder;
        var expedited = activeDelivery?.Expedited ?? _expediteFoodDelivery;
        var penaltyMinutes = _simulation.GetFoodOrderPenaltyMinutes(option.Choice, selectedModifiers, reviewReceipt);
        var introText = activeDelivery is null
            ? "Mix delivery and home cooking to manage time, cash, and recovery. Takeout lands faster, but cooking is cheaper, steadier, and better for long micromanagement runs."
            : "Your meal loop is already active. Keep an eye on the ETA here or from the desk strip while the timer runs down.";
        var mealIntroText = activeDelivery is null
            ? "Delivery speed versus home-cooked stability."
            : "Meal locked until the timer resolves.";
        var orderStateText = activeDelivery is null
            ? penaltyMinutes <= 0
                ? "Details locked. This meal should land clean with no sluggish penalty."
                : penaltyMinutes < option.SluggishMinutesWhenUnchecked
                    ? $"Partial cleanup. Expected sluggishness falls to {FormatRemainingTime(penaltyMinutes)} after delivery."
                    : $"Messy order. Expect the full {FormatRemainingTime(option.SluggishMinutesWhenUnchecked)} sluggish hit when it arrives."
            : $"{option.Name} is {(_simulation.IsHomeCooked(activeDelivery.Choice) ? "cooking" : activeDelivery.Expedited ? "expedited" : "on standard delivery")} with {FormatRemainingTime(activeDelivery.RemainingInGameMinutes)} left before the stats land.";
        var summaryDescriptionText = activeDelivery is null
            ? option.Description
            : $"{option.Description} The driver will apply the focus and sanity bump only when the food actually arrives.";
        var summarySupportText = activeDelivery is null
            ? _simulation.IsHomeCooked(option.Choice)
                ? "Home cooking is slower, cheaper, and calmer than takeout. It still clears hunger only when the meal is ready."
                : "Delivery clears hunger on arrival. Sleep is slower, but it is still the only full reset."
            : $"Expected on arrival: sanity {option.SanityGain:+0;-0;0}, hunger reset, and {(penaltyMinutes <= 0 ? "no sluggishness" : $"{FormatRemainingTime(penaltyMinutes)} sluggishness")}.";

        DrawOverlayHeaderLabel(
            spriteBatch,
            "Food + Kitchen",
            new Vector2(_foodAppBounds.X + 24, _foodAppBounds.Y + 18),
            _closeFoodAppButton.Bounds,
            _foodAppBounds.Right - 24,
            UiTheme.TextPrimary,
            1.0f,
            0.86f);
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
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            mealIntroText,
            new Vector2(_foodChoicePanelBounds.X + 14, _foodChoicePanelBounds.Y + 38),
            _foodChoicePanelBounds.Width - 28,
            UiTheme.TextMuted,
            0.62f,
            2f,
            2);
        foreach (var choice in FoodChoices)
        {
            GetFoodButton(choice).Draw(spriteBatch, _pixel, _font);
        }

        UiPanel.Draw(spriteBatch, _pixel, _foodSummaryPanelBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        _doubleCheckOrderButton.Draw(spriteBatch, _pixel, _font);
        _expediteOrderButton.Draw(spriteBatch, _pixel, _font);
        _confirmFoodOrderButton.Draw(spriteBatch, _pixel, _font);
        _closeFoodAppButton.Draw(spriteBatch, _pixel, _font);

        DrawFittedLabel(
            spriteBatch,
            $"Selected meal: {option.Name}",
            new Vector2(_foodSummaryPanelBounds.X + 16, _foodSummaryPanelBounds.Y + 14),
            _foodSummaryPanelBounds.Width - 32,
            UiTheme.TextPrimary,
            0.82f,
            0.62f);
        DrawFittedLabel(
            spriteBatch,
            activeDelivery is null
                ? $"Cost: ${_simulation.GetFoodTotalCost(_state, option.Choice, expedited):0}   ETA: {FormatRemainingTime(_simulation.GetFoodDeliveryDuration(_state, option.Choice, expedited))}   Focus on arrival: +{option.FocusGain:0}"
                : $"Paid: ${activeDelivery.TotalFundsCost:0}   ETA: {FormatRemainingTime(activeDelivery.RemainingInGameMinutes)}   Mode: {(_simulation.IsHomeCooked(activeDelivery.Choice) ? "Home Cook" : activeDelivery.Expedited ? "Expedited" : "Delivery")}",
            new Vector2(_foodSummaryPanelBounds.X + 16, _foodSummaryPanelBounds.Y + 42),
            _foodSummaryPanelBounds.Width - 32,
            UiTheme.Warning,
            0.74f,
            0.58f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            summaryDescriptionText,
            new Vector2(_foodSummaryPanelBounds.X + 16, _foodSummaryPanelBounds.Y + 76),
            _foodSummaryPanelBounds.Width - 32,
            UiTheme.TextMuted,
            0.68f,
            2f,
            3);
        var supportTextY = _foodSummaryPanelBounds.Y + 76 + UiTextBlock.MeasureWrappedHeight(
            _font,
            summaryDescriptionText,
            _foodSummaryPanelBounds.Width - 32,
            0.68f,
            2f,
            3) + 10;
        var supportTextHeight = UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            summarySupportText,
            new Vector2(_foodSummaryPanelBounds.X + 16, supportTextY),
            _foodSummaryPanelBounds.Width - 32,
            UiTheme.TextMuted,
            0.62f,
            2f,
            3);
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

        var noteTextHeight = UiTextBlock.MeasureWrappedHeight(
            _font,
            orderStateText,
            _foodSummaryPanelBounds.Width - 48,
            0.62f,
            1f,
            3);
        var noteBounds = new Rectangle(
            _foodSummaryPanelBounds.X + 14,
            (int)Math.Ceiling(supportTextY + supportTextHeight + 10),
            _foodSummaryPanelBounds.Width - 28,
            Math.Max(34, (int)Math.Ceiling(noteTextHeight) + 12));
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

        DrawOverlayHeaderLabel(
            spriteBatch,
            "Banking",
            new Vector2(_bankAppBounds.X + 24, _bankAppBounds.Y + 18),
            _closeBankAppButton.Bounds,
            _bankAppBounds.Right - 24,
            UiTheme.TextPrimary,
            1.0f,
            0.86f);
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

        var accountBounds = _bankAccountBounds;
        var rentBounds = _bankRentBounds;
        var houseBounds = _bankHouseBounds;
        var retirementBounds = _bankRetirementBounds;
        var coinBounds = _bankCoinBounds;
        var ledgerBounds = _bankLedgerBounds;

        UiPanel.Draw(spriteBatch, _pixel, accountBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, rentBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, houseBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, retirementBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(
            spriteBatch,
            _pixel,
            coinBounds,
            _state.HasFirstCoin ? new Color(48, 44, 28) : UiTheme.PanelMuted,
            _state.HasFirstCoin ? UiTheme.CoinAccent : UiTheme.PanelBorder,
            2);
        UiPanel.Draw(spriteBatch, _pixel, ledgerBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        var dailyBill = _simulation.GetCurrentDailyBillAmount(_state);
        var runwayBills = dailyBill <= 0
            ? 0
            : Math.Max(0, (int)Math.Floor(_state.Funds / dailyBill));
        var billMinutesRemaining = Math.Max(1d, SimulationConfig.MinutesPerDay - _state.TimeOfDayMinutes);

        UiLabel.Draw(spriteBatch, _font, "Available Cash", new Vector2(accountBounds.X + 16, accountBounds.Y + 14), UiTheme.TextMuted, 0.72f);
        DrawFittedLabel(spriteBatch, $"${_state.Funds:0}", new Vector2(accountBounds.X + 16, accountBounds.Y + 40), accountBounds.Width - 32, UiTheme.Warning, 1.16f, 0.76f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"Roughly {runwayBills} rent hit{(runwayBills == 1 ? string.Empty : "s")} of runway at the current bill rate.",
            new Vector2(accountBounds.X + 16, accountBounds.Y + 78),
            accountBounds.Width - 32,
            UiTheme.TextMuted,
            0.68f,
            2f,
            3);

        UiLabel.Draw(spriteBatch, _font, "Rent Pressure", new Vector2(rentBounds.X + 16, rentBounds.Y + 14), UiTheme.TextMuted, 0.72f);
        DrawFittedLabel(
            spriteBatch,
            $"Next bill in {FormatRemainingTime(billMinutesRemaining)}",
            new Vector2(rentBounds.X + 16, rentBounds.Y + 40),
            rentBounds.Width - 32,
            UiTheme.TextPrimary,
            0.88f,
            0.62f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"Daily bill: -${dailyBill:0}. Difficulty: {_state.Difficulty}. Seed: {_state.RunSeed}.",
            new Vector2(rentBounds.X + 16, rentBounds.Y + 78),
            rentBounds.Width - 32,
            UiTheme.TextMuted,
            0.68f,
            2f,
            2);

        var housingHeadline = GetHousingTrackHeadline();
        var housingBody = GetHousingTrackDescription();
        UiLabel.Draw(spriteBatch, _font, "Housing Track", new Vector2(houseBounds.X + 16, houseBounds.Y + 14), UiTheme.TextMuted, 0.72f);
        DrawFittedLabel(
            spriteBatch,
            housingHeadline,
            new Vector2(houseBounds.X + 16, houseBounds.Y + 40),
            houseBounds.Width - 32,
            _state.HasHouse ? UiTheme.Success : UiTheme.TextPrimary,
            0.86f,
            0.62f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            housingBody,
            new Vector2(houseBounds.X + 16, houseBounds.Y + 68),
            houseBounds.Width - 32,
            UiTheme.TextMuted,
            0.64f,
            2f,
            4);
        _buyHouseButton.Draw(spriteBatch, _pixel, _font);

        var retirementHeadline = GetRetirementGoalHeadline();
        var retirementBody = GetRetirementGoalDescription();
        UiLabel.Draw(spriteBatch, _font, "Retirement Goal", new Vector2(retirementBounds.X + 16, retirementBounds.Y + 14), UiTheme.TextMuted, 0.72f);
        DrawFittedLabel(
            spriteBatch,
            retirementHeadline,
            new Vector2(retirementBounds.X + 16, retirementBounds.Y + 40),
            retirementBounds.Width - 32,
            _state.HasRetired ? UiTheme.Success : UiTheme.TextPrimary,
            0.86f,
            0.62f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            retirementBody,
            new Vector2(retirementBounds.X + 16, retirementBounds.Y + 68),
            retirementBounds.Width - 32,
            UiTheme.TextMuted,
            0.64f,
            2f,
            4);
        _retireButton.Draw(spriteBatch, _pixel, _font);

        UiLabel.Draw(spriteBatch, _font, "Emergency Buffer", new Vector2(coinBounds.X + 16, coinBounds.Y + 14), _state.HasFirstCoin ? UiTheme.CoinAccent : UiTheme.TextMuted, 0.78f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            GetEmergencyBufferDescription(),
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
            if (transactionY + height > ledgerBounds.Bottom - 14)
            {
                break;
            }

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
        }

        _closeBankAppButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawCommunicationOverlay(SpriteBatch spriteBatch)
    {
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        UiPanel.Draw(spriteBatch, _pixel, _communicationBounds, UiTheme.PanelFill, UiTheme.Success, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_communicationBounds.X + 1, _communicationBounds.Y + 1, _communicationBounds.Width - 2, 4), UiTheme.Success);

        DrawOverlayHeaderLabel(
            spriteBatch,
            "Communication",
            new Vector2(_communicationBounds.X + 24, _communicationBounds.Y + 18),
            _closeCommunicationButton.Bounds,
            _communicationBounds.Right - 24,
            UiTheme.TextPrimary,
            1.0f,
            0.86f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Text or call the people already in your orbit. Dates build the relationship track, friends steady sanity, and mentor calls sharpen the stories and prep that keep the career moving.",
            new Vector2(_communicationBounds.X + 24, _communicationBounds.Y + 52),
            _communicationBounds.Width - 48,
            UiTheme.TextMuted,
            0.74f,
            2f,
            3);

        var listPanelBounds = new Rectangle(
            _communicationViewportBounds.X - 12,
            _communicationViewportBounds.Y - 12,
            _communicationViewportBounds.Width + 28,
            _communicationViewportBounds.Height + 24);
        UiPanel.Draw(spriteBatch, _pixel, listPanelBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _communicationDetailBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, "People Met", new Vector2(listPanelBounds.X + 14, listPanelBounds.Y + 12), UiTheme.TextPrimary, 0.76f);
        UiLabel.Draw(spriteBatch, _font, "Selected Contact", new Vector2(_communicationDetailBounds.X + 18, _communicationDetailBounds.Y + 12), UiTheme.TextPrimary, 0.76f);

        if (_state.KnownContacts.Count == 0)
        {
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                "Nobody is in the contacts list yet. Keep building, ship work, meet a match, or stay in motion long enough for people to actually enter the orbit.",
                new Vector2(_communicationDetailBounds.X + 18, _communicationDetailBounds.Y + 46),
                _communicationDetailBounds.Width - 36,
                UiTheme.TextMuted,
                0.72f,
                2f,
                5);

            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                "Once you meet someone, pick them from the left list to text or call them here.",
                new Vector2(_communicationDetailBounds.X + 18, _communicationDetailBounds.Y + 138),
                _communicationDetailBounds.Width - 36,
                UiTheme.Accent,
                0.68f,
                2f,
                3);

            _closeCommunicationButton.Draw(spriteBatch, _pixel, _font);
            return;
        }

        var graphicsDevice = spriteBatch.GraphicsDevice;
        var previousScissor = graphicsDevice.ScissorRectangle;
        spriteBatch.End();
        graphicsDevice.ScissorRectangle = _communicationViewportBounds;
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp, rasterizerState: UiRenderStates.ScissorRasterizer);

        foreach (var contact in _state.KnownContacts)
        {
            if (!_communicationCardBounds.TryGetValue(contact.Id, out var cardBounds))
            {
                continue;
            }

            var isPartner = _state.HasFoundLove && string.Equals(_state.PartnerName, contact.Name, StringComparison.Ordinal);
            var roleAccent = contact.Role == SocialContactRole.Date || isPartner
                ? UiTheme.Success
                : contact.Role == SocialContactRole.Mentor
                    ? UiTheme.Warning
                    : UiTheme.Accent;
            var roleLabel = GetCommunicationRoleLabel(contact, isPartner);
            var progressLabel = GetCommunicationProgressLabel(contact, isPartner);
            var progressFit = UiTextBlock.FitText(_font, progressLabel, 92f, 0.58f, 0.5f);
            var progressWidth = _font.MeasureString(progressFit.Text).X * progressFit.Scale;
            var roleWidth = Math.Max(70f, cardBounds.Width - 134f);
            var selected = string.Equals(contact.Id, _selectedCommunicationContactId, StringComparison.Ordinal);

            UiPanel.Draw(
                spriteBatch,
                _pixel,
                cardBounds,
                selected ? new Color(36, 58, 78) : UiTheme.PanelMuted,
                selected ? UiTheme.Accent : UiTheme.PanelBorder,
                2);
            UiLabel.Draw(spriteBatch, _font, contact.Name, new Vector2(cardBounds.X + 14, cardBounds.Y + 12), UiTheme.TextPrimary, 0.8f);
            UiLabel.Draw(
                spriteBatch,
                _font,
                UiTextBlock.TrimToWidth(_font, roleLabel, roleWidth, 0.64f),
                new Vector2(cardBounds.X + 14, cardBounds.Y + 36),
                roleAccent,
                0.64f);
            UiLabel.Draw(
                spriteBatch,
                _font,
                progressFit.Text,
                new Vector2(cardBounds.Right - 14 - progressWidth, cardBounds.Y + 36),
                UiTheme.TextMuted,
                progressFit.Scale);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                $"Texts {contact.MessageCount}  |  Calls {contact.CallCount}",
                new Vector2(cardBounds.X + 14, cardBounds.Y + 58),
                cardBounds.Width - 28,
                UiTheme.TextMuted,
                0.6f,
                1f,
                1);
        }

        spriteBatch.End();
        graphicsDevice.ScissorRectangle = previousScissor;
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp, rasterizerState: UiRenderStates.ScissorRasterizer);

        if (_communicationMaxScrollOffset > 0f)
        {
            UiPanel.Draw(
                spriteBatch,
                _pixel,
                _communicationScrollbarTrackBounds,
                UiTheme.WithOpacity(UiTheme.PanelMuted, 0.78f),
                UiTheme.PanelBorder,
                1);
            UiPanel.Draw(
                spriteBatch,
                _pixel,
                _communicationScrollbarThumbBounds,
                UiTheme.Accent,
                UiTheme.TextPrimary,
                1);
        }

        var selectedContact = GetSelectedCommunicationContact();
        if (selectedContact is not null)
        {
            var isPartner = _state.HasFoundLove && string.Equals(_state.PartnerName, selectedContact.Name, StringComparison.Ordinal);
            var roleAccent = selectedContact.Role == SocialContactRole.Date || isPartner
                ? UiTheme.Success
                : selectedContact.Role == SocialContactRole.Mentor
                    ? UiTheme.Warning
                    : UiTheme.Accent;
            var prompt = ProceduralRunContent.GetCommunicationPrompt(_state.RunSeed, selectedContact.Id, selectedContact.Name, selectedContact.Role, selectedContact.BondProgress, isPartner);
            var roleLabel = GetCommunicationRoleLabel(selectedContact, isPartner);
            var progressLabel = GetCommunicationProgressLabel(selectedContact, isPartner);

            UiLabel.Draw(spriteBatch, _font, selectedContact.Name, new Vector2(_communicationDetailBounds.X + 18, _communicationDetailBounds.Y + 42), UiTheme.TextPrimary, 0.92f);
            UiLabel.Draw(spriteBatch, _font, roleLabel, new Vector2(_communicationDetailBounds.X + 18, _communicationDetailBounds.Y + 72), roleAccent, 0.68f);
            UiLabel.Draw(spriteBatch, _font, progressLabel, new Vector2(_communicationDetailBounds.X + 18, _communicationDetailBounds.Y + 96), UiTheme.TextMuted, 0.62f);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                prompt,
                new Vector2(_communicationDetailBounds.X + 18, _communicationDetailBounds.Y + 128),
                _communicationDetailBounds.Width - 36,
                UiTheme.TextPrimary,
                0.7f,
                2f,
                4);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                $"Messages {selectedContact.MessageCount}  |  Calls {selectedContact.CallCount}  |  Bond {selectedContact.BondProgress}",
                new Vector2(_communicationDetailBounds.X + 18, _communicationDetailBounds.Y + 222),
                _communicationDetailBounds.Width - 36,
                UiTheme.TextMuted,
                0.62f,
                1f,
                2);

            _communicationMessageButtons[selectedContact.Id].Draw(spriteBatch, _pixel, _font);
            _communicationCallButtons[selectedContact.Id].Draw(spriteBatch, _pixel, _font);
        }

        _closeCommunicationButton.Draw(spriteBatch, _pixel, _font);
    }

    private string GetCommunicationRoleLabel(SocialContact contact, bool isPartner)
    {
        if (isPartner)
        {
            return "Partner";
        }

        return contact.Role switch
        {
            SocialContactRole.Date => "Dating",
            SocialContactRole.Mentor => "Mentor",
            _ => "Friend",
        };
    }

    private string GetCommunicationProgressLabel(SocialContact contact, bool isPartner)
    {
        if (isPartner)
        {
            return contact.BondProgress >= 7
                ? "Steady"
                : "Growing";
        }

        return contact.Role switch
        {
            SocialContactRole.Date => $"Bond {contact.BondProgress}/{_simulation.Config.RelationshipProgressNeededForLove}",
            SocialContactRole.Mentor => contact.BondProgress switch
            {
                >= 6 => "Trusted",
                >= 3 => "Growing",
                _ => "Warm Intro",
            },
            _ => contact.BondProgress switch
            {
                >= 6 => "Close",
                >= 3 => "Solid",
                _ => "New",
            },
        };
    }

    private void EnsureValidCommunicationSelection()
    {
        if (_state.KnownContacts.Count == 0)
        {
            _selectedCommunicationContactId = null;
            return;
        }

        if (string.IsNullOrWhiteSpace(_selectedCommunicationContactId) ||
            _state.KnownContacts.All(contact => !string.Equals(contact.Id, _selectedCommunicationContactId, StringComparison.Ordinal)))
        {
            _selectedCommunicationContactId = _state.KnownContacts[0].Id;
        }
    }

    private SocialContact? GetSelectedCommunicationContact()
    {
        EnsureValidCommunicationSelection();
        if (string.IsNullOrWhiteSpace(_selectedCommunicationContactId))
        {
            return null;
        }

        return _state.KnownContacts.FirstOrDefault(contact => string.Equals(contact.Id, _selectedCommunicationContactId, StringComparison.Ordinal));
    }

    private void DrawCommitPromptOverlay(SpriteBatch spriteBatch)
    {
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        UiPanel.Draw(spriteBatch, _pixel, _commitPromptBounds, UiTheme.PanelFill, UiTheme.Accent, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_commitPromptBounds.X + 1, _commitPromptBounds.Y + 1, _commitPromptBounds.Width - 2, 4), UiTheme.Accent);

        var dirtyLines = _state.VersionControl.PendingChangeLines;
        var completedFiles = Math.Max(1, _simulation.GetUncommittedCompletedFileCount(_state));
        var lastCommit = string.IsNullOrWhiteSpace(_state.VersionControl.LastCommitSummary)
            ? "No commit yet."
            : $"Last commit: {_state.VersionControl.LastCommitSummary}.";

        DrawOverlayHeaderLabel(
            spriteBatch,
            "Commit Checkpoint",
            new Vector2(_commitPromptBounds.X + 24, _commitPromptBounds.Y + 18),
            _keepCodingButton.Bounds,
            _commitPromptBounds.Right - 24,
            UiTheme.TextPrimary,
            1.0f,
            0.86f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"{_state.RecentCompletedFileName ?? "A file"} just finished. Dirty work now sits at {dirtyLines} LoC across {completedFiles} completed file{(completedFiles == 1 ? string.Empty : "s")}. Small commits are faster to explain, and desk chaos can wipe everything back to the last commit.",
            new Vector2(_commitPromptBounds.X + 24, _commitPromptBounds.Y + 56),
            _commitPromptBounds.Width - 48,
            UiTheme.TextMuted,
            0.74f,
            2f,
            4);

        var bodyBounds = new Rectangle(_commitPromptBounds.X + 24, _commitPromptBounds.Y + 152, _commitPromptBounds.Width - 48, 132);
        UiPanel.Draw(spriteBatch, _pixel, bodyBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, "Git Discipline", new Vector2(bodyBounds.X + 16, bodyBounds.Y + 14), UiTheme.Accent, 0.8f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"{lastCommit} Commit now if you want the current files locked in. Keep coding if you are deliberately batching work, but the next computer freeze or ignored desk interruption can cost the whole uncommitted stack.",
            new Vector2(bodyBounds.X + 16, bodyBounds.Y + 42),
            bodyBounds.Width - 32,
            UiTheme.TextPrimary,
            0.7f,
            2f,
            4);

        _commitFileButton.Draw(spriteBatch, _pixel, _font);
        _keepCodingButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawProjectStudioOverlay(SpriteBatch spriteBatch)
    {
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        UiPanel.Draw(spriteBatch, _pixel, _projectStudioBounds, UiTheme.PanelFill, UiTheme.Success, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_projectStudioBounds.X + 1, _projectStudioBounds.Y + 1, _projectStudioBounds.Width - 2, 4), UiTheme.Success);

        DrawOverlayHeaderLabel(
            spriteBatch,
            "Build Studio",
            new Vector2(_projectStudioBounds.X + 24, _projectStudioBounds.Y + 18),
            _closeProjectStudioButton.Bounds,
            _projectStudioBounds.Right - 24,
            UiTheme.TextPrimary,
            1.0f,
            0.86f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Choose whether the next release is an app or a game, then tune the theme, tone, platform, and business model before typing begins.",
            new Vector2(_projectStudioBounds.X + 24, _projectStudioBounds.Y + 52),
            _projectStudioBounds.Width - 48,
            UiTheme.TextMuted,
            0.76f,
            2f,
            3);

        UiPanel.Draw(spriteBatch, _pixel, _projectStudioViewportBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        var previewPrograms = PortfolioWorkspace.GetProjectPreviewPrograms(_state, 5);
        var contentX = _projectStudioViewportBounds.X;
        var contentWidth = _projectStudioViewportBounds.Width;
        var contentTop = _projectStudioViewportBounds.Y - (int)MathF.Round(_projectStudioScrollOffset);
        var titleBounds = new Rectangle(contentX, contentTop, contentWidth, 112);
        var optionWidth = (contentWidth - 24) / 2;
        var leftX = contentX;
        var rightX = leftX + optionWidth + 24;
        const int rowHeight = 74;
        const int rowGap = 12;
        var topY = titleBounds.Bottom + 18;
        var productBounds = new Rectangle(leftX, topY, optionWidth, rowHeight);
        var themeBounds = new Rectangle(rightX, topY, optionWidth, rowHeight);
        var toneBounds = new Rectangle(leftX, topY + rowHeight + rowGap, optionWidth, rowHeight);
        var platformBounds = new Rectangle(rightX, topY + rowHeight + rowGap, optionWidth, rowHeight);
        var businessBounds = new Rectangle(leftX, topY + ((rowHeight + rowGap) * 2), optionWidth, rowHeight);
        var rerollBounds = new Rectangle(rightX, topY + ((rowHeight + rowGap) * 2), optionWidth, rowHeight);
        var previewBounds = new Rectangle(contentX, businessBounds.Bottom + 18, contentWidth, 176);
        var economyBounds = new Rectangle(contentX, previewBounds.Bottom + 16, contentWidth, 98);
        var lockMessageBounds = new Rectangle(contentX, economyBounds.Bottom + 16, contentWidth, 68);
        var graphicsDevice = spriteBatch.GraphicsDevice;
        var previousScissor = graphicsDevice.ScissorRectangle;
        spriteBatch.End();
        graphicsDevice.ScissorRectangle = _projectStudioViewportBounds;
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp, rasterizerState: UiRenderStates.ScissorRasterizer);

        UiPanel.Draw(spriteBatch, _pixel, titleBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        DrawFittedLabel(
            spriteBatch,
            _state.CurrentProjectBlueprint.Title,
            new Vector2(titleBounds.X + 16, titleBounds.Y + 14),
            titleBounds.Width - 32,
            UiTheme.Success,
            0.96f,
            0.7f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            $"{_state.CurrentProjectBlueprint.Pitch} Publish x{_state.CurrentProjectBlueprint.PublishIncomeMultiplier:0.00}  |  Store x{_state.CurrentProjectBlueprint.SaleIncomeMultiplier:0.00}",
            new Vector2(titleBounds.X + 16, titleBounds.Y + 44),
            titleBounds.Width - 32,
            UiTheme.TextMuted,
            0.68f,
            2f,
            3);

        DrawProjectOptionCard(spriteBatch, productBounds, "Product", _state.CurrentProjectBlueprint.ProductType.ToString(), _projectTypeButton);
        DrawProjectOptionCard(spriteBatch, themeBounds, "Theme", _state.CurrentProjectBlueprint.Theme, _projectThemeButton);
        DrawProjectOptionCard(spriteBatch, toneBounds, "Tone", _state.CurrentProjectBlueprint.Tone, _projectToneButton);
        DrawProjectOptionCard(spriteBatch, platformBounds, "Platform", _state.CurrentProjectBlueprint.Platform, _projectPlatformButton);
        DrawProjectOptionCard(spriteBatch, businessBounds, "Business Model", _state.CurrentProjectBlueprint.BusinessModel, _projectBusinessButton);

        UiPanel.Draw(spriteBatch, _pixel, rerollBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, "Concept Reroll", new Vector2(rerollBounds.X + 16, rerollBounds.Y + 12), UiTheme.TextMuted, 0.72f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Keep the same plan shape, but reroll the actual title and flavor to chase a fresher pitch.",
            new Vector2(rerollBounds.X + 16, rerollBounds.Y + 34),
            rerollBounds.Width - 150,
            UiTheme.TextMuted,
            0.62f,
            2f,
            3);
        _projectRerollButton.Draw(spriteBatch, _pixel, _font);

        UiPanel.Draw(spriteBatch, _pixel, previewBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, "Planned Files", new Vector2(previewBounds.X + 16, previewBounds.Y + 14), UiTheme.TextPrimary, 0.82f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            _state.CurrentProjectBlueprint.ProductType == ProjectProductType.Game
                ? "The current plan now spins up gameplay-facing files so the code pass reads like a real game build."
                : "The current plan now spins up app-facing files so the code pass reads like a real product instead of a random snippet pile.",
            new Vector2(previewBounds.X + 16, previewBounds.Y + 40),
            previewBounds.Width - 32,
            UiTheme.TextMuted,
            0.66f,
            2f,
            2);

        var previewY = previewBounds.Y + 84f;
        foreach (var program in previewPrograms)
        {
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                $"{program.FileName}  |  {program.ProjectName}",
                new Vector2(previewBounds.X + 16, previewY),
                previewBounds.Width - 32,
                UiTheme.Accent,
                0.66f,
                2f,
                1);
            previewY += 22f;
        }

        UiPanel.Draw(spriteBatch, _pixel, economyBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, "Release Economy", new Vector2(economyBounds.X + 16, economyBounds.Y + 14), UiTheme.TextPrimary, 0.82f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            BuildProjectStudioEconomyText(),
            new Vector2(economyBounds.X + 16, economyBounds.Y + 40),
            economyBounds.Width - 32,
            UiTheme.Warning,
            0.66f,
            2f,
            3);

        UiPanel.Draw(spriteBatch, _pixel, lockMessageBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            _simulation.CanEditProjectBlueprint(_state)
                ? "Planning is unlocked because this batch has not started yet. Once you type project code, the plan locks until the next fresh release."
                : "Planning is locked for the current batch. Ship this release and start the next clean batch before you reopen Build Studio.",
            new Vector2(lockMessageBounds.X + 16, lockMessageBounds.Y + 16),
            lockMessageBounds.Width - 32,
            _simulation.CanEditProjectBlueprint(_state) ? UiTheme.TextPrimary : UiTheme.Warning,
            0.66f,
            2f,
            2);

        spriteBatch.End();
        graphicsDevice.ScissorRectangle = previousScissor;
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp, rasterizerState: UiRenderStates.ScissorRasterizer);

        if (_projectStudioMaxScrollOffset > 0f)
        {
            UiPanel.Draw(
                spriteBatch,
                _pixel,
                _projectStudioScrollbarTrackBounds,
                UiTheme.WithOpacity(UiTheme.PanelMuted, 0.78f),
                UiTheme.PanelBorder,
                1);
            UiPanel.Draw(
                spriteBatch,
                _pixel,
                _projectStudioScrollbarThumbBounds,
                UiTheme.Success,
                UiTheme.TextPrimary,
                1);
        }

        _closeProjectStudioButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawProjectOptionCard(SpriteBatch spriteBatch, Rectangle bounds, string label, string value, UiButton button)
    {
        UiPanel.Draw(spriteBatch, _pixel, bounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, label, new Vector2(bounds.X + 16, bounds.Y + 12), UiTheme.TextMuted, 0.72f);
        DrawFittedLabel(spriteBatch, value, new Vector2(bounds.X + 16, bounds.Y + 38), bounds.Width - 148, UiTheme.TextPrimary, 0.82f, 0.62f);
        button.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawFreelanceBoardOverlay(SpriteBatch spriteBatch)
    {
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        UiPanel.Draw(spriteBatch, _pixel, _freelanceBoardBounds, UiTheme.PanelFill, UiTheme.Accent, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_freelanceBoardBounds.X + 1, _freelanceBoardBounds.Y + 1, _freelanceBoardBounds.Width - 2, 4), UiTheme.Accent);

        DrawOverlayHeaderLabel(
            spriteBatch,
            "Freelance Board",
            new Vector2(_freelanceBoardBounds.X + 24, _freelanceBoardBounds.Y + 18),
            _closeFreelanceBoardButton.Bounds,
            _freelanceBoardBounds.Right - 24,
            UiTheme.TextPrimary,
            1.0f,
            0.86f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            _state.ActiveFreelanceGig is null
                ? $"Each contract is seeded off the current build so the board feels less generic. You need at least {_simulation.Config.FreelanceMinimumFocusRequired:0} focus to start any gig, and the money only lands after you finish the contract file."
                : "Contract in progress. Click inside the contract editor to finish the requested file, bank the payout, and clear the board slot.",
            new Vector2(_freelanceBoardBounds.X + 24, _freelanceBoardBounds.Y + 52),
            _freelanceBoardBounds.Width - 48,
            UiTheme.TextMuted,
            0.76f,
            2f,
            3);

        if (_state.ActiveFreelanceGig is not null)
        {
            var gig = _state.ActiveFreelanceGig;
            var summaryBounds = _freelanceGigSummaryBounds;
            UiPanel.Draw(spriteBatch, _pixel, summaryBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
            var summaryWidth = summaryBounds.Width - 32f;
            var metadataText = BuildActiveFreelanceGigMetadataText(gig);
            var briefText = BuildActiveFreelanceGigBriefText(gig);
            var titleScale = UiTextBlock.GetFittedScale(_font, gig.Title, summaryWidth, 0.84f, 0.66f);
            var titleTop = summaryBounds.Y + 16f;
            var metadataTop = titleTop + (_font.LineSpacing * titleScale) + 10f;
            var metadataMaxLines = GetWrappedLineCapacity((summaryBounds.Bottom - 16f) - metadataTop, 0.68f, 2f, 2);
            var metadataHeight = UiTextBlock.MeasureWrappedHeight(_font, metadataText, summaryWidth, 0.68f, 2f, metadataMaxLines);
            var briefTop = metadataTop + metadataHeight + 8f;
            var briefMaxLines = GetWrappedLineCapacity((summaryBounds.Bottom - 16f) - briefTop, 0.64f, 2f, 3);

            DrawFittedLabel(
                spriteBatch,
                gig.Title,
                new Vector2(summaryBounds.X + 16, titleTop),
                summaryWidth,
                UiTheme.TextPrimary,
                0.84f,
                0.66f);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                metadataText,
                new Vector2(summaryBounds.X + 16, metadataTop),
                summaryWidth,
                UiTheme.Accent,
                0.68f,
                2f,
                metadataMaxLines);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                briefText,
                new Vector2(summaryBounds.X + 16, briefTop),
                summaryWidth,
                UiTheme.Warning,
                0.64f,
                2f,
                briefMaxLines);

            UiPanel.Draw(spriteBatch, _pixel, _freelanceGigEditorBounds, UiTheme.EditorFill, UiTheme.EditorBorder, 2);
            UiLabel.Draw(spriteBatch, _font, gig.FileName, new Vector2(_freelanceGigEditorBounds.X + 16, _freelanceGigEditorBounds.Y + 12), UiTheme.Success, 0.82f);
            UiLabel.Draw(
                spriteBatch,
                _font,
                $"{gig.VisibleLineCount}/{gig.CodeLines.Count} lines revealed",
                new Vector2(_freelanceGigEditorBounds.X + 16, _freelanceGigEditorBounds.Y + 40),
                UiTheme.TextMuted,
                0.66f);

            var visibleLines = BuildDisplayedCodeLines(_simulation.GetVisibleFreelanceGigLines(_state));
            var lineHeight = (_font.LineSpacing * CodeScale) + 3f;
            var lineY = _freelanceGigEditorBounds.Y + 72f;
            var codeWidth = _freelanceGigEditorBounds.Width - 64;
            var footer = _state.ActiveCatInterruption is not null
                ? $"{_state.ActiveCatInterruption.Title} is all over the contract. Click the editor {_state.ActiveCatInterruption.PatsRemaining} more times to clear it while the freelance file stays noisy."
                : _simulation.CanWorkOnFreelanceGig(_state)
                    ? "Click inside the contract editor to keep shipping this freelance file."
                    : $"You need enough focus to keep contract work moving. Current focus {_state.Focus:0}, minimum board requirement {_simulation.Config.FreelanceMinimumFocusRequired:0}.";
            var footerMaxLines = GetWrappedLineCapacity(_freelanceGigEditorBounds.Height - 24f, 0.66f, 2f, 2);
            var footerHeight = UiTextBlock.MeasureWrappedHeight(_font, footer, _freelanceGigEditorBounds.Width - 32, 0.66f, 2f, footerMaxLines);
            var footerTop = _freelanceGigEditorBounds.Bottom - 12f - footerHeight;
            var maxVisibleLines = Math.Max(1, (int)Math.Floor((footerTop - lineY - 10f) / lineHeight));
            var (displayLines, startingLine) = GetAutoFollowCodeWindow(visibleLines, maxVisibleLines);
            var codeBottom = footerTop - 8f;
            for (var index = 0; index < displayLines.Count && lineY + lineHeight <= codeBottom; index++)
            {
                var lineNumber = $"{startingLine + index,2}";
                UiLabel.Draw(spriteBatch, _font, lineNumber, new Vector2(_freelanceGigEditorBounds.X + 14, lineY), UiTheme.TextMuted, CodeScale);
                spriteBatch.DrawString(
                    _font,
                    UiTextBlock.TrimToWidth(_font, displayLines[index], codeWidth, CodeScale),
                    new Vector2(_freelanceGigEditorBounds.X + 54, lineY),
                    GetCodeLineColor(displayLines[index]),
                    0f,
                    Vector2.Zero,
                    CodeScale,
                    SpriteEffects.None,
                    0f);
                lineY += lineHeight;
            }

            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                footer,
                new Vector2(_freelanceGigEditorBounds.X + 16, footerTop),
                _freelanceGigEditorBounds.Width - 32,
                _state.ActiveCatInterruption is not null ? UiTheme.CatAccent : UiTheme.TextMuted,
                0.66f,
                2f,
                footerMaxLines);

            _closeFreelanceBoardButton.Draw(spriteBatch, _pixel, _font);
            return;
        }

        UiPanel.Draw(spriteBatch, _pixel, _freelanceBoardViewportBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);
        var graphicsDevice = spriteBatch.GraphicsDevice;
        var previousScissor = graphicsDevice.ScissorRectangle;
        spriteBatch.End();
        graphicsDevice.ScissorRectangle = _freelanceBoardViewportBounds;
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp, rasterizerState: UiRenderStates.ScissorRasterizer);

        foreach (var type in FreelanceGigOrder)
        {
            var gig = _simulation.GetFreelanceGig(_state, type);
            var cardBounds = _freelanceGigCardBounds[type];
            var contentWidth = cardBounds.Width - 168;
            var railBounds = new Rectangle(cardBounds.Right - 132, cardBounds.Y + 12, 118, cardBounds.Height - 24);
            UiPanel.Draw(spriteBatch, _pixel, cardBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
            UiPanel.Draw(spriteBatch, _pixel, railBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 1);
            DrawFittedLabel(
                spriteBatch,
                gig.Name,
                new Vector2(cardBounds.X + 14, cardBounds.Y + 14),
                contentWidth,
                UiTheme.TextPrimary,
                0.8f,
                0.62f);
            UiLabel.Draw(
                spriteBatch,
                _font,
                UiTextBlock.TrimToWidth(
                    _font,
                    $"{gig.DurationMinutes:0}m  |  +${gig.FundsGain:0}  |  -{gig.FocusCost:0} focus  |  -{gig.SanityCost:0} sanity",
                    contentWidth,
                    0.66f),
                new Vector2(cardBounds.X + 14, cardBounds.Y + 38),
                UiTheme.Warning,
                0.64f);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                gig.Description,
                new Vector2(cardBounds.X + 14, cardBounds.Y + 58),
                contentWidth,
                UiTheme.TextMuted,
                0.62f,
                1.5f,
                3);

            var qualityLabel = gig.CodeQualityGain > 0
                ? $"+{gig.CodeQualityGain:0.#} quality"
                : "No quality gain";

            if (gig.CodeQualityGain > 0)
            {
                DrawFittedLabel(
                    spriteBatch,
                    qualityLabel,
                    new Vector2(railBounds.X + 10, railBounds.Y + 10),
                    railBounds.Width - 20,
                    UiTheme.Success,
                    0.66f,
                    0.54f);
            }
            else
            {
                UiTextBlock.DrawWrapped(
                    spriteBatch,
                    _font,
                    qualityLabel,
                    new Vector2(railBounds.X + 10, railBounds.Y + 10),
                    railBounds.Width - 20,
                    UiTheme.TextMuted,
                    0.58f,
                    0f,
                    2);
            }

            _freelanceGigButtons[type].Draw(spriteBatch, _pixel, _font);
        }

        spriteBatch.End();
        graphicsDevice.ScissorRectangle = previousScissor;
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp, rasterizerState: UiRenderStates.ScissorRasterizer);

        if (_freelanceBoardMaxScrollOffset > 0f)
        {
            UiPanel.Draw(
                spriteBatch,
                _pixel,
                _freelanceBoardScrollbarTrackBounds,
                UiTheme.WithOpacity(UiTheme.PanelMuted, 0.78f),
                UiTheme.PanelBorder,
                1);
            UiPanel.Draw(
                spriteBatch,
                _pixel,
                _freelanceBoardScrollbarThumbBounds,
                UiTheme.Accent,
                UiTheme.TextPrimary,
                1);
        }

        _closeFreelanceBoardButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawUpgradesOverlay(SpriteBatch spriteBatch)
    {
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        UiPanel.Draw(spriteBatch, _pixel, _upgradesBounds, UiTheme.PanelFill, UiTheme.Accent, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_upgradesBounds.X + 1, _upgradesBounds.Y + 1, _upgradesBounds.Width - 2, 4), UiTheme.Accent);

        DrawOverlayHeaderLabel(
            spriteBatch,
            "Rig Upgrades",
            new Vector2(_upgradesBounds.X + 24, _upgradesBounds.Y + 18),
            _closeUpgradesButton.Bounds,
            _upgradesBounds.Right - 24,
            UiTheme.TextPrimary,
            1.02f,
            0.86f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Base typing is slow on purpose now. Every upgrade stacks from tier 1 to tier 5, so a +1 throughput tool can grow into +5 on its own if you keep investing in it.",
            new Vector2(_upgradesBounds.X + 24, _upgradesBounds.Y + 52),
            _upgradesBounds.Width - 48,
            UiTheme.TextMuted,
            0.76f,
            2f,
            3);

        var graphicsDevice = spriteBatch.GraphicsDevice;
        var previousScissor = graphicsDevice.ScissorRectangle;
        spriteBatch.End();
        graphicsDevice.ScissorRectangle = _upgradesViewportBounds;
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp, rasterizerState: UiRenderStates.ScissorRasterizer);

        foreach (var definition in EfficiencyUpgradeCatalog.All)
        {
            var cardBounds = _upgradeCardBounds[definition.Type];
            if (cardBounds.Bottom < _upgradesViewportBounds.Top - 4 ||
                cardBounds.Y > _upgradesViewportBounds.Bottom + 4)
            {
                continue;
            }

            var tier = _simulation.GetUpgradeTier(_state, definition.Type);
            var maxTier = _simulation.GetUpgradeMaxTier();
            var purchased = tier > 0;
            var nextCost = _simulation.GetUpgradePurchaseCost(_state, definition.Type);
            var summaryText = $"Per tier: {definition.SummaryEffect}";
            var fill = purchased ? new Color(26, 56, 43) : UiTheme.PanelRaised;
            var border = purchased ? UiTheme.Success : UiTheme.PanelBorder;
            var railBounds = new Rectangle(cardBounds.Right - 132, cardBounds.Y + 12, 118, cardBounds.Height - 24);
            var contentLeft = cardBounds.X + 12;
            var contentWidth = railBounds.X - contentLeft - 12;
            var summaryTop = cardBounds.Y + 38;
            var summaryHeight = UiTextBlock.MeasureWrappedHeight(_font, summaryText, contentWidth, 0.68f, 2f, 2);
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
                summaryText,
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
                $"Tier {tier}/{maxTier}",
                new Vector2(railBounds.X + 10, railBounds.Y + 10),
                purchased ? UiTheme.Success : UiTheme.TextPrimary,
                0.68f);
            DrawFittedLabel(
                spriteBatch,
                tier >= maxTier
                    ? "Maxed"
                    : tier > 0
                        ? $"Next ${nextCost:0}"
                        : $"Cost ${nextCost:0}",
                new Vector2(railBounds.X + 10, _upgradeButtons[definition.Type].Bounds.Y - 18),
                railBounds.Width - 20,
                tier >= maxTier ? UiTheme.TextMuted : UiTheme.Warning,
                0.62f,
                0.56f);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                tier >= maxTier
                    ? $"Maxed out. Live total: {definition.GetTotalEffectSummary(tier)}."
                    : purchased
                        ? $"Live total: {definition.GetTotalEffectSummary(tier)}. Next tier ${nextCost:0}."
                        : $"Buy tier 1 for ${nextCost:0}. This stacks up to 5/5.",
                new Vector2(railBounds.X + 10, railBodyTop),
                railBounds.Width - 20,
                UiTheme.TextMuted,
                0.62f,
                1f,
                railMaxLines);
            _upgradeButtons[definition.Type].Draw(spriteBatch, _pixel, _font);
        }

        spriteBatch.End();
        graphicsDevice.ScissorRectangle = previousScissor;
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp, rasterizerState: UiRenderStates.ScissorRasterizer);

        if (_upgradesMaxScrollOffset > 0f)
        {
            UiPanel.Draw(
                spriteBatch,
                _pixel,
                _upgradesScrollbarTrackBounds,
                UiTheme.WithOpacity(UiTheme.PanelMuted, 0.78f),
                UiTheme.PanelBorder,
                1);
            UiPanel.Draw(
                spriteBatch,
                _pixel,
                _upgradesScrollbarThumbBounds,
                UiTheme.Accent,
                UiTheme.TextPrimary,
                1);
        }

        _closeUpgradesButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawStatsOverlay(SpriteBatch spriteBatch)
    {
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        UiPanel.Draw(spriteBatch, _pixel, _statsBounds, UiTheme.PanelFill, UiTheme.Success, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_statsBounds.X + 1, _statsBounds.Y + 1, _statsBounds.Width - 2, 4), UiTheme.Success);

        DrawOverlayHeaderLabel(
            spriteBatch,
            "Run Stats + Achievements",
            new Vector2(_statsBounds.X + 24, _statsBounds.Y + 18),
            _closeStatsButton.Bounds,
            _statsBounds.Right - 24,
            UiTheme.TextPrimary,
            1.0f,
            0.86f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "A full run breakdown across output, career, money, survival, and life outside the editor. The top summary is fast to scan, and the longer sections below keep every tracked stat in one place.",
            new Vector2(_statsBounds.X + 24, _statsBounds.Y + 52),
            _statsBounds.Width - 48,
            UiTheme.TextMuted,
            0.74f,
            2f,
            3);

        var graphicsDevice = spriteBatch.GraphicsDevice;
        var previousScissor = graphicsDevice.ScissorRectangle;
        spriteBatch.End();
        graphicsDevice.ScissorRectangle = _statsViewportBounds;
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp, rasterizerState: UiRenderStates.ScissorRasterizer);

        var contentX = _statsViewportBounds.X;
        var contentWidth = _statsViewportBounds.Width;
        var contentY = _statsViewportBounds.Y - (int)MathF.Round(_statsScrollOffset);
        const int summaryGap = 16;
        const int summaryHeight = 104;
        var summaryWidth = (contentWidth - (summaryGap * 3)) / 4;
        var unlockedCount = _state.Stats.AchievementUnlockCount;
        var achievementTotal = RunAchievementCatalog.All.Count;
        var latestUnlockTitle = GetLatestUnlockedAchievementTitle();
        var nextAchievement = GetNextLockedAchievement();
        var achievementsDetail = latestUnlockTitle is not null
            ? $"Latest: {latestUnlockTitle}"
            : nextAchievement is not null
                ? $"Next: {nextAchievement.ProgressFactory(_state)}"
                : "Everything in the current catalog is unlocked.";

        DrawStatsSummaryCard(
            spriteBatch,
            new Rectangle(contentX, contentY, summaryWidth, summaryHeight),
            "Run Arc",
            $"Day {_state.Stats.HighestDayReached}  |  {_state.GameplayMode}",
            $"{_state.Status}  |  {FormatRemainingTime(_state.Stats.TotalInGameMinutes)} tracked",
            UiTheme.Success);
        DrawStatsSummaryCard(
            spriteBatch,
            new Rectangle(contentX + summaryWidth + summaryGap, contentY, summaryWidth, summaryHeight),
            "Output",
            $"{_state.Stats.TotalLinesTyped:0} LoC typed",
            $"{_state.Stats.PortfolioFilesCompleted} files  |  {_state.PublishedAppCount} releases",
            UiTheme.Accent);
        DrawStatsSummaryCard(
            spriteBatch,
            new Rectangle(contentX + ((summaryWidth + summaryGap) * 2), contentY, summaryWidth, summaryHeight),
            "Career",
            $"{_state.Stats.JobApplicationsStarted} applications",
            $"{_state.Stats.InterviewsAttempted} interviews  |  {_state.SuccessfulApplications} wins",
            UiTheme.Warning);
        DrawStatsSummaryCard(
            spriteBatch,
            new Rectangle(contentX + ((summaryWidth + summaryGap) * 3), contentY, summaryWidth, summaryHeight),
            "Achievements",
            $"{unlockedCount}/{achievementTotal} unlocked",
            achievementsDetail,
            unlockedCount > 0 ? UiTheme.Success : UiTheme.TextPrimary);

        contentY += summaryHeight + 18;

        var achievementsHeight = MeasureStatsAchievementSectionHeight(contentWidth);
        var achievementsBounds = new Rectangle(contentX, contentY, contentWidth, achievementsHeight);
        DrawStatsAchievementSection(spriteBatch, achievementsBounds);
        contentY = achievementsBounds.Bottom + 16;

        foreach (var section in RunStatsCatalog.Sections)
        {
            var sectionHeight = MeasureStatsSectionHeight(section, contentWidth);
            var sectionBounds = new Rectangle(contentX, contentY, contentWidth, sectionHeight);
            DrawStatsSection(spriteBatch, sectionBounds, section);
            contentY = sectionBounds.Bottom + 14;
        }

        spriteBatch.End();
        graphicsDevice.ScissorRectangle = previousScissor;
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp, rasterizerState: UiRenderStates.ScissorRasterizer);

        if (_statsMaxScrollOffset > 0f)
        {
            UiPanel.Draw(
                spriteBatch,
                _pixel,
                _statsScrollbarTrackBounds,
                UiTheme.WithOpacity(UiTheme.PanelMuted, 0.78f),
                UiTheme.PanelBorder,
                1);
            UiPanel.Draw(
                spriteBatch,
                _pixel,
                _statsScrollbarThumbBounds,
                UiTheme.Success,
                UiTheme.TextPrimary,
                1);
        }

        _closeStatsButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawStatsSummaryCard(
        SpriteBatch spriteBatch,
        Rectangle bounds,
        string title,
        string headline,
        string detail,
        Color headlineColor)
    {
        UiPanel.Draw(spriteBatch, _pixel, bounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, title, new Vector2(bounds.X + 14, bounds.Y + 12), UiTheme.TextMuted, 0.68f);
        DrawFittedLabel(
            spriteBatch,
            headline,
            new Vector2(bounds.X + 14, bounds.Y + 36),
            bounds.Width - 28,
            headlineColor,
            0.8f,
            0.6f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            detail,
            new Vector2(bounds.X + 14, bounds.Y + 62),
            bounds.Width - 28,
            UiTheme.TextMuted,
            0.6f,
            1f,
            2);
    }

    private void DrawStatsAchievementSection(SpriteBatch spriteBatch, Rectangle bounds)
    {
        UiPanel.Draw(spriteBatch, _pixel, bounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, "Achievement Board", new Vector2(bounds.X + 16, bounds.Y + 14), UiTheme.TextPrimary, 0.82f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Unlocked achievements log the major beats of the run, while locked ones show the next threshold to chase.",
            new Vector2(bounds.X + 16, bounds.Y + 40),
            bounds.Width - 32,
            UiTheme.TextMuted,
            0.64f,
            2f,
            2);

        var entryY = bounds.Y + 88f;
        foreach (var achievement in RunAchievementCatalog.All)
        {
            var entryHeight = MeasureStatsAchievementEntryHeight(achievement, bounds.Width - 52);
            var entryBounds = new Rectangle(bounds.X + 14, (int)MathF.Round(entryY), bounds.Width - 28, entryHeight);
            var unlocked = _state.Stats.UnlockedAchievementIds.Contains(achievement.Id);
            var contentX = entryBounds.X + 12;
            var contentWidth = entryBounds.Width - 24;
            var statusText = GetAchievementStatusText(achievement);
            UiPanel.Draw(
                spriteBatch,
                _pixel,
                entryBounds,
                unlocked ? new Color(27, 61, 44) : UiTheme.PanelMuted,
                unlocked ? UiTheme.Success : UiTheme.PanelBorder,
                1);
            var textY = entryBounds.Y + 10f;
            var titleHeight = UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                achievement.Title,
                new Vector2(contentX, textY),
                contentWidth,
                unlocked ? UiTheme.Success : UiTheme.TextPrimary,
                0.74f,
                1f);
            textY += titleHeight + 6f;
            var descriptionHeight = UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                achievement.Description,
                new Vector2(contentX, textY),
                contentWidth,
                UiTheme.TextMuted,
                0.6f,
                1f);
            textY += descriptionHeight + 8f;
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                statusText,
                new Vector2(contentX, textY),
                contentWidth,
                unlocked ? UiTheme.Success : UiTheme.Warning,
                0.58f,
                1f);

            entryY = entryBounds.Bottom + 8;
        }
    }

    private void DrawStatsSection(SpriteBatch spriteBatch, Rectangle bounds, RunStatSectionDefinition section)
    {
        UiPanel.Draw(spriteBatch, _pixel, bounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiLabel.Draw(spriteBatch, _font, section.Title, new Vector2(bounds.X + 16, bounds.Y + 14), UiTheme.TextPrimary, 0.82f);
        var descriptionHeight = UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            section.Description,
            new Vector2(bounds.X + 16, bounds.Y + 40),
            bounds.Width - 32,
            UiTheme.TextMuted,
            0.64f,
            2f,
            2);
        var rowY = bounds.Y + 40f + descriptionHeight + 14f;
        var labelWidth = bounds.Width - 220f;
        foreach (var stat in section.Stats)
        {
            UiLabel.Draw(
                spriteBatch,
                _font,
                UiTextBlock.TrimToWidth(_font, stat.Label, labelWidth, 0.66f),
                new Vector2(bounds.X + 16, rowY),
                UiTheme.TextMuted,
                0.66f);
            DrawFittedLabel(
                spriteBatch,
                stat.ValueFactory(_state),
                new Vector2(bounds.Right - 188, rowY - 1),
                172,
                UiTheme.TextPrimary,
                0.68f,
                0.56f);
            rowY += 24f;
        }
    }

    private void DrawJobApplicationOverlay(SpriteBatch spriteBatch)
    {
        var application = _state.ActiveJobApplication!;
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        UiPanel.Draw(spriteBatch, _pixel, _applicationBounds, UiTheme.PanelFill, UiTheme.Success, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_applicationBounds.X + 1, _applicationBounds.Y + 1, _applicationBounds.Width - 2, 4), UiTheme.Success);

        DrawOverlayHeaderLabel(
            spriteBatch,
            application.ListingTitle,
            new Vector2(_applicationBounds.X + 24, _applicationBounds.Y + 18),
            _closeApplicationButton.Bounds,
            _applicationBounds.Right - 24,
            UiTheme.TextPrimary,
            1.0f,
            0.76f);
        DrawOverlayHeaderLabel(
            spriteBatch,
            application.TechStack,
            new Vector2(_applicationBounds.X + 24, _applicationBounds.Y + 48),
            _closeApplicationButton.Bounds,
            _applicationBounds.Right - 24,
            UiTheme.Accent,
            0.72f,
            0.62f);
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
            var maxVisibleLines = Math.Max(1, (int)Math.Floor((_applicationEditorBounds.Bottom - 44 - lineY) / lineHeight));
            var (displayLines, startingLine) = GetAutoFollowCodeWindow(visibleLines, maxVisibleLines);
            for (var index = 0; index < displayLines.Count && lineY < _applicationEditorBounds.Bottom - 44; index++)
            {
                var lineNumber = $"{startingLine + index,2}";
                UiLabel.Draw(spriteBatch, _font, lineNumber, new Vector2(_applicationEditorBounds.X + 14, lineY), UiTheme.TextMuted, CodeScale);
                spriteBatch.DrawString(
                    _font,
                    UiTextBlock.TrimToWidth(_font, displayLines[index], codeWidth, CodeScale),
                    new Vector2(_applicationEditorBounds.X + 54, lineY),
                    GetCodeLineColor(displayLines[index]),
                    0f,
                    Vector2.Zero,
                    CodeScale,
                    SpriteEffects.None,
                    0f);
                lineY += lineHeight;
            }

            var footer = _state.ActiveCatInterruption is not null
                ? $"{_state.ActiveCatInterruption.Title} is on top of the take-home. Click the editor {_state.ActiveCatInterruption.PatsRemaining} more times to clear it while {_state.ActiveCatInterruption.GibberishLinesTyped} gibberish lines stay smeared across the solution."
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
            IncidentType.BossCheckIn => UiTheme.Warning,
            IncidentType.OnlineMatch or IncidentType.PartnerCheckIn => UiTheme.Success,
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

        if (lifeEvent.Type is IncidentType.OnlineMatch or IncidentType.PartnerCheckIn)
        {
            UiLabel.Draw(
                spriteBatch,
                _font,
                lifeEvent.Type == IncidentType.OnlineMatch
                    ? $"Dating round {lifeEvent.StageIndex + 1}/3  |  score {lifeEvent.ProgressScore}/{Math.Max(1, lifeEvent.TargetScore)}"
                    : _state.HasFoundLove
                        ? $"{_state.PartnerName} | relationship strength {_state.RelationshipProgress}"
                        : $"Relationship progress: {_state.RelationshipProgress}/{_simulation.Config.RelationshipProgressNeededForLove}",
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
            RunStatus.Won => (_state.HasRetired ? "Retired" : "Hired", UiTheme.Success),
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
            "Restart replays this exact seed. New Run rolls forward using the current seed mode from Options.",
            new Vector2(modalBounds.X + 30, modalBounds.Y + 216),
            modalBounds.Width - 60,
            UiTheme.TextMuted,
            0.78f,
            2f,
            2);

        _restartButton.Draw(spriteBatch, _pixel, _font);
        _newRunButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawTutorialOverlay(SpriteBatch spriteBatch)
    {
        var page = GetTutorialPage(_tutorialPageIndex);
        var fullscreen = new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y);
        UiPanel.Draw(spriteBatch, _pixel, fullscreen, UiTheme.Overlay, Color.Transparent, 0);

        UiPanel.Draw(spriteBatch, _pixel, _tutorialBounds, UiTheme.PanelFill, UiTheme.Accent, 3);
        spriteBatch.Draw(_pixel, new Rectangle(_tutorialBounds.X + 1, _tutorialBounds.Y + 1, _tutorialBounds.Width - 2, 4), UiTheme.Accent);

        UiLabel.Draw(spriteBatch, _font, "How To Survive The Run", new Vector2(_tutorialBounds.X + 28, _tutorialBounds.Y + 24), UiTheme.TextPrimary, 1.08f);
        UiLabel.Draw(
            spriteBatch,
            _font,
            $"Page {_tutorialPageIndex + 1}/{GetTutorialPageCount()}  |  Seed {_state.RunSeed}",
            new Vector2(_tutorialBounds.X + 28, _tutorialBounds.Y + 56),
            UiTheme.TextMuted,
            0.68f);
        DrawOverlayHeaderLabel(
            spriteBatch,
            page.Title,
            new Vector2(_tutorialBounds.X + 28, _tutorialBounds.Y + 86),
            _tutorialCloseButton.Bounds,
            _tutorialBounds.Right - 28,
            UiTheme.Accent,
            1.0f,
            0.82f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            page.Intro,
            new Vector2(_tutorialBounds.X + 28, _tutorialBounds.Y + 122),
            _tutorialBounds.Width - 56,
            UiTheme.TextPrimary,
            0.74f,
            2f,
            3);

        var cardsTop = _tutorialBounds.Y + 206;
        var cardsBottom = _tutorialBackButton.Bounds.Y - 18;
        var cardGap = 12;
        var cardCount = Math.Max(1, page.Sections.Length);
        var totalGap = Math.Max(0, cardCount - 1) * cardGap;
        var cardHeight = Math.Max(76, (cardsBottom - cardsTop - totalGap) / cardCount);

        for (var index = 0; index < page.Sections.Length; index++)
        {
            var section = page.Sections[index];
            var bounds = new Rectangle(_tutorialBounds.X + 28, cardsTop + (index * (cardHeight + cardGap)), _tutorialBounds.Width - 56, cardHeight);
            UiPanel.Draw(spriteBatch, _pixel, bounds, index == 0 ? UiTheme.PanelRaised : UiTheme.PanelMuted, UiTheme.PanelBorder, 2);
            UiLabel.Draw(spriteBatch, _font, section.Heading, new Vector2(bounds.X + 16, bounds.Y + 12), UiTheme.TextPrimary, 0.78f);
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                section.Body,
                new Vector2(bounds.X + 16, bounds.Y + 38),
                bounds.Width - 32,
                UiTheme.TextMuted,
                0.66f,
                2f,
                4);
        }

        _tutorialBackButton.Draw(spriteBatch, _pixel, _font);
        _tutorialNextButton.Draw(spriteBatch, _pixel, _font);
        _tutorialCloseButton.Draw(spriteBatch, _pixel, _font);
    }

    private void UpdateButtons()
    {
        const int sidebarPadding = 16;
        const int gap = 10;
        var contentX = _sidebarBounds.X + sidebarPadding;
        var contentWidth = _sidebarBounds.Width - (sidebarPadding * 2);
        var halfWidth = (contentWidth - gap) / 2;
        var actionButtonsY = GetSidebarActionButtonsTop();
        var alertsTop = GetSidebarAlertsTop();
        var activeDelivery = _state.ActiveFoodDelivery;
        var hasActiveFoodDelivery = activeDelivery is not null;
        if (!hasActiveFoodDelivery && !_simulation.AllowsExpeditedDelivery(_selectedFood))
        {
            _expediteFoodDelivery = false;
        }

        var reviewReceiptEnabled = activeDelivery?.ReviewReceipt ?? _doubleCheckOrder;
        var expeditedDeliveryEnabled = activeDelivery?.Expedited ?? _expediteFoodDelivery;

        _foodAppButton.Enabled = _state.Status == RunStatus.InProgress;
        _foodAppButton.Text = hasActiveFoodDelivery ? "Track Order" : "Food + Kitchen";
        _freelanceButton.Enabled = _state.Status == RunStatus.InProgress;
        _freelanceButton.Text = _state.ActiveFreelanceGig is null ? "Freelance Board" : "Resume Gig";
        _bankAppButton.Enabled = _state.Status == RunStatus.InProgress;
        _bankAppButton.Text = "Banking";
        _communicationButton.Enabled = _state.Status == RunStatus.InProgress;
        _communicationButton.Text = "Communication";
        _projectStudioButton.Enabled = _state.Status == RunStatus.InProgress;
        _projectStudioButton.Text = "Build Studio";
        _sleepButton.Enabled = _simulation.CanApplyAction(_state, PlayerAction.Sleep);
        _sleepButton.Text = _simulation.RequiresSleep(_state) ? "Sleep Now" : "Sleep";
        _sleepButton.IsSelected = _simulation.RequiresSleep(_state);
        _upgradesButton.Enabled = _state.Status == RunStatus.InProgress;
        _statsButton.Enabled = true;
        _statsButton.Text = $"Run Stats ({_state.Stats.AchievementUnlockCount}/{RunAchievementCatalog.All.Count})";
        _statsButton.IsSelected = _statsOpen;
        _guideButton.Enabled = _state.Status == RunStatus.InProgress;
        _guideButton.Text = "Guide";
        _newRunButton.Enabled = true;
        _newRunButton.Text = "New Run";
        _menuButton.Enabled = true;
        _menuButton.Text = "Menu";
        _optionsButton.Enabled = true;
        var manualDebugRequired = _state.IsRealisticMode && _state.ActiveTechDebtBug is not null;
        _squashBugButton.Enabled = !manualDebugRequired && _simulation.CanApplyAction(_state, PlayerAction.SquashBug);
        _squashBugButton.Text = manualDebugRequired ? "Editor" : "Fix";
        _applyForJobButton.Enabled = _simulation.CanApplyAction(_state, PlayerAction.ApplyForJob);
        _applyForJobButton.Text = "Start";
        var publishReady = _simulation.CanApplyAction(_state, PlayerAction.PublishApp);
        var batchComplete = PortfolioWorkspace.IsCurrentBatchComplete(_state);
        var canOpenCommitFromPublish = batchComplete && _simulation.CanCommitChanges(_state);
        _publishAppButton.Enabled = publishReady || canOpenCommitFromPublish;
        _publishAppButton.Text = publishReady
            ? "Publish"
            : canOpenCommitFromPublish
                ? "Commit"
                : "Blocked";
        _restartButton.Enabled = true;
        _restartButton.Text = _state.Status == RunStatus.InProgress ? "Restart" : "Restart Run";
        _deskDistractionFocusButton.Enabled = _simulation.CanSpendFocusOnDistraction(_state);
        _deskDistractionQuickFixButton.Enabled = _simulation.CanQuickResolveDistraction(_state);
        _commitFileButton.Enabled = _simulation.CanCommitChanges(_state);
        _keepCodingButton.Enabled = true;
        _buyHouseButton.Enabled = _simulation.CanBuyHouse(_state);
        if (!_state.HasApartment)
        {
            _buyHouseButton.Enabled = _simulation.CanMoveToApartment(_state);
        }
        _retireButton.Enabled = _simulation.CanRetire(_state);
        var canEditProjectBlueprint = _simulation.CanEditProjectBlueprint(_state);
        _projectTypeButton.Enabled = canEditProjectBlueprint;
        _projectThemeButton.Enabled = canEditProjectBlueprint;
        _projectToneButton.Enabled = canEditProjectBlueprint;
        _projectPlatformButton.Enabled = canEditProjectBlueprint;
        _projectBusinessButton.Enabled = canEditProjectBlueprint;
        _projectRerollButton.Enabled = canEditProjectBlueprint;
        _openApplicationButton.Enabled = _state.ActiveJobApplication is not null;
        _openApplicationButton.Text = _state.ActiveJobApplication is not null && _state.ActiveJobApplication.TakeHomeComplete
            ? "Interview"
            : "Continue";
        _projectTypeButton.Text = canEditProjectBlueprint ? "Cycle" : "Locked";
        _projectThemeButton.Text = canEditProjectBlueprint ? "Cycle" : "Locked";
        _projectToneButton.Text = canEditProjectBlueprint ? "Cycle" : "Locked";
        _projectPlatformButton.Text = canEditProjectBlueprint ? "Cycle" : "Locked";
        _projectBusinessButton.Text = canEditProjectBlueprint ? "Cycle" : "Locked";
        _projectRerollButton.Text = canEditProjectBlueprint ? "Reroll" : "Locked";
        _buyHouseButton.Text = _state.HasHouse
            ? "Owned"
            : _state.HasApartment
                ? "Buy House"
                : "Move Out";
        _retireButton.Text = _state.HasRetired ? "Retired" : "Retire";

        _menuButton.Bounds = new Rectangle(_sidebarBounds.Right - 220, _sidebarBounds.Y + 10, 96, 30);
        _optionsButton.Bounds = new Rectangle(_sidebarBounds.Right - 112, _sidebarBounds.Y + 10, 96, 30);
        _foodAppButton.Bounds = new Rectangle(contentX, actionButtonsY, halfWidth, 34);
        _freelanceButton.Bounds = new Rectangle(contentX + halfWidth + gap, actionButtonsY, halfWidth, 34);
        _bankAppButton.Bounds = new Rectangle(contentX, actionButtonsY + 40, halfWidth, 34);
        _upgradesButton.Bounds = new Rectangle(contentX + halfWidth + gap, actionButtonsY + 40, halfWidth, 34);
        _communicationButton.Bounds = new Rectangle(contentX, actionButtonsY + 80, halfWidth, 34);
        _projectStudioButton.Bounds = new Rectangle(contentX + halfWidth + gap, actionButtonsY + 80, halfWidth, 34);
        _sleepButton.Bounds = new Rectangle(contentX, actionButtonsY + 120, contentWidth, 34);
        _statsButton.Bounds = new Rectangle(contentX, actionButtonsY + 160, contentWidth, 34);

        _coinFrameBounds = new Rectangle(_editorViewportBounds.Right - 148, _editorViewportBounds.Y + 6, 128, 86);
        _runControlsBounds = _state.Status == RunStatus.InProgress
            ? new Rectangle(_logBounds.Right - 350, _logBounds.Y + 10, 334, _logBounds.Height - 20)
            : Rectangle.Empty;
        _deskDistractionFocusButton.Bounds = Rectangle.Empty;
        _deskDistractionQuickFixButton.Bounds = Rectangle.Empty;
        _buyHouseButton.Bounds = Rectangle.Empty;
        _retireButton.Bounds = Rectangle.Empty;
        _commitFileButton.Bounds = Rectangle.Empty;
        _keepCodingButton.Bounds = Rectangle.Empty;
        _closeProjectStudioButton.Bounds = Rectangle.Empty;
        _projectStudioViewportBounds = Rectangle.Empty;
        _projectStudioScrollbarTrackBounds = Rectangle.Empty;
        _projectStudioScrollbarThumbBounds = Rectangle.Empty;
        _freelanceBoardViewportBounds = Rectangle.Empty;
        _freelanceBoardScrollbarTrackBounds = Rectangle.Empty;
        _freelanceBoardScrollbarThumbBounds = Rectangle.Empty;
        _freelanceGigEditorBounds = Rectangle.Empty;
        _projectTypeButton.Bounds = Rectangle.Empty;
        _projectThemeButton.Bounds = Rectangle.Empty;
        _projectToneButton.Bounds = Rectangle.Empty;
        _projectPlatformButton.Bounds = Rectangle.Empty;
        _projectBusinessButton.Bounds = Rectangle.Empty;
        _projectRerollButton.Bounds = Rectangle.Empty;

        _alertsPanelBounds = new Rectangle(contentX, alertsTop, contentWidth, _sidebarBounds.Bottom - alertsTop - 16);

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

        if (PortfolioWorkspace.IsCurrentBatchComplete(_state) || _simulation.HasPublishedApps(_state))
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
            var buttonWidth = _state.IsRealisticMode && _state.ActiveTechDebtBug is not null ? 82 : 70;
            _squashBugButton.Bounds = new Rectangle(_techDebtCardBounds.Right - (buttonWidth + 10), _techDebtCardBounds.Y + 8, buttonWidth, 24);
        }

        if (_jobListingCardBounds != Rectangle.Empty)
        {
            _applyForJobButton.Bounds = new Rectangle(_jobListingCardBounds.Right - 94, _jobListingCardBounds.Y + 8, 84, 24);
        }

        if (_applicationCardBounds != Rectangle.Empty)
        {
            _openApplicationButton.Bounds = new Rectangle(_applicationCardBounds.Right - 104, _applicationCardBounds.Y + 8, 94, 24);
        }

        if (_publishCardBounds != Rectangle.Empty && (publishReady || canOpenCommitFromPublish))
        {
            _publishAppButton.Bounds = new Rectangle(_publishCardBounds.Right - 94, _publishCardBounds.Y + 8, 84, 24);
        }

        if (_state.ActiveCatInterruption is not null)
        {
            var distraction = _state.ActiveCatInterruption;
            var buttonY = _catOverlayBounds.Bottom - 44;
            var buttonWidth = (_catOverlayBounds.Width - 66) / 2;
            _deskDistractionFocusButton.Bounds = new Rectangle(_catOverlayBounds.X + 24, buttonY, buttonWidth, 30);
            _deskDistractionQuickFixButton.Bounds = new Rectangle(_deskDistractionFocusButton.Bounds.Right + 18, buttonY, buttonWidth, 30);
            _deskDistractionFocusButton.Text = $"{distraction.FocusActionLabel} (-{distraction.FocusActionFocusCost:0} focus)";
            _deskDistractionQuickFixButton.Text = $"{distraction.QuickResolveLabel} (-${distraction.QuickResolveFundsCost:0})";
        }

        if (_runControlsBounds != Rectangle.Empty)
        {
            var runButtonY = _runControlsBounds.Bottom - 38;
            var runButtonWidth = (_runControlsBounds.Width - 40) / 3;
            _guideButton.Bounds = new Rectangle(_runControlsBounds.X + 12, runButtonY, runButtonWidth, 28);
            _restartButton.Bounds = new Rectangle(_guideButton.Bounds.Right + 8, runButtonY, runButtonWidth, 28);
            _newRunButton.Bounds = new Rectangle(_restartButton.Bounds.Right + 8, runButtonY, runButtonWidth, 28);
        }
        else
        {
            _guideButton.Bounds = Rectangle.Empty;
            _restartButton.Bounds = new Rectangle(468, 494, 304, 54);
            _newRunButton.Bounds = new Rectangle(828, 494, 304, 54);
        }

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

        var foodWidth = Math.Min(_editorViewportBounds.Width - 48, 940);
        var topPanelsGap = 20;
        var foodContentWidth = foodWidth - 48;
        var choiceWidth = Math.Clamp(((foodContentWidth - topPanelsGap) / 2) - 18, 340, 390);
        var modifierWidth = foodContentWidth - choiceWidth - topPanelsGap;
        var selectedFoodForOverlay = activeDelivery?.Choice ?? _selectedFood;
        var selectedFoodOption = _simulation.GetFoodOption(_state, selectedFoodForOverlay);
        var modifierOptions = _simulation.GetFoodOrderModifiers(_state, selectedFoodForOverlay);
        var mealIntroText = activeDelivery is null
            ? "Delivery speed versus home-cooked stability."
            : "Meal locked until the timer resolves.";
        var modifierIntroHeight = UiTextBlock.MeasureWrappedHeight(
            _font,
            GetFoodModifierIntroText(activeDelivery),
            modifierWidth - 28,
            0.64f,
            2f,
            3);
        var mealIntroHeight = UiTextBlock.MeasureWrappedHeight(
            _font,
            mealIntroText,
            choiceWidth - 28,
            0.62f,
            2f,
            2);
        const int mealButtonHeight = 44;
        const int mealButtonGap = 10;
        const int modifierButtonHeight = 30;
        const int modifierRowGap = 40;
        const int baseTopPanelsHeight = 182;
        const int baseSummaryPanelHeight = 178;
        var mealRows = (int)Math.Ceiling(FoodChoices.Length / 2f);
        var mealHeaderHeight = 20 + (int)Math.Ceiling(_font.LineSpacing * 0.8f) + 8 + (int)Math.Ceiling(mealIntroHeight) + 14;
        var requiredChoicePanelHeight = mealHeaderHeight + (mealButtonHeight * mealRows) + (mealButtonGap * Math.Max(0, mealRows - 1)) + 18;
        var requiredModifierPanelHeight = 50 + (int)Math.Ceiling(modifierIntroHeight) + 10 + modifierButtonHeight + 18;
        if (modifierOptions.Count > 1)
        {
            requiredModifierPanelHeight += (modifierOptions.Count - 1) * modifierRowGap;
        }

        var topPanelsHeight = Math.Max(baseTopPanelsHeight, Math.Max(requiredChoicePanelHeight, requiredModifierPanelHeight));
        IReadOnlyCollection<FoodOrderModifier> selectedModifiers = activeDelivery is not null
            ? activeDelivery.SelectedModifiers
            : _selectedFoodModifiers;
        var reviewReceipt = activeDelivery?.ReviewReceipt ?? _doubleCheckOrder;
        var penaltyMinutes = _simulation.GetFoodOrderPenaltyMinutes(selectedFoodOption.Choice, selectedModifiers, reviewReceipt);
        var orderStateText = activeDelivery is null
            ? penaltyMinutes <= 0
                ? "Details locked. This meal should land clean with no sluggish penalty."
                : penaltyMinutes < selectedFoodOption.SluggishMinutesWhenUnchecked
                    ? $"Partial cleanup. Expected sluggishness falls to {FormatRemainingTime(penaltyMinutes)} after delivery."
                    : $"Messy order. Expect the full {FormatRemainingTime(selectedFoodOption.SluggishMinutesWhenUnchecked)} sluggish hit when it arrives."
            : $"{selectedFoodOption.Name} is {(_simulation.IsHomeCooked(activeDelivery.Choice) ? "cooking" : activeDelivery.Expedited ? "expedited" : "on standard delivery")} with {FormatRemainingTime(activeDelivery.RemainingInGameMinutes)} left before the stats land.";
        var summaryDescriptionText = activeDelivery is null
            ? selectedFoodOption.Description
            : $"{selectedFoodOption.Description} The driver will apply the focus and sanity bump only when the food actually arrives.";
        var summarySupportText = activeDelivery is null
            ? _simulation.IsHomeCooked(selectedFoodOption.Choice)
                ? "Home cooking is slower, cheaper, and calmer than takeout. It still clears hunger only when the meal is ready."
                : "Delivery clears hunger on arrival. Sleep is slower, but it is still the only full reset."
            : $"Expected on arrival: sanity {selectedFoodOption.SanityGain:+0;-0;0}, hunger reset, and {(penaltyMinutes <= 0 ? "no sluggishness" : $"{FormatRemainingTime(penaltyMinutes)} sluggishness")}.";
        var summaryWidth = foodContentWidth - 32;
        var descriptionHeight = UiTextBlock.MeasureWrappedHeight(_font, summaryDescriptionText, summaryWidth, 0.68f, 2f, 3);
        var supportHeight = UiTextBlock.MeasureWrappedHeight(_font, summarySupportText, summaryWidth, 0.62f, 2f, 3);
        var noteHeight = Math.Max(34, (int)Math.Ceiling(UiTextBlock.MeasureWrappedHeight(_font, orderStateText, summaryWidth - 16, 0.62f, 1f, 3)) + 12);
        var requiredSummaryPanelHeight =
            14 +
            (int)Math.Ceiling(_font.LineSpacing * 0.82f) +
            10 +
            (int)Math.Ceiling(_font.LineSpacing * 0.74f) +
            12 +
            (int)Math.Ceiling(descriptionHeight) +
            10 +
            (int)Math.Ceiling(supportHeight) +
            10 +
            noteHeight +
            14;
        var foodY = _editorViewportBounds.Y + 12;
        var foodX = _editorViewportBounds.X + ((_editorViewportBounds.Width - foodWidth) / 2);
        var maxFoodHeight = _virtualResolution.Y - foodY - 18;
        var foodHeight = Math.Min(
            maxFoodHeight,
            206 + topPanelsHeight + Math.Max(baseSummaryPanelHeight, requiredSummaryPanelHeight));
        _foodAppBounds = new Rectangle(foodX, foodY, foodWidth, foodHeight);
        var foodContentX = _foodAppBounds.X + 24;
        var topPanelsY = _foodAppBounds.Y + 122;
        _foodChoicePanelBounds = new Rectangle(foodContentX, topPanelsY, choiceWidth, topPanelsHeight);
        _foodModifierPanelBounds = new Rectangle(_foodChoicePanelBounds.Right + topPanelsGap, topPanelsY, modifierWidth, topPanelsHeight);

        var mealButtonWidth = (_foodChoicePanelBounds.Width - 48) / 2;
        var mealButtonX = _foodChoicePanelBounds.X + 18;
        var mealButtonY = _foodChoicePanelBounds.Y + mealHeaderHeight;
        for (var index = 0; index < FoodChoices.Length; index++)
        {
            var button = GetFoodButton(FoodChoices[index]);
            var column = index % 2;
            var row = index / 2;
            button.Bounds = new Rectangle(
                mealButtonX + (column * (mealButtonWidth + mealButtonGap)),
                mealButtonY + (row * (mealButtonHeight + mealButtonGap)),
                mealButtonWidth,
                mealButtonHeight);
        }

        var foodActionsY = _foodAppBounds.Bottom - 52;
        _doubleCheckOrderButton.Bounds = new Rectangle(_foodAppBounds.X + 24, foodActionsY, 208, 34);
        _expediteOrderButton.Bounds = new Rectangle(_foodAppBounds.X + 246, foodActionsY, 208, 34);
        _confirmFoodOrderButton.Bounds = new Rectangle(_foodAppBounds.X + 468, foodActionsY, 188, 34);
        _closeFoodAppButton.Bounds = new Rectangle(_foodAppBounds.Right - 112, _foodAppBounds.Y + 18, 88, 30);
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

        const int bankWidth = 812;
        var bankX = _editorViewportBounds.X + 54;
        var bankY = _editorViewportBounds.Y + 12;
        var bankCardWidth = (bankWidth - 72) / 2;
        var housingBody = GetHousingTrackDescription();
        var retirementBody = GetRetirementGoalDescription();
        var emergencyBody = GetEmergencyBufferDescription();
        var housingBodyHeight = UiTextBlock.MeasureWrappedHeight(_font, housingBody, bankCardWidth - 32, 0.64f, 2f, 4);
        var retirementBodyHeight = UiTextBlock.MeasureWrappedHeight(_font, retirementBody, bankCardWidth - 32, 0.64f, 2f, 4);
        var lowerCardHeight = Math.Max(
            132,
            (int)MathF.Ceiling(68f + Math.Max(housingBodyHeight, retirementBodyHeight) + 12f + 28f + 16f));
        var coinHeight = Math.Max(
            76,
            (int)MathF.Ceiling(40f + UiTextBlock.MeasureWrappedHeight(_font, emergencyBody, bankWidth - 80, 0.7f, 2f, 3) + 16f));
        var ledgerPreviewHeight = 42f;
        foreach (var entry in GetRecentMoneyEntries().Take(3))
        {
            ledgerPreviewHeight += UiTextBlock.MeasureWrappedHeight(_font, entry, bankWidth - 80, 0.7f, 2f, 2) + 8f;
        }

        var ledgerHeight = Math.Max(108, (int)MathF.Ceiling(ledgerPreviewHeight + 10f));
        var desiredBankHeight = 118 + 124 + 16 + lowerCardHeight + 16 + coinHeight + 16 + ledgerHeight + 24;
        var maxBankHeight = _virtualResolution.Y - bankY - 18;
        var bankHeight = Math.Min(maxBankHeight, desiredBankHeight);
        _bankAppBounds = new Rectangle(bankX, bankY, bankWidth, bankHeight);
        _closeBankAppButton.Bounds = new Rectangle(_bankAppBounds.Right - 112, _bankAppBounds.Y + 18, 88, 30);
        _bankAccountBounds = new Rectangle(_bankAppBounds.X + 24, _bankAppBounds.Y + 118, bankCardWidth, 124);
        _bankRentBounds = new Rectangle(_bankAccountBounds.Right + 24, _bankAppBounds.Y + 118, bankCardWidth, 124);
        _bankHouseBounds = new Rectangle(_bankAppBounds.X + 24, _bankAccountBounds.Bottom + 16, bankCardWidth, lowerCardHeight);
        _bankRetirementBounds = new Rectangle(_bankHouseBounds.Right + 24, _bankHouseBounds.Y, bankCardWidth, lowerCardHeight);
        _bankCoinBounds = new Rectangle(_bankAppBounds.X + 24, _bankHouseBounds.Bottom + 16, _bankAppBounds.Width - 48, coinHeight);
        _bankLedgerBounds = new Rectangle(
            _bankAppBounds.X + 24,
            _bankCoinBounds.Bottom + 16,
            _bankAppBounds.Width - 48,
            _bankAppBounds.Bottom - (_bankCoinBounds.Bottom + 40));
        _buyHouseButton.Bounds = new Rectangle(_bankHouseBounds.Right - 124, _bankHouseBounds.Bottom - 44, 108, 28);
        _retireButton.Bounds = new Rectangle(_bankRetirementBounds.Right - 140, _bankRetirementBounds.Bottom - 44, 124, 28);

        _communicationBounds = new Rectangle(_editorViewportBounds.X + 62, _editorViewportBounds.Y + 24, 860, 480);
        _closeCommunicationButton.Bounds = new Rectangle(_communicationBounds.Right - 112, _communicationBounds.Y + 18, 88, 30);
        _communicationViewportBounds = new Rectangle(_communicationBounds.X + 24, _communicationBounds.Y + 122, 248, _communicationBounds.Height - 146);
        _communicationScrollbarTrackBounds = new Rectangle(_communicationViewportBounds.Right + 6, _communicationViewportBounds.Y, 10, _communicationViewportBounds.Height);
        _communicationDetailBounds = new Rectangle(
            _communicationScrollbarTrackBounds.Right + 22,
            _communicationViewportBounds.Y - 12,
            _communicationBounds.Right - (_communicationScrollbarTrackBounds.Right + 46),
            _communicationViewportBounds.Height + 24);
        _communicationCardBounds.Clear();
        EnsureValidCommunicationSelection();
        const int communicationCardGap = 10;
        const int communicationCardHeight = 74;
        const int communicationContentPadding = 8;
        var communicationContentHeight =
            (_state.KnownContacts.Count * communicationCardHeight) +
            (Math.Max(0, _state.KnownContacts.Count - 1) * communicationCardGap) +
            (communicationContentPadding * 2);
        _communicationMaxScrollOffset = Math.Max(0f, communicationContentHeight - _communicationViewportBounds.Height);
        _communicationScrollOffset = Math.Clamp(_communicationScrollOffset, 0f, _communicationMaxScrollOffset);
        for (var index = 0; index < _state.KnownContacts.Count; index++)
        {
            var contact = _state.KnownContacts[index];
            var cardY =
                _communicationViewportBounds.Y +
                communicationContentPadding +
                (index * (communicationCardHeight + communicationCardGap)) -
                (int)MathF.Round(_communicationScrollOffset);
            _communicationCardBounds[contact.Id] = new Rectangle(
                _communicationViewportBounds.X,
                cardY,
                _communicationViewportBounds.Width - 4,
                communicationCardHeight);
        }

        if (_communicationMaxScrollOffset <= 0f)
        {
            _communicationScrollbarThumbBounds = _communicationScrollbarTrackBounds;
        }
        else
        {
            var thumbHeight = Math.Max(54, (int)MathF.Round(_communicationScrollbarTrackBounds.Height * (_communicationViewportBounds.Height / (float)communicationContentHeight)));
            var thumbTravel = _communicationScrollbarTrackBounds.Height - thumbHeight;
            var thumbY = _communicationScrollbarTrackBounds.Y + (int)MathF.Round((_communicationScrollOffset / _communicationMaxScrollOffset) * thumbTravel);
            _communicationScrollbarThumbBounds = new Rectangle(_communicationScrollbarTrackBounds.X, thumbY, _communicationScrollbarTrackBounds.Width, thumbHeight);
        }

        _commitPromptBounds = new Rectangle(_editorViewportBounds.X + 112, _editorViewportBounds.Y + 88, 672, 336);
        var commitButtonY = _commitPromptBounds.Bottom - 48;
        _commitFileButton.Bounds = new Rectangle(_commitPromptBounds.Right - 308, commitButtonY, 136, 32);
        _keepCodingButton.Bounds = new Rectangle(_commitPromptBounds.Right - 156, commitButtonY, 132, 32);
        _commitFileButton.Text = "Commit Now";
        _keepCodingButton.Text = "Keep Coding";

        _projectStudioBounds = new Rectangle(_editorViewportBounds.X + 44, _editorViewportBounds.Y + 14, 812, 568);
        _closeProjectStudioButton.Bounds = new Rectangle(_projectStudioBounds.Right - 112, _projectStudioBounds.Y + 18, 88, 30);
        _projectStudioViewportBounds = new Rectangle(_projectStudioBounds.X + 24, _projectStudioBounds.Y + 118, _projectStudioBounds.Width - 64, _projectStudioBounds.Height - 150);
        _projectStudioScrollbarTrackBounds = new Rectangle(_projectStudioBounds.Right - 26, _projectStudioViewportBounds.Y, 10, _projectStudioViewportBounds.Height);
        const int studioRowHeight = 74;
        const int studioRowGap = 12;
        const int projectStudioContentHeight = 768;
        _projectStudioMaxScrollOffset = Math.Max(0f, projectStudioContentHeight - _projectStudioViewportBounds.Height);
        _projectStudioScrollOffset = Math.Clamp(_projectStudioScrollOffset, 0f, _projectStudioMaxScrollOffset);
        var studioOptionWidth = (_projectStudioViewportBounds.Width - 24) / 2;
        var studioLeftX = _projectStudioViewportBounds.X;
        var studioRightX = studioLeftX + studioOptionWidth + 24;
        var studioContentTop = _projectStudioViewportBounds.Y - (int)MathF.Round(_projectStudioScrollOffset);
        var studioTitleBounds = new Rectangle(_projectStudioViewportBounds.X, studioContentTop, _projectStudioViewportBounds.Width, 112);
        var studioTopY = studioTitleBounds.Bottom + 18;
        _projectTypeButton.Bounds = new Rectangle(studioLeftX + studioOptionWidth - 122, studioTopY + 20, 106, 30);
        _projectThemeButton.Bounds = new Rectangle(studioRightX + studioOptionWidth - 122, studioTopY + 20, 106, 30);
        _projectToneButton.Bounds = new Rectangle(studioLeftX + studioOptionWidth - 122, studioTopY + studioRowHeight + studioRowGap + 20, 106, 30);
        _projectPlatformButton.Bounds = new Rectangle(studioRightX + studioOptionWidth - 122, studioTopY + studioRowHeight + studioRowGap + 20, 106, 30);
        _projectBusinessButton.Bounds = new Rectangle(studioLeftX + studioOptionWidth - 122, studioTopY + ((studioRowHeight + studioRowGap) * 2) + 20, 106, 30);
        _projectRerollButton.Bounds = new Rectangle(studioRightX + studioOptionWidth - 122, studioTopY + ((studioRowHeight + studioRowGap) * 2) + 20, 106, 30);
        if (_projectStudioMaxScrollOffset <= 0f)
        {
            _projectStudioScrollbarThumbBounds = _projectStudioScrollbarTrackBounds;
        }
        else
        {
            var thumbHeight = Math.Max(54, (int)MathF.Round(_projectStudioScrollbarTrackBounds.Height * (_projectStudioViewportBounds.Height / (float)projectStudioContentHeight)));
            var thumbTravel = _projectStudioScrollbarTrackBounds.Height - thumbHeight;
            var thumbY = _projectStudioScrollbarTrackBounds.Y + (int)MathF.Round((_projectStudioScrollOffset / _projectStudioMaxScrollOffset) * thumbTravel);
            _projectStudioScrollbarThumbBounds = new Rectangle(_projectStudioScrollbarTrackBounds.X, thumbY, _projectStudioScrollbarTrackBounds.Width, thumbHeight);
        }

        _applicationBounds = new Rectangle(_editorViewportBounds.X + 70, _editorViewportBounds.Y + 16, 774, 566);
        _applicationEditorBounds = new Rectangle(_applicationBounds.X + 24, _applicationBounds.Y + 174, _applicationBounds.Width - 48, _applicationBounds.Height - 198);
        _closeApplicationButton.Bounds = new Rectangle(_applicationBounds.Right - 112, _applicationBounds.Y + 18, 88, 30);
        _tutorialBounds = new Rectangle(226, 92, 1148, 628);
        _tutorialCloseButton.Bounds = new Rectangle(_tutorialBounds.Right - 112, _tutorialBounds.Y + 18, 88, 30);
        _tutorialBackButton.Bounds = new Rectangle(_tutorialBounds.X + 28, _tutorialBounds.Bottom - 52, 136, 34);
        _tutorialNextButton.Bounds = new Rectangle(_tutorialBounds.Right - 188, _tutorialBounds.Bottom - 52, 160, 34);
        _tutorialBackButton.Enabled = _tutorialPageIndex > 0;
        _tutorialNextButton.Enabled = true;
        _tutorialNextButton.Text = _tutorialPageIndex >= GetTutorialPageCount() - 1 ? "Begin Run" : "Next";
        _tutorialCloseButton.Text = "Skip";
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

        _freelanceBoardBounds = new Rectangle(_editorViewportBounds.X + 72, _editorViewportBounds.Y + 18, 768, 556);
        _closeFreelanceBoardButton.Bounds = new Rectangle(_freelanceBoardBounds.Right - 112, _freelanceBoardBounds.Y + 18, 88, 30);
        _freelanceBoardViewportBounds = new Rectangle(_freelanceBoardBounds.X + 24, _freelanceBoardBounds.Y + 126, _freelanceBoardBounds.Width - 64, _freelanceBoardBounds.Height - 150);
        _freelanceBoardScrollbarTrackBounds = new Rectangle(_freelanceBoardBounds.Right - 26, _freelanceBoardViewportBounds.Y, 10, _freelanceBoardViewportBounds.Height);
        _freelanceGigSummaryBounds = Rectangle.Empty;
        _freelanceGigEditorBounds = Rectangle.Empty;
        _freelanceGigCardBounds.Clear();
        var freelanceCardWidth = _freelanceBoardViewportBounds.Width;
        if (_state.ActiveFreelanceGig is not null)
        {
            _freelanceBoardMaxScrollOffset = 0f;
            _freelanceBoardScrollOffset = 0f;
            _freelanceBoardScrollbarThumbBounds = _freelanceBoardScrollbarTrackBounds;
            var gig = _state.ActiveFreelanceGig;
            var summaryHeight = MeasureActiveFreelanceGigSummaryHeight(gig);
            var maxSummaryHeight = Math.Max(112, _freelanceBoardViewportBounds.Height - 200);
            summaryHeight = Math.Min(summaryHeight, maxSummaryHeight);
            _freelanceGigSummaryBounds = new Rectangle(
                _freelanceBoardViewportBounds.X,
                _freelanceBoardViewportBounds.Y,
                _freelanceBoardViewportBounds.Width,
                summaryHeight);
            var editorY = _freelanceGigSummaryBounds.Bottom + 14;
            _freelanceGigEditorBounds = new Rectangle(
                _freelanceBoardViewportBounds.X,
                editorY,
                _freelanceBoardViewportBounds.Width,
                _freelanceBoardViewportBounds.Bottom - editorY);

            foreach (var type in FreelanceGigOrder)
            {
                _freelanceGigButtons[type].Enabled = false;
                _freelanceGigButtons[type].Bounds = Rectangle.Empty;
            }
        }
        else
        {
            var freelanceContentHeight =
                (FreelanceGigOrder.Length * FreelanceCardHeight) +
                (Math.Max(0, FreelanceGigOrder.Length - 1) * FreelanceCardGap) +
                (FreelanceBoardContentPadding * 2);
            _freelanceBoardMaxScrollOffset = Math.Max(0f, freelanceContentHeight - _freelanceBoardViewportBounds.Height);
            _freelanceBoardScrollOffset = Math.Clamp(_freelanceBoardScrollOffset, 0f, _freelanceBoardMaxScrollOffset);
            for (var index = 0; index < FreelanceGigOrder.Length; index++)
            {
                var type = FreelanceGigOrder[index];
                var y = _freelanceBoardViewportBounds.Y +
                        FreelanceBoardContentPadding +
                        (index * (FreelanceCardHeight + FreelanceCardGap)) -
                        (int)MathF.Round(_freelanceBoardScrollOffset);
                var cardBounds = new Rectangle(_freelanceBoardViewportBounds.X, y, freelanceCardWidth, FreelanceCardHeight);
                _freelanceGigCardBounds[type] = cardBounds;
                var button = _freelanceGigButtons[type];
                button.Enabled = _simulation.CanTakeFreelanceGig(_state, type);
                button.Bounds = new Rectangle(cardBounds.Right - 120, cardBounds.Bottom - 42, 110, 30);
            }

            if (_freelanceBoardMaxScrollOffset <= 0f)
            {
                _freelanceBoardScrollbarThumbBounds = _freelanceBoardScrollbarTrackBounds;
            }
            else
            {
                var thumbHeight = Math.Max(54, (int)MathF.Round(_freelanceBoardScrollbarTrackBounds.Height * (_freelanceBoardViewportBounds.Height / (float)freelanceContentHeight)));
                var thumbTravel = _freelanceBoardScrollbarTrackBounds.Height - thumbHeight;
                var thumbY = _freelanceBoardScrollbarTrackBounds.Y + (int)MathF.Round((_freelanceBoardScrollOffset / _freelanceBoardMaxScrollOffset) * thumbTravel);
                _freelanceBoardScrollbarThumbBounds = new Rectangle(_freelanceBoardScrollbarTrackBounds.X, thumbY, _freelanceBoardScrollbarTrackBounds.Width, thumbHeight);
            }
        }

        var upgradesWidth = Math.Min(_editorViewportBounds.Width - 48, 992);
        var upgradesX = _editorViewportBounds.X + ((_editorViewportBounds.Width - upgradesWidth) / 2);
        _upgradesBounds = new Rectangle(upgradesX, _editorViewportBounds.Y + 8, upgradesWidth, 636);
        _closeUpgradesButton.Bounds = new Rectangle(_upgradesBounds.Right - 112, _upgradesBounds.Y + 18, 88, 30);
        _upgradesViewportBounds = new Rectangle(_upgradesBounds.X + 24, _upgradesBounds.Y + 126, _upgradesBounds.Width - 64, _upgradesBounds.Height - 150);
        _upgradesScrollbarTrackBounds = new Rectangle(_upgradesBounds.Right - 26, _upgradesViewportBounds.Y, 10, _upgradesViewportBounds.Height);
        var upgradeCardWidth = (_upgradesViewportBounds.Width - 24) / 2;
        var upgradeCardHeight = 132;
        var firstCardX = _upgradesViewportBounds.X;
        var secondCardX = firstCardX + upgradeCardWidth + 24;
        var firstRowY = _upgradesViewportBounds.Y;
        const int upgradeRowGap = 12;
        var upgradeRows = (int)Math.Ceiling(EfficiencyUpgradeCatalog.All.Count / 2f);
        var upgradesContentHeight = (upgradeRows * upgradeCardHeight) + (Math.Max(0, upgradeRows - 1) * upgradeRowGap);
        _upgradesMaxScrollOffset = Math.Max(0f, upgradesContentHeight - _upgradesViewportBounds.Height);
        _upgradesScrollOffset = Math.Clamp(_upgradesScrollOffset, 0f, _upgradesMaxScrollOffset);
        for (var index = 0; index < EfficiencyUpgradeCatalog.All.Count; index++)
        {
            var definition = EfficiencyUpgradeCatalog.All[index];
            var column = index % 2;
            var row = index / 2;
            var x = column == 0 ? firstCardX : secondCardX;
            var y = firstRowY + (row * (upgradeCardHeight + upgradeRowGap)) - (int)MathF.Round(_upgradesScrollOffset);
            _upgradeCardBounds[definition.Type] = new Rectangle(x, y, upgradeCardWidth, upgradeCardHeight);
        }

        if (_upgradesMaxScrollOffset <= 0f)
        {
            _upgradesScrollbarThumbBounds = _upgradesScrollbarTrackBounds;
        }
        else
        {
            var thumbHeight = Math.Max(54, (int)MathF.Round(_upgradesScrollbarTrackBounds.Height * (_upgradesViewportBounds.Height / (float)upgradesContentHeight)));
            var thumbTravel = _upgradesScrollbarTrackBounds.Height - thumbHeight;
            var thumbY = _upgradesScrollbarTrackBounds.Y + (int)MathF.Round((_upgradesScrollOffset / _upgradesMaxScrollOffset) * thumbTravel);
            _upgradesScrollbarThumbBounds = new Rectangle(_upgradesScrollbarTrackBounds.X, thumbY, _upgradesScrollbarTrackBounds.Width, thumbHeight);
        }

        var statsWidth = Math.Min(_virtualResolution.X - 120, 1360);
        var statsHeight = Math.Min(_virtualResolution.Y - 88, 724);
        var statsX = (_virtualResolution.X - statsWidth) / 2;
        var statsY = (_virtualResolution.Y - statsHeight) / 2;
        _statsBounds = new Rectangle(statsX, statsY, statsWidth, statsHeight);
        _closeStatsButton.Bounds = new Rectangle(_statsBounds.Right - 112, _statsBounds.Y + 18, 88, 30);
        _statsViewportBounds = new Rectangle(_statsBounds.X + 24, _statsBounds.Y + 118, _statsBounds.Width - 64, _statsBounds.Height - 150);
        _statsScrollbarTrackBounds = new Rectangle(_statsBounds.Right - 26, _statsViewportBounds.Y, 10, _statsViewportBounds.Height);
        var statsContentHeight = MeasureStatsOverlayContentHeight(_statsViewportBounds.Width);
        _statsMaxScrollOffset = Math.Max(0f, statsContentHeight - _statsViewportBounds.Height);
        _statsScrollOffset = Math.Clamp(_statsScrollOffset, 0f, _statsMaxScrollOffset);
        if (_statsMaxScrollOffset <= 0f)
        {
            _statsScrollbarThumbBounds = _statsScrollbarTrackBounds;
        }
        else
        {
            var thumbHeight = Math.Max(54, (int)MathF.Round(_statsScrollbarTrackBounds.Height * (_statsViewportBounds.Height / (float)statsContentHeight)));
            var thumbTravel = _statsScrollbarTrackBounds.Height - thumbHeight;
            var thumbY = _statsScrollbarTrackBounds.Y + (int)MathF.Round((_statsScrollOffset / _statsMaxScrollOffset) * thumbTravel);
            _statsScrollbarThumbBounds = new Rectangle(_statsScrollbarTrackBounds.X, thumbY, _statsScrollbarTrackBounds.Width, thumbHeight);
        }

        foreach (var definition in EfficiencyUpgradeCatalog.All)
        {
            var button = _upgradeButtons[definition.Type];
            var tier = _simulation.GetUpgradeTier(_state, definition.Type);
            var maxTier = _simulation.GetUpgradeMaxTier();
            button.Text = tier >= maxTier
                ? "Maxed 5/5"
                : tier > 0
                    ? $"Buy T{tier + 1}"
                    : "Buy T1";
            button.Enabled = _simulation.CanPurchaseUpgrade(_state, definition.Type);

            var cardBounds = _upgradeCardBounds[definition.Type];
            button.Bounds = new Rectangle(cardBounds.Right - 126, cardBounds.Bottom - 42, 112, 30);
        }

        foreach (var choice in FoodChoices)
        {
            var button = GetFoodButton(choice);
            button.Text = _simulation.GetFoodOption(_state, choice).Name;
            button.IsSelected = _selectedFood == choice;
            button.Enabled = !hasActiveFoodDelivery;
        }
        _doubleCheckOrderButton.Enabled = !hasActiveFoodDelivery;
        _doubleCheckOrderButton.IsSelected = reviewReceiptEnabled;
        _doubleCheckOrderButton.Text = reviewReceiptEnabled ? "Check Details: ON" : "Check Details: OFF";
        var canExpediteSelectedFood = _simulation.AllowsExpeditedDelivery(_selectedFood);
        _expediteOrderButton.Enabled = !hasActiveFoodDelivery && canExpediteSelectedFood;
        _expediteOrderButton.IsSelected = expeditedDeliveryEnabled;
        _expediteOrderButton.Text = !canExpediteSelectedFood
            ? "Expedite: N/A"
            : expeditedDeliveryEnabled
                ? $"Expedite (+${_simulation.GetFoodTipAmount(_selectedFood, true):0}): ON"
                : $"Expedite (+${_simulation.GetFoodTipAmount(_selectedFood, true):0}): OFF";
        _confirmFoodOrderButton.Text = hasActiveFoodDelivery ? "Order Active" : "Place Order";
        _confirmFoodOrderButton.Enabled = _simulation.CanPlaceFoodOrder(_state, _selectedFood, _expediteFoodDelivery);

        EnsureValidCommunicationSelection();
        var selectedCommunicationContact = GetSelectedCommunicationContact();
        foreach (var contact in _state.KnownContacts)
        {
            var pickButton = _communicationPickButtons[contact.Id];
            var messageButton = _communicationMessageButtons[contact.Id];
            var callButton = _communicationCallButtons[contact.Id];
            pickButton.Text = contact.Name;
            pickButton.Enabled = true;
            pickButton.IsSelected = string.Equals(contact.Id, _selectedCommunicationContactId, StringComparison.Ordinal);
            messageButton.Text = "Text";
            callButton.Text = "Call";
            messageButton.Enabled = selectedCommunicationContact is not null &&
                                    string.Equals(contact.Id, selectedCommunicationContact.Id, StringComparison.Ordinal) &&
                                    _simulation.CanMessageContact(_state, contact.Id);
            callButton.Enabled = selectedCommunicationContact is not null &&
                                 string.Equals(contact.Id, selectedCommunicationContact.Id, StringComparison.Ordinal) &&
                                 _simulation.CanCallContact(_state, contact.Id);

            if (_communicationCardBounds.TryGetValue(contact.Id, out var cardBounds))
            {
                pickButton.Bounds = cardBounds;
            }
            else
            {
                pickButton.Bounds = Rectangle.Empty;
            }

            if (selectedCommunicationContact is not null &&
                string.Equals(contact.Id, selectedCommunicationContact.Id, StringComparison.Ordinal))
            {
                var buttonY = _communicationDetailBounds.Bottom - 44;
                var buttonWidth = (_communicationDetailBounds.Width - 54) / 2;
                messageButton.Bounds = new Rectangle(_communicationDetailBounds.X + 18, buttonY, buttonWidth, 30);
                callButton.Bounds = new Rectangle(_communicationDetailBounds.X + 36 + buttonWidth, buttonY, buttonWidth, 30);
            }
            else
            {
                messageButton.Bounds = Rectangle.Empty;
                callButton.Bounds = Rectangle.Empty;
            }
        }
    }

    private string GetFoodModifierIntroText(ActiveFoodDelivery? activeDelivery)
    {
        return activeDelivery is null
            ? "Toggle the details you want fixed before the meal gets locked in."
            : $"{(_simulation.IsHomeCooked(activeDelivery.Choice) ? "Prep notes" : "Kitchen notes")} locked in. Detail check: {(activeDelivery.ReviewReceipt ? "ON" : "OFF")}  |  Tip: ${activeDelivery.TipAmount:0}.";
    }

    private void OpenTutorial()
    {
        _tutorialOpen = true;
        _tutorialPageIndex = 0;
        _foodAppOpen = false;
        _bankAppOpen = false;
        _communicationOpen = false;
        _commitPromptOpen = false;
        _projectStudioOpen = false;
        _freelanceBoardOpen = false;
        _upgradesOpen = false;
        _statsOpen = false;
        _jobApplicationOpen = false;
    }

    private void RestartCurrentRun()
    {
        _state = _simulation.CreateNewRun(_state.RunSeed);
        ResetWorkspaceForFreshState();
    }

    private void StartFreshRun()
    {
        _state = _simulation.CreateNewRun();
        ResetWorkspaceForFreshState();
    }

    private void ResetWorkspaceForFreshState()
    {
        _selectedFood = FoodChoice.Burger;
        _selectedFoodModifiers.Clear();
        _doubleCheckOrder = false;
        _expediteFoodDelivery = false;
        _projectStudioScrollOffset = 0f;
        _projectStudioMaxScrollOffset = 0f;
        _communicationScrollOffset = 0f;
        _communicationMaxScrollOffset = 0f;
        _freelanceBoardScrollOffset = 0f;
        _freelanceBoardMaxScrollOffset = 0f;
        _upgradesScrollOffset = 0f;
        _upgradesMaxScrollOffset = 0f;
        _statsScrollOffset = 0f;
        _statsMaxScrollOffset = 0f;
        _lastCelebratedFileName = null;
        _lastCommitPromptedFileName = null;
        _selectedCommunicationContactId = null;
        _communicationOpen = false;
        _statsOpen = false;
        _activeScrollbarDrag = OverlayScrollArea.None;
        EnsureCommunicationButtons();
        OpenTutorial();
        UpdateLayout();
        UpdateButtons();
    }

    private sealed record TutorialSection(string Heading, string Body);

    private sealed record TutorialPage(string Title, string Intro, TutorialSection[] Sections);

    private int GetTutorialPageCount()
    {
        return 7;
    }

    private TutorialPage GetTutorialPage(int pageIndex)
    {
        var dailyBill = _simulation.GetCurrentDailyBillAmount(_state);
        var apartmentCost = _simulation.GetApartmentCost(_state);
        var houseCost = _simulation.GetHouseCost(_state);
        var retirementCost = _simulation.GetRetirementCost(_state);

        return pageIndex switch
        {
            0 => new TutorialPage(
                "What A Run Actually Is",
                $"You start on Day {_state.Day} with ${_state.Funds:0}, focus {_state.Focus:0}, sanity {_state.Sanity:0}, and a daily bill currently landing for ${dailyBill:0} at in-game midnight. Micro Dev is now a branching life-and-career sim: Interview is the seven-day opener, and the other main modes keep running indefinitely.",
                [
                    new TutorialSection(
                        "How you lose",
                        "If bills push funds below zero and you cannot cover them, the run ends in eviction. You can also burn out if sanity collapses. Most losses come from stacking small mistakes: low focus, ignored hunger, skipped sleep, and panic spending."),
                    new TutorialSection(
                        "How you win",
                        "Interview Mode can end on the job offer, but the long-form victory path is bigger now: move out of the basement, reach an apartment, then a house, then retire. Corporate, Indie, and Founder keep building toward that life-sim finish line."),
                    new TutorialSection(
                        "First-day mindset",
                        "Do not chase every button at once. Stabilize the desk first, keep enough runway for the next bill, and only take side opportunities when your focus and sanity can absorb them.")
                ]),
            1 => new TutorialPage(
                "Interview Mode: The First Loop",
                "Interview Mode is the seven-day opening sprint. The whole job here is to build proof, survive the week, and reach a company offer before the deadline closes the route.",
                [
                    new TutorialSection(
                        "What the sprint asks for",
                        "Build enough portfolio lines, keep code quality healthy, and answer recruiter windows without letting bills, hunger, or sleep debt blow the run up first. Interview Mode is not indefinite: it is the pressure-cooker opener."),
                    new TutorialSection(
                        "How branching works",
                        "Once you land an offer, the company route decides the next career lane. Some offers branch into Corporate, some into Indie. You can also reject the offer path and spin the momentum into Founder Mode instead."),
                    new TutorialSection(
                        "What counts as success",
                        "You can end the run on the interview win, or treat it as the prologue and continue into the longer housing-and-retirement game. If the seven days expire before you land the offer, Interview Mode burns out.")
                ]),
            2 => new TutorialPage(
                "Corporate, Indie, And Founder",
                "The long-form modes are built to feel different instead of being cosmetic labels. They all aim at apartment, house, and retirement progression, but the day-to-day pressure comes from different places.",
                [
                    new TutorialSection(
                        "Corporate",
                        $"Corporate Mode has office hours from {FormatRemainingTime(_simulation.Config.CorporateOfficeHoursStartMinutes)} to {FormatRemainingTime(_simulation.Config.CorporateOfficeHoursEndMinutes)}. Salary lands every day and pays more, but bosses, check-ins, and micromanagement make it the strictest long-form route."),
                    new TutorialSection(
                        "Indie",
                        "Indie Mode is more self-directed and goal-oriented. Sanity pressure is lighter than Corporate, but focus is harder to recover and income stays thinner unless you keep shipping."),
                    new TutorialSection(
                        "Founder",
                        "Founder Mode starts in the basement with your own studio name, lower cash, and no employer to catch you. It is the grassroots route: freelance to stay solvent, ship your own work, and turn a struggling studio into a real business.")
                ]),
            3 => new TutorialPage(
                "Core Desk Loop",
                "Most of the run is still a desk rhythm: click the editor to write code, finish files, publish the batch, and react to whatever chaos hits the workspace. Every click trades focus for progress, so efficiency and timing matter.",
                [
                    new TutorialSection(
                        "Writing code",
                        $"Click inside the editor to type. Each click currently spends about {_simulation.GetCurrentWriteFocusCost(_state):0.0} focus for {_simulation.GetCurrentWriteLinesPerClick(_state)} line{(_simulation.GetCurrentWriteLinesPerClick(_state) == 1 ? string.Empty : "s")}, while steady work keeps pushing code quality upward."),
                    new TutorialSection(
                        "Build Studio and coherent snippets",
                        "Each batch is now tied to a project plan. Before you start typing, Build Studio lets you decide whether you are making an app or a game and tune the theme, tone, platform, and business model so the snippets stay related to the actual project."),
                    new TutorialSection(
                        "Commit discipline matters",
                        "Finished files can be committed the moment they land. Small clean commits are safer, faster to explain, and protect your work because computer freezes and ignored desk chaos can wipe anything that still sits outside the last commit.")
                ]),
            4 => new TutorialPage(
                "Food, Sleep, And Freelance",
                "The sim leans hard into micromanagement, so upkeep is real strategy rather than flavor text. Hunger, fatigue, sluggish meals, and focus thresholds all decide whether the next hour works.",
                [
                    new TutorialSection(
                        "Food + Kitchen",
                        $"Takeout lands faster, and expedited delivery can cut ETA to about {FormatRemainingTime(_simulation.Config.ExpeditedFoodDeliveryDurationMinutes)} for a tip. Home-cooked meals take longer, cost less, and usually recover you more calmly over longer runs."),
                    new TutorialSection(
                        "Sleep is a real gate",
                        $"Sleep lasts {FormatRemainingTime(_simulation.Config.SleepDurationMinutes)} and clears the worst fatigue. If you keep pushing past the limit, coding, gigs, and interviews eventually lock behind rest, so sleep before the desk is already collapsing."),
                    new TutorialSection(
                        "Freelance board",
                        $"Freelance gigs now require at least {_simulation.Config.FreelanceMinimumFocusRequired:0} focus to begin. They are emergency money, not free spam. Use them when the budget needs it and your focus bar can still survive the hit.")
                ]),
            5 => new TutorialPage(
                "Money, Housing, And Retirement",
                "Money is pressure, not score. Banking is where the long-term life-sim progression becomes readable, from basement survival through apartment, house, and retirement milestones.",
                [
                    new TutorialSection(
                        "Basement to apartment",
                        $"The first visible housing goal is moving out. Once you can cover the apartment buy-in of about ${apartmentCost:0}, the Banking app flips the run from basement survival into the next longer housing track."),
                    new TutorialSection(
                        "Apartment to house",
                        $"After that, the next major milestone is the house at about ${houseCost:0}. Corporate standing helps a little here, but every route still has to earn its way into real stability."),
                    new TutorialSection(
                        "Retirement as the long win",
                        $"Retiring needs a house and about ${retirementCost:0} in savings. That is the shared life-sim finish line for Corporate, Indie, and Founder, so every route keeps expanding instead of stopping after one good week.")
                ]),
            _ => new TutorialPage(
                "Seeds, Restart, And New Run",
                $"This run is using seed {_state.RunSeed}. The seed drives project order, names, timing, food flavor, relationship threads, and career variance, so the same seed can be replayed while a new one feels genuinely different.",
                [
                    new TutorialSection(
                        "Restart",
                        "Restart replays the exact same seed from the opening state. Use it when you want to test a new plan against the same interview branch, project order, boss personality, or event cadence."),
                    new TutorialSection(
                        "New Run",
                        "New Run starts over using the current seed rules from Options. Random mode rolls a fresh scenario. Manual mode keeps replaying the chosen seed until you change it."),
                    new TutorialSection(
                        "Good opening plan",
                        "Read Alerts & Inbox, write until the first real resource warning appears, lock in food before hunger spikes, and keep enough cash for the next bill. Once the desk is stable, start reaching for gigs, upgrades, applications, or release momentum.")
                ]),
        };
    }

    private void UpdateLayout()
    {
        const int margin = 16;
        const int gap = 14;
        const int sidebarWidth = 408;
        const int logHeight = 138;

        var contentHeight = _virtualResolution.Y - (margin * 3) - logHeight;
        var editorWidth = _virtualResolution.X - (margin * 2) - gap - sidebarWidth;

        _editorPanelBounds = new Rectangle(margin, margin, editorWidth, contentHeight);
        _editorViewportBounds = new Rectangle(_editorPanelBounds.X + 16, _editorPanelBounds.Y + 58, _editorPanelBounds.Width - 32, _editorPanelBounds.Height - 76);
        _sidebarBounds = new Rectangle(_editorPanelBounds.Right + gap, margin, sidebarWidth, contentHeight);
        _logBounds = new Rectangle(margin, _editorPanelBounds.Bottom + gap, _virtualResolution.X - (margin * 2), logHeight);
        _catOverlayBounds = new Rectangle(_editorViewportBounds.X + 144, _editorViewportBounds.Y + 108, _editorViewportBounds.Width - 288, 334);
    }

    private Rectangle GetSidebarStatusStripBounds()
    {
        return new Rectangle(_sidebarBounds.X + 16, _sidebarBounds.Y + 292, _sidebarBounds.Width - 32, 60);
    }

    private float GetSidebarActionsHeaderY()
    {
        return GetSidebarStatusStripBounds().Bottom + 10f;
    }

    private int GetSidebarActionButtonsTop()
    {
        return GetSidebarStatusStripBounds().Bottom + 36;
    }

    private int GetSidebarAlertsTop()
    {
        return GetSidebarActionButtonsTop() + 206;
    }

    private void DrawOverlayHeaderLabel(
        SpriteBatch spriteBatch,
        string text,
        Vector2 position,
        Rectangle closeButtonBounds,
        int contentRight,
        Color color,
        float preferredScale,
        float minimumScale)
    {
        var maxWidth = Math.Max(140f, closeButtonBounds.X - position.X - 16f);
        if (closeButtonBounds == Rectangle.Empty)
        {
            maxWidth = Math.Max(140f, contentRight - position.X);
        }

        DrawFittedLabel(spriteBatch, text, position, maxWidth, color, preferredScale, minimumScale);
    }

    private void DrawFittedLabel(
        SpriteBatch spriteBatch,
        string text,
        Vector2 position,
        float maxWidth,
        Color color,
        float preferredScale,
        float minimumScale)
    {
        var (displayText, scale) = UiTextBlock.FitText(_font, text, maxWidth, preferredScale, minimumScale);
        UiLabel.Draw(spriteBatch, _font, displayText, position, color, scale);
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
            foreach (var choice in FoodChoices)
            {
                if (GetFoodButton(choice).IsHovered)
                {
                    return BuildFoodTooltip(choice);
                }
            }

            if (_doubleCheckOrderButton.IsHovered)
            {
                return ("Check Details", "Lock in the important meal details before the choice goes live. Pair it with the right prep notes to eliminate the sluggish penalty.");
            }

            if (_expediteOrderButton.IsHovered)
            {
                if (!_simulation.AllowsExpeditedDelivery(_selectedFood))
                {
                    return ("Expedite", "Home-cooked meals cannot be expedited. They trade speed for lower cost and steadier recovery.");
                }

                return (
                    "Expedite Delivery",
                    $"Add a ${_simulation.GetFoodTipAmount(_selectedFood, true):0} tip to cut delivery time down to {FormatRemainingTime(_simulation.GetFoodDeliveryDuration(_state, _selectedFood, true))} instead of {FormatRemainingTime(_simulation.GetFoodDeliveryDuration(_state, _selectedFood, false))}.");
            }

            var orderOptions = _simulation.GetFoodOrderModifiers(_state, _selectedFood);
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
                var option = _simulation.GetFoodOption(_state, _selectedFood);
                var penalty = _simulation.GetFoodOrderPenaltyMinutes(_selectedFood, _selectedFoodModifiers, _doubleCheckOrder);
                return (
                    "Place Order",
                    $"Spend ${_simulation.GetFoodTotalCost(_state, _selectedFood, _expediteFoodDelivery):0} for {option.Name}. ETA {FormatRemainingTime(_simulation.GetFoodDeliveryDuration(_state, _selectedFood, _expediteFoodDelivery))}. Focus {FormatSigned(option.FocusGain)}, sanity {FormatSigned(option.SanityGain)}, and expected sluggishness {(penalty <= 0 ? "removed" : FormatRemainingTime(penalty))} on arrival.");
                }

            return null;
        }

        if (_freelanceBoardOpen)
        {
            if (_state.ActiveFreelanceGig is not null)
            {
                if (_freelanceGigEditorBounds.Contains(_mousePosition))
                {
                    return (
                        _state.ActiveFreelanceGig.Title,
                        $"Resume the contract editor for {_state.ActiveFreelanceGig.ClientName}. Finish {_state.ActiveFreelanceGig.FileName} to cash out the gig.");
                }

                return null;
            }

            foreach (var type in FreelanceGigOrder)
            {
                if (_freelanceGigButtons[type].IsHovered)
                {
                    return BuildFreelanceTooltip(type);
                }
            }

            return null;
        }

        if (_bankAppOpen)
        {
            if (_buyHouseButton.IsHovered)
            {
                return (
                    _state.HasHouse
                        ? "Housing Complete"
                        : _state.HasApartment ? "Buy House" : "Move Out",
                    _state.HasHouse
                        ? "This milestone is already complete."
                        : _state.HasApartment
                            ? $"Spend ${_simulation.GetHouseCost(_state):0} to lock in the house milestone and gain a stronger long-term stability cushion."
                            : $"Spend ${_simulation.GetApartmentCost(_state):0} to get out of the basement and unlock the next housing step toward the house.");
            }

            if (_retireButton.IsHovered)
            {
                return (
                    "Retire",
                    _state.HasRetired
                        ? "This long-term victory path is already complete."
                        : $"Requires a house and ${_simulation.GetRetirementCost(_state):0} in savings. This ends the run on the shared life-sim finish line.");
            }

            return null;
        }

        if (_communicationOpen)
        {
            foreach (var contact in _state.KnownContacts)
            {
                if (_communicationPickButtons[contact.Id].IsHovered)
                {
                    var isPartner = _state.HasFoundLove && string.Equals(_state.PartnerName, contact.Name, StringComparison.Ordinal);
                    return (
                        contact.Name,
                        $"{GetCommunicationRoleLabel(contact, isPartner)} contact. {GetCommunicationProgressLabel(contact, isPartner)}. Pick them to open the full thread and choose whether to text or call.");
                }
            }

            foreach (var contact in _state.KnownContacts)
            {
                var isPartner = _state.HasFoundLove && string.Equals(_state.PartnerName, contact.Name, StringComparison.Ordinal);
                if (_communicationMessageButtons[contact.Id].IsHovered)
                {
                    return (
                        $"Text {contact.Name}",
                        $"{GetCommunicationRoleLabel(contact, isPartner)} touchpoint. Quick and cheap on focus, but it still grows the bond. Mentor texts build prep, friend texts steady sanity, and date texts keep the relationship moving.");
                }

                if (_communicationCallButtons[contact.Id].IsHovered)
                {
                    return (
                        $"Call {contact.Name}",
                        $"{GetCommunicationRoleLabel(contact, isPartner)} call. Costs more time and focus than a text, but it hits harder for bond progress and usually lands the bigger sanity swing.");
                }
            }

            return null;
        }

        if (_commitPromptOpen)
        {
            if (_commitFileButton.IsHovered)
            {
                return ("Commit Now", $"Lock {_state.VersionControl.PendingChangeLines} dirty lines into a real commit. Smaller commit groups earn better Git discipline rewards.");
            }

            if (_keepCodingButton.IsHovered)
            {
                return ("Keep Coding", "Skip the commit for now and keep batching work, but remember that computer freezes and ignored desk chaos can roll everything back to the last commit.");
            }

            return null;
        }

        if (_projectStudioOpen)
        {
            if (_projectTypeButton.IsHovered)
            {
                return ("Product", "Cycle between building an app or a game for the current release.");
            }

            if (_projectThemeButton.IsHovered)
            {
                return ("Theme", "Change the main design lane. This feeds project flavor, snippet naming, and some payout shaping.");
            }

            if (_projectToneButton.IsHovered)
            {
                return ("Tone", "Shift the aesthetic mood of the release.");
            }

            if (_projectPlatformButton.IsHovered)
            {
                return ("Platform", "Choose where the release is aimed. This changes the pitch and generated flavor.");
            }

            if (_projectBusinessButton.IsHovered)
            {
                return ("Business Model", "Choose how this project tries to make money.");
            }

            if (_projectRerollButton.IsHovered)
            {
                return ("Concept Reroll", "Keep the same plan shape but reroll the actual title and flavor for a fresh pitch.");
            }

            return null;
        }

        if (_upgradesOpen)
        {
            foreach (var definition in EfficiencyUpgradeCatalog.All)
            {
                if (_upgradeButtons[definition.Type].IsHovered)
                {
                    var tier = _simulation.GetUpgradeTier(_state, definition.Type);
                    var maxTier = _simulation.GetUpgradeMaxTier();
                    var nextCost = _simulation.GetUpgradePurchaseCost(_state, definition.Type);
                    return (
                        definition.Name,
                        tier >= maxTier
                            ? $"{definition.Description} Tier {tier}/{maxTier}. Total live effect: {definition.GetTotalEffectSummary(tier)}."
                            : $"{definition.Description} Tier {tier}/{maxTier}. Per-tier effect: {definition.SummaryEffect} Next tier cost: ${nextCost:0}.");
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
                    $"{_simulation.GetFoodOption(_state, delivery.Choice).Name} is already in progress. ETA {FormatRemainingTime(delivery.RemainingInGameMinutes)} before the stats land.");
            }

            return ("Food + Kitchen", "Balance delivery speed against home-cooked efficiency. Meals restore focus and hunger on arrival, but the details still matter.");
        }

        if (_freelanceButton.IsHovered)
        {
            return _state.ActiveFreelanceGig is null
                ? ("Freelance Board", $"Choose between seeded contracts that match the current project flavor. Every gig now needs at least {_simulation.Config.FreelanceMinimumFocusRequired:0} focus to start, and the payout only lands after you finish the contract file.")
                : ("Resume Gig", $"Jump back into {_state.ActiveFreelanceGig.Title} for {_state.ActiveFreelanceGig.ClientName}. The money is waiting on the finished file, not the accept click.");
        }

        if (_upgradesButton.IsHovered)
        {
            return ("Upgrades", "Spend saved funds on permanent typing throughput, quality, or focus-efficiency improvements.");
        }

        if (_bankAppButton.IsHovered)
        {
            return ("Banking", "Open a clean finance view with runway, rent countdown, and first-coin emergency status.");
        }

        if (_communicationButton.IsHovered)
        {
            return ("Communication", "Message or call the people you know. Dates build the relationship track, friends help steady the sprint, and mentors sharpen prep and career momentum.");
        }

        if (_projectStudioButton.IsHovered)
        {
            return ("Build Studio", "Pick whether the next release is an app or a game, then tune its theme, tone, platform, and business model before typing starts. The studio panel now scrolls and previews the planned file set.");
        }

        if (_guideButton.IsHovered)
        {
            return ("Guide", "Reopen the multi-step tutorial with survival advice, core loop guidance, and seed/run-control explanations.");
        }

        if (_restartButton.IsHovered)
        {
            return ("Restart", $"Replay seed {_state.RunSeed} from the opening state to test a different route against the exact same scenario.");
        }

        if (_newRunButton.IsHovered)
        {
            return ("New Run", "Start a fresh run using the current seed mode from Options. Random mode rolls a new seed, manual mode reuses the chosen one.");
        }

        if (_menuButton.IsHovered)
        {
            return ("Menu", "Leave the current run and go back to the main menu.");
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

        if (_debugSnippetBounds.Contains(_mousePosition) && _state.ActiveTechDebtBug is not null)
        {
            return _debugHighlightBounds.Contains(_mousePosition)
                ? ("Highlighted Bug", $"{_state.ActiveTechDebtBug.CompilerHint} Click the red token to debug it.")
                : ("Compile Error", $"{_state.ActiveTechDebtBug.CompilerHint} The red token is the broken bit.");
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
            return _state.IsRealisticMode
                ? ("Debug In Editor", $"Realistic+ requires clicking the highlighted token in the editor. Fixing it still costs {_simulation.GetCurrentSquashBugFocusCost(_state):0} focus and recovers +4 code quality.")
                : ("Fix Bug", $"Spend {_simulation.GetCurrentSquashBugFocusCost(_state):0} focus to stop the drain and recover +4 code quality.");
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
            if (PortfolioWorkspace.IsCurrentBatchComplete(_state) &&
                _state.VersionControl.PendingChangeLines > 0)
            {
                return (
                    "Commit Release",
                    $"The release is finished, but {_state.VersionControl.PendingChangeLines} dirty lines still sit outside the last commit. Open the commit checkpoint from here, lock the batch in, then publish.");
            }

            var payoutWindow = $"${_simulation.Config.PublishAppFundsMin:0}-${_simulation.Config.PublishAppFundsMax:0}";
            return (
                "Publish App",
                $"Ship the completed release for a randomized {payoutWindow} payout, then roll into the next batch of snippets. Every dirty line must be committed before shipping.");
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
        var option = _simulation.GetFoodOption(_state, choice);
        var recommended = _simulation.GetFoodOrderModifiers(_state, choice)
            .Where(static modifier => modifier.Recommended)
            .Select(static modifier => modifier.Label)
            .ToArray();
        return (
            option.Name,
            $"{option.Description} Base cost ${_simulation.GetFoodTotalCost(_state, choice, false):0}. ETA {FormatRemainingTime(_simulation.GetFoodDeliveryDuration(_state, choice, false))}{(_simulation.AllowsExpeditedDelivery(choice) ? $", expedited {FormatRemainingTime(_simulation.GetFoodDeliveryDuration(_state, choice, true))}" : ", home-cooked only")}. Clean order notes: {string.Join(", ", recommended)}.");
    }

    private (string Title, string Body) BuildFreelanceTooltip(FreelanceGigType type)
    {
        var gig = _simulation.GetFreelanceGig(_state, type);
        return (
            gig.Name,
            $"{gig.Description} Needs at least {_simulation.Config.FreelanceMinimumFocusRequired:0} focus to start. Duration {FormatRemainingTime(gig.DurationMinutes)}. Funds +${gig.FundsGain:0}, focus -{gig.FocusCost:0}, sanity -{gig.SanityCost:0}, quality {FormatSigned(gig.CodeQualityGain)}.");
    }

    private int MeasureStatsOverlayContentHeight(int contentWidth)
    {
        const int summaryHeight = 104;
        var height = summaryHeight + 18;
        height += MeasureStatsAchievementSectionHeight(contentWidth) + 16;
        foreach (var section in RunStatsCatalog.Sections)
        {
            height += MeasureStatsSectionHeight(section, contentWidth) + 14;
        }

        return height + 12;
    }

    private int MeasureStatsAchievementSectionHeight(int contentWidth)
    {
        var height = 88;
        foreach (var achievement in RunAchievementCatalog.All)
        {
            height += MeasureStatsAchievementEntryHeight(achievement, contentWidth - 52) + 8;
        }

        return height + 8;
    }

    private int MeasureStatsAchievementEntryHeight(RunAchievementDefinition achievement, int contentWidth)
    {
        var titleHeight = UiTextBlock.MeasureWrappedHeight(_font, achievement.Title, contentWidth, 0.74f, 1f);
        var descriptionHeight = UiTextBlock.MeasureWrappedHeight(_font, achievement.Description, contentWidth, 0.6f, 1f);
        var statusHeight = UiTextBlock.MeasureWrappedHeight(_font, GetAchievementStatusText(achievement), contentWidth, 0.58f, 1f);
        return Math.Max(68, (int)MathF.Ceiling(10 + titleHeight + 6 + descriptionHeight + 8 + statusHeight + 10));
    }

    private string GetAchievementStatusText(RunAchievementDefinition achievement)
    {
        var progress = achievement.ProgressFactory(_state);
        return _state.Stats.UnlockedAchievementIds.Contains(achievement.Id)
            ? $"Unlocked | {progress}"
            : progress;
    }

    private int MeasureStatsSectionHeight(RunStatSectionDefinition section, int contentWidth)
    {
        var descriptionHeight = UiTextBlock.MeasureWrappedHeight(_font, section.Description, contentWidth - 32, 0.64f, 2f, 2);
        return (int)MathF.Ceiling(40 + descriptionHeight + 14 + (section.Stats.Count * 24) + 14);
    }

    private string? GetLatestUnlockedAchievementTitle()
    {
        if (_state.Stats.AchievementUnlockOrder.Count == 0)
        {
            return null;
        }

        var latestId = _state.Stats.AchievementUnlockOrder[^1];
        return RunAchievementCatalog.All
            .FirstOrDefault(achievement => string.Equals(achievement.Id, latestId, StringComparison.Ordinal))
            ?.Title;
    }

    private RunAchievementDefinition? GetNextLockedAchievement()
    {
        return RunAchievementCatalog.All
            .FirstOrDefault(achievement => !_state.Stats.UnlockedAchievementIds.Contains(achievement.Id));
    }

    private int MeasureActiveFreelanceGigSummaryHeight(ActiveFreelanceGig gig)
    {
        var contentWidth = _freelanceBoardViewportBounds.Width - 32f;
        var titleScale = UiTextBlock.GetFittedScale(_font, gig.Title, contentWidth, 0.84f, 0.66f);
        var titleHeight = (int)Math.Ceiling(_font.LineSpacing * titleScale);
        var metadataHeight = (int)Math.Ceiling(UiTextBlock.MeasureWrappedHeight(_font, BuildActiveFreelanceGigMetadataText(gig), contentWidth, 0.68f, 2f, 2));
        var briefHeight = (int)Math.Ceiling(UiTextBlock.MeasureWrappedHeight(_font, BuildActiveFreelanceGigBriefText(gig), contentWidth, 0.64f, 2f, 3));
        return Math.Max(112, 16 + titleHeight + 10 + metadataHeight + 8 + briefHeight + 18);
    }

    private string BuildActiveFreelanceGigMetadataText(ActiveFreelanceGig gig)
    {
        return $"{gig.ClientName}  |  {gig.FileName}";
    }

    private string BuildActiveFreelanceGigBriefText(ActiveFreelanceGig gig)
    {
        return $"{gig.Brief} Finish the file for +${gig.FundsGain:0}, -{gig.FocusCost:0} focus, -{gig.SanityCost:0} sanity, +{gig.CodeQualityGain:0.#} quality after {FormatRemainingTime(gig.DurationMinutes)}.";
    }

    private int GetWrappedLineCapacity(float availableHeight, float scale, float lineGap, int preferredMaxLines)
    {
        var lineHeight = (_font.LineSpacing * scale) + lineGap;
        var lineCount = (int)Math.Floor((availableHeight + lineGap) / lineHeight);
        return Math.Clamp(lineCount, 1, preferredMaxLines);
    }

    private string BuildProjectStudioEconomyText()
    {
        var blueprint = _state.CurrentProjectBlueprint;
        var routeText = _state.GameplayMode switch
        {
            GameplayLoopMode.Indie => $"Indie path: every completed file can now drip ${_simulation.Config.IndieProjectProgressFundsMin:0}-${_simulation.Config.IndieProjectProgressFundsMax:0} before release, scaled by this plan. It is a self-driven route, so momentum matters.",
            GameplayLoopMode.Founder => "Founder path: the plan leans harder on publish and storefront upside because there is no employer safety net underneath it. This is the grassroots business route.",
            GameplayLoopMode.Corporate => "Corporate path: the project still matters, but salary and standing cushion the run more than speculative release money while the office keeps the pressure high.",
            _ => "Interview path: this batch is mostly about portfolio proof, but publish and store multipliers still shape the cash ceiling if you ship.",
        };

        return $"{routeText} Publish x{blueprint.PublishIncomeMultiplier:0.00}, store x{blueprint.SaleIncomeMultiplier:0.00}, business model {blueprint.BusinessModel.ToLowerInvariant()}.";
    }

    private UiButton GetFoodButton(FoodChoice choice)
    {
        return choice switch
        {
            FoodChoice.Burrito => _burritoButton,
            FoodChoice.Pizza => _pizzaButton,
            FoodChoice.Dumplings => _dumplingsButton,
            FoodChoice.Ramen => _ramenButton,
            FoodChoice.RiceBowl => _riceBowlButton,
            FoodChoice.SkilletPasta => _skilletPastaButton,
            FoodChoice.MealPrepChili => _mealPrepChiliButton,
            _ => _burgerButton,
        };
    }

    private string GetLifeEventDecisionText(PendingLifeEvent lifeEvent)
    {
        return lifeEvent.Type switch
        {
            IncidentType.ComputerFreeze =>
                $"The machine is dead until you deal with it. Fixing it yourself burns {FormatRemainingTime(_simulation.Config.ComputerFreezeSelfRepairDurationMinutes)} and sanity. Tech support is faster but costs ${_simulation.Config.ComputerFreezeTechSupportFundsCost:0}. The repair shop is the slowest and most expensive, but least mentally brutal.",
            IncidentType.OnlineMatch =>
                lifeEvent.StageIndex switch
                {
                    0 => $"{lifeEvent.SubjectName} is not a one-button event anymore. Pick the opener that fits the person, not the algorithm. You need {Math.Max(1, lifeEvent.TargetScore)} good reads across three rounds to convert this into a real connection.",
                    1 => $"{lifeEvent.SubjectName} answered. Now the minigame is about momentum: plan something that fits the vibe instead of forcing a generic script.",
                    _ => $"{lifeEvent.SubjectName} gives you one last read. Land this round and the thread can turn into something real; miss and it probably dies here.",
                },
            IncidentType.PartnerCheckIn =>
                $"{lifeEvent.SubjectName} is part of the run now, not just a distraction from it. A quick reply keeps the line alive, making room for them costs ${_simulation.Config.PartnerCheckInDinnerFundsCost:0} and real time, and staying heads-down buys focus back at the cost of warmth.",
            IncidentType.CareerPathChoice =>
                $"Interview Mode is the seven-day sprint. This offer decides the long-form route: take the company path attached to the offer, bet on Founder Mode instead, or end the sprint on the win.",
            IncidentType.BossCheckIn =>
                $"{lifeEvent.SubjectName} wants a status ritual. You can over-polish the update, push back with a calm boundary, or stay late and feed the management appetite. Different bosses reward different instincts.",
            IncidentType.CoworkerInterruption =>
                $"{lifeEvent.SubjectName} needs something during office hours. Pairing can improve the work, deflecting protects the block at a social cost, and taking the fire drill buys money or standing by burning yourself harder.",
            IncidentType.FounderNaming =>
                "Founder Mode starts with choosing the studio name you are going to ship under. Pick the one that feels like something you would actually keep building when rent is due.",
            _ =>
                $"{lifeEvent.SubjectName} is queued and autoplay is ready to steal the rest of the night. Binging buys sanity at the cost of real time, one episode is the compromise line, and shutting it off protects the schedule but feels bad in the moment.",
        };
    }

    private static string[] GetLifeEventOptionLabels(PendingLifeEvent lifeEvent)
    {
        if (lifeEvent.OptionLabels.Length > 0)
        {
            return lifeEvent.OptionLabels;
        }

        return lifeEvent.Type switch
        {
            IncidentType.ComputerFreeze => ["Repair Myself", "Tech Support", "Repair Shop"],
            IncidentType.OnlineMatch => GetOnlineMatchOptionLabels(lifeEvent.StageIndex),
            IncidentType.PartnerCheckIn => ["Reply", "Make Time", "Stay Heads-Down"],
            IncidentType.CareerPathChoice => ["Take Offer", "Go Founder", "End Run Here"],
            IncidentType.BossCheckIn => ["Polish Status Deck", "Push Back Kindly", "Stay Late"],
            IncidentType.CoworkerInterruption => ["Pair Up", "Deflect Politely", "Take The Fire Drill"],
            IncidentType.FounderNaming => ["Studio A", "Studio B", "Studio C"],
            _ => ["Binge", "One Episode", "Turn It Off"],
        };
    }

    private static string[] GetOnlineMatchOptionLabels(int stageIndex)
    {
        return stageIndex switch
        {
            0 => ["Ask what they're building", "Lead with a sharp joke", "Ask how their week is really going"],
            1 => ["Coffee + walk", "Arcade + noodles", "Keep it online longer"],
            _ => ["Be honest and make time", "Over-optimize the schedule", "Keep it playful and dodge the serious part"],
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
            return $"{_state.ActiveCatInterruption.Title} is active. Click the editor {_state.ActiveCatInterruption.PatsRemaining} more times to clear it while {_state.ActiveCatInterruption.GibberishLinesTyped} gibberish lines and {_state.ActiveCatInterruption.PhantomBugCount} bug bursts stay on screen.";
        }

        if (_state.ActiveTechDebtBug is not null)
        {
            return _state.IsRealisticMode
                ? $"Realistic+ debug live. Click the highlighted token to spend {_simulation.GetCurrentSquashBugFocusCost(_state):0} focus, recover +4 quality, and stop the drain."
                : $"Live compile error on the desk. Spend {_simulation.GetCurrentSquashBugFocusCost(_state):0} focus from Alerts to stabilize code quality.";
        }

        if (_state.ActiveFoodDelivery is not null)
        {
            var foodChoice = _state.ActiveFoodDelivery.Choice;
            var prepMode = _simulation.IsHomeCooked(foodChoice) ? "is cooking" : "is on the way";
            return $"{_simulation.GetFoodOption(_state, foodChoice).Name} {prepMode}. ETA {FormatRemainingTime(_state.ActiveFoodDelivery.RemainingInGameMinutes)} before the focus refill lands.";
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

        if (_state.VersionControl.PendingChangeLines > 0)
        {
            var completedFiles = Math.Max(1, _simulation.GetUncommittedCompletedFileCount(_state));
            return $"{_state.VersionControl.PendingChangeLines} dirty LoC sit outside the last commit across {completedFiles} finished file{(completedFiles == 1 ? string.Empty : "s")}. Small commits are safer because freezes and ignored desk chaos can wipe the whole uncommitted stack.";
        }

        if (_simulation.CanEditProjectBlueprint(_state))
        {
            return $"Build Studio is still unlocked. You can pivot { _state.CurrentProjectBlueprint.Title } before the first line of this batch gets typed.";
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
            lines.Add(GetDistractionBugLine(cat.Kind, cat.VisualSeed, index));
        }

        for (var index = 0; index < gibberishCount; index++)
        {
            lines.Add(GetDistractionGibberishLine(cat.Kind, cat.VisualSeed, index));
        }

        return lines;
    }

    private static string GetDistractionBugLine(DeskDistractionKind kind, int seed, int index)
    {
        if (kind == DeskDistractionKind.PhoneBuzz)
        {
            return ((seed + index) % 6) switch
            {
                0 => "BUG: phone vibration stole the semicolon again",
                1 => "warning MD2026: unread notification storm detected",
                2 => "focusLock = false; // buzz buzz buzz",
                3 => "if (DoNotDisturb == false) return chaos;",
                4 => "// TODO: untangle notification-driven branch",
                _ => "throw new PushAlertException();",
            };
        }

        if (kind == DeskDistractionKind.NeighborNoise)
        {
            return ((seed + index) % 6) switch
            {
                0 => "BUG: wall-shaking bass invalidated the thought",
                1 => "error CS4040: concentration not found",
                2 => "if (headphones == null) return despair;",
                3 => "noiseFloor++; // there goes the sentence",
                4 => "// TODO: survive the apartment soundscape",
                _ => "throw new NeighborNoiseException();",
            };
        }

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

    private static string GetDistractionGibberishLine(DeskDistractionKind kind, int seed, int index)
    {
        if (kind == DeskDistractionKind.PhoneBuzz)
        {
            return ((seed + (index * 7)) % 8) switch
            {
                0 => "buzz buzz buzz /////",
                1 => "ping(); ping(); ping();",
                2 => "[] unread[] unread[]",
                3 => "mute??? later??? nope???",
                4 => "vvvvvvvv notification vvvvvvvv",
                5 => ">>>> lock screen <<<<",
                6 => "do_not_disturb = ???",
                _ => "reply later reply later",
            };
        }

        if (kind == DeskDistractionKind.NeighborNoise)
        {
            return ((seed + (index * 7)) % 8) switch
            {
                0 => "thump thump thump ////",
                1 => "???? bassline ????",
                2 => "wall.wall.wall.wall",
                3 => "need headphones now",
                4 => "vvvv apartment noise vvvv",
                5 => "<<<< door slam >>>>",
                6 => "focus?? not anymore??",
                _ => "zzzzzzz not from sleep",
            };
        }

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

    private string GetHousingTrackHeadline()
    {
        var apartmentCost = _simulation.GetApartmentCost(_state);
        var houseCost = _simulation.GetHouseCost(_state);
        return _state.HasHouse
            ? "House Purchased"
            : _state.HasApartment
                ? $"House ${houseCost:0}"
                : $"Apartment ${apartmentCost:0}";
    }

    private string GetHousingTrackDescription()
    {
        return _state.HasHouse
            ? "You own a place now. The run gains a passive sanity cushion and the next visible finish line is retirement."
            : _state.HasApartment
                ? $"You are out of the basement. Save for the house next. Corporate standing {_state.CorporateStanding} still nudges the price down a little."
                : "You are still in the basement. The first real housing win is moving into an apartment, then building toward a house.";
    }

    private string GetRetirementGoalHeadline()
    {
        var retirementCost = _simulation.GetRetirementCost(_state);
        return _state.HasRetired
            ? "Retired"
            : _state.HasHouse
                ? $"Savings ${retirementCost:0}"
                : "Need A House";
    }

    private string GetRetirementGoalDescription()
    {
        var retirementCost = _simulation.GetRetirementCost(_state);
        return _state.HasRetired
            ? "This run already reached the long-term retirement milestone."
            : _state.HasHouse
                ? $"Requires a house and ${retirementCost:0} in savings. Corporate, Indie, and Founder can all finish the run here."
                : "Buy a house first, then bank enough savings to retire and close the long-form life-sim arc.";
    }

    private string GetEmergencyBufferDescription()
    {
        return _state.HasFirstCoin
            ? $"The framed first coin is still intact. It steadies sanity and can still rescue one missed rent cycle for +${_simulation.Config.FirstCoinEmergencyFundsGain:0}."
            : "The first coin rescue has already been spent. There is no second emergency buffer on this desk.";
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
