using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlueDebug.Models;
using BlueDebug.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BlueDebug.ViewModels;

public partial class ConsoleViewModel : ViewModelBase
{
    private readonly IBleService _ble;

    [ObservableProperty]
    private string _inputText = "AA 55 03 01 FF";

    [ObservableProperty]
    private string _txBytes = "1.24 KB";

    [ObservableProperty]
    private string _rxBytes = "8.63 KB";

    [ObservableProperty]
    private string _packetRate = "12 包/s";

    public ObservableCollection<LogEntry> Logs => _ble.Logs;

    public ConsoleViewModel(IBleService ble)
    {
        _ble = ble;
    }

    [RelayCommand]
    private async Task Send()
    {
        if (string.IsNullOrEmpty(InputText)) return;
        var data = Convert.FromHexString(InputText.Replace(" ", ""));
        // Send to last written characteristic or a default TX characteristic
    }

    [RelayCommand]
    private void Clear()
    {
        _ble.Logs.Clear();
    }

    [RelayCommand]
    private void Export()
    {
        // Export logs
    }
}