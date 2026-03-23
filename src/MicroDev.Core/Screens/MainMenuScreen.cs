using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MicroDev.Core.Audio;
using MicroDev.Core.Input;
using MicroDev.Core.UI;

namespace MicroDev.Core.Screens;

public sealed class MainMenuScreen : IScreen
{
    private readonly Rectangle _heroBounds = new(104, 96, 708, 430);
    private readonly Rectangle _detailBounds = new(848, 130, 620, 292);
    private readonly Rectangle _footerBounds = new(104, 556, 1364, 182);
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;
    private readonly GameAudio _audio;
    private readonly Point _virtualResolution;
    private readonly Action _startGame;
    private readonly Action _showOptions;
    private readonly Action _exitGame;
    private readonly UiButton _startButton = new("Start");
    private readonly UiButton _optionsButton = new("Options");
    private readonly UiButton _exitButton = new("Exit");

    public MainMenuScreen(
        SpriteFont font,
        Texture2D pixel,
        GameAudio audio,
        Point virtualResolution,
        Action startGame,
        Action showOptions,
        Action exitGame)
    {
        _font = font;
        _pixel = pixel;
        _audio = audio;
        _virtualResolution = virtualResolution;
        _startGame = startGame;
        _showOptions = showOptions;
        _exitGame = exitGame;

        _startButton.TextScale = 0.92f;
        _optionsButton.TextScale = 0.86f;
        _exitButton.TextScale = 0.86f;
    }

    public void Update(GameTime gameTime, InputSnapshot input)
    {
        UpdateLayout();

        if (_startButton.Update(input))
        {
            _audio.PlayButtonClick();
            _startGame();
            return;
        }

        if (_optionsButton.Update(input))
        {
            _audio.PlayButtonClick();
            _showOptions();
            return;
        }

        if (_exitButton.Update(input))
        {
            _audio.PlayButtonClick();
            _exitGame();
        }
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        DrawBackdrop(spriteBatch);

        UiPanel.Draw(spriteBatch, _pixel, _heroBounds, UiTheme.PanelFill, UiTheme.EditorBorder, 3);
        UiPanel.Draw(spriteBatch, _pixel, _detailBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _footerBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        spriteBatch.Draw(_pixel, new Rectangle(_heroBounds.X + 1, _heroBounds.Y + 1, _heroBounds.Width - 2, 5), UiTheme.Accent);
        spriteBatch.Draw(_pixel, new Rectangle(_detailBounds.X + 1, _detailBounds.Y + 1, _detailBounds.Width - 2, 4), UiTheme.AccentDim);

        var buttonRailX = _heroBounds.Right - 280;
        spriteBatch.Draw(_pixel, new Rectangle(buttonRailX - 28, _heroBounds.Y + 34, 2, _heroBounds.Height - 68), UiTheme.AccentDim * 0.8f);

        UiLabel.Draw(spriteBatch, _font, "Micro Dev", new Vector2(_heroBounds.X + 30, _heroBounds.Y + 34), UiTheme.TextPrimary, 1.96f);
        UiLabel.Draw(spriteBatch, _font, "One more file. One more day. One real shot.", new Vector2(_heroBounds.X + 32, _heroBounds.Y + 96), UiTheme.Accent, 0.72f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Build a real portfolio one file at a time, survive rent, and make the recruiter callback arrive before burnout does.",
            new Vector2(_heroBounds.X + 32, _heroBounds.Y + 136),
            348,
            UiTheme.TextMuted,
            0.86f,
            4f,
            4);

        UiLabel.Draw(spriteBatch, _font, "Current Build Loop", new Vector2(_detailBounds.X + 24, _detailBounds.Y + 26), UiTheme.Accent, 0.96f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Type real C# portfolio files, react to bugs and recruiter pings, buy efficiency upgrades, and keep your focus stable enough to finish the week.",
            new Vector2(_detailBounds.X + 24, _detailBounds.Y + 62),
            _detailBounds.Width - 48,
            UiTheme.TextPrimary,
            0.8f,
            3f,
            4);

        UiLabel.Draw(spriteBatch, _font, "This iteration adds", new Vector2(_detailBounds.X + 24, _detailBounds.Y + 170), UiTheme.TextMuted, 0.76f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Tooltips, food variety, freelance gig choices, and a cleaner front-end menu flow.",
            new Vector2(_detailBounds.X + 24, _detailBounds.Y + 198),
            _detailBounds.Width - 48,
            UiTheme.Success,
            0.78f,
            3f,
            3);

        _startButton.Draw(spriteBatch, _pixel, _font);
        _optionsButton.Draw(spriteBatch, _pixel, _font);
        _exitButton.Draw(spriteBatch, _pixel, _font);

        UiLabel.Draw(spriteBatch, _font, "Week Survival Brief", new Vector2(_footerBounds.X + 24, _footerBounds.Y + 20), UiTheme.TextPrimary, 0.94f);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            "Rent still lands at midnight. Food and sleep protect focus, upgrades buy back efficiency, and every finished file pushes the portfolio toward a real application.",
            new Vector2(_footerBounds.X + 24, _footerBounds.Y + 58),
            _footerBounds.Width - 48,
            UiTheme.TextMuted,
            0.78f,
            3f,
            4);
    }

    private void DrawBackdrop(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y), UiTheme.DesktopBackground);
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, 220), UiTheme.DesktopGlow * 0.24f);
        spriteBatch.Draw(_pixel, new Rectangle(88, 84, 600, 578), UiTheme.AccentDim * 0.10f);
        spriteBatch.Draw(_pixel, new Rectangle(0, 252, _virtualResolution.X, 1), UiTheme.AccentDim * 0.22f);
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, 2), UiTheme.Accent);
    }

    private void UpdateLayout()
    {
        var buttonX = _heroBounds.Right - 236;
        _startButton.Bounds = new Rectangle(buttonX, _heroBounds.Y + 120, 188, 52);
        _optionsButton.Bounds = new Rectangle(buttonX, _heroBounds.Y + 186, 188, 46);
        _exitButton.Bounds = new Rectangle(buttonX, _heroBounds.Y + 242, 188, 46);
    }
}
