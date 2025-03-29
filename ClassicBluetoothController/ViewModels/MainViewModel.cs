using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;

namespace ClassicBluetoothController.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public BtPermissions BtPermissions { get; set; }
    public ObservableCollection<string> BluetoothMessages { get; } = [];
    public string SendMessage { get; }

    [ObservableProperty] private string _connectionStatus;
    bool connected = false;

    public MainViewModel()
    {
        BluetoothMessages.Add("List of messages received via Bluetooth");
        SendMessage = "Hallo";
        ConnectionStatus = "Click the Connect button";
    }

    /// <summary>
    /// Clears the list of Bluetooth messages
    /// </summary>
    [RelayCommand]
    private void ClearList()
    {
        BluetoothMessages.Clear();
    }

    /// <summary>
    /// Sends bytes to the bluetooth device
    /// </summary>
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


    [RelayCommand]
    private void Receive()
    {
        //var connected = true;
        Task.Run(ConnectAndReceiveMessages);
        return;

        async Task? ConnectAndReceiveMessages()
        {
            try
            {
                while (true)
                {
                    if (!connected)
                    {
                        ConnectionStatus = "Lost connection";
                    }

                    while (!connected)
                    {
                        if (App.RegisteredServices.BluetoothService is not null)
                            connected = await App.RegisteredServices.BluetoothService.Connect("Kar1");
                    }

                    ConnectionStatus = "Connected";

                    // wait for a length byte
                    try
                    {
                        byte length = 0;
                        while (length <= 0x80)
                        {
                            if (App.RegisteredServices.BluetoothService == null) continue;

                            var data = await App.RegisteredServices.BluetoothService.Receive(1);
                            length = data[0];
                        }

                        // read the message
                        length = (byte)(length & 0x7f);
                        if (App.RegisteredServices.BluetoothService != null)
                        {
                            var msg = await App.RegisteredServices.BluetoothService.Receive(length);
                            BluetoothMessages.Add(
                                System.Text.Encoding.Default.GetString(msg));
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ConnectionStatus = ex.Message;
                        connected = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus = ex.Message;
            }
        }
    }

    [RelayCommand]
    private async Task Connect()
    {
        connected = await App.RegisteredServices.BluetoothService?.Connect("Kar1");
        ConnectionStatus = connected ? "Connected" : "Not Connected";
    }
}