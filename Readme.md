# ClassicBluetoothController
## Author: W Breytenbach
### Versions: 

   0.0.0 - First commit - 2025/03/22

This Avalonia Android project that provides the basic functions to use Classic Bluetooth.

As there is a severe lack of examples of how to implement Classic Bluetooth in Avalonia, I wrote this program to function as a starting point for electronic hobbyists who are not that deep into programming.
It is the intention to just demonstrate how it is done, and not to be a complete program, as everyone will have their own ideas on how to implement their intended functionality.
I do not want to clutter the issue with my own ideas.
The capable hobbyist should easily take it from here to a fully functional Bluetooth interface.

### Steps

#### Setting up the HC-05

You will also have to program your Bluetooth Module (HC-05 or others) to set it up. There are enough examples on the internet of how to do that.
Your Bluetooth Module should get a name, so that you can scan for it on your phone and pair with it. This program has been hardcoded to connect to that name, which in my case was "Kar1".
I used a NanoMega168, powered through the USB cable, and simple connections to the HC-05 to test my code.

#### Permissions

The first task of the interface is to ensure that the phone has the proper permissions to access Bluetooth.
The developer needs to ensure that the Bluetooth adapter is enabled, and then get permission to use Bluetooth.
Permission needs to be set both in the AndroidManifest.xml file and at runtime.
You can initially concentrate on getting this working following my example code.
Be aware that it seems like you will need to evaluate your code on the real hardware and not in an emulator.

#### Receiving data from the Arduino
To simplify matters, I provide a very basic Arduino sketch that merely periodically sends data to the phone, consisting of a message and a counter.
This provides a way to get the reception communication to the phone working, and does not require the command link to be working.

Once this is in place one can proceed to receiving the messages from the Arduino.

#### Sending data to the Arduino

The phone can send a new message, after which the counter will be reset.
The by now messages to the phone will thus prove that the command gets received.

My Arduino sketch interfaces to a HC-05 bluetooth module via the serial port.
Take note that I am using ASCII encoded messages, so none of them will have the most significant bit set. I then use this bit to specify that the lower seven bits specifies the length of ASCII characters that will follow. This way my program can very easily re-sync should the Bluetooth drop some data.

## Challenges to overcome in realizing this demonstration program:

### Calling Android specific code

Avalonia, like Xamarin and MAUI, doesn't provide Classic Bluetooth, so you have to implement that yourself.
This requires that you should know how to use interfaces and dependency injection.
I did find information of how to use DI in Avalonia in MVVM, but failed to get any example of how to implement it to tunnel down to the Android specific code.
Unlike Xamarin, that has its Android project underneath the common project, Avalonia has its projects at the same level.
All my attempts resulted in circular references, so I resorted to defining a static field in MainActivity.cs of the Android project.

I think that this is just as neat, and understandable by non-specialists:
By letting the project specific code set a reference to the static field one can call the platform specific code from the common code.
I only implemented it for Android because the iPhone does not support Bluetooth Classic.
The technique however provides for multi-platform solutions.
You also can easily expand it to more than one service, so I used a class called RegisteredServices, so that you can add additional interfaces.

#### MainActivity.cs

```csharp
public class MainActivity : AvaloniaMainActivity<App>
{
   public static MainActivity? Instance;
   
   protected override void OnCreate(Bundle? savedInstanceState)
   {
      base.OnCreate(savedInstanceState);
      Instance = this;
   }
```

#### App.axaml.cs

I then define a static class inside App.axaml.cs.

```csharp
namespace ClassicBluetoothController;

public partial class App : Application
{
   public static class RegisteredServices
   {
       public static IBluetoothService? BluetoothService { get; set; }
   }
```
#### IBluetoothServices
You can now define the Android functions that you need in an interface in the global project.

````csharp
public interface IBluetoothService
{
   Task<PermissionStatus> CheckPermission(BtPermissions permission);
   Task<PermissionStatus> RequestPermission(BtPermissions permission);
   Task <bool>Connect(string deviceName);
   Task Disconnect();
   Task Send(byte[] content);
   Task<byte[]> Receive(int count);
}
````
#### AndroidBluetoothServices
We implement the interface inside all device dependent projects.

````csharp
internal class AndroidBluetoothServices : IBluetoothService
   {     
      public async Task<PermissionStatus> CheckPermission(BtPermissions permission)
      {
         ...
         return await Task.FromResult(PermissionStatus.Denied);
      }

      public async Task<PermissionStatus> RequestPermission(BtPermissions permission)
         ...
         return await CheckPermission(BtPermissions.BluetoothConnect);
      }

      public async Task<bool> Connect(string deviceName)
      {
         ...
      }

      private static BluetoothAdapter PrepareAdapter()
      {
         var bluetoothAdapter = BluetoothAdapter.DefaultAdapter ?? throw new Exception("No Bluetooth adapter found.");
         ...
         return bluetoothAdapter;
      }

      public Task Disconnect()
      {
         ...
         return Task.CompletedTask;
      }

      public async Task<byte[]> Receive(int count)
      {
         ...
         return content;
      }

      public async Task Send(byte[] content)
      {
       ...
   }
````
#### Calling platform specific code
By using the newly defined service you can call the platform specific from the common code:  
````csharp
   App.RegisteredServices.BluetoothService.Connect("Kar1");
````

### Emulator problems
The current emulators does not correctly implement the Bluetooth adapter, so during debugging you will end up calling a foreign thread.
Rather use the real hardware and stop chasing ghost problems.

### Problem of constant changes to Android by Google

It is impossible to implement something in Android and expect that to be still valid after a couple of years.
It is a pity, but it seems that Google lacked the insight to start from a proper baseline.
They have changed the way to program in Android so many times!
Their latest direction looks good and Kotlin seems good, but for reasons that I could not discover, their development system is crawlingly slow on my PC, even though Jetbrains and Avalonia runs like greased lightning on my PC, so I can't even consider doing something with Android Studio as a compilation takes 10 minutes!

When I refer to the Google documentation I can find examples of how to implement Classic Bluetooth.
However, when I try their code it always contains depreciated code.
By the time that I found out how to do it, I forgot what I wanted to do.

### Problem of using outdated advice

When one search the internet for advice you will fall into the trap of not realizing that the advice was valid at some time in the past, and end up wasting a lot of time.
Make sure that the article is recent and relevant.
Look for Avalonia specific advice.
Learn how to translate the Java, Kotlin and Xamarin examples.

