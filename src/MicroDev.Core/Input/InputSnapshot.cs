using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MicroDev.Core.Rendering;

namespace MicroDev.Core.Input;

public readonly struct InputSnapshot
{
    public InputSnapshot(
        Point mousePosition,
        bool isMouseOverGame,
        bool leftDown,
        bool leftClicked,
        bool leftReleased,
        Point mouseDelta,
        int scrollWheelDelta)
    {
        MousePosition = mousePosition;
        IsMouseOverGame = isMouseOverGame;
        LeftDown = leftDown;
        LeftClicked = leftClicked;
        LeftReleased = leftReleased;
        MouseDelta = mouseDelta;
        ScrollWheelDelta = scrollWheelDelta;
    }

    public Point MousePosition { get; }

    public bool IsMouseOverGame { get; }

    public bool LeftDown { get; }

    public bool LeftClicked { get; }

    public bool LeftReleased { get; }

    public Point MouseDelta { get; }

    public int ScrollWheelDelta { get; }

    public bool IsLeftClickInside(Rectangle bounds)
    {
        return LeftClicked && IsMouseOverGame && bounds.Contains(MousePosition);
    }

    public static InputSnapshot Create(MouseState currentMouse, MouseState previousMouse, VirtualCanvas canvas)
    {
        var mousePosition = canvas.MapToVirtual(currentMouse.Position, out var isMouseOverGame);
        var previousMousePosition = canvas.MapToVirtual(previousMouse.Position, out var wasMouseOverGame);
        var leftDown = currentMouse.LeftButton == ButtonState.Pressed;
        var leftClicked = leftDown && previousMouse.LeftButton == ButtonState.Released;
        var leftReleased = currentMouse.LeftButton == ButtonState.Released &&
                           previousMouse.LeftButton == ButtonState.Pressed;
        var mouseDelta = isMouseOverGame && wasMouseOverGame
            ? mousePosition - previousMousePosition
            : Point.Zero;
        var scrollWheelDelta = currentMouse.ScrollWheelValue - previousMouse.ScrollWheelValue;

        return new InputSnapshot(
            mousePosition,
            isMouseOverGame,
            leftDown,
            leftClicked,
            leftReleased,
            mouseDelta,
            scrollWheelDelta);
    }
}
