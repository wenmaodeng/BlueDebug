using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.ObjectModel;

namespace BlueDebug.Models;

public partial class BleService : ObservableObject
{
    public IService? NativeService { get; set; }

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _uuid = "";

    [ObservableProperty]
    private string _type = "标准服务";

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private ObservableCollection<BleCharacteristic> _characteristics = new();
}