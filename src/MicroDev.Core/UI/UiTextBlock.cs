using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MicroDev.Core.UI;

public static class UiTextBlock
{
    public static float DrawWrapped(
        SpriteBatch spriteBatch,
        SpriteFont font,
        string text,
        Vector2 position,
        float maxWidth,
        Color color,
        float scale = 1f,
        float lineGap = 2f,
        int maxLines = int.MaxValue)
    {
        var wrapped = WrapText(font, text, maxWidth, scale);
        var lines = wrapped.Split('\n');
        var y = position.Y;
        var lineHeight = (font.LineSpacing * scale) + lineGap;
        var linesToDraw = Math.Min(lines.Length, maxLines);

        for (var index = 0; index < linesToDraw; index++)
        {
            spriteBatch.DrawString(font, lines[index], new Vector2(position.X, y), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            y += lineHeight;
        }

        return linesToDraw == 0 ? 0 : (linesToDraw * lineHeight) - lineGap;
    }

    public static float MeasureWrappedHeight(
        SpriteFont font,
        string text,
        float maxWidth,
        float scale = 1f,
        float lineGap = 2f,
        int maxLines = int.MaxValue)
    {
        var wrapped = WrapText(font, text, maxWidth, scale);
        var lineCount = Math.Min(wrapped.Split('\n').Length, maxLines);
        if (lineCount <= 0)
        {
            return 0;
        }

        return (lineCount * ((font.LineSpacing * scale) + lineGap)) - lineGap;
    }

    public static string TrimToWidth(SpriteFont font, string text, float maxWidth, float scale = 1f)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (font.MeasureString(text).X * scale <= maxWidth)
        {
            return text;
        }

        const string ellipsis = "...";
        var trimmed = text;

        while (trimmed.Length > 1 &&
               font.MeasureString(trimmed + ellipsis).X * scale > maxWidth)
        {
            trimmed = trimmed[..^1];
        }

        return trimmed + ellipsis;
    }

    public static (string Text, float Scale) FitText(
        SpriteFont font,
        string text,
        float maxWidth,
        float preferredScale = 1f,
        float minimumScale = UiTypography.Small)
    {
        if (string.IsNullOrEmpty(text))
        {
            return (string.Empty, preferredScale);
        }

        if (maxWidth <= 0f)
        {
            return (TrimToWidth(font, text, 1f, preferredScale), preferredScale);
        }

        var fittedScale = GetFittedScale(font, text, maxWidth, preferredScale, minimumScale);
        var displayText = font.MeasureString(text).X * fittedScale <= maxWidth
            ? text
            : TrimToWidth(font, text, maxWidth, fittedScale);

        return (displayText, fittedScale);
    }

    public static float GetFittedScale(
        SpriteFont font,
        string text,
        float maxWidth,
        float preferredScale = 1f,
        float minimumScale = UiTypography.Small)
    {
        if (string.IsNullOrEmpty(text) || maxWidth <= 0f)
        {
            return preferredScale;
        }

        var fittedScale = preferredScale;
        while (fittedScale > minimumScale &&
               font.MeasureString(text).X * fittedScale > maxWidth)
        {
            fittedScale -= 0.02f;
        }

        return Math.Max(minimumScale, fittedScale);
    }

    public static string WrapText(SpriteFont font, string text, float maxWidth, float scale = 1f)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text.Replace("\r", string.Empty);
        var paragraphs = normalized.Split('\n');
        var builder = new StringBuilder();

        for (var paragraphIndex = 0; paragraphIndex < paragraphs.Length; paragraphIndex++)
        {
            var paragraph = paragraphs[paragraphIndex];
            if (string.IsNullOrWhiteSpace(paragraph))
            {
                if (paragraphIndex > 0)
                {
                    builder.Append('\n');
                }

                continue;
            }

            var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var currentLine = string.Empty;

            foreach (var word in words)
            {
                var candidate = string.IsNullOrEmpty(currentLine)
                    ? word
                    : $"{currentLine} {word}";

                if (font.MeasureString(candidate).X * scale <= maxWidth || string.IsNullOrEmpty(currentLine))
                {
                    currentLine = candidate;
                }
                else
                {
                    if (builder.Length > 0)
                    {
                        builder.Append('\n');
                    }

                    builder.Append(currentLine);
                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                if (builder.Length > 0)
                {
                    builder.Append('\n');
                }

                builder.Append(currentLine);
            }
        }

        return builder.ToString();
    }
}
