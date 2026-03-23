using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MicroDev.Core.Input;

namespace MicroDev.Core.UI;

public sealed class UiButton
{
    public UiButton(string text)
    {
        Text = text;
    }

    public Rectangle Bounds { get; set; }

    public string Text { get; set; }

    public bool Enabled { get; set; } = true;

    public bool IsHovered { get; private set; }

    public bool IsSelected { get; set; }

    public float TextScale { get; set; } = 0.84f;

    public bool Update(InputSnapshot input)
    {
        IsHovered = input.IsMouseOverGame && Bounds.Contains(input.MousePosition);
        return Enabled && input.IsLeftClickInside(Bounds);
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font)
    {
        if (Bounds == Rectangle.Empty)
        {
            return;
        }

        var fillColor = !Enabled
            ? UiTheme.ButtonDisabled
            : IsSelected
                ? UiTheme.ButtonSelected
                : IsHovered
                    ? UiTheme.ButtonHover
                    : UiTheme.ButtonFill;

        var borderColor = Enabled ? UiTheme.PanelBorder : UiTheme.TextMuted;

        UiPanel.Draw(spriteBatch, pixel, Bounds, fillColor, borderColor, 2);
        spriteBatch.Draw(pixel, new Rectangle(Bounds.X + 1, Bounds.Y + 1, Bounds.Width - 2, 3), Enabled ? UiTheme.AccentDim : UiTheme.ButtonDisabled);

        var displayText = UiTextBlock.TrimToWidth(font, Text, Bounds.Width - 18, TextScale);
        var textSize = font.MeasureString(displayText) * TextScale;
        var position = new Vector2(
            Bounds.Center.X - (textSize.X / 2f),
            Bounds.Center.Y - (textSize.Y / 2f));

        spriteBatch.DrawString(
            font,
            displayText,
            position,
            Enabled ? UiTheme.TextPrimary : UiTheme.TextMuted,
            0f,
            Vector2.Zero,
            TextScale,
            SpriteEffects.None,
            0f);
    }
}
