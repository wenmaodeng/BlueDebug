using BlueDebug.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace BlueDebug.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private readonly Stack<ViewModelBase> _history = new();

    public ViewModelBase CurrentView { get; private set; } = null!;

    public event EventHandler<ViewModelBase>? OnCurrentViewChanged;

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public void NavigateTo<T>() where T : ViewModelBase
    {
        var vm = _services.GetRequiredService<T>();
        NavigateTo(vm);
    }

    public void NavigateTo(Type viewModelType)
    {
        if (!typeof(ViewModelBase).IsAssignableFrom(viewModelType))
            throw new ArgumentException($"{viewModelType} 不是 ViewModelBase", nameof(viewModelType));

        var vm = (ViewModelBase)_services.GetRequiredService(viewModelType);
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
}
