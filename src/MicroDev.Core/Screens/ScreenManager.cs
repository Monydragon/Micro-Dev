using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MicroDev.Core.Input;

namespace MicroDev.Core.Screens;

public sealed class ScreenManager
{
    private const float FadeDurationSeconds = 0.18f;
    private IScreen? _pendingScreen;
    private ScreenTransitionPhase _transitionPhase;
    private float _transitionTimer;

    public IScreen? CurrentScreen { get; private set; }

    public float TransitionOpacity { get; private set; }

    public void SetScreen(IScreen screen, bool immediate = false)
    {
        if (CurrentScreen is null || immediate)
        {
            CurrentScreen = screen;
            _pendingScreen = null;
            _transitionPhase = ScreenTransitionPhase.None;
            _transitionTimer = 0f;
            TransitionOpacity = 0f;
            return;
        }

        _pendingScreen = screen;
        _transitionPhase = ScreenTransitionPhase.FadeOut;
        _transitionTimer = 0f;
        TransitionOpacity = 0f;
    }

    public void Update(GameTime gameTime, InputSnapshot input)
    {
        if (_transitionPhase == ScreenTransitionPhase.None)
        {
            CurrentScreen?.Update(gameTime, input);
            return;
        }

        var elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var frozenTime = new GameTime(gameTime.TotalGameTime, TimeSpan.Zero);

        if (_transitionPhase == ScreenTransitionPhase.FadeOut)
        {
            CurrentScreen?.Update(frozenTime, default);
            _transitionTimer += elapsedSeconds;
            TransitionOpacity = MathHelper.Clamp(_transitionTimer / FadeDurationSeconds, 0f, 1f);

            if (_transitionTimer < FadeDurationSeconds)
            {
                return;
            }

            CurrentScreen = _pendingScreen;
            _pendingScreen = null;
            _transitionPhase = ScreenTransitionPhase.FadeIn;
            _transitionTimer = FadeDurationSeconds;
            CurrentScreen?.Update(frozenTime, default);
            return;
        }

        CurrentScreen?.Update(frozenTime, default);
        _transitionTimer -= elapsedSeconds;
        TransitionOpacity = MathHelper.Clamp(_transitionTimer / FadeDurationSeconds, 0f, 1f);
        if (_transitionTimer > 0f)
        {
            return;
        }

        _transitionPhase = ScreenTransitionPhase.None;
        _transitionTimer = 0f;
        TransitionOpacity = 0f;
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        CurrentScreen?.Draw(gameTime, spriteBatch);
    }

    private enum ScreenTransitionPhase
    {
        None,
        FadeOut,
        FadeIn,
    }
}
