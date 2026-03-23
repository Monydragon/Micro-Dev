using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MicroDev.Core.UI;

public sealed class UiProgressBar
{
    public UiProgressBar(string label, Color fillColor)
    {
        Label = label;
        FillColor = fillColor;
    }

    public Rectangle Bounds { get; set; }

    public string Label { get; }

    public double Value { get; set; }

    public double MaxValue { get; set; } = 100;

    public Color FillColor { get; }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font)
    {
        UiLabel.Draw(spriteBatch, font, Label, new Vector2(Bounds.X, Bounds.Y - 20), UiTheme.TextMuted, 0.8f);

        var valueText = $"{Value:0}/{MaxValue:0}";
        var size = font.MeasureString(valueText) * 0.8f;
        var valuePosition = new Vector2(Bounds.Right - size.X, Bounds.Y - 20);
        spriteBatch.DrawString(font, valueText, valuePosition, UiTheme.TextPrimary, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);

        UiPanel.Draw(spriteBatch, pixel, Bounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        var fillWidth = (int)MathF.Round((float)(Math.Clamp(Value / MaxValue, 0d, 1d) * (Bounds.Width - 6)));
        var fillBounds = new Rectangle(Bounds.X + 3, Bounds.Y + 3, fillWidth, Bounds.Height - 6);
        if (fillBounds.Width > 0)
        {
            spriteBatch.Draw(pixel, fillBounds, FillColor);
        }
    }
}
