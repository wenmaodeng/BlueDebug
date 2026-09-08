using BlueDebug.ViewModels;
using System;
using System.Collections.Generic;

namespace BlueDebug.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private readonly Stack<ViewModelBase> _history = new();

    public ViewModelBase CurrentView { get; private set; } = null!;

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public void NavigateTo<T>() where T : ViewModelBase
    {
        var vm = _services.GetRequiredService<T>();
        NavigateTo(vm);
    }

    public void NavigateTo(ViewModelBase viewModel)
    {
        if (CurrentView != null)
            _history.Push(CurrentView);
        CurrentView = viewModel;
        OnCurrentViewChanged?.Invoke(this, CurrentView);
    }

    public void GoBack()
    {
        if (_history.Count > 0)
        {
            CurrentView = _history.Pop();
            OnCurrentViewChanged?.Invoke(this, CurrentView);
        }
    }

    public event EventHandler<ViewModelBase>? OnCurrentViewChanged;
}