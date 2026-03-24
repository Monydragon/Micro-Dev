using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MicroDev.Core.Input;

namespace MicroDev.Core.UI;

public sealed class UiButton
{
    private float _pressAnimation;
    private bool _pressed;

    public UiButton(string text)
    {
        Text = text;
    }

    public Rectangle Bounds { get; set; }

    public string Text { get; set; }

    public bool Enabled { get; set; } = true;

    public bool IsHovered { get; private set; }

    public bool IsSelected { get; set; }

    public float TextScale { get; set; } = UiTypography.Button;

    public UiTextAlignment TextAlignment { get; set; }

    public int HorizontalPadding { get; set; } = 14;

    public Color? AccentColor { get; set; }

    public void AdvanceAnimation(float elapsedSeconds)
    {
        _pressAnimation = MathF.Max(0f, _pressAnimation - (elapsedSeconds * 5.25f));
    }

    public bool Update(InputSnapshot input, bool activateOnRelease = false)
    {
        IsHovered = input.IsMouseOverGame && Bounds.Contains(input.MousePosition);
        if (!Enabled)
        {
            _pressed = false;
            return false;
        }

        if (!activateOnRelease)
        {
            if (!input.IsLeftClickInside(Bounds))
            {
                return false;
            }

            _pressAnimation = 1f;
            return true;
        }

        if (input.LeftClicked && IsHovered)
        {
            _pressed = true;
            return false;
        }

        if (_pressed && input.LeftReleased)
        {
            var activated = IsHovered;
            _pressed = false;
            if (activated)
            {
                _pressAnimation = 1f;
            }

            return activated;
        }

        if (!input.LeftDown && !input.LeftReleased)
        {
            _pressed = false;
        }

        return false;
    }

    public void CancelInteraction()
    {
        _pressed = false;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font)
    {
        if (Bounds == Rectangle.Empty)
        {
            return;
        }

        var accentColor = AccentColor ?? UiTheme.Accent;
        var fillColor = !Enabled
            ? UiTheme.ButtonDisabled
            : IsSelected
                ? UiTheme.Mix(UiTheme.ButtonSelected, accentColor, 0.76f)
                : IsHovered
                    ? UiTheme.Mix(UiTheme.ButtonHover, accentColor, 0.34f)
                    : UiTheme.Mix(UiTheme.ButtonFill, accentColor, 0.16f);

        var borderColor = Enabled
            ? UiTheme.Mix(UiTheme.PanelBorder, accentColor, IsSelected ? 0.72f : 0.34f + (_pressAnimation * 0.18f))
            : UiTheme.TextMuted;
        var accentStripColor = Enabled
            ? UiTheme.Mix(UiTheme.AccentDim, accentColor, 0.7f)
            : UiTheme.ButtonDisabled;
        var pressOffset = (int)MathF.Round(_pressAnimation * 2f);
        var drawBounds = new Rectangle(Bounds.X, Bounds.Y + pressOffset, Bounds.Width, Math.Max(28, Bounds.Height - pressOffset));

        UiPanel.Draw(spriteBatch, pixel, drawBounds, fillColor, borderColor, 2);
        if (IsSelected)
        {
            UiPanel.Draw(
                spriteBatch,
                pixel,
                new Rectangle(drawBounds.X - 2, drawBounds.Y - 2, drawBounds.Width + 4, drawBounds.Height + 4),
                Color.Transparent,
                UiTheme.WithOpacity(accentColor, 0.9f),
                2);
        }
        spriteBatch.Draw(pixel, new Rectangle(drawBounds.X + 1, drawBounds.Y + 1, drawBounds.Width - 2, 3), accentStripColor);

        if (_pressAnimation > 0.01f)
        {
            DrawDigitalPulse(spriteBatch, pixel, drawBounds, accentColor);
        }

        var maxTextWidth = Math.Max(12, drawBounds.Width - (HorizontalPadding * 2));
        var minimumScale = Math.Max(UiTypography.Small, TextScale - 0.18f);
        var (displayText, fittedScale) = UiTextBlock.FitText(font, Text, maxTextWidth, TextScale, minimumScale);
        var textSize = font.MeasureString(displayText) * fittedScale;
        var positionX = TextAlignment == UiTextAlignment.Left
            ? drawBounds.X + HorizontalPadding
            : drawBounds.Center.X - (textSize.X / 2f);
        var position = new Vector2(
            positionX,
            drawBounds.Center.Y - (textSize.Y / 2f));

        spriteBatch.DrawString(
            font,
            displayText,
            position,
            !Enabled
                ? UiTheme.TextMuted
                : IsSelected
                ? UiTheme.Mix(UiTheme.TextPrimary, accentColor, 0.24f)
                : UiTheme.TextPrimary,
            0f,
            Vector2.Zero,
            fittedScale,
            SpriteEffects.None,
            0f);
    }

    private void DrawDigitalPulse(SpriteBatch spriteBatch, Texture2D pixel, Rectangle bounds, Color accentColor)
    {
        var lineColor = UiTheme.WithOpacity(accentColor, 0.22f + (_pressAnimation * 0.24f));
        var innerGlow = UiTheme.WithOpacity(accentColor, 0.08f + (_pressAnimation * 0.16f));
        var scanlineHeight = Math.Max(2, (int)MathF.Round(3f + (_pressAnimation * 4f)));

        spriteBatch.Draw(pixel, new Rectangle(bounds.X + 3, bounds.Y + 6, bounds.Width - 6, scanlineHeight), innerGlow);
        spriteBatch.Draw(pixel, new Rectangle(bounds.X + 3, bounds.Center.Y - 1, bounds.Width - 6, 2), lineColor);
        spriteBatch.Draw(pixel, new Rectangle(bounds.X + 3, bounds.Bottom - 9, bounds.Width - 6, 2), lineColor);
        spriteBatch.Draw(pixel, new Rectangle(bounds.Right - 10, bounds.Y + 4, 3, bounds.Height - 8), UiTheme.WithOpacity(accentColor, 0.18f));
    }
}
