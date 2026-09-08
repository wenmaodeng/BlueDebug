using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.BLE.Abstractions.Contracts;

namespace BlueDebug.Models;

public partial class BleDevice : ObservableObject
{
    public IDevice? NativeDevice { get; set; }

    [ObservableProperty]
    private string _name = "未知设备";

    [ObservableProperty]
    private string _macAddress = "";

    [ObservableProperty]
    private int _rssi;

    [ObservableProperty]
    private bool _isConnectable;

    [ObservableProperty]
    private string _deviceType = "BLE";

    [ObservableProperty]
    private string _serviceUuids = "";

    public string Id => NativeDevice?.Id.ToString() ?? MacAddress;
}