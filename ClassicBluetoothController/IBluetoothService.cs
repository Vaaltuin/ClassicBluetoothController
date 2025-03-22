namespace ClassicBluetoothController;

using System.Threading.Tasks;

public enum PermissionStatus
{
   //
   // Summary:
   //     The permission hasn't been granted or requested and is in an unknown state.
   Unknown,
   //
   // Summary:
   //     The user has denied the permission.
   Denied,
   //
   // Summary:
   //     The permission is disabled for the app.
   Disabled,
   //
   // Summary:
   //     The user has granted permission.
   Granted,
   //
   // Summary:
   //     The permission is in a restricted state.
   Restricted
}
public enum BtPermissions
{
   Bluetooth,
   BluetoothAdmin,
   BluetoothAdvertise,
   BluetoothConnect,
   BluetoothPrivileged,
   BluetoothScan,
}
public interface IBluetoothService
{
   Task<PermissionStatus> CheckPermission(BtPermissions permission);
   Task<PermissionStatus> RequestPermission(BtPermissions permission);
   /// <summary>
   /// Try to connect to the given bluetooth device
   /// </summary>
   /// <param name="deviceName"></param>
   /// <returns>True if connected</returns>
   Task <bool>Connect(string deviceName);
   Task Disconnect();
   Task Send(byte[] content);
   Task<byte[]> Receive(int count);
}
