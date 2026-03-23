using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MicroDev.Core.Input;

namespace MicroDev.Core.Screens;

public sealed class ScreenManager
{
    public IScreen? CurrentScreen { get; private set; }

    public void SetScreen(IScreen screen)
    {
        CurrentScreen = screen;
    }

    public void Update(GameTime gameTime, InputSnapshot input)
    {
        CurrentScreen?.Update(gameTime, input);
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        CurrentScreen?.Draw(gameTime, spriteBatch);
    }
}
