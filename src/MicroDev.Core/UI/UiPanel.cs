using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MicroDev.Core.UI;

public sealed class UiPanel
{
    public UiPanel(Rectangle bounds)
    {
        Bounds = bounds;
    }

    public Rectangle Bounds { get; set; }

    public Color FillColor { get; set; } = UiTheme.PanelFill;

    public Color BorderColor { get; set; } = UiTheme.PanelBorder;

    public int BorderThickness { get; set; } = 2;

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        Draw(spriteBatch, pixel, Bounds, FillColor, BorderColor, BorderThickness);
    }

    public static void Draw(
        SpriteBatch spriteBatch,
        Texture2D pixel,
        Rectangle bounds,
        Color fillColor,
        Color borderColor,
        int borderThickness = 2)
    {
        spriteBatch.Draw(pixel, bounds, fillColor);

        if (borderThickness <= 0)
        {
            return;
        }

        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, borderThickness), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - borderThickness, bounds.Width, borderThickness), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, borderThickness, bounds.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(bounds.Right - borderThickness, bounds.Y, borderThickness, bounds.Height), borderColor);
    }
}
