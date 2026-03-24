using Microsoft.JSInterop;
using Microsoft.Xna.Framework;

namespace MicroDev.WebGL.Pages;

public sealed partial class Index : IDisposable
{
    private Game? _game;
    private DotNetObjectReference<Index>? _selfReference;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
        {
            return;
        }

        _selfReference = DotNetObjectReference.Create(this);
        await JsRuntime.InvokeVoidAsync("microDev.startGameHost", _selfReference);
    }

    [JSInvokable]
    public void TickDotNet()
    {
        _game ??= StartGame();
        _game.Tick();
    }

    [JSInvokable]
    public void ResizeDotNet(int renderWidth, int renderHeight, int inputWidth, int inputHeight)
    {
        _game ??= StartGame();
        if (_game is MicroDev.Core.MicroDevGame microDevGame)
        {
            microDevGame.SetBrowserViewport(renderWidth, renderHeight, inputWidth, inputHeight);
        }
    }

    public void Dispose()
    {
        _selfReference?.Dispose();
        _game?.Dispose();
    }

    private static Game StartGame()
    {
        var game = new MicroDev.Core.MicroDevGame();
        game.Run();
        return game;
    }
}
