using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MicroDev.Core.UI;

public sealed class UiLabel
{
    public string Text { get; set; } = string.Empty;

    public Vector2 Position { get; set; }

    public Color Color { get; set; } = UiTheme.TextPrimary;

    public float Scale { get; set; } = 1f;

    public void Draw(SpriteBatch spriteBatch, SpriteFont font)
    {
        Draw(spriteBatch, font, Text, Position, Color, Scale);
    }

    public static void Draw(
        SpriteBatch spriteBatch,
        SpriteFont font,
        string text,
        Vector2 position,
        Color color,
        float scale = 1f)
    {
        spriteBatch.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
