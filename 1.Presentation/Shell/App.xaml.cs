using System;
using System.Globalization;
using System.Windows;
using AppDomain.Setting.Services;
using AppDomain.UseCases._Contracts;
using DataAccess.DbContexts;
using DataAccess.Repositories;
using DataAccess.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Shell.Views;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Exceptions;

// using ILogger = Serilog.ILogger;

namespace Presentation.Shell;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    // private readonly IHost _host;

    public App()
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
        
        // _host = CreateHostBuilder(Environment.GetCommandLineArgs()).Build();
        
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
        
        services
            .AddSingleton<MainView>()                                    // внедряем главное представление
            .AddSingleton(startupItemsFactory.CreateAppSettingService()) // внедряем сервис настроек приложения
            .AddSingleton(dbConfigurator)                                // внедряем конфигуратор БД
            .AddDbContext<AppDbContext>(options =>
                dbConfigurator.UseProvider<AppDbContext>(options)) // внедряем контекст БД
            .AddTransient<IRepository, Repository<AppDbContext>>()         // внедряем репозиторий
            .AddTransient<IInitRepository, Repository<AppDbContext>>()
            // .AddLogging(builder => builder.AddSerilog(dispose: true))
            .AddSingleton<Serilog.ILogger>
            (x => new LoggerConfiguration()
                // .MinimumLevel.Information()
                .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                .CreateLogger()
            )
            ;
    }

    /// <summary>
    /// Обработчик события запуска приложения.
    /// </summary>
    private async void OnStartup(object sender, StartupEventArgs e)
    {
        // Создаем каталоги приложения
        var appSettingService = _serviceProvider.GetService<AppSettingService>();
        var result = appSettingService?.AppDir.CreateAppDirs();     // TODO: обработать result
        
        AppDbContext dbContext = _serviceProvider.GetService<AppDbContext>()!;
        ILogger logger = _serviceProvider.GetService<ILogger>()!;
        IInitRepository initRepository = _serviceProvider.GetService<IInitRepository>()!;
        try
        {
            // Применяем последнюю миграцию
            if (dbContext is { /*IsPossibleConnect: true,*/ IsNullOrEmptyConnectionString: false })
                await dbContext.Database.MigrateAsync();

            if (! await initRepository.IsExistLibrary())
            {
                logger.Error("Одна или несколько сущностей Библиотеки отсутствуют!");
                
                // Пересоздание репозитория
                await initRepository.RebuildRepository();
            }
        }
        catch (Exception exception)
        {
            // TODO: Обработка исключений при запуске - временно
            
            logger.Error(new DbFatalException(innerException: exception), "Исключение: ");

            // Пересоздание репозитория
            await initRepository.RebuildRepository();
            
            // Shutdown();
        }
        
        // Получаем главное представление (окно) и показываем его
        var mainView = _serviceProvider.GetService<MainView>();
        mainView!.Show();
    }
}