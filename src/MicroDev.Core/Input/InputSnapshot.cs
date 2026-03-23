using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MicroDev.Core.Rendering;

namespace MicroDev.Core.Input;

public readonly struct InputSnapshot
{
    public InputSnapshot(Point mousePosition, bool isMouseOverGame, bool leftDown, bool leftClicked)
    {
        MousePosition = mousePosition;
        IsMouseOverGame = isMouseOverGame;
        LeftDown = leftDown;
        LeftClicked = leftClicked;
    }

    public Point MousePosition { get; }

    public bool IsMouseOverGame { get; }

    public bool LeftDown { get; }

    public bool LeftClicked { get; }

    public bool IsLeftClickInside(Rectangle bounds)
    {
        return LeftClicked && IsMouseOverGame && bounds.Contains(MousePosition);
    }

    public static InputSnapshot Create(MouseState currentMouse, MouseState previousMouse, VirtualCanvas canvas)
    {
        var mousePosition = canvas.MapToVirtual(currentMouse.Position, out var isMouseOverGame);
        var leftDown = currentMouse.LeftButton == ButtonState.Pressed;
        var leftClicked = leftDown && previousMouse.LeftButton == ButtonState.Released;

        return new InputSnapshot(mousePosition, isMouseOverGame, leftDown, leftClicked);
    }
}
