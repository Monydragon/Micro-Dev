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
        Window.AllowUserResizing = true;
        Window.Title = "Micro Dev";

        _virtualCanvas = new VirtualCanvas(VirtualWidth, VirtualHeight);
    }

    protected override void Initialize()
    {
        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _virtualCanvas.EnsureResources(GraphicsDevice);
        _audio.Enabled = _settings.SoundEffectsEnabled;

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);

        _font = Content.Load<SpriteFont>("Fonts/Ui");
        ShowMainMenu();
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            Exit();
            return;
        }

        _virtualCanvas.EnsureResources(GraphicsDevice);
        _virtualCanvas.UpdateDestination(GraphicsDevice.Viewport);

        var currentMouseState = Mouse.GetState();
        var input = InputSnapshot.Create(currentMouseState, _previousMouseState, _virtualCanvas);
        _screenManager.Update(gameTime, input);
        _previousMouseState = currentMouseState;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
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
            new Point(VirtualWidth, VirtualHeight),
            StartRun,
            ShowOptions,
            Exit));
    }

    private void ShowOptions()
    {
        _screenManager.SetScreen(new OptionsScreen(
            _font,
            _pixel,
            _audio,
            _settings,
            new Point(VirtualWidth, VirtualHeight),
            ShowMainMenu));
    }

    private void StartRun()
    {
        var simulation = new SimulationEngine(SimulationConfig.Default);
        var incidentScheduler = new IncidentScheduler();
        _screenManager.SetScreen(new WorkspaceScreen(
            _font,
            _pixel,
            simulation,
            incidentScheduler,
            _audio,
            new Point(VirtualWidth, VirtualHeight)));
    }
}
