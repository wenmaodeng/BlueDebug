using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlueDebug.Services;
using System.Threading.Tasks;

namespace BlueDebug.ViewModels;

public partial class ToolsViewModel : ViewModelBase
{
    private readonly IBleService _ble;

    [ObservableProperty]
    private string _currentRssi = "-48 dBm";

    [ObservableProperty]
    private string _currentMtu = "247 B";

    [ObservableProperty]
    private string _currentPhy = "2M";

    public ToolsViewModel(IBleService ble)
    {
        _ble = ble;
    }

    [RelayCommand]
    private async Task RequestMtu()
    {
        await _ble.RequestMtuAsync(512);
    }
}