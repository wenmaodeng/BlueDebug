using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlueDebug.Models;
using BlueDebug.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BlueDebug.ViewModels;

public partial class DeviceDetailViewModel : ViewModelBase
{
    private readonly IBleService _ble;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private BleDevice? _device;

    [ObservableProperty]
    private string _rssi = "--";

    [ObservableProperty]
    private string _mtu = "247 B";

    [ObservableProperty]
    private string _phy = "2M";

    [ObservableProperty]
    private string _interval = "30 ms";

    public ObservableCollection<Models.BleService> DiscoveredServices => _ble.DiscoveredServices;

    public DeviceDetailViewModel(IBleService ble, INavigationService navigation)
    {
        _ble = ble;
        _navigation = navigation;
    }

    public void LoadDevice(BleDevice device)
    {
        Device = device;
        Rssi = $"{device.Rssi} dBm";
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigation.GoBack();
    }

    [RelayCommand]
    private async Task Disconnect()
    {
        await _ble.DisconnectAsync();
        _navigation.GoBack();
    }

    [RelayCommand]
    private void SelectCharacteristic(BleCharacteristic ch)
    {
        var vm = App.Services.GetRequiredService<CharacteristicViewModel>();
        vm.LoadCharacteristic(ch);
        _navigation.NavigateTo(vm);
    }

    [RelayCommand]
    private async Task ReadAll()
    {
        foreach (var s in _ble.DiscoveredServices)
            foreach (var c in s.Characteristics)
                if (c.CanRead)
                    await _ble.ReadAsync(c);
    }

    [RelayCommand]
    private async Task SubscribeAll()
    {
        foreach (var s in _ble.DiscoveredServices)
            foreach (var c in s.Characteristics)
                if (c.CanNotify && !c.IsSubscribed)
                    await _ble.SubscribeAsync(c);
    }
}
