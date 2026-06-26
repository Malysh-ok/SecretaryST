using System;
using System.Globalization;
using System.IO;
using System.Windows;
using AppDomain.AppExceptions;
using AppDomain.Phrases;
using AppDomain.Setting.Services;
using AppDomain.UseCases._Contracts;
using AppDomain.UseCases.Services;
using Common.WpfModule.Components.Services;
using DataAccess.DbContexts;
using DataAccess.Repositories;
using DataAccess.Repositories.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Shell.Views;
using Presentation.ViewModels._Contracts;
using Presentation.ViewModels.Common;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Presentation.Shell;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        // Меняем текущую локализацию и локализацию фраз приложения на русскую
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
        AppPhrases.Culture = CultureInfo.CurrentUICulture;
        
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
        var startupItemsFactory = new StartupItemsFactory();
        var dbConfigurator = startupItemsFactory.CreateDbConfigurator();
        var appSettingService = startupItemsFactory.CreateAppSettingService();
        
        services
            .AddSingleton<MainView>()        // регистрируем главное представление
            .AddSingleton(appSettingService) // регистрируем сервис настроек приложения
            .AddSingleton(dbConfigurator)    // регистрируем конфигуратор БД
            
            .AddDbContext<AppDbContext>(options =>
                dbConfigurator.UseProvider<AppDbContext>(options),
                ServiceLifetime.Transient)                       // регистрируем контекст БД
            .AddScoped<IRepository, Repository<AppDbContext>>()  // регистрируем репозиторий
            .AddTransient<IRepositoryHelper, RepositoryHelper>() // регистрируем "помощник" репозитория
            
            // .AddLogging(builder => builder.AddSerilog(dispose: true))
            .AddSingleton<ILogger>
            (_ => new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(theme: SystemConsoleTheme.Colored)
                .WriteTo.File(Path.Combine(appSettingService.AppDir.LogsPath, "Application-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 50)
                .CreateLogger()
            )
            .AddSingleton<StatusBarService>()
            .AddSingleton<IExceptionsProvider, ExceptionsProvider>()
            .AddScoped<CompetitionDataService>()
            .AddScoped<RefereeService>()
            ;
    }

    /// <summary>
    /// Обработчик события запуска приложения.
    /// </summary>
    private async void OnStartup(object sender, StartupEventArgs e)
    {
        var exceptionsProvider = _serviceProvider.GetService<IExceptionsProvider>()!;
        try
        {
            // Создаем каталоги приложения
            var appSettingService = _serviceProvider.GetService<AppSettingService>();
            var result = appSettingService?.AppDir.CreateAppDirs();
            if (result != null && ! result)
            {
                exceptionsProvider.Exception = new DbFatalException(innerException: result.Excptn);
                exceptionsProvider.IsFatal = true;
                return;
            }
            
            var repositoryHelper = _serviceProvider.GetService<IRepositoryHelper>()!;

            if (! await repositoryHelper.IsExistLibrary())
            {
                Exception? exception;

                // Пересоздание репозитория
                result = await repositoryHelper.RebuildRepository();
                if (! result)
                {
                    // Пробрасываем исключение "Ошибка создания БД."
                    exception = new AppException($"{AppPhrases.DatabaseCreateError}", result.Excptn);
                    exceptionsProvider.Exception = exception;
                    return;
                }

                // Пробрасываем исключение "Одна или несколько сущностей Библиотеки отсутствуют..."
                exception = new AppException($"{AppPhrases.MissingEntitiesError}\n{AppPhrases.DatabaseRebuilt}");
                exceptionsProvider.Exception = exception;
            }
        }
        catch (Exception ex)
        {
            // Пробрасываем фатальное исключение
            exceptionsProvider.Exception = new DbFatalException(innerException: ex);
            exceptionsProvider.IsFatal = true;
        }
        finally
        {
            // Получаем главное представление (окно) и показываем его
            var mainView = _serviceProvider.GetRequiredService<MainView>();
            mainView.Show();
        }
    }
}