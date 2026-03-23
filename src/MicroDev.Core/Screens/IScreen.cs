using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MicroDev.Core.Input;

namespace MicroDev.Core.Screens;

public interface IScreen
{
    void Update(GameTime gameTime, InputSnapshot input);

    void Draw(GameTime gameTime, SpriteBatch spriteBatch);
}
