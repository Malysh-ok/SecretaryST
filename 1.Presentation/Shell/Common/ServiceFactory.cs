using System;
using AppDomain.AppEntities;
using AppDomain.AppUseCases._Contracts;
using AppDomain.AppUseCases.Services;
using DataAccess.DbContexts.DbConfigure;

namespace Presentation.Shell.Common;

/// <summary>
/// Фабрика для создания сервисов приложения, используемых как в рантайме, так и в дизайн-тайме (миграции).
/// </summary>
/// <remarks>
/// Предоставляет методы для создания экземпляров <see cref="AppDirService"/>, <see cref="AppSettingService"/>
/// и <see cref="DbConfigurator"/> с единой логикой.
/// </remarks>
public static class ServiceFactory
{
    /// <summary>
    /// Создаёт экземпляр <see cref="AppInfo"/> из параметров.
    /// </summary>
    public static AppInfo CreateAppInfo(string name, Version version, DateTime buildDate)
        => new(name, version, buildDate);

    /// <summary>
    /// Создаёт экземпляр <see cref="AppInfo"/> из сборки (BuildInformation).
    /// </summary>
    public static AppInfo CreateAppInfoFromAssembly()
        // Используем NuGet-пакет LinkDotNet.BuildInformation
        // с генератором исходного кода
        => CreateAppInfo(
            BuildInformation.AssemblyName,
            new Version(BuildInformation.AssemblyVersion),
            BuildInformation.BuildAt.ToLocalTime()
        );
    /// <summary>
    /// Создаёт экземпляр <see cref="AppDirService"/>.
    /// </summary>
    /// <param name="errorMsgProvider">Провайдер сообщений об ошибках.</param>
    /// <returns>Сконфигурированный <see cref="AppDirService"/>.</returns>
    public static AppDirService CreateAppDirService(
        IAppErrorMsgProvider errorMsgProvider)
        => new(errorMsgProvider);

    /// <summary>
    /// Создаёт экземпляр <see cref="AppSettingService"/>.
    /// </summary>
    /// <param name="errorMsgProvider">Провайдер сообщений об ошибках.</param>
    /// <param name="appDirService">Сервис для работы с путями.</param>
    /// <param name="appInfo">Информация о приложении.</param>
    /// <returns>Сконфигурированный <see cref="AppSettingService"/>.</returns>
    public static AppSettingService CreateAppSettingService(
        IAppErrorMsgProvider errorMsgProvider,
        AppDirService appDirService,
        AppInfo appInfo)
        => new(errorMsgProvider, appDirService, appInfo);

    /// <summary>
    /// Создаёт экземпляр <see cref="DbConfigurator"/>.
    /// </summary>
    /// <param name="appDirService">Сервис для получения путей к файлам настроек и базе данных.</param>
    /// <returns>Сконфигурированный <see cref="DbConfigurator"/>.</returns>
    public static DbConfigurator CreateDbConfigurator(AppDirService appDirService)
        => new(appDirService.SettingFilePath, appDirService.AbsoluteDatabasePath);
}