using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.BLE.Abstractions.Contracts;

namespace BlueDebug.Models;

public partial class BleCharacteristic : ObservableObject
{
    public ICharacteristic? NativeCharacteristic { get; set; }

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _uuid = "";

    [ObservableProperty]
    private string _properties = "";

    [ObservableProperty]
    private string _valueHex = "";

    [ObservableProperty]
    private string _valueUtf8 = "";

    [ObservableProperty]
    private bool _canRead;

    [ObservableProperty]
    private bool _canWrite;

    [ObservableProperty]
    private bool _canNotify;

    [ObservableProperty]
    private bool _isSubscribed;

    [ObservableProperty]
    private bool _isIndicate;
}