using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MicroDev.Core.Audio;
using MicroDev.Core.Input;
using MicroDev.Core.Rendering;
using MicroDev.Core.Screens;
using MicroDev.Core.Simulation;
using MicroDev.Core.UI;

namespace MicroDev.Core;

public sealed class MicroDevGame : Game
{
    public const int VirtualWidth = 1600;
    public const int VirtualHeight = 900;

    private readonly GraphicsDeviceManager _graphics;
    private readonly VirtualCanvas _virtualCanvas;
    private readonly ScreenManager _screenManager = new();
    private readonly GameSettings _settings = new();
    private readonly GameAudio _audio = new();
    private readonly Dictionary<UiFontOption, SpriteFont> _fonts = [];
    private readonly string? _captureDirectory;
    private MouseState _previousMouseState;
    private KeyboardState _previousKeyboardState;
    private Point _browserBackBufferSize = new(VirtualWidth, VirtualHeight);
    private Point _browserInputViewportSize = new(VirtualWidth, VirtualHeight);
    private BrowserViewportState? _pendingBrowserViewportState;
    private RasterizerState _uiRasterizerState = null!;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private SpriteFont _font = null!;
    private WorkspaceScreen? _workspaceScreen;
    private UiCaptureStep[] _captureSteps = [];
    private int _captureStepIndex = -1;
    private int _captureFramesRemaining;
    private bool _captureSavePending;

    public MicroDevGame(string? captureDirectory = null)
    {
        _captureDirectory = string.IsNullOrWhiteSpace(captureDirectory)
            ? null
            : Path.GetFullPath(captureDirectory);
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = VirtualWidth,
            PreferredBackBufferHeight = VirtualHeight,
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        if (!OperatingSystem.IsBrowser())
        {
            Window.AllowUserResizing = true;
            Window.Title = "Micro Dev";
        }

        _virtualCanvas = new VirtualCanvas(VirtualWidth, VirtualHeight);
    }

    protected override void Initialize()
    {
        base.Initialize();
        ApplyRuntimeSettings();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _uiRasterizerState = new RasterizerState
        {
            ScissorTestEnable = true,
        };
        _virtualCanvas.EnsureResources(GraphicsDevice);
        ApplyRuntimeSettings();

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);

        foreach (var option in UiFontCatalog.All)
        {
            _fonts[option] = Content.Load<SpriteFont>(UiFontCatalog.GetAssetName(option));
        }

        _font = GetSelectedFont();
        ApplyRuntimeSettings();
        ShowMainMenu();
        InitializeCaptureSequence();
    }

    protected override void Update(GameTime gameTime)
    {
        ApplyPendingBrowserBackBufferSize();

        _virtualCanvas.EnsureResources(GraphicsDevice);
        _virtualCanvas.UpdateDestination(GraphicsDevice.Viewport);

        var keyboardState = Keyboard.GetState();
        var currentMouseState = Mouse.GetState();
        if (IsCapturingUi)
        {
            _screenManager.Update(gameTime, default);
            UpdateCaptureState();
            UpdateAudio(gameTime);
            _previousMouseState = currentMouseState;
            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
            return;
        }

        var escapePressed = keyboardState.IsKeyDown(Keys.Escape) &&
                            !_previousKeyboardState.IsKeyDown(Keys.Escape);
        if (escapePressed &&
            _screenManager.CurrentScreen is MainMenuScreen)
        {
            RequestExit();
            return;
        }

        var input = InputSnapshot.Create(currentMouseState, _previousMouseState, _virtualCanvas);
        _screenManager.Update(gameTime, input);
        UpdateAudio(gameTime);
        _previousMouseState = currentMouseState;
        _previousKeyboardState = keyboardState;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        ApplyPendingBrowserBackBufferSize();
        _virtualCanvas.EnsureResources(GraphicsDevice);
        _virtualCanvas.UpdateDestination(GraphicsDevice.Viewport);

        GraphicsDevice.SetRenderTarget(_virtualCanvas.RenderTarget);
        GraphicsDevice.Clear(UiTheme.DesktopBackground);

        GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, VirtualWidth, VirtualHeight);
        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp, rasterizerState: _uiRasterizerState);
        _screenManager.Draw(gameTime, _spriteBatch);
        if (_screenManager.TransitionOpacity > 0f)
        {
            _spriteBatch.Draw(
                _pixel,
                new Rectangle(0, 0, VirtualWidth, VirtualHeight),
                UiTheme.WithOpacity(UiTheme.DesktopBackground, _screenManager.TransitionOpacity * 0.92f));
        }
        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _spriteBatch.Draw(_virtualCanvas.RenderTarget, _virtualCanvas.DestinationRectangle, Color.White);
        _spriteBatch.End();

        SaveCaptureFrameIfNeeded();

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _audio.Dispose();
            _virtualCanvas.Dispose();
            _uiRasterizerState?.Dispose();
            _pixel?.Dispose();
            _spriteBatch?.Dispose();
        }

        base.Dispose(disposing);
    }

    private bool IsCapturingUi => !string.IsNullOrWhiteSpace(_captureDirectory);

    private void ShowMainMenu(bool immediate = false)
    {
        _workspaceScreen = null;
        _screenManager.SetScreen(new MainMenuScreen(
            _font,
            _pixel,
            _audio,
            _settings,
            new Point(VirtualWidth, VirtualHeight),
            () => StartRun(),
            () => ShowOptions(),
            RequestExit),
            immediate);
    }

    private void ShowOptions(bool immediate = false)
    {
        _screenManager.SetScreen(new OptionsScreen(
            _font,
            _pixel,
            _audio,
            _settings,
            OperatingSystem.IsBrowser(),
            new Point(VirtualWidth, VirtualHeight),
            () => ShowMainMenu(),
            ApplyRuntimeSettings),
            immediate);
    }

    private void StartRun(bool immediate = false)
    {
        var simulation = new SimulationEngine(SimulationConfig.ForDifficulty(_settings.SelectedDifficulty));
        var incidentScheduler = new IncidentScheduler();
        _workspaceScreen = new WorkspaceScreen(
            _font,
            _pixel,
            simulation,
            incidentScheduler,
            _audio,
            new Point(VirtualWidth, VirtualHeight),
            ShowWorkspaceOptions);
        _screenManager.SetScreen(_workspaceScreen, immediate);
    }

    private void UpdateAudio(GameTime gameTime)
    {
        var mode = BackgroundMusicMode.None;
        var sanityRatio = 1f;

        switch (_screenManager.CurrentScreen)
        {
            case WorkspaceScreen workspaceScreen:
                mode = BackgroundMusicMode.Workspace;
                sanityRatio = workspaceScreen.GetSanityRatio();
                break;
            case MainMenuScreen:
            case OptionsScreen:
                mode = BackgroundMusicMode.Menu;
                break;
        }

        _audio.UpdateMusic(gameTime.ElapsedGameTime.TotalSeconds, mode, sanityRatio);
    }

    private void ShowWorkspaceOptions(WorkspaceScreen workspace)
    {
        _workspaceScreen = workspace;
        _screenManager.SetScreen(new OptionsScreen(
            _font,
            _pixel,
            _audio,
            _settings,
            OperatingSystem.IsBrowser(),
            new Point(VirtualWidth, VirtualHeight),
            () => _screenManager.SetScreen(workspace),
            ApplyRuntimeSettings));
    }

    private void ApplyRuntimeSettings()
    {
        UiTheme.Apply(_settings.ThemeMode);
        _audio.Enabled = _settings.SoundEffectsEnabled;
        _audio.MusicEnabled = _settings.MusicEnabled;
        _audio.MasterVolume = _settings.MasterVolume;
        _audio.SoundEffectsVolume = _settings.SoundEffectsVolume;
        _audio.MusicVolume = _settings.MusicVolume;

        if (_fonts.Count > 0)
        {
            _font = GetSelectedFont();
            ApplyFontToScreen(_screenManager.CurrentScreen, _font);
            if (_workspaceScreen is not null &&
                !ReferenceEquals(_workspaceScreen, _screenManager.CurrentScreen))
            {
                ApplyFontToScreen(_workspaceScreen, _font);
            }
        }

        var backBufferSize = OperatingSystem.IsBrowser()
            ? _browserBackBufferSize
            : _settings.PreferredResolution;

        _graphics.PreferredBackBufferWidth = backBufferSize.X;
        _graphics.PreferredBackBufferHeight = backBufferSize.Y;
        if (!OperatingSystem.IsBrowser())
        {
            _graphics.HardwareModeSwitch = _settings.WindowMode == WindowModeSetting.Fullscreen;
            _graphics.IsFullScreen = _settings.WindowMode != WindowModeSetting.Windowed;
            Window.AllowUserResizing = _settings.WindowMode == WindowModeSetting.Windowed;
        }

        _graphics.ApplyChanges();
    }

    private SpriteFont GetSelectedFont()
    {
        return _fonts.TryGetValue(_settings.UiFont, out var selectedFont)
            ? selectedFont
            : _fonts[UiFontOption.Consolas];
    }

    private static void ApplyFontToScreen(IScreen? screen, SpriteFont font)
    {
        if (screen is IUiFontAware fontAware)
        {
            fontAware.ApplyFont(font);
        }
    }

    private void RequestExit()
    {
        try
        {
            Exit();
        }
        catch (PlatformNotSupportedException) when (OperatingSystem.IsBrowser())
        {
        }
    }

    public void SetBrowserBackBufferSize(int width, int height)
    {
        SetBrowserViewport(width, height, width, height);
    }

    public void SetBrowserViewport(int renderWidth, int renderHeight, int inputWidth, int inputHeight)
    {
        if (!OperatingSystem.IsBrowser() ||
            renderWidth <= 0 ||
            renderHeight <= 0 ||
            inputWidth <= 0 ||
            inputHeight <= 0)
        {
            return;
        }

        _pendingBrowserViewportState = new BrowserViewportState(
            new Point(renderWidth, renderHeight),
            new Point(inputWidth, inputHeight));
    }

    private void ApplyPendingBrowserBackBufferSize()
    {
        if (!OperatingSystem.IsBrowser() || !_pendingBrowserViewportState.HasValue)
        {
            return;
        }

        var pendingState = _pendingBrowserViewportState.Value;
        if (pendingState.RenderSize == _browserBackBufferSize &&
            pendingState.InputViewportSize == _browserInputViewportSize)
        {
            _pendingBrowserViewportState = null;
            return;
        }

        _browserBackBufferSize = pendingState.RenderSize;
        _browserInputViewportSize = pendingState.InputViewportSize;
        _pendingBrowserViewportState = null;
        _virtualCanvas.SetInputScale(
            _browserBackBufferSize.X / (float)_browserInputViewportSize.X,
            _browserBackBufferSize.Y / (float)_browserInputViewportSize.Y);
        ApplyRuntimeSettings();
    }

    private void InitializeCaptureSequence()
    {
        if (!IsCapturingUi)
        {
            return;
        }

        Directory.CreateDirectory(_captureDirectory!);
        _captureSteps =
        [
            new UiCaptureStep("01-main-menu-dark-consolas.png", game =>
            {
                game._settings.SelectedDifficulty = GameDifficulty.Normal;
                game._settings.UiFont = UiFontOption.Consolas;
                game._settings.ThemeMode = UiThemeMode.Dark;
                game.ApplyRuntimeSettings();
                game.ShowMainMenu(immediate: true);
            }),
            new UiCaptureStep("02-main-menu-light-cascadia-mono.png", game =>
            {
                game._settings.SelectedDifficulty = GameDifficulty.ContinualUpgradeLoop;
                game._settings.UiFont = UiFontOption.CascadiaMono;
                game._settings.ThemeMode = UiThemeMode.Light;
                game.ApplyRuntimeSettings();
                game.ShowMainMenu(immediate: true);
            }),
            new UiCaptureStep("03-options-layout-closed.png", game =>
            {
                game._settings.UiFont = UiFontOption.Consolas;
                game._settings.ThemeMode = UiThemeMode.Dark;
                game.ApplyRuntimeSettings();
                game.ShowOptions(immediate: true);
            }),
            new UiCaptureStep("04-options-font-palette.png", game =>
            {
                game._settings.UiFont = UiFontOption.CascadiaCode;
                game._settings.ThemeMode = UiThemeMode.Dark;
                game.ApplyRuntimeSettings();
                game.ShowOptions(immediate: true);
                if (game._screenManager.CurrentScreen is OptionsScreen optionsScreen)
                {
                    var clickPosition = new Point(210, 330);
                    optionsScreen.Update(CreateCaptureFrameTime(), CreatePressedInput(clickPosition));
                    optionsScreen.Update(CreateCaptureFrameTime(), CreateReleasedInput(clickPosition));
                }
            }),
            new UiCaptureStep("05-options-light-bahnschrift.png", game =>
            {
                game._settings.UiFont = UiFontOption.Bahnschrift;
                game._settings.ThemeMode = UiThemeMode.Light;
                game.ApplyRuntimeSettings();
                game.ShowOptions(immediate: true);
            }),
            new UiCaptureStep("06-options-audio-scroll-light.png", game =>
            {
                game._settings.UiFont = UiFontOption.Bahnschrift;
                game._settings.ThemeMode = UiThemeMode.Light;
                game.ApplyRuntimeSettings();
                game.ShowOptions(immediate: true);
                if (game._screenManager.CurrentScreen is OptionsScreen optionsScreen)
                {
                    optionsScreen.Update(CreateCaptureFrameTime(), CreateScrollInput(new Point(820, 540), -360));
                }
            }),
            new UiCaptureStep("07-workspace-dark-consolas.png", game =>
            {
                game._settings.SelectedDifficulty = GameDifficulty.Normal;
                game._settings.UiFont = UiFontOption.Consolas;
                game._settings.ThemeMode = UiThemeMode.Dark;
                game.ApplyRuntimeSettings();
                game.StartRun(immediate: true);
                game.PrimeWorkspacePreview();
            }),
            new UiCaptureStep("08-workspace-light-bahnschrift.png", game =>
            {
                game._settings.SelectedDifficulty = GameDifficulty.Endless;
                game._settings.UiFont = UiFontOption.Bahnschrift;
                game._settings.ThemeMode = UiThemeMode.Light;
                game.ApplyRuntimeSettings();
                game.StartRun(immediate: true);
                game.PrimeWorkspacePreview();
            }),
            new UiCaptureStep("09-workspace-food-overlay-dark.png", game =>
            {
                game._settings.SelectedDifficulty = GameDifficulty.Normal;
                game._settings.UiFont = UiFontOption.Consolas;
                game._settings.ThemeMode = UiThemeMode.Dark;
                game.ApplyRuntimeSettings();
                game.StartRun(immediate: true);
                game.PrimeWorkspacePreview();
                if (game._workspaceScreen is not null)
                {
                    var clickPosition = new Point(1283, 421);
                    game._workspaceScreen.Update(CreateCaptureFrameTime(), CreatePressedInput(clickPosition));
                    game._workspaceScreen.Update(CreateCaptureFrameTime(), CreateReleasedInput(clickPosition));
                }
            }),
            new UiCaptureStep("10-workspace-freelance-overlay-dark.png", game =>
            {
                game._settings.SelectedDifficulty = GameDifficulty.Normal;
                game._settings.UiFont = UiFontOption.Consolas;
                game._settings.ThemeMode = UiThemeMode.Dark;
                game.ApplyRuntimeSettings();
                game.StartRun(immediate: true);
                game.PrimeWorkspacePreview();
                if (game._workspaceScreen is not null)
                {
                    var clickPosition = new Point(1477, 421);
                    game._workspaceScreen.Update(CreateCaptureFrameTime(), CreatePressedInput(clickPosition));
                    game._workspaceScreen.Update(CreateCaptureFrameTime(), CreateReleasedInput(clickPosition));
                }
            }),
            new UiCaptureStep("11-workspace-upgrades-overlay-dark.png", game =>
            {
                game._settings.SelectedDifficulty = GameDifficulty.Normal;
                game._settings.UiFont = UiFontOption.Consolas;
                game._settings.ThemeMode = UiThemeMode.Dark;
                game.ApplyRuntimeSettings();
                game.StartRun(immediate: true);
                game.PrimeWorkspacePreview();
                if (game._workspaceScreen is not null)
                {
                    var clickPosition = new Point(1477, 461);
                    game._workspaceScreen.Update(CreateCaptureFrameTime(), CreatePressedInput(clickPosition));
                    game._workspaceScreen.Update(CreateCaptureFrameTime(), CreateReleasedInput(clickPosition));
                }
            }),
        ];

        StartNextCaptureStep();
    }

    private void StartNextCaptureStep()
    {
        _captureStepIndex++;
        if (_captureStepIndex >= _captureSteps.Length)
        {
            Exit();
            return;
        }

        _captureSteps[_captureStepIndex].Configure(this);
        _captureFramesRemaining = 2;
        _captureSavePending = false;
    }

    private void UpdateCaptureState()
    {
        if (!IsCapturingUi || _captureSavePending)
        {
            return;
        }

        if (_captureFramesRemaining <= 0)
        {
            _captureSavePending = true;
            return;
        }

        _captureFramesRemaining--;
    }

    private void SaveCaptureFrameIfNeeded()
    {
        if (!IsCapturingUi || !_captureSavePending)
        {
            return;
        }

        var captureStep = _captureSteps[_captureStepIndex];
        var outputPath = Path.Combine(_captureDirectory!, captureStep.FileName);
        using (var stream = File.Create(outputPath))
        {
            _virtualCanvas.RenderTarget.SaveAsPng(stream, _virtualCanvas.RenderTarget.Width, _virtualCanvas.RenderTarget.Height);
        }

        _captureSavePending = false;
        StartNextCaptureStep();
    }

    private void PrimeWorkspacePreview()
    {
        if (_workspaceScreen is null)
        {
            return;
        }

        var clickTime = CreateCaptureFrameTime();
        var editorClick = CreatePressedInput(new Point(160, 188));
        for (var index = 0; index < 18; index++)
        {
            _workspaceScreen.Update(clickTime, editorClick);
        }

        _workspaceScreen.Update(new GameTime(TimeSpan.Zero, TimeSpan.Zero), default);
    }

    private static GameTime CreateCaptureFrameTime()
    {
        return new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1d / 60d));
    }

    private static InputSnapshot CreatePressedInput(Point mousePosition)
    {
        return new InputSnapshot(mousePosition, true, true, true, false, Point.Zero, 0);
    }

    private static InputSnapshot CreateReleasedInput(Point mousePosition)
    {
        return new InputSnapshot(mousePosition, true, false, false, true, Point.Zero, 0);
    }

    private static InputSnapshot CreateScrollInput(Point mousePosition, int scrollWheelDelta)
    {
        return new InputSnapshot(mousePosition, true, false, false, false, Point.Zero, scrollWheelDelta);
    }

    private readonly record struct BrowserViewportState(Point RenderSize, Point InputViewportSize);

    private readonly record struct UiCaptureStep(string FileName, Action<MicroDevGame> Configure);
}
