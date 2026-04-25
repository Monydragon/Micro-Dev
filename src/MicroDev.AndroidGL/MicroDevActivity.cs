using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using MicroDev.Core;

namespace MicroDev.AndroidGL;

[Activity(
    Label = "Micro Dev",
    MainLauncher = true,
    Theme = "@style/AppTheme",
    AlwaysRetainTaskState = true,
    LaunchMode = LaunchMode.SingleTask,
    Exported = true,
    ConfigurationChanges = ConfigChanges.Orientation
        | ConfigChanges.Keyboard
        | ConfigChanges.KeyboardHidden
        | ConfigChanges.ScreenSize
        | ConfigChanges.ScreenLayout
        | ConfigChanges.UiMode
        | ConfigChanges.SmallestScreenSize,
    ScreenOrientation = ScreenOrientation.SensorLandscape)]
public sealed class MicroDevActivity : Microsoft.Xna.Framework.AndroidGameActivity
{
    private MicroDevGame? _game;

    protected override void OnCreate(Bundle? bundle)
    {
        base.OnCreate(bundle);

        try
        {
            _game = new MicroDevGame();
            if (_game.Services.GetService(typeof(View)) is not View gameView)
            {
                throw new InvalidOperationException("Android game view was not created.");
            }

            SetContentView(gameView);
            _game.Run();
        }
        catch (Exception ex)
        {
            Log.Error("MicroDev", ex.ToString());

            var fallbackView = new TextView(this)
            {
                Text = "Micro Dev failed to start on this device.",
                Gravity = GravityFlags.Center,
                TextSize = 18f,
            };
            fallbackView.SetBackgroundColor(Android.Graphics.Color.Rgb(13, 16, 24));
            fallbackView.SetTextColor(Android.Graphics.Color.Rgb(234, 236, 242));
            SetContentView(fallbackView);
            Toast.MakeText(this, "Micro Dev startup error logged.", ToastLength.Long)?.Show();
        }
    }

    protected override void OnDestroy()
    {
        _game?.Dispose();
        _game = null;
        base.OnDestroy();
    }
}
