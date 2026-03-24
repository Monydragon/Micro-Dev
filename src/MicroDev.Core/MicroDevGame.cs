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
    private MouseState _previousMouseState;
    private KeyboardState _previousKeyboardState;
    private Point _browserBackBufferSize = new(VirtualWidth, VirtualHeight);
    private Point _browserInputViewportSize = new(VirtualWidth, VirtualHeight);
    private BrowserViewportState? _pendingBrowserViewportState;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private SpriteFont _font = null!;

    public MicroDevGame()
    {
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
        _virtualCanvas.EnsureResources(GraphicsDevice);
        ApplyRuntimeSettings();

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);

        _font = Content.Load<SpriteFont>("Fonts/Ui");
        ShowMainMenu();
    }

    protected override void Update(GameTime gameTime)
    {
        ApplyPendingBrowserBackBufferSize();

        var keyboardState = Keyboard.GetState();
        var escapePressed = keyboardState.IsKeyDown(Keys.Escape) &&
                            !_previousKeyboardState.IsKeyDown(Keys.Escape);
        if (escapePressed &&
            _screenManager.CurrentScreen is MainMenuScreen)
        {
            RequestExit();
            return;
        }

        _virtualCanvas.EnsureResources(GraphicsDevice);
        _virtualCanvas.UpdateDestination(GraphicsDevice.Viewport);

        var currentMouseState = Mouse.GetState();
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

        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _screenManager.Draw(gameTime, _spriteBatch);
        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _spriteBatch.Draw(_virtualCanvas.RenderTarget, _virtualCanvas.DestinationRectangle, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _audio.Dispose();
            _virtualCanvas.Dispose();
            _pixel?.Dispose();
            _spriteBatch?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void ShowMainMenu()
    {
        _screenManager.SetScreen(new MainMenuScreen(
            _font,
            _pixel,
            _audio,
            _settings,
            new Point(VirtualWidth, VirtualHeight),
            StartRun,
            ShowOptions,
            RequestExit));
    }

    private void ShowOptions()
    {
        _screenManager.SetScreen(new OptionsScreen(
            _font,
            _pixel,
            _audio,
            _settings,
            OperatingSystem.IsBrowser(),
            new Point(VirtualWidth, VirtualHeight),
            ShowMainMenu,
            ApplyRuntimeSettings));
    }

    private void StartRun()
    {
        var simulation = new SimulationEngine(SimulationConfig.ForDifficulty(_settings.SelectedDifficulty));
        var incidentScheduler = new IncidentScheduler();
        _screenManager.SetScreen(new WorkspaceScreen(
            _font,
            _pixel,
            simulation,
            incidentScheduler,
            _audio,
            new Point(VirtualWidth, VirtualHeight),
            ShowWorkspaceOptions));
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
        _audio.Enabled = _settings.SoundEffectsEnabled;
        _audio.MusicEnabled = _settings.MusicEnabled;
        _audio.MasterVolume = _settings.MasterVolume;
        _audio.SoundEffectsVolume = _settings.SoundEffectsVolume;
        _audio.MusicVolume = _settings.MusicVolume;

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

    private readonly record struct BrowserViewportState(Point RenderSize, Point InputViewportSize);
}
