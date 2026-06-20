using System;
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
    private readonly AppSettingService _appSettingService;

    public StartupItemsFactory()
    {
        // Используем NuGet-пакет LinkDotNet.BuildInformation
        // с генератором исходного кода
        const string appName = BuildInformation.AssemblyName;
        var appVersion = new Version(BuildInformation.AssemblyVersion);
        var buildDate = BuildInformation.BuildAt.ToLocalTime();

        _appSettingService = new AppSettingService(appName, appVersion, buildDate);
    }
    
    /// <summary>
    /// Создание сервиса настроек приложения.
    /// </summary>
    public AppSettingService CreateAppSettingService()
        => _appSettingService;

    /// <summary>
    /// Создание конфигуратора БД.
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