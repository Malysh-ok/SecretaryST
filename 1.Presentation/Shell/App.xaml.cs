using System;
using System.Windows;
using AppDomain.Setting.Services;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Shell.Views;

namespace Presentation.Shell;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        
        InitializeComponent();
    }

    /// <summary>
    /// Конфигурирование сервисов.
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton<MainView>()               // главное представление
            .AddSingleton<AppSettingService>();     // сервис настроек приложения
    }

    /// <summary>
    /// Обработчик события запуска приложения.
    /// </summary>
    private void OnStartup(object sender, StartupEventArgs e)
    {
        // Получаем главное представление (окно) и показываем его
        var mainView = _serviceProvider.GetService<MainView>();
        mainView!.Show();
    }
}