using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MicroDev.Core.Audio;
using MicroDev.Core.Input;
using MicroDev.Core.UI;

namespace MicroDev.Core.Screens;

public sealed class OptionsScreen : IScreen, IUiFontAware
{
    private const int ScrollbarWidth = 12;
    private const int ScrollActivationThreshold = 12;
    private const float ScrollWheelScale = 0.48f;

    private static readonly Point[] ResolutionOptions =
    [
        new(1280, 720),
        new(1366, 768),
        new(1600, 900),
        new(1920, 1080),
        new(2560, 1440),
    ];

    private readonly Texture2D _pixel;
    private readonly GameAudio _audio;
    private readonly GameSettings _settings;
    private readonly bool _isBrowserPlatform;
    private readonly Point _virtualResolution;
    private readonly Action _goBack;
    private readonly Action _applySettings;
    private readonly UiButton _themeToggleButton = new("Theme: Dark");
    private readonly UiButton _fontButton = new("Primary Font");
    private readonly UiButton[] _fontOptionButtons;
    private readonly UiButton _displayModeButton = new("Windowed");
    private readonly UiButton _resolutionButton = new("1600 x 900");
    private readonly UiButton[] _resolutionOptionButtons =
    [
        new UiButton("1280 x 720"),
        new UiButton("1366 x 768"),
        new UiButton("1600 x 900"),
        new UiButton("1920 x 1080"),
        new UiButton("2560 x 1440"),
    ];
    private readonly UiButton _soundToggleButton = new("Sound Effects: ON");
    private readonly UiButton _musicToggleButton = new("Background Music: ON");
    private readonly UiButton _masterDownButton = new("-");
    private readonly UiButton _masterUpButton = new("+");
    private readonly UiButton _effectsDownButton = new("-");
    private readonly UiButton _effectsUpButton = new("+");
    private readonly UiButton _musicDownButton = new("-");
    private readonly UiButton _musicUpButton = new("+");
    private readonly UiButton _backButton = new("Back");

    private SpriteFont _font;
    private Rectangle _modalBounds;
    private Rectangle _headerBandBounds;
    private Rectangle _contentFrameBounds;
    private Rectangle _contentViewportBounds;
    private Rectangle _scrollbarTrackBounds;
    private Rectangle _scrollbarThumbBounds;
    private Rectangle _appearanceBounds;
    private Rectangle _displayBounds;
    private Rectangle _audioBounds;
    private Rectangle _notesBounds;
    private Rectangle _appearancePreviewBounds;
    private Rectangle _audioToggleBandBounds;
    private Rectangle _masterVolumeRowBounds;
    private Rectangle _effectsVolumeRowBounds;
    private Rectangle _musicVolumeRowBounds;
    private Rectangle _notesSummaryDividerBounds;
    private float _scrollOffset;
    private float _maxScrollOffset;
    private float _contentHeight;
    private bool _resolutionDropdownOpen;
    private bool _fontDropdownOpen;
    private bool _scrollGestureArmed;
    private bool _isScrollDragging;
    private int _scrollDragStartMouseY;
    private float _scrollDragStartOffset;

    public OptionsScreen(
        SpriteFont font,
        Texture2D pixel,
        GameAudio audio,
        GameSettings settings,
        bool isBrowserPlatform,
        Point virtualResolution,
        Action goBack,
        Action applySettings)
    {
        _font = font;
        _pixel = pixel;
        _audio = audio;
        _settings = settings;
        _isBrowserPlatform = isBrowserPlatform;
        _virtualResolution = virtualResolution;
        _goBack = goBack;
        _applySettings = applySettings;
        _fontOptionButtons = UiFontCatalog.All
            .Select(static option => new UiButton(UiFontCatalog.GetDisplayName(option))
            {
                TextAlignment = UiTextAlignment.Left,
                HorizontalPadding = 12,
            })
            .ToArray();

        ConfigureButtons();
        SyncButtonLabels();
        UpdateLayout();
    }

    public void ApplyFont(SpriteFont font)
    {
        _font = font;
    }

    public void Update(GameTime gameTime, InputSnapshot input)
    {
        ConfigureButtons();
        SyncButtonLabels();
        AdvanceButtonAnimations((float)gameTime.ElapsedGameTime.TotalSeconds);
        UpdateLayout();

        if (HandleDropdownInput(input))
        {
            return;
        }

        var contentInput = GetContentInput(input);
        HandleScrollInput(input, contentInput);
        UpdateLayout();

        if (_isScrollDragging)
        {
            return;
        }

        if (TryHandleContentButtons(contentInput))
        {
            return;
        }

        if (_backButton.Update(input, activateOnRelease: true))
        {
            _audio.PlayButtonClick();
            _goBack();
        }
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        ConfigureButtons();
        UpdateLayout();
        DrawBackdrop(spriteBatch);

        UiPanel.Draw(spriteBatch, _pixel, _modalBounds, UiTheme.WithOpacity(UiTheme.PanelFill, 0.95f), UiTheme.EditorBorder, 3);
        UiPanel.Draw(spriteBatch, _pixel, _headerBandBounds, UiTheme.WithOpacity(UiTheme.PanelRaised, 0.94f), Color.Transparent, 0);
        spriteBatch.Draw(_pixel, new Rectangle(_modalBounds.X + 1, _modalBounds.Y + 1, _modalBounds.Width - 2, 4), UiTheme.Accent);
        spriteBatch.Draw(_pixel, new Rectangle(_headerBandBounds.X + 20, _headerBandBounds.Bottom, _headerBandBounds.Width - 40, 1), UiTheme.WithOpacity(UiTheme.PanelBorder, 0.84f));
        DrawHeader(spriteBatch);
        UiPanel.Draw(
            spriteBatch,
            _pixel,
            _contentFrameBounds,
            UiTheme.WithOpacity(UiTheme.PanelMuted, 0.76f),
            UiTheme.WithOpacity(UiTheme.PanelBorder, 0.92f),
            2);

        var graphicsDevice = spriteBatch.GraphicsDevice;
        var previousScissor = graphicsDevice.ScissorRectangle;
        graphicsDevice.ScissorRectangle = _contentViewportBounds;

        UiPanel.Draw(spriteBatch, _pixel, _appearanceBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _displayBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _audioBounds, UiTheme.PanelRaised, UiTheme.PanelBorder, 2);
        UiPanel.Draw(spriteBatch, _pixel, _notesBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        DrawAppearancePanel(spriteBatch);
        DrawDisplayPanel(spriteBatch);
        DrawAudioPanel(spriteBatch);
        DrawNotesPanel(spriteBatch);

        graphicsDevice.ScissorRectangle = previousScissor;

        DrawContentFrameOverlay(spriteBatch);
        DrawScrollbar(spriteBatch);
        _backButton.Draw(spriteBatch, _pixel, _font);
        DrawDropdownOverlays(spriteBatch);
    }

    private void ConfigureButtons()
    {
        foreach (var button in GetAllButtons())
        {
            button.TextScale = UiTypography.Button;
        }

        _themeToggleButton.AccentColor = UiTheme.Accent;
        _fontButton.AccentColor = UiTheme.Success;
        _displayModeButton.AccentColor = UiTheme.Accent;
        _resolutionButton.AccentColor = UiTheme.Warning;
        _soundToggleButton.AccentColor = UiTheme.Accent;
        _musicToggleButton.AccentColor = UiTheme.Success;
        _masterDownButton.AccentColor = UiTheme.Accent;
        _masterUpButton.AccentColor = UiTheme.Accent;
        _effectsDownButton.AccentColor = UiTheme.Warning;
        _effectsUpButton.AccentColor = UiTheme.Warning;
        _musicDownButton.AccentColor = UiTheme.Success;
        _musicUpButton.AccentColor = UiTheme.Success;
        _backButton.AccentColor = UiTheme.Warning;

        _themeToggleButton.TextAlignment = UiTextAlignment.Left;
        _fontButton.TextAlignment = UiTextAlignment.Left;
        _displayModeButton.TextAlignment = UiTextAlignment.Left;
        _resolutionButton.TextAlignment = UiTextAlignment.Left;
        _soundToggleButton.TextAlignment = UiTextAlignment.Left;
        _musicToggleButton.TextAlignment = UiTextAlignment.Left;

        _themeToggleButton.HorizontalPadding = 14;
        _fontButton.HorizontalPadding = 14;
        _displayModeButton.HorizontalPadding = 14;
        _resolutionButton.HorizontalPadding = 14;
        _soundToggleButton.HorizontalPadding = 14;
        _musicToggleButton.HorizontalPadding = 14;

        foreach (var button in GetVolumeButtons())
        {
            button.TextScale = 0.94f;
            button.HorizontalPadding = 0;
            button.TextAlignment = UiTextAlignment.Center;
        }

        foreach (var button in _fontOptionButtons)
        {
            button.TextAlignment = UiTextAlignment.Left;
            button.HorizontalPadding = 12;
        }

        foreach (var button in _resolutionOptionButtons)
        {
            button.TextAlignment = UiTextAlignment.Left;
            button.HorizontalPadding = 12;
        }
    }

    private void AdvanceButtonAnimations(float elapsedSeconds)
    {
        foreach (var button in GetAllButtons())
        {
            button.AdvanceAnimation(elapsedSeconds);
        }
    }

    private IEnumerable<UiButton> GetAllButtons()
    {
        foreach (var button in GetContentButtons())
        {
            yield return button;
        }

        yield return _backButton;

        foreach (var button in _fontOptionButtons)
        {
            yield return button;
        }

        foreach (var button in _resolutionOptionButtons)
        {
            yield return button;
        }
    }

    private IEnumerable<UiButton> GetContentButtons()
    {
        yield return _themeToggleButton;
        yield return _fontButton;
        yield return _displayModeButton;
        yield return _resolutionButton;
        yield return _soundToggleButton;
        yield return _musicToggleButton;
        yield return _masterDownButton;
        yield return _masterUpButton;
        yield return _effectsDownButton;
        yield return _effectsUpButton;
        yield return _musicDownButton;
        yield return _musicUpButton;
    }

    private IEnumerable<UiButton> GetVolumeButtons()
    {
        yield return _masterDownButton;
        yield return _masterUpButton;
        yield return _effectsDownButton;
        yield return _effectsUpButton;
        yield return _musicDownButton;
        yield return _musicUpButton;
    }

    private void DrawBackdrop(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, _virtualResolution.Y), UiTheme.DesktopBackground);
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, 248), UiTheme.WithOpacity(UiTheme.DesktopGlow, 0.42f));
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, _virtualResolution.X, 2), UiTheme.Accent);

        for (var index = 0; index < 7; index++)
        {
            var y = 124 + (index * 94);
            spriteBatch.Draw(_pixel, new Rectangle(0, y, _virtualResolution.X, 1), UiTheme.WithOpacity(UiTheme.AccentDim, 0.16f));
        }
    }

    private void DrawHeader(SpriteBatch spriteBatch)
    {
        UiLabel.Draw(spriteBatch, _font, "Options", new Vector2(_modalBounds.X + 28, _modalBounds.Y + 22), UiTheme.TextPrimary, UiTypography.Title);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            GetHeaderSummaryText(),
            new Vector2(_modalBounds.X + 28, _modalBounds.Y + 58),
            _modalBounds.Width - 268,
            UiTheme.TextMuted,
            UiTypography.Body,
            3f,
            2);
    }

    private void DrawAppearancePanel(SpriteBatch spriteBatch)
    {
        var left = _appearanceBounds.X + 20;
        UiLabel.Draw(spriteBatch, _font, "Appearance", new Vector2(left, _appearanceBounds.Y + 18), UiTheme.TextPrimary, UiTypography.Section);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            GetAppearanceIntroText(),
            new Vector2(left, _appearanceBounds.Y + 48),
            _appearanceBounds.Width - 40,
            UiTheme.TextMuted,
            UiTypography.Caption,
            2f,
            3);
        spriteBatch.Draw(
            _pixel,
            new Rectangle(_appearanceBounds.X + 20, _themeToggleButton.Bounds.Y - 10, _appearanceBounds.Width - 40, 1),
            UiTheme.WithOpacity(UiTheme.PanelBorder, 0.72f));

        _themeToggleButton.Draw(spriteBatch, _pixel, _font);
        _fontButton.Draw(spriteBatch, _pixel, _font);

        UiPanel.Draw(spriteBatch, _pixel, _appearancePreviewBounds, UiTheme.EditorFill, UiTheme.EditorBorder, 2);
        spriteBatch.Draw(_pixel, new Rectangle(_appearancePreviewBounds.X + 1, _appearancePreviewBounds.Y + 1, _appearancePreviewBounds.Width - 2, 3), UiTheme.Success);
        UiLabel.Draw(spriteBatch, _font, "Font Preview", new Vector2(_appearancePreviewBounds.X + 12, _appearancePreviewBounds.Y + 12), UiTheme.Success, UiTypography.Caption);
        UiLabel.Draw(
            spriteBatch,
            _font,
            "public static async Task ShipAsync()",
            new Vector2(_appearancePreviewBounds.X + 12, _appearancePreviewBounds.Y + 44),
            UiTheme.TextPrimary,
            UiTypography.Body);
    }

    private void DrawDisplayPanel(SpriteBatch spriteBatch)
    {
        var left = _displayBounds.X + 20;
        UiLabel.Draw(spriteBatch, _font, "Display", new Vector2(left, _displayBounds.Y + 18), UiTheme.TextPrimary, UiTypography.Section);

        if (_isBrowserPlatform)
        {
            UiTextBlock.DrawWrapped(
                spriteBatch,
                _font,
                GetDisplayIntroText(),
                new Vector2(left, _displayBounds.Y + 48),
                _displayBounds.Width - 40,
                UiTheme.TextMuted,
                UiTypography.Body,
                3f,
                4);
            return;
        }

        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            GetDisplayIntroText(),
            new Vector2(left, _displayBounds.Y + 48),
            _displayBounds.Width - 40,
            UiTheme.TextMuted,
            UiTypography.Caption,
            2f,
            2);
        spriteBatch.Draw(
            _pixel,
            new Rectangle(_displayBounds.X + 20, _displayModeButton.Bounds.Y - 38, _displayBounds.Width - 40, 1),
            UiTheme.WithOpacity(UiTheme.PanelBorder, 0.72f));

        UiLabel.Draw(spriteBatch, _font, "Window Mode", new Vector2(left, _displayModeButton.Bounds.Y - 26), UiTheme.TextPrimary, UiTypography.Caption);
        _displayModeButton.Draw(spriteBatch, _pixel, _font);

        UiLabel.Draw(spriteBatch, _font, "Resolution", new Vector2(left, _resolutionButton.Bounds.Y - 26), UiTheme.TextPrimary, UiTypography.Caption);
        _resolutionButton.Draw(spriteBatch, _pixel, _font);
    }

    private void DrawAudioPanel(SpriteBatch spriteBatch)
    {
        var left = _audioBounds.X + 20;
        UiLabel.Draw(spriteBatch, _font, "Audio", new Vector2(left, _audioBounds.Y + 18), UiTheme.TextPrimary, UiTypography.Section);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            GetAudioIntroText(),
            new Vector2(left, _audioBounds.Y + 48),
            _audioBounds.Width - 40,
            UiTheme.TextMuted,
            UiTypography.Caption,
            2f,
            2);
        spriteBatch.Draw(
            _pixel,
            new Rectangle(_audioBounds.X + 20, _audioToggleBandBounds.Y - 10, _audioBounds.Width - 40, 1),
            UiTheme.WithOpacity(UiTheme.PanelBorder, 0.72f));
        UiPanel.Draw(
            spriteBatch,
            _pixel,
            _audioToggleBandBounds,
            UiTheme.WithOpacity(UiTheme.PanelMuted, 0.7f),
            UiTheme.WithOpacity(UiTheme.PanelBorder, 0.84f),
            1);

        _soundToggleButton.Draw(spriteBatch, _pixel, _font);
        _musicToggleButton.Draw(spriteBatch, _pixel, _font);

        DrawVolumeRow(
            spriteBatch,
            "Master Volume",
            _masterVolumeRowBounds,
            _settings.MasterVolume,
            UiTheme.Accent,
            _masterDownButton,
            _masterUpButton);
        DrawVolumeRow(
            spriteBatch,
            "SFX Volume",
            _effectsVolumeRowBounds,
            _settings.SoundEffectsVolume,
            UiTheme.Warning,
            _effectsDownButton,
            _effectsUpButton);
        DrawVolumeRow(
            spriteBatch,
            "BGM Volume",
            _musicVolumeRowBounds,
            _settings.MusicVolume,
            UiTheme.Success,
            _musicDownButton,
            _musicUpButton);
    }

    private void DrawNotesPanel(SpriteBatch spriteBatch)
    {
        var left = _notesBounds.X + 20;
        UiLabel.Draw(spriteBatch, _font, "Quick Notes", new Vector2(left, _notesBounds.Y + 18), UiTheme.TextPrimary, UiTypography.Section);
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            GetNotesBodyText(),
            new Vector2(left, _notesBounds.Y + 48),
            _notesBounds.Width - 40,
            UiTheme.TextMuted,
            UiTypography.Caption,
            2f,
            2);

        spriteBatch.Draw(_pixel, _notesSummaryDividerBounds, UiTheme.WithOpacity(UiTheme.PanelBorder, 0.8f));
        UiTextBlock.DrawWrapped(
            spriteBatch,
            _font,
            GetNotesSummaryText(),
            new Vector2(left, _notesSummaryDividerBounds.Bottom + 10),
            _notesBounds.Width - 40,
            UiTheme.Accent,
            UiTypography.Caption,
            2f,
            3);
    }

    private void DrawContentFrameOverlay(SpriteBatch spriteBatch)
    {
        var maskFill = UiTheme.Mix(UiTheme.PanelMuted, UiTheme.PanelFill, 0.72f);
        var borderColor = UiTheme.WithOpacity(UiTheme.PanelBorder, 0.96f);

        if (_contentViewportBounds.Y > _contentFrameBounds.Y)
        {
            spriteBatch.Draw(
                _pixel,
                new Rectangle(_contentFrameBounds.X, _contentFrameBounds.Y, _contentFrameBounds.Width, _contentViewportBounds.Y - _contentFrameBounds.Y),
                maskFill);
        }

        if (_contentFrameBounds.Bottom > _contentViewportBounds.Bottom)
        {
            spriteBatch.Draw(
                _pixel,
                new Rectangle(_contentFrameBounds.X, _contentViewportBounds.Bottom, _contentFrameBounds.Width, _contentFrameBounds.Bottom - _contentViewportBounds.Bottom),
                maskFill);
        }

        if (_contentViewportBounds.X > _contentFrameBounds.X)
        {
            spriteBatch.Draw(
                _pixel,
                new Rectangle(_contentFrameBounds.X, _contentViewportBounds.Y, _contentViewportBounds.X - _contentFrameBounds.X, _contentViewportBounds.Height),
                maskFill);
        }

        if (_contentFrameBounds.Right > _contentViewportBounds.Right)
        {
            spriteBatch.Draw(
                _pixel,
                new Rectangle(_contentViewportBounds.Right, _contentViewportBounds.Y, _contentFrameBounds.Right - _contentViewportBounds.Right, _contentViewportBounds.Height),
                maskFill);
        }

        UiPanel.Draw(
            spriteBatch,
            _pixel,
            _contentFrameBounds,
            Color.Transparent,
            borderColor,
            2);
        spriteBatch.Draw(_pixel, new Rectangle(_contentViewportBounds.X, _contentViewportBounds.Y, _contentViewportBounds.Width, 1), borderColor);
        spriteBatch.Draw(_pixel, new Rectangle(_contentViewportBounds.X, _contentViewportBounds.Bottom - 1, _contentViewportBounds.Width, 1), borderColor);
    }

    private void DrawScrollbar(SpriteBatch spriteBatch)
    {
        if (_maxScrollOffset <= 0f)
        {
            return;
        }

        UiPanel.Draw(
            spriteBatch,
            _pixel,
            _scrollbarTrackBounds,
            UiTheme.WithOpacity(UiTheme.PanelMuted, 0.8f),
            UiTheme.WithOpacity(UiTheme.PanelBorder, 0.8f),
            1);
        UiPanel.Draw(
            spriteBatch,
            _pixel,
            _scrollbarThumbBounds,
            UiTheme.WithOpacity(UiTheme.Accent, 0.52f),
            UiTheme.Accent,
            1);
    }

    private bool HandleDropdownInput(InputSnapshot input)
    {
        if (_fontDropdownOpen)
        {
            var dropdownBounds = GetFontDropdownBounds();
            var isPointerInside = dropdownBounds.Contains(input.MousePosition) || _fontButton.Bounds.Contains(input.MousePosition);

            if (_fontButton.Update(input, activateOnRelease: true))
            {
                _fontDropdownOpen = false;
                return true;
            }

            for (var index = 0; index < _fontOptionButtons.Length; index++)
            {
                if (!_fontOptionButtons[index].Update(input, activateOnRelease: true))
                {
                    continue;
                }

                _settings.UiFont = UiFontCatalog.All[index];
                _fontDropdownOpen = false;
                ApplyAndPreview();
                return true;
            }

            if (input.LeftClicked &&
                !dropdownBounds.Contains(input.MousePosition) &&
                !_fontButton.Bounds.Contains(input.MousePosition))
            {
                _fontDropdownOpen = false;
                return true;
            }

            if (isPointerInside && (input.LeftClicked || input.LeftReleased || input.ScrollWheelDelta != 0))
            {
                return true;
            }
        }

        if (_resolutionDropdownOpen)
        {
            var dropdownBounds = GetResolutionDropdownBounds();
            var isPointerInside = dropdownBounds.Contains(input.MousePosition) || _resolutionButton.Bounds.Contains(input.MousePosition);

            if (_resolutionButton.Update(input, activateOnRelease: true))
            {
                _resolutionDropdownOpen = false;
                return true;
            }

            for (var index = 0; index < _resolutionOptionButtons.Length; index++)
            {
                if (!_resolutionOptionButtons[index].Update(input, activateOnRelease: true))
                {
                    continue;
                }

                _settings.PreferredResolution = ResolutionOptions[index];
                _resolutionDropdownOpen = false;
                ApplyAndPreview();
                return true;
            }

            if (input.LeftClicked &&
                !dropdownBounds.Contains(input.MousePosition) &&
                !_resolutionButton.Bounds.Contains(input.MousePosition))
            {
                _resolutionDropdownOpen = false;
                return true;
            }

            if (isPointerInside && (input.LeftClicked || input.LeftReleased || input.ScrollWheelDelta != 0))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryHandleContentButtons(InputSnapshot input)
    {
        if (_themeToggleButton.Update(input, activateOnRelease: true))
        {
            _settings.ThemeMode = _settings.ThemeMode == UiThemeMode.Dark
                ? UiThemeMode.Light
                : UiThemeMode.Dark;
            ApplyAndPreview();
            return true;
        }

        if (_fontButton.Update(input, activateOnRelease: true))
        {
            _fontDropdownOpen = !_fontDropdownOpen;
            _resolutionDropdownOpen = false;
            _audio.PlayButtonClick();
            return true;
        }

        if (!_isBrowserPlatform &&
            _displayModeButton.Update(input, activateOnRelease: true))
        {
            _settings.WindowMode = GetNextWindowMode(_settings.WindowMode);
            ApplyAndPreview();
            return true;
        }

        if (!_isBrowserPlatform &&
            _resolutionButton.Update(input, activateOnRelease: true))
        {
            _resolutionDropdownOpen = !_resolutionDropdownOpen;
            _fontDropdownOpen = false;
            _audio.PlayButtonClick();
            return true;
        }

        if (_soundToggleButton.Update(input, activateOnRelease: true))
        {
            var enabling = !_settings.SoundEffectsEnabled;
            if (!enabling)
            {
                _audio.PlayButtonClick();
            }

            _settings.SoundEffectsEnabled = enabling;
            _applySettings();

            if (enabling)
            {
                _audio.PlayButtonClick();
            }

            return true;
        }

        if (_musicToggleButton.Update(input, activateOnRelease: true))
        {
            _settings.MusicEnabled = !_settings.MusicEnabled;
            ApplyAndPreview();
            return true;
        }

        return TryAdjustVolume(_masterDownButton, () => _settings.MasterVolume, value => _settings.MasterVolume = value, -0.05f, input) ||
               TryAdjustVolume(_masterUpButton, () => _settings.MasterVolume, value => _settings.MasterVolume = value, 0.05f, input) ||
               TryAdjustVolume(_effectsDownButton, () => _settings.SoundEffectsVolume, value => _settings.SoundEffectsVolume = value, -0.05f, input) ||
               TryAdjustVolume(_effectsUpButton, () => _settings.SoundEffectsVolume, value => _settings.SoundEffectsVolume = value, 0.05f, input) ||
               TryAdjustVolume(_musicDownButton, () => _settings.MusicVolume, value => _settings.MusicVolume = value, -0.05f, input) ||
               TryAdjustVolume(_musicUpButton, () => _settings.MusicVolume, value => _settings.MusicVolume = value, 0.05f, input);
    }

    private void HandleScrollInput(InputSnapshot rawInput, InputSnapshot contentInput)
    {
        if (_maxScrollOffset <= 0f)
        {
            _scrollOffset = 0f;
            _scrollGestureArmed = false;
            _isScrollDragging = false;
            return;
        }

        if (contentInput.ScrollWheelDelta != 0)
        {
            CloseDropdowns();
            SetScrollOffset(_scrollOffset - (contentInput.ScrollWheelDelta * ScrollWheelScale));
        }

        if (contentInput.LeftClicked)
        {
            _scrollGestureArmed = true;
            _isScrollDragging = false;
            _scrollDragStartMouseY = rawInput.MousePosition.Y;
            _scrollDragStartOffset = _scrollOffset;
        }

        if (_scrollGestureArmed &&
            rawInput.LeftDown)
        {
            var dragDistance = rawInput.MousePosition.Y - _scrollDragStartMouseY;
            if (!_isScrollDragging &&
                Math.Abs(dragDistance) >= ScrollActivationThreshold)
            {
                _isScrollDragging = true;
                CloseDropdowns();
                CancelContentButtonInteractions();
            }

            if (_isScrollDragging)
            {
                SetScrollOffset(_scrollDragStartOffset - dragDistance);
            }
        }

        if (_scrollGestureArmed &&
            rawInput.LeftReleased)
        {
            _scrollGestureArmed = false;
            _isScrollDragging = false;
        }
    }

    private void CancelContentButtonInteractions()
    {
        foreach (var button in GetContentButtons())
        {
            button.CancelInteraction();
        }

        foreach (var button in _fontOptionButtons)
        {
            button.CancelInteraction();
        }

        foreach (var button in _resolutionOptionButtons)
        {
            button.CancelInteraction();
        }
    }

    private void CloseDropdowns()
    {
        _fontDropdownOpen = false;
        _resolutionDropdownOpen = false;
    }

    private InputSnapshot GetContentInput(InputSnapshot input)
    {
        var isMouseOverViewport = input.IsMouseOverGame && _contentViewportBounds.Contains(input.MousePosition);
        return new InputSnapshot(
            input.MousePosition,
            isMouseOverViewport,
            input.LeftDown,
            input.LeftClicked,
            input.LeftReleased,
            input.MouseDelta,
            isMouseOverViewport ? input.ScrollWheelDelta : 0);
    }

    private bool TryAdjustVolume(UiButton button, Func<float> getter, Action<float> setter, float delta, InputSnapshot input)
    {
        if (!button.Update(input, activateOnRelease: true))
        {
            return false;
        }

        var current = getter();
        var updated = Math.Clamp(current + delta, 0f, 1f);
        if (Math.Abs(updated - current) < 0.001f)
        {
            _audio.PlayFailure();
            return true;
        }

        setter(updated);
        ApplyAndPreview();
        return true;
    }

    private void ApplyAndPreview()
    {
        _applySettings();
        _audio.PlayButtonClick();
    }

    private void DrawFontDropdown(SpriteBatch spriteBatch)
    {
        var dropdownBounds = GetFontDropdownBounds();
        UiPanel.Draw(spriteBatch, _pixel, new Rectangle(dropdownBounds.X + 4, dropdownBounds.Y + 4, dropdownBounds.Width, dropdownBounds.Height), UiTheme.WithOpacity(Color.Black, 0.25f), Color.Transparent, 0);
        UiPanel.Draw(spriteBatch, _pixel, dropdownBounds, UiTheme.PanelFill, UiTheme.Success, 2);

        foreach (var button in _fontOptionButtons)
        {
            button.Draw(spriteBatch, _pixel, _font);
        }
    }

    private void DrawResolutionDropdown(SpriteBatch spriteBatch)
    {
        var dropdownBounds = GetResolutionDropdownBounds();
        UiPanel.Draw(spriteBatch, _pixel, new Rectangle(dropdownBounds.X + 4, dropdownBounds.Y + 4, dropdownBounds.Width, dropdownBounds.Height), UiTheme.WithOpacity(Color.Black, 0.25f), Color.Transparent, 0);
        UiPanel.Draw(spriteBatch, _pixel, dropdownBounds, UiTheme.PanelFill, UiTheme.Warning, 2);

        foreach (var button in _resolutionOptionButtons)
        {
            button.Draw(spriteBatch, _pixel, _font);
        }
    }

    private void DrawVolumeRow(
        SpriteBatch spriteBatch,
        string label,
        Rectangle rowBounds,
        float value,
        Color fillColor,
        UiButton downButton,
        UiButton upButton)
    {
        var percentText = ToPercent(value);
        var percentSize = _font.MeasureString(percentText) * UiTypography.Caption;

        UiLabel.Draw(spriteBatch, _font, label, new Vector2(rowBounds.X, rowBounds.Y), UiTheme.TextPrimary, UiTypography.Caption);
        spriteBatch.DrawString(
            _font,
            percentText,
            new Vector2(rowBounds.Right - 112 - percentSize.X, rowBounds.Y),
            UiTheme.TextMuted,
            0f,
            Vector2.Zero,
            UiTypography.Caption,
            SpriteEffects.None,
            0f);

        var trackBounds = new Rectangle(rowBounds.X + 136, rowBounds.Y + 26, rowBounds.Width - 252, 16);
        UiPanel.Draw(spriteBatch, _pixel, trackBounds, UiTheme.PanelMuted, UiTheme.PanelBorder, 2);

        var fillWidth = (int)MathF.Round((trackBounds.Width - 6) * value);
        if (fillWidth > 0)
        {
            spriteBatch.Draw(_pixel, new Rectangle(trackBounds.X + 3, trackBounds.Y + 3, fillWidth, trackBounds.Height - 6), fillColor);
        }

        downButton.Draw(spriteBatch, _pixel, _font);
        upButton.Draw(spriteBatch, _pixel, _font);
    }

    private void SyncButtonLabels()
    {
        _themeToggleButton.Text = $"Theme: {_settings.ThemeMode}";
        _fontButton.Text = $"Primary Font: {UiFontCatalog.GetDisplayName(_settings.UiFont)}";
        _displayModeButton.Text = $"Window Mode: {GetWindowModeLabel(_settings.WindowMode)}";
        _resolutionButton.Text = $"Resolution: {FormatResolution(_settings.PreferredResolution)}";
        _soundToggleButton.Text = _settings.SoundEffectsEnabled ? "Sound Effects: ON" : "Sound Effects: OFF";
        _musicToggleButton.Text = _settings.MusicEnabled ? "Background Music: ON" : "Background Music: OFF";

        for (var index = 0; index < _resolutionOptionButtons.Length; index++)
        {
            _resolutionOptionButtons[index].Text = FormatResolution(ResolutionOptions[index]);
            _resolutionOptionButtons[index].IsSelected = ResolutionOptions[index] == _settings.PreferredResolution;
        }

        for (var index = 0; index < _fontOptionButtons.Length; index++)
        {
            var option = UiFontCatalog.All[index];
            _fontOptionButtons[index].Text = UiFontCatalog.GetDisplayName(option);
            _fontOptionButtons[index].IsSelected = option == _settings.UiFont;
        }
    }

    private void UpdateLayout()
    {
        _modalBounds = new Rectangle(72, 34, _virtualResolution.X - 144, _virtualResolution.Y - 68);
        _headerBandBounds = new Rectangle(_modalBounds.X + 1, _modalBounds.Y + 1, _modalBounds.Width - 2, 90);
        _contentFrameBounds = new Rectangle(_modalBounds.X + 18, _headerBandBounds.Bottom + 14, _modalBounds.Width - 36, _modalBounds.Bottom - _headerBandBounds.Bottom - 24);
        _contentViewportBounds = new Rectangle(_contentFrameBounds.X + 12, _contentFrameBounds.Y + 12, _contentFrameBounds.Width - 24, _contentFrameBounds.Height - 24);
        _backButton.Bounds = new Rectangle(_modalBounds.Right - 208, _modalBounds.Y + 22, 180, 40);

        LayoutScrollableContent();
    }

    private void LayoutScrollableContent()
    {
        const int contentInsetHorizontal = 8;
        const int contentInsetTop = 10;
        const int contentInsetBottom = 12;
        const int sectionPadding = 20;
        const int sectionGap = 20;
        const int buttonHeight = 42;
        const int previewHeight = 84;
        const int toggleBandHeight = 60;
        const int volumeRowHeight = 54;
        const int volumeRowGap = 10;

        var contentWidth = _contentViewportBounds.Width - 26 - (contentInsetHorizontal * 2);
        var leftColumnWidth = 468;
        var rightColumnWidth = contentWidth - leftColumnWidth - sectionGap;

        var appearanceIntroHeight = (int)MathF.Ceiling(UiTextBlock.MeasureWrappedHeight(
            _font,
            GetAppearanceIntroText(),
            leftColumnWidth - (sectionPadding * 2),
            UiTypography.Caption,
            2f,
            3));
        var displayIntroHeight = (int)MathF.Ceiling(UiTextBlock.MeasureWrappedHeight(
            _font,
            GetDisplayIntroText(),
            rightColumnWidth - (sectionPadding * 2),
            _isBrowserPlatform ? UiTypography.Body : UiTypography.Caption,
            _isBrowserPlatform ? 3f : 2f,
            _isBrowserPlatform ? 4 : 2));
        var audioIntroHeight = (int)MathF.Ceiling(UiTextBlock.MeasureWrappedHeight(
            _font,
            GetAudioIntroText(),
            contentWidth - (sectionPadding * 2),
            UiTypography.Caption,
            2f,
            2));
        var notesBodyHeight = (int)MathF.Ceiling(UiTextBlock.MeasureWrappedHeight(
            _font,
            GetNotesBodyText(),
            contentWidth - (sectionPadding * 2),
            UiTypography.Caption,
            2f,
            2));
        var notesSummaryHeight = (int)MathF.Ceiling(UiTextBlock.MeasureWrappedHeight(
            _font,
            GetNotesSummaryText(),
            contentWidth - (sectionPadding * 2),
            UiTypography.Caption,
            2f,
            3));

        var appearanceButtonsTopOffset = 48 + appearanceIntroHeight + 20;
        var appearancePreviewTopOffset = appearanceButtonsTopOffset + buttonHeight + 12 + buttonHeight + 16;
        var appearanceHeight = appearancePreviewTopOffset + previewHeight + 20;

        var displayFirstControlLabelOffset = 48 + displayIntroHeight + 20;
        var displayFirstButtonTopOffset = displayFirstControlLabelOffset + 24;
        var displaySecondControlLabelOffset = displayFirstButtonTopOffset + buttonHeight + 18;
        var displaySecondButtonTopOffset = displaySecondControlLabelOffset + 24;
        var displayHeight = _isBrowserPlatform
            ? 48 + displayIntroHeight + 28
            : displaySecondButtonTopOffset + buttonHeight + 20;

        var topRowHeight = Math.Max(appearanceHeight, displayHeight);
        var audioToggleBandTopOffset = 48 + audioIntroHeight + 20;
        var volumeRowsTopOffset = audioToggleBandTopOffset + toggleBandHeight + 18;
        var audioHeight = volumeRowsTopOffset + (volumeRowHeight * 3) + (volumeRowGap * 2) + 22;
        var notesDividerTopOffset = 48 + notesBodyHeight + 18;
        var notesHeight = notesDividerTopOffset + 10 + notesSummaryHeight + 20;

        _contentHeight = contentInsetTop + topRowHeight + sectionGap + audioHeight + sectionGap + notesHeight + contentInsetBottom;
        _maxScrollOffset = Math.Max(0f, _contentHeight - _contentViewportBounds.Height);
        _scrollOffset = Math.Clamp(_scrollOffset, 0f, _maxScrollOffset);

        var scrollY = (int)MathF.Round(_scrollOffset);
        var contentLeft = _contentViewportBounds.X + contentInsetHorizontal;
        var contentTop = _contentViewportBounds.Y + contentInsetTop - scrollY;
        var currentY = contentTop;

        _appearanceBounds = new Rectangle(contentLeft, currentY, leftColumnWidth, topRowHeight);
        _displayBounds = new Rectangle(_appearanceBounds.Right + sectionGap, currentY, rightColumnWidth, topRowHeight);
        currentY += topRowHeight + sectionGap;
        _audioBounds = new Rectangle(contentLeft, currentY, contentWidth, audioHeight);
        currentY += audioHeight + sectionGap;
        _notesBounds = new Rectangle(contentLeft, currentY, contentWidth, notesHeight);

        _themeToggleButton.Bounds = new Rectangle(_appearanceBounds.X + sectionPadding, _appearanceBounds.Y + appearanceButtonsTopOffset, _appearanceBounds.Width - (sectionPadding * 2), buttonHeight);
        _fontButton.Bounds = new Rectangle(_appearanceBounds.X + sectionPadding, _themeToggleButton.Bounds.Bottom + 12, _appearanceBounds.Width - (sectionPadding * 2), buttonHeight);
        _appearancePreviewBounds = new Rectangle(_appearanceBounds.X + sectionPadding, _appearanceBounds.Y + appearancePreviewTopOffset, _appearanceBounds.Width - (sectionPadding * 2), previewHeight);

        var fontDropdownBounds = GetFontDropdownBounds();
        for (var index = 0; index < _fontOptionButtons.Length; index++)
        {
            _fontOptionButtons[index].Bounds = new Rectangle(fontDropdownBounds.X + 10, fontDropdownBounds.Y + 10 + (index * 34), fontDropdownBounds.Width - 20, 28);
        }

        _displayModeButton.Bounds = new Rectangle(_displayBounds.X + sectionPadding, _displayBounds.Y + displayFirstButtonTopOffset, 336, buttonHeight);
        _resolutionButton.Bounds = new Rectangle(_displayBounds.X + sectionPadding, _displayBounds.Y + displaySecondButtonTopOffset, 372, buttonHeight);

        var resolutionDropdownBounds = GetResolutionDropdownBounds();
        for (var index = 0; index < _resolutionOptionButtons.Length; index++)
        {
            _resolutionOptionButtons[index].Bounds = new Rectangle(resolutionDropdownBounds.X + 10, resolutionDropdownBounds.Y + 10 + (index * 34), resolutionDropdownBounds.Width - 20, 28);
        }

        _audioToggleBandBounds = new Rectangle(_audioBounds.X + sectionPadding, _audioBounds.Y + audioToggleBandTopOffset, _audioBounds.Width - (sectionPadding * 2), toggleBandHeight);
        _soundToggleButton.Bounds = new Rectangle(_audioToggleBandBounds.X + 12, _audioToggleBandBounds.Y + 10, 332, 40);
        _musicToggleButton.Bounds = new Rectangle(_soundToggleButton.Bounds.Right + 16, _audioToggleBandBounds.Y + 10, 362, 40);

        _masterVolumeRowBounds = new Rectangle(_audioBounds.X + sectionPadding, _audioBounds.Y + volumeRowsTopOffset, _audioBounds.Width - (sectionPadding * 2), volumeRowHeight);
        _effectsVolumeRowBounds = new Rectangle(_masterVolumeRowBounds.X, _masterVolumeRowBounds.Bottom + volumeRowGap, _masterVolumeRowBounds.Width, volumeRowHeight);
        _musicVolumeRowBounds = new Rectangle(_effectsVolumeRowBounds.X, _effectsVolumeRowBounds.Bottom + volumeRowGap, _effectsVolumeRowBounds.Width, volumeRowHeight);

        LayoutVolumeButtons(_masterDownButton, _masterUpButton, _masterVolumeRowBounds);
        LayoutVolumeButtons(_effectsDownButton, _effectsUpButton, _effectsVolumeRowBounds);
        LayoutVolumeButtons(_musicDownButton, _musicUpButton, _musicVolumeRowBounds);
        _notesSummaryDividerBounds = new Rectangle(_notesBounds.X + sectionPadding, _notesBounds.Y + notesDividerTopOffset, _notesBounds.Width - (sectionPadding * 2), 1);

        _scrollbarTrackBounds = new Rectangle(_contentFrameBounds.Right - 16, _contentViewportBounds.Y + 2, ScrollbarWidth, _contentViewportBounds.Height - 4);

        if (_maxScrollOffset <= 0f)
        {
            _scrollbarThumbBounds = _scrollbarTrackBounds;
            return;
        }

        var thumbHeight = Math.Max(54, (int)MathF.Round(_scrollbarTrackBounds.Height * (_contentViewportBounds.Height / _contentHeight)));
        var thumbTravel = _scrollbarTrackBounds.Height - thumbHeight;
        var thumbY = _scrollbarTrackBounds.Y + (int)MathF.Round((_scrollOffset / _maxScrollOffset) * thumbTravel);
        _scrollbarThumbBounds = new Rectangle(_scrollbarTrackBounds.X, thumbY, _scrollbarTrackBounds.Width, thumbHeight);
    }

    private void LayoutVolumeButtons(UiButton downButton, UiButton upButton, Rectangle rowBounds)
    {
        downButton.Bounds = new Rectangle(rowBounds.Right - 92, rowBounds.Y + 18, 36, 34);
        upButton.Bounds = new Rectangle(rowBounds.Right - 46, rowBounds.Y + 18, 36, 34);
    }

    private void DrawDropdownOverlays(SpriteBatch spriteBatch)
    {
        if (_resolutionDropdownOpen)
        {
            DrawResolutionDropdown(spriteBatch);
        }

        if (_fontDropdownOpen)
        {
            DrawFontDropdown(spriteBatch);
        }
    }

    private Rectangle GetResolutionDropdownBounds()
    {
        return new Rectangle(_resolutionButton.Bounds.X, _resolutionButton.Bounds.Bottom + 8, _resolutionButton.Bounds.Width, 10 + (_resolutionOptionButtons.Length * 34) + 10);
    }

    private Rectangle GetFontDropdownBounds()
    {
        return new Rectangle(_fontButton.Bounds.X, _fontButton.Bounds.Bottom + 8, _fontButton.Bounds.Width, 10 + (_fontOptionButtons.Length * 34) + 10);
    }

    private string GetHeaderSummaryText()
    {
        return "Appearance settings apply live. Pick a palette, switch the primary font,\nand tune display and audio without leaving the current run.";
    }

    private static string GetAppearanceIntroText()
    {
        return "Consolas stays the default anchor.\nPick from common programming fonts\nplus a few UI-friendly alternates.";
    }

    private string GetDisplayIntroText()
    {
        return _isBrowserPlatform
            ? "The WebGL build keeps the same 1600 x 900 virtual desktop and scales it to the browser canvas.\nResize the browser window or use browser fullscreen for presentation changes."
            : "Desktop presentation changes apply immediately.\nThe game world still renders on the same virtual desktop underneath.";
    }

    private static string GetAudioIntroText()
    {
        return "Balance typing clicks and the desk loop separately.\nMaster volume scales every channel live.";
    }

    private string GetNotesBodyText()
    {
        return _isBrowserPlatform
            ? "Click once to unlock browser audio.\nUse browser fullscreen when you want a presentation mode."
            : "Borderless is best for quick alt-tab flow.\nFullscreen applies the chosen display mode directly.";
    }

    private string GetNotesSummaryText()
    {
        return $"Current profile\nTheme {_settings.ThemeMode}  |  Font {UiFontCatalog.GetDisplayName(_settings.UiFont)}\nMaster {ToPercent(_settings.MasterVolume)}  |  SFX {ToPercent(_settings.SoundEffectsVolume)}  |  BGM {ToPercent(_settings.MusicVolume)}";
    }

    private void SetScrollOffset(float value)
    {
        _scrollOffset = Math.Clamp(value, 0f, _maxScrollOffset);
    }

    private static WindowModeSetting GetNextWindowMode(WindowModeSetting current)
    {
        return current switch
        {
            WindowModeSetting.Windowed => WindowModeSetting.Borderless,
            WindowModeSetting.Borderless => WindowModeSetting.Fullscreen,
            _ => WindowModeSetting.Windowed,
        };
    }

    private static string GetWindowModeLabel(WindowModeSetting windowMode)
    {
        return windowMode switch
        {
            WindowModeSetting.Borderless => "Borderless",
            WindowModeSetting.Fullscreen => "Fullscreen",
            _ => "Windowed",
        };
    }

    private static string FormatResolution(Point resolution)
    {
        return $"{resolution.X} x {resolution.Y}";
    }

    private static string ToPercent(float value)
    {
        return $"{MathF.Round(value * 100f):0}%";
    }
}
