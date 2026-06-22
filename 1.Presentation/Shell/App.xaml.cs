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
                ServiceLifetime.Transient)                              // регистрируем контекст БД
            .AddTransient<IRepository, Repository<AppDbContext>>()      // регистрируем репозиторий
            .AddTransient<IRepositoryHelper, RepositoryHelper>()        // регистрируем "помощник" репозитория
            
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
        // Создаем каталоги приложения
        var appSettingService = _serviceProvider.GetService<AppSettingService>();
        var result = appSettingService?.AppDir.CreateAppDirs();     // TODO: обработать result в App.OnStartup после создания директорий
        
        var dbContext = _serviceProvider.GetService<AppDbContext>()!;
        var initRepository = _serviceProvider.GetService<IRepositoryHelper>()!;
        try
        {
            // Применяем последнюю миграцию
            if (dbContext is { /*IsPossibleConnect: true,*/ IsNullOrEmptyConnectionString: false })
                await dbContext.Database.MigrateAsync();

            if (! await initRepository.IsExistLibrary())
            {
                // Пересоздание репозитория
                result = await initRepository.RebuildRepository();  // TODO: обработать result в App.OnStartup после пересоздания репозитория

                // Пробрасываем исключение "Одна или несколько сущностей Библиотеки отсутствуют..."
                var exception = new AppException($"{AppPhrases.MissingEntitiesError}\n{AppPhrases.DatabaseRebuilt}");
                var exceptionsProvider = _serviceProvider.GetService<IExceptionsProvider>();
                exceptionsProvider!.Exception = exception;
            }
        }
        catch (Exception exception)
        {
            // TODO: Пересоздание репозитория
            // await initRepository.RebuildRepository();
            
            // Пробрасываем фатальное исключение
            var exceptionsProvider = _serviceProvider.GetService<IExceptionsProvider>();
            exceptionsProvider?.Exception = new DbFatalException(innerException: exception);
            exceptionsProvider?.IsFatal = true;
        }
        
        // Получаем главное представление (окно) и показываем его
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView!.Show();
    }
}