using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;

namespace ClassicBluetoothController.Android;

[Activity(
    Label = "ClassicBluetoothController.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
   public static MainActivity? Instance;

   protected override void OnCreate(Bundle? savedInstanceState)
   {
      base.OnCreate(savedInstanceState);
      Instance = this;
   }

   protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
      App.RegisteredServices.BluetoothService = new AndroidBluetoothServices();
      return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
