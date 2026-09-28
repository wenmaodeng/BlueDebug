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
        // 页面 ViewModel 统一使用单例：它们共享同一个 BLE 服务上下文，
        // 同时避免每次导航重复订阅 BLE 事件造成回调累积。
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<ScanViewModel>();
        services.AddSingleton<DeviceDetailViewModel>();
        services.AddSingleton<CharacteristicViewModel>();
        services.AddSingleton<ConsoleViewModel>();
        services.AddSingleton<ToolsViewModel>();
    }
}
