using Microsoft.Extensions.DependencyInjection;
using Presentation.ViewModels.AppSetting;
using Presentation.ViewModels.Main;

namespace Presentation.ViewModels.Shared.Infrastructure;

/// <summary>
/// Обеспечивает доступ к ViewModel через DI-контейнер для привязки из XAML.
/// </summary>
public class ViewModelLocator
{
    private static IServiceProvider _serviceProvider = null!;

    /// <summary>
    /// Конструктор без параметров для использования в XAML.
    /// </summary>
    public ViewModelLocator()
    {
    }

    /// <summary>
    /// Инициализирует локатор контейнером зависимостей.
    /// Вызывается один раз при запуске приложения (в App.xaml.cs).
    /// </summary>
    public static void Initialize(IServiceProvider? serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Главная ViewModel главного представления.
    /// </summary>
    public static MainVM Main => _serviceProvider.GetRequiredService<MainVM>();

    /// <summary>
    /// ViewModel Setting главного представления.
    /// </summary>
    public static SettingVM Setting => _serviceProvider.GetRequiredService<SettingVM>();
    
    /// <summary>
    /// ViewModel Setting главного представления.
    /// </summary>
    public static BackstageVM Backstage => _serviceProvider.GetRequiredService<BackstageVM>();
    
    /// <summary>
    /// Главная ViewModel представления AppSettingView.
    /// </summary>
    public static AppSettingVM AppSetting => _serviceProvider.GetRequiredService<AppSettingVM>();
}