using AppDomain.Setting.Services;
using DataAccess.DbContexts.DbConfigure;
using Microsoft.Extensions.Configuration;

namespace Presentation.Shell;

/// <summary>
/// Фабрика создания экземпляров классов, необходимых для запуска приложения.
/// </summary>
public class StartupItemsFactory
{
    /// <summary>
    /// Сервис настроек приложения.
    /// </summary>
    private readonly AppSettingService _appSettingService = new();

    /// <summary>
    /// Создаем сервис настроек приложения.
    /// </summary>
    public AppSettingService CreateAppSettingService()
        => _appSettingService;

    /// <summary>
    /// Создаем конфигуратор БД.
    /// </summary>
    public DbConfigurator CreateDbConfigurator()
    {
        // Создаем конфигурацию, наполняем ее данными из файла конфигурации
        var configuration = new ConfigurationManager();
        configuration.AddXmlFile(_appSettingService.SettingFilePath, optional: true).Build();
        // configuration.AddJsonFile(_appSettingService.SettingJsonFilePath, optional: true).Build();

        // Создаем и возвращаем конфигуратор БД
        return new DbConfigurator(configuration, _appSettingService.AppDir.AbsoluteDatabasePath);
    }
}