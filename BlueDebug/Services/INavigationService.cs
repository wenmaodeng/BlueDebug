using BlueDebug.ViewModels;

namespace BlueDebug.Services;

public interface INavigationService
{
    ViewModelBase CurrentView { get; }
    void NavigateTo<T>() where T : ViewModelBase;
    void NavigateTo(ViewModelBase viewModel);
    void GoBack();
}