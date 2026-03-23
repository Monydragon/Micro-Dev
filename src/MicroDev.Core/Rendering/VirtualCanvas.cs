using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MicroDev.Core.Rendering;

public sealed class VirtualCanvas : IDisposable
{
    public VirtualCanvas(int virtualWidth, int virtualHeight)
    {
        VirtualWidth = virtualWidth;
        VirtualHeight = virtualHeight;
        DestinationRectangle = new Rectangle(0, 0, virtualWidth, virtualHeight);
    }

    public int VirtualWidth { get; }

    public int VirtualHeight { get; }

    public Rectangle DestinationRectangle { get; private set; }

    public RenderTarget2D RenderTarget { get; private set; } = null!;

    public void EnsureResources(GraphicsDevice graphicsDevice)
    {
        if (RenderTarget is not null &&
            !RenderTarget.IsDisposed &&
            RenderTarget.Width == VirtualWidth &&
            RenderTarget.Height == VirtualHeight)
        {
            return;
        }

        RenderTarget?.Dispose();
        RenderTarget = new RenderTarget2D(
            graphicsDevice,
            VirtualWidth,
            VirtualHeight,
            false,
            SurfaceFormat.Color,
            DepthFormat.None);
    }

    public void UpdateDestination(Viewport viewport)
    {
        var scale = MathF.Min(
            viewport.Width / (float)VirtualWidth,
            viewport.Height / (float)VirtualHeight);

        var width = Math.Max(1, (int)MathF.Round(VirtualWidth * scale));
        var height = Math.Max(1, (int)MathF.Round(VirtualHeight * scale));
        var x = viewport.X + ((viewport.Width - width) / 2);
        var y = viewport.Y + ((viewport.Height - height) / 2);

        DestinationRectangle = new Rectangle(x, y, width, height);
    }

    public Point MapToVirtual(Point windowPoint, out bool isInside)
    {
        isInside = DestinationRectangle.Contains(windowPoint);
        if (!isInside)
        {
            return new Point(-1, -1);
        }

        var relativeX = (windowPoint.X - DestinationRectangle.X) / (float)DestinationRectangle.Width;
        var relativeY = (windowPoint.Y - DestinationRectangle.Y) / (float)DestinationRectangle.Height;

        var virtualX = Math.Clamp((int)(relativeX * VirtualWidth), 0, VirtualWidth - 1);
        var virtualY = Math.Clamp((int)(relativeY * VirtualHeight), 0, VirtualHeight - 1);

        return new Point(virtualX, virtualY);
    }

    public void Dispose()
    {
        RenderTarget?.Dispose();
    }
}
