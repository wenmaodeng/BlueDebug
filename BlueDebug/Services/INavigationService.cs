using BlueDebug.ViewModels;
using System;

namespace BlueDebug.Services;

public interface INavigationService
{
    ViewModelBase CurrentView { get; }

    event EventHandler<ViewModelBase>? OnCurrentViewChanged;

    void NavigateTo<T>() where T : ViewModelBase;
    void NavigateTo(Type viewModelType);
    void NavigateTo(ViewModelBase viewModel);
    void GoBack();
}
