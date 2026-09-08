using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlueDebug.Services;
using System;

namespace BlueDebug.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private ViewModelBase _currentView = null!;

    [ObservableProperty]
    private int _selectedNavIndex;

    public MainWindowViewModel(INavigationService navigation, HomeViewModel homeVm)
    {
        _navigation = navigation;
        _navigation.OnCurrentViewChanged += (s, vm) => CurrentView = vm;
        CurrentView = homeVm;
    }

    [RelayCommand]
    private void Navigate(string page)
    {
        SelectedNavIndex = page switch
        {
            "Home" => 0,
            "Scan" => 1,
            "Console" => 2,
            "Tools" => 3,
            _ => 0
        };
        _navigation.NavigateTo(page switch
        {
            "Home" => typeof(HomeViewModel),
            "Scan" => typeof(ScanViewModel),
            "Console" => typeof(ConsoleViewModel),
            "Tools" => typeof(ToolsViewModel),
            _ => typeof(HomeViewModel)
        });
    }
}