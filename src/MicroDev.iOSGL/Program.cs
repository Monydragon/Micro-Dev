using Foundation;
using MicroDev.Core;
using UIKit;

namespace MicroDev.iOSGL;

[Register("AppDelegate")]
internal sealed class Program : UIApplicationDelegate
{
    private static MicroDevGame? game;

    private static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(Program));
    }

    public override void FinishedLaunching(UIApplication application)
    {
        game = new MicroDevGame();
        game.Run();
    }
}
