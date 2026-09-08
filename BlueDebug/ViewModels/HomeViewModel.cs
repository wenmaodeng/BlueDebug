using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlueDebug.Services;

namespace BlueDebug.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;

    public HomeViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }

    [RelayCommand]
    private void NavigateTo(string page)
    {
        _navigation.NavigateTo(page switch
        {
            "Scan" => typeof(ScanViewModel),
            "Console" => typeof(ConsoleViewModel),
            "Tools" => typeof(ToolsViewModel),
            _ => typeof(ScanViewModel)
        });
    }
}