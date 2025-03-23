# ClassicBluetoothController
## Author: W Breytenbach
### Versions:

0.0.0 - First commit - 2025/03/23

The Avalonia Android project provides the basic functionality to use Classic Bluetooth.

As there is a severe lack of examples of how to implement Classic Bluetooth in Avalonia, I wrote this program to function as a starting point for electronic hobbyists who are not that deep into programming.
This program demonstrates how to do Classic Bluetooth communication, and doesn't attempt to be a complete program, as everyone will have their own ideas on how to implement the intended functionality.
I do not want to clutter the issue with my own ideas.
A capable hobbyist should easily take it from here to a fully functional Bluetooth interface.

### Topics

#### Setting up the HC-05

You will also have to program your Bluetooth Module (HC-05 or other) to set it up.
There are enough examples on the internet of how to do that.
Your Bluetooth Module should get a name, so that you can scan for it on your phone and pair with it.
The program has been hardcoded to connect to this Bluetooth name, which in my case was "Kar1".
I used a NanoMega168, powered through the USB cable, and simple connections to the HC-05 to test my code.

#### Permissions

The first task of the interface is to ensure that the phone has the proper permissions to access Bluetooth.
The developer needs to ensure that the Bluetooth adapter is enabled, and then get permission to use Bluetooth.
Permission needs to be set both in the AndroidManifest.xml file and at runtime.
You can initially concentrate on getting this working following my example code.
Be aware that it seems like you will need to evaluate your code on real hardware and not in an emulator.

#### Receiving data from the Arduino
To simplify matters, I provide a very basic Arduino sketch that merely periodically sends data to the phone, consisting of a message and a counter.
This provides a way to get the reception communication to the phone working, and does not require the command link to be working.

Once this is in place one can proceed to receiving the messages from the Arduino.

#### Sending data to the Arduino

The phone can send a new message, after which the counter will be reset.
Messages to the phone will thus prove that the command gets received.

My Arduino sketch interfaces to a HC-05 bluetooth module via the serial port.
Take note that I am using ASCII encoded messages, so none of them will have the most significant bit set.
I then use this bit to specify that the lower seven bits specify the length of ASCII characters that will follow. This way my program can very easily re-sync should the Bluetooth drop some data.

### CommunityToolkit.Mvvm
It is also worth mentioning to use the CommunityToolkit.Mvvm.
Most hobbyists steer away from more complex programming concepts.
In this case it is really worth taking this step as it becomes very difficult to access your variables in the more direct approach.
There is more than one Mvvm approach, and I have used the CommunityToolkit, which makes life easy, provided that you understand the little bit of magic that comes with it.

The CommunityToolkit does some magic with the naming. Let's look at some:

#### Properties
In your viewmodel you define it with a _ preceding the name and with the name starting in lower case.
```csharp
    [ObservableProperty] private string _connectionStatus;
```

You can now refer to it in your axaml View as follows:
```xaml
<TextBlock Text="{Binding ConnectionStatus}"></TextBlock>
```
Note that here the name starts with uppercase.
If you don't stick to this rule then your code would not work.

#### Functions

Define your functions in your viewmodel, and name them starting with a capital letter.

```csharp
    [RelayCommand]
    private async void SendButton()
    {
        var len = SendMessage.Length;
        var msg = new byte[len + 1]; // Provide space for the message plus one for the length.
        msg[0] = (byte)(0x80 + len); // by setting the eighth bit the Arduino can easily sync on the serial stream,
        // and by setting the length it knows how many bytes to read.
        // Build the message to send
        var source = Encoding.UTF8.GetBytes(SendMessage);
        Buffer.BlockCopy(source, 0, msg, 1, SendMessage.Length);
        await App.RegisteredServices.BluetoothService.Send(msg);
    }
```

In your View bind it as follows:

```xaml
<Button Command="{Binding SendButtonCommand}">Send</Button>
```
Note that here you have to append **Command** after the name. Failing to do this will result in non-working code.

Using Command instead of Click means that your code will have full access to all the definitions inside the ViewModel, while with Click you will encounter endless problems accessing them.
## Challenges to overcome in realizing this demonstration program:

### Calling Android specific code

Avalonia, like Xamarin and MAUI, doesn't provide Classic Bluetooth, so you have to implement that yourself.
This requires that you know how to use **interfaces** and **dependency injection**.
Although did find information on how to use DI in Avalonia in MVVM, I failed to get any example of how to implement it to tunnel down to the Android specific code.
Unlike Xamarin, which has its Android project underneath the common project, Avalonia has its projects at the same level.
All my attempts resulted in circular references, so I resorted to defining a static field in MainActivity.cs of the Android project.
This is accessible from the project specific code, making it easy to specify the project specific code.

I think this is just as neat, and understandable by non-specialists:
By letting the platform specific code set a reference to the static field, one can call the platform specific code from the common code.
I only implemented it for Android because the iPhone does not support Bluetooth Classic.
The technique, however, provides for other platform specific functions.
You just have to add more services to my RegisteredServices class.

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
This Instance can then be called inside the platform specific implementation of the Interface, for example

```csharp
if (MainActivity.Instance != null)
{
   var perm = MainActivity.Instance.CheckSelfPermission(manifestString);
}
```
#### App.axaml.cs

I also define a static class inside App.axaml.cs to contain all the services.
For now, we are for now only using Bluetooth, but could add more.

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
We implement the interface inside all platform specific projects.

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
#### Calling platform-specific code
When you now use the service, you will be calling the platform specific code from the common code:
````csharp
   App.RegisteredServices.BluetoothService.Connect("Kar1");
````

### Emulator problems
The current emulators do not correctly implement the Bluetooth adapter, so during debugging you will end up calling a foreign thread.
Rather use real hardware and stop chasing ghost problems.

### Problem of constant changes to Android by Google

It is impossible to implement something in Android and expect that to still be valid after a couple of years.
It is a pity, but it seems that Google lacked the insight to start from a proper baseline.
They have changed the way to program in Android so many times!
Their latest direction looks good and Kotlin seems good, but for reasons that I could not discover, their development system is crawlingly slow on my PC, even though Jetbrains and Avalonia runs like greased lightning on my PC, so I can't even consider doing something with Android Studio as the compilation takes 10 minutes!

When I refer to the Google documentation I can find examples of how to implement Classic Bluetooth.
However, when I try their code it always contains depreciated code.
By the time that I found out how to do it, I forgot what I wanted to do.

### Problem of using outdated advice

When one searches the internet for advice you will fall into the trap of not realizing that the advice was valid at some time in the past, and end up wasting a lot of time.
Make sure that the article is recent and relevant.
Lookout for Avalonia specific advice.
Learn how to translate the Java, Kotlin and Xamarin examples.

