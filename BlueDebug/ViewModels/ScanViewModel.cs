using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlueDebug.Models;
using BlueDebug.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BlueDebug.ViewModels;

public partial class ScanViewModel : ViewModelBase
{
    private readonly IBleService _ble;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _scanStatus = "点击开始扫描";

    [ObservableProperty]
    private string _scanTime = "00:00";

    [ObservableProperty]
    private int _deviceCount;

    [ObservableProperty]
    private string _filterText = "";

    public ObservableCollection<BleDevice> Devices => _ble.ScannedDevices;

    public ScanViewModel(IBleService ble, INavigationService navigation)
    {
        _ble = ble;
        _navigation = navigation;
    }

    [RelayCommand]
    private async Task ToggleScan()
    {
        if (IsScanning)
        {
            await _ble.StopScanAsync();
            IsScanning = false;
            ScanStatus = "扫描停止";
        }
        else
        {
            IsScanning = true;
            ScanStatus = "正在扫描...";
            await _ble.StartScanAsync();
        }
    }

    [RelayCommand]
    private async Task Connect(BleDevice device)
    {
        await _ble.StopScanAsync();
        IsScanning = false;
        var success = await _ble.ConnectAsync(device);
        if (success)
        {
            await _ble.DiscoverServicesAsync();
            var detailVm = App.Services.GetRequiredService<DeviceDetailViewModel>();
            detailVm.LoadDevice(device);
            _navigation.NavigateTo(detailVm);
        }
    }
}