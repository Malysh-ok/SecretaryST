using System;
using AppDomain.AppEntities;
using AppDomain.AppUseCases._Contracts;
using AppDomain.AppUseCases.Services;
using Common.WpfModule.Ui.Views;
using Common.WpfModule.Ui.Views._Contracts;
using DataAccess.DbContexts.DbConfigure;
using Microsoft.Extensions.DependencyInjection;
using Presentation.ViewModels.MainView;
// ReSharper disable InconsistentNaming

namespace Presentation.Shell.Common;

/// <summary>
/// Фабрика для создания сервисов приложения, используемых как в рантайме, так и в дизайн-тайме (миграции).
/// </summary>
/// <remarks>
/// Предоставляет методы для создания экземпляров с единой логикой.
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
    
    /// <summary>
    /// Создаёт экземпляр <see cref="SettingVM"/> с использованием DI-контейнера.
    /// </summary>
    /// <param name="sp">Провайдер сервисов для разрешения зависимостей.</param>
    /// <param name="view">Представление, реализующее <see cref="IViewWithResources"/>.</param>
    /// <returns>Сконфигурированный экземпляр <see cref="SettingVM"/>.</returns>
    /// <remarks>
    /// Использует <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/>,
    /// что позволяет автоматически разрешать все зарегистрированные зависимости
    /// и передавать дополнительные параметры (например, <paramref name="view"/>).
    /// </remarks>
    public static SettingVM CreateSettingVM(IServiceProvider sp, IViewWithResources view)
        => ActivatorUtilities.CreateInstance<SettingVM>(sp, view);

    /// <summary>
    /// Создаёт экземпляр <see cref="BackstageVM"/> с использованием DI-контейнера.
    /// </summary>
    /// <param name="sp">Провайдер сервисов для разрешения зависимостей.</param>
    /// <returns>Сконфигурированный экземпляр <see cref="BackstageVM"/>.</returns>
    /// <remarks>
    /// Аналогично <see cref="CreateSettingVM"/>, но без дополнительных параметров.
    /// Все зависимости разрешаются автоматически из <paramref name="sp"/>.
    /// </remarks>
    public static BackstageVM CreateBackstageVM(IServiceProvider sp)
        => ActivatorUtilities.CreateInstance<BackstageVM>(sp);
}