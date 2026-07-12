using System;
using System.Globalization;
using System.IO;
using System.Windows;
using AppDomain.AppAssets.Services;
using AppDomain.AppAssets.Strings;
using AppDomain.AppEntities;
using AppDomain.AppUseCases._Contracts;
using AppDomain.AppUseCases.Services;
using Common.BaseExtensions;
using Common.WpfModule.Ui.Services;
using DataAccess.DataAccessAssets.Services;
using DataAccess.DataAccessExceptions;
using DataAccess.DbContexts;
using DataAccess.DbContexts.DbConfigure;
using DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Shell.Common;
using Presentation.Shell.Views;
using Presentation.ViewModels._Contracts;
using Presentation.ViewModels.Common;
using ProblemDomain.UseCases.Services;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using ProblemDomain.UseCases._Contracts;

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
            // Регистрируем сервисы Presentation
            .AddSingleton<MainView>()                                           // регистрируем главное представление
            .AddSingleton<IExceptionsProvider, ExceptionsProvider>()
            
            // Регистрируем сервисы предметной области приложения (AppDomain)
            .AddSingleton(ServiceFactory.CreateAppInfoFromAssembly())           // регистрируем AppInfo
            .AddSingleton<IAppErrorMsgProvider, DomainErrorMsgProvider>()       // регистрируем провайдер сообщений об ошибках
            .AddSingleton<AppDirService>(sp =>
            {
                var provider = sp.GetRequiredService<IAppErrorMsgProvider>();
                return ServiceFactory.CreateAppDirService(provider);
            })                                                                  // регистрируем сервис директорий приложения
            .AddSingleton<AppSettingService>(sp =>
            {
                var provider = sp.GetRequiredService<IAppErrorMsgProvider>();
                var appDir = sp.GetRequiredService<AppDirService>();
                var appInfo = sp.GetRequiredService<AppInfo>();
                return ServiceFactory.CreateAppSettingService(provider, appDir, appInfo);
            })                                                                  // регистрируем сервис настроек приложения
            
            // Регистрируем сервисы предметной области (ProblemDomain)
            .AddScoped<CompetitionDataService>()
            .AddScoped<RefereeService>()
            .AddScoped<SportEventService>()
            .AddSingleton<IProblemErrorMsgProvider, DomainErrorMsgProvider>()   // регистрируем провайдер сообщений об ошибках
            
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
                var appSettings = sp.GetRequiredService<AppSettingService>();
                return new LoggerConfiguration()
                       .MinimumLevel.Information()
                       // REMARK: Тут баг в Райдере с раскраской консоли (https://youtrack.jetbrains.com/issue/RIDER-71410)
                       // Костыль такой: в настройках проекта поменять выходной тип с WinExe на Exe.
                       // П.С. Для темной темы лучше использовать AnsiConsoleTheme.Sixteen, а не AnsiConsoleTheme.Literate, SystemConsoleTheme.Colored.
                       .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen)
                       .WriteTo.File(Path.Combine(appSettings.AppDir.LogsPath, "Application-.log"),
                           rollingInterval: RollingInterval.Day,
                           retainedFileCountLimit: 50)
                       .CreateLogger();
            })                                                                  // регистрируем логгер
            .AddSingleton<StatusBarService>()                                   // регистрируем сервис статус-бара
            ;
    }

    /// <summary>
    /// Обработчик события запуска приложения.
    /// </summary>
    private async void OnStartup(object sender, StartupEventArgs e)
    {
        var exceptionsProvider = _serviceProvider.GetRequiredService<IExceptionsProvider>();
        var dataAccessErrorMsgProvider = _serviceProvider.GetRequiredService<DataAccessErrorMsgProvider>();
        var domainErrorMsgProvider = _serviceProvider.GetRequiredService<IAppErrorMsgProvider>();
        try
        {
            var appSettingService = _serviceProvider.GetService<AppSettingService>();
            
            // Берем название языка из настроек и устанавливаем язык культуры и фраз
            var lang = appSettingService?.AppLocalization.GetLangFromSetting();
            var ci = lang.IsValidCulture()
                ? CultureInfo.GetCultureInfo(lang!)
                : CultureInfo.GetCultureInfo("ru-RU");  // если язык не найден - делаем русский по умолчанию
            CultureInfo.CurrentUICulture = ci;
            CultureInfo.CurrentCulture = ci;
            AppPhrases.Culture = ci;
                        
            // Создаем каталоги приложения
            var result = appSettingService?.AppDir.CreateAppDirs();
            if (result != null && ! result)
            {
                exceptionsProvider.Exception = domainErrorMsgProvider.CreateFatalException(result.Excptn);
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
            // Получаем главное представление (окно) и показываем его
            var mainView = _serviceProvider.GetRequiredService<MainView>();
            mainView.Show();
        }
    }
}