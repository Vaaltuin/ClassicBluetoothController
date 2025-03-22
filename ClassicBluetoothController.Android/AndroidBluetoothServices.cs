using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace ClassicBluetoothController.Android
{
   internal class AndroidBluetoothServices : IBluetoothService
   {
      private readonly BluetoothManager _btManager = (BluetoothManager)Application.Context.GetSystemService(Context.BluetoothService)!;
      private BluetoothSocket? _socket;
      private string? _deviceName;
      private const int BluetoothRequestPermissionCode = 10023;
      
      public async Task<PermissionStatus> CheckPermission(BtPermissions permission)
      {
         var manifestString = permission switch
         {
            BtPermissions.Bluetooth => Manifest.Permission.Bluetooth,
            BtPermissions.BluetoothAdmin => Manifest.Permission.BluetoothAdmin,
            BtPermissions.BluetoothAdvertise => Manifest.Permission.BluetoothAdvertise,
            BtPermissions.BluetoothConnect => Manifest.Permission.BluetoothConnect,
            BtPermissions.BluetoothPrivileged => Manifest.Permission.BluetoothPrivileged,
            BtPermissions.BluetoothScan => Manifest.Permission.BluetoothScan,
            _ => ""
         };
         if (MainActivity.Instance != null)
         {
            var perm = MainActivity.Instance.CheckSelfPermission(manifestString);
         }

         if (MainActivity.Instance != null && MainActivity.Instance.CheckSelfPermission(manifestString) == global::Android.Content.PM.Permission.Granted)
            return await Task.FromResult(PermissionStatus.Granted);
         return await Task.FromResult(PermissionStatus.Denied);
      }

      public async Task<PermissionStatus> RequestPermission(BtPermissions permission)
      {
         ArrayList permissions = [];

         if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
         {
            //if Android version bigger than Android12:
            //permissions.Add(Manifest.Permission.AccessFineLocation);
            //permissions.Add(Manifest.Permission.AccessBackgroundLocation);
            permissions.Add(Manifest.Permission.BluetoothScan);
            permissions.Add(Manifest.Permission.BluetoothConnect);
            permissions.Add(Manifest.Permission.BluetoothAdvertise);
            permissions.Add(Manifest.Permission.BluetoothConnect);
         }
         else
         {
            // Android version less than  Android12
            permissions.Add(Manifest.Permission.AccessFineLocation);
         }

         var requestPermissions = (string[])permissions.ToArray(typeof(string));
         MainActivity.Instance?.RequestPermissions(requestPermissions, BluetoothRequestPermissionCode);

         return await CheckPermission(BtPermissions.BluetoothConnect);
      }

      public async Task<bool> Connect(string deviceName)
      {
         _deviceName = deviceName;

         var bluetoothAdapter = PrepareAdapter();

         // To connect you need android.permission.BLUETOOTH_CONNECT permission
         await RequestPermission(BtPermissions.BluetoothConnect);

         var device = bluetoothAdapter.BondedDevices?.FirstOrDefault(d => d.Name == deviceName);

         var idArray = device?.GetUuids();

         if (idArray == null)
            return false;

         using var myUuid1 = Java.Util.UUID.FromString(idArray[1].ToString());

         _socket = device?.CreateRfcommSocketToServiceRecord(myUuid1);

         if (_socket is null)
            return false;

         try
         {
            await _socket.ConnectAsync();
            return true;
         }
         catch (Exception e)
         {
            var msg = e.Message.ToString();
            throw new Exception("On Connecting: " + msg);
         }
      }

      /// <summary>
      /// Prepares the Bluetooth adapter
      /// It ensures that it is enabled.
      /// </summary>
      /// <returns></returns>
      /// <exception cref="Exception"></exception>
      private static BluetoothAdapter PrepareAdapter()
      {
         var bluetoothAdapter = BluetoothAdapter.DefaultAdapter ?? throw new Exception("No Bluetooth adapter found.");

         if (bluetoothAdapter.IsEnabled) return bluetoothAdapter;
         
         // Request Bluetooth permission using the Android Activity
         Intent enableBtIntent = new(BluetoothAdapter.ActionRequestEnable);
         // Start activity for result to handle the permission request
         Application.Context.StartActivity(enableBtIntent);

         return bluetoothAdapter;
      }

      public Task Disconnect()
      {
         if (_socket is null || !_socket.IsConnected) return Task.CompletedTask;
         
         _socket?.Close();
         _socket?.Dispose();
         _socket = null;

         return Task.CompletedTask;
      }

      public async Task<byte[]> Receive(int count)
      {
         if (_socket is null || !_socket.IsConnected)
         {
            await Connect(_deviceName!);
         }

         if (_socket?.InputStream is null)
            return [];

         var content = new byte[count];
         await _socket.InputStream.ReadExactlyAsync(content, 0, count);
         return content;
      }

      public async Task Send(byte[] content)
      {
         if (_socket is null || !_socket.IsConnected)
         {
            try
            {
               await Connect(_deviceName!);
            }
            catch (Exception e)
            {
               throw new Exception("On Sending: " + e.Message.ToString());
            }
         }

         if (_socket?.OutputStream is null)
            return;
         
         try
         {
            await _socket.OutputStream.WriteAsync(content, 0, content.Length);
         }
         catch (Exception e)
         {
            throw new Exception("On OutputStream: " + e.Message.ToString());
         }
      }
   }
}
