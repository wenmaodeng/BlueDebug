using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BlueDebug.Views;
using BlueDebug.ViewModels;
using BlueDebug.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace BlueDebug;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static MainWindowViewModel MainVM { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        MainVM = Services.GetRequiredService<MainWindowViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = MainVM
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new MainView
            {
                DataContext = MainVM
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IBleService, BleService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<ScanViewModel>();
        services.AddTransient<DeviceDetailViewModel>();
        services.AddTransient<CharacteristicViewModel>();
        services.AddTransient<ConsoleViewModel>();
        services.AddTransient<ToolsViewModel>();
    }
}
