using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using ClassicBluetoothController.ViewModels;
using ClassicBluetoothController.Views;

namespace ClassicBluetoothController;

public partial class App : Application
{
   public static class RegisteredServices
   {
      public static IBluetoothService? BluetoothService { get; set; }
   }

   public override void Initialize()
   {
      AvaloniaXamlLoader.Load(this);
   }

   public override void OnFrameworkInitializationCompleted()
   {
      // Line below is needed to remove Avalonia data validation.
      // Without this line you will get duplicate validations from both Avalonia and CT
      BindingPlugins.DataValidators.RemoveAt(0);

      if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
      {
         desktop.MainWindow = new MainWindow
         {
            DataContext = new MainViewModel()
         };
      }
      else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
      {
         singleViewPlatform.MainView = new MainView
         {
            DataContext = new MainViewModel()
         };
      }

      base.OnFrameworkInitializationCompleted();
   }
}
