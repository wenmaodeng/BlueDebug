using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlueDebug.Models;
using BlueDebug.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BlueDebug.ViewModels;

public partial class CharacteristicViewModel : ViewModelBase
{
    private readonly IBleService _ble;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private BleCharacteristic? _characteristic;

    [ObservableProperty]
    private string _hexValue = "";

    [ObservableProperty]
    private string _parsedValue = "";

    [ObservableProperty]
    private string _writeInput = "01";

    [ObservableProperty]
    private bool _isNotifying;

    [ObservableProperty]
    private string _serviceName = "";

    public ObservableCollection<HeartRateData> HeartRateHistory => _ble.HeartRateHistory;

    public CharacteristicViewModel(IBleService ble, INavigationService navigation)
    {
        _ble = ble;
        _navigation = navigation;
        _ble.OnCharacteristicChanged += (s, data) => UpdateValue(data);
        _ble.OnHeartRateUpdated += (s, hr) => ParsedValue = $"心率 {hr.Bpm} bpm";
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigation.GoBack();
    }

    public void LoadCharacteristic(BleCharacteristic ch)
    {
        Characteristic = ch;
        ServiceName = "Heart Rate Service";
        IsNotifying = ch.IsSubscribed;
    }

    private void UpdateValue(byte[] data)
    {
        HexValue = BitConverter.ToString(data).Replace("-", " ");
    }

    [RelayCommand]
    private async Task Read()
    {
        if (Characteristic == null) return;
        var data = await _ble.ReadAsync(Characteristic);
        if (data != null) HexValue = BitConverter.ToString(data).Replace("-", " ");
    }

    [RelayCommand]
    private async Task ToggleNotify()
    {
        if (Characteristic == null) return;
        if (IsNotifying)
        {
            await _ble.UnsubscribeAsync(Characteristic);
            IsNotifying = false;
        }
        else
        {
            await _ble.SubscribeAsync(Characteristic);
            IsNotifying = true;
        }
    }

    [RelayCommand]
    private async Task Send()
    {
        if (Characteristic == null || string.IsNullOrEmpty(WriteInput)) return;
        var data = Convert.FromHexString(WriteInput.Replace(" ", ""));
        await _ble.WriteAsync(Characteristic, data);
    }
}