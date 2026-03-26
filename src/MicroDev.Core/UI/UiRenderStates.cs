using Microsoft.Xna.Framework.Graphics;

namespace MicroDev.Core.UI;

public static class UiRenderStates
{
    public static readonly RasterizerState ScissorRasterizer = new()
    {
        ScissorTestEnable = true,
    };
}

