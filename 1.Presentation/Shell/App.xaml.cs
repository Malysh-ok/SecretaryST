using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Windows;
using AppDomain.AppAssets.Services;
using AppDomain.AppEntities;
using AppDomain.AppUseCases._Contracts;
using AppDomain.AppUseCases.Services;
using Common.BaseExtensions;
using Common.WpfModule.Ui.Services;
using Common.WpfModule.Ui.Services._Contracts;
using Common.WpfModule.Ui.Views._Contracts;
using DataAccess.DataAccessAssets.Services;
using DataAccess.DataAccessExceptions;
using DataAccess.DbContexts;
using DataAccess.DbContexts.DbConfigure;
using DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Shell.Infrastructure;
using Presentation.Shell.Views;
using Presentation.ViewModels.AppSetting;
using Presentation.ViewModels.Main;
using Presentation.ViewModels.Shared.Infrastructure;
using Presentation.ViewModels.Shared.Infrastructure._Contracts;
using ProblemDomain.UseCases._Contracts;
using ProblemDomain.UseCases.Services;
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
            // Регистрируем сервисы предметной области приложения (AppDomain)
            .AddSingleton(ServiceFactory.CreateAppInfoFromAssembly())               // регистрируем AppInfo
            .AddSingleton<IAppErrorMsgProvider, DomainErrorMsgProvider>()           // регистрируем провайдер сообщений об ошибках
            .AddSingleton<IEmbeddedResourceProvider, EmbeddedResourceProvider>()    // регистрируем провайдер ресурсов
            .AddSingleton<AppDirService>(sp =>
            {
                var provider = sp.GetRequiredService<IAppErrorMsgProvider>();
                return ServiceFactory.CreateAppDirService(provider);
            })                                                                  // регистрируем сервис директорий приложения
            .AddSingleton<AppSettingsService>(sp =>
            {
                var errProvider = sp.GetRequiredService<IAppErrorMsgProvider>();
                var resourceProvider = sp.GetRequiredService<IEmbeddedResourceProvider>();
                var appDir = sp.GetRequiredService<AppDirService>();
                var appInfo = sp.GetRequiredService<AppInfo>();
                return ServiceFactory.CreateAppSettingService(errProvider, resourceProvider, appDir, appInfo);
            })                                                                  // регистрируем сервис настроек приложения

            // Регистрируем сервисы предметной области (ProblemDomain)
            .AddScoped<CompetitionDataService>()
            .AddScoped<RefereeService>()
            .AddScoped<SportEventService>()
            .AddSingleton<IProblemErrorMsgProvider,
                DomainErrorMsgProvider>()                                       // регистрируем провайдер сообщений об ошибках

            // Регистрируем сервисы слоя доступа к данным (DataAccess)
            .AddScoped<DbConfigurator>(sp =>
            {
                var appDir = sp.GetRequiredService<AppDirService>();
                return ServiceFactory.CreateDbConfigurator(appDir);
            })                                                                  // регистрируем конфигуратор БД
            .AddDbContext<AppDbContext>((sp, options) =>
            {
                var configurator = sp.GetRequiredService<DbConfigurator>();
                configurator.UseProvider<AppDbContext>(options);
            })                                                                  // регистрируем контекст БД
            .AddScoped<IRepository, Repository<AppDbContext>>()                 // регистрируем репозиторий
            .AddTransient<IRepositoryHelper, RepositoryHelper>()                // регистрируем "помощник" репозитория
            .AddSingleton<DataAccessErrorMsgProvider>()                         // регистрируем провайдер сообщений об ошибках

            // Регистрируем общие сервисы (Common)
            .AddSingleton<ILogger>(sp =>
            {
                var appDirService = sp.GetRequiredService<AppDirService>();
                return new LoggerConfiguration()
                       .MinimumLevel.Information()
                       // REMARK: Тут баг в Rider с раскраской консоли (https://youtrack.jetbrains.com/issue/RIDER-71410)
                       // Костыль такой: в настройках проекта поменять выходной тип с WinExe на Exe.
                       // П.С. Для темной темы лучше использовать AnsiConsoleTheme.Sixteen, а не AnsiConsoleTheme.Literate, SystemConsoleTheme.Colored.
                       .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen)
                       .WriteTo.File(Path.Combine(appDirService.LogsPath, "Application-.log"),
                           rollingInterval: RollingInterval.Day,
                           retainedFileCountLimit: 50)
                       .CreateLogger();
            })                                                                  // регистрируем логгер
            .AddSingleton<StatusBarService>()                                   // регистрируем сервис статус-бара

            // Регистрируем сервисы Presentation
            .AddSingleton<IExceptionsProvider, ExceptionsProvider>()            // регистрируем поставщика исключений
            .AddSingleton<Func<IViewWithResources, SettingVM>>(sp => view =>
                ServiceFactory.CreateSettingVM(sp, view))                       // регистрируем фабрику для создания SettingVM
            .AddSingleton<Func<BackstageVM>>(sp => () =>
                ServiceFactory.CreateBackstageVM(sp))                           // регистрируем фабрику для создания BackstageVM
            .AddSingleton<MainVM>()                                             // регистрируем главную ViewModel для главного представления
            .AddTransient<AppSettingVM>()
            
            .AddSingleton<IViewService, ViewService>()
            .AddSingleton<MainView>()                                           // регистрируем главное представление
            .AddTransient<AppSettingView>()
            ;
    }

    /// <summary>
    /// Обработчик события запуска приложения.
    /// </summary>
    private async void OnStartup(object sender, StartupEventArgs e)
    {
        // Получаем необходимые сервисы
        var exceptionsProvider = _serviceProvider.GetRequiredService<IExceptionsProvider>();
        var domainErrorMsgProvider = _serviceProvider.GetRequiredService<IAppErrorMsgProvider>();
        var dataAccessErrorMsgProvider = _serviceProvider.GetRequiredService<DataAccessErrorMsgProvider>();
        try
        {
            // Инициализация ViewModelLocator
            ViewModelLocator.Initialize(_serviceProvider);

            // Получаем сервис директорий приложения
            var appDirService = _serviceProvider.GetRequiredService<AppDirService>();

            // Создаем каталоги приложения
            var result = appDirService.CreateAppDirs();
            if (! result)
            {
                exceptionsProvider.Exception = domainErrorMsgProvider.CreateFatalException(result.Excptn);
                exceptionsProvider.IsFatal = true;
                return;
            }

            // Получаем сервис настроек приложения (с целью выявления исключений)
            _ = _serviceProvider.GetRequiredService<AppSettingsService>();
            
            var repositoryHelper = _serviceProvider.GetRequiredService<IRepositoryHelper>();

            if (! await repositoryHelper.IsExistLibrary())
            {
                Exception? exception;

                // Пересоздание репозитория
                result = await repositoryHelper.RebuildRepository();
                if (! result)
                {
                    // Пробрасываем исключение "Ошибка создания БД."
                    exception = dataAccessErrorMsgProvider.CreateException(DataAccessErrorCodes.DbCreateError, result.Excptn);
                    exceptionsProvider.Exception = exception;
                    return;
                }

                // Пробрасываем исключение "Одна или несколько сущностей Библиотеки отсутствуют..."
                var innerEx = dataAccessErrorMsgProvider.CreateException(
                    DataAccessErrorCodes.MissingEntitiesError, result.Excptn);
                exception = dataAccessErrorMsgProvider.CreateException(
                    DataAccessErrorCodes.DbRecreated, innerEx);
                exceptionsProvider.Exception = exception;
            }
        }
        catch (Exception ex)
        {
            // Пробрасываем фатальное исключение
            exceptionsProvider.Exception = domainErrorMsgProvider.CreateFatalException(ex);
            exceptionsProvider.IsFatal = true;
        }
        finally
        {
            // Получаем главное представление (окно)
            var mainView = _serviceProvider.GetRequiredService<MainView>();
            
            // Получаем главную ViewModel и инициализируем
            var mainViewModel = _serviceProvider.GetRequiredService<MainVM>();
            mainViewModel.Initialize(mainView);
            
            // Показываем главное представление
            mainView.Show();
        }
    }
}