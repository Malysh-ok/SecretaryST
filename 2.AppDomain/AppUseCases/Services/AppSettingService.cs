using System.Configuration;
using AppDomain.AppEntities;
using AppDomain.AppUseCases._Contracts;

namespace AppDomain.AppUseCases.Services;

/// <summary>
/// Сервис настроек приложения.
/// </summary>
public class AppSettingService
{
    #region [---------- НЕ публичные члены ----------]
    
    /// <summary>
    /// Конфигурация (настройки).
    /// </summary>
    private readonly Configuration _config;
    
    /// <summary>
    /// Получаем наименование языка из настроек.
    /// </summary>
    private string GetLangConfigItem()
        => GetConfigItem("Lang") ?? string.Empty;

    /// <summary>
    /// Сохраняем наименование языка в настройки.
    /// </summary>
    private void SetOrUpdateLangConfigItem(string value)
        => SetConfigItem("Lang", value);

    #endregion

    /// <summary>
    /// Название программы.
    /// </summary>
    public string AppName { get; }
    
    /// <summary>
    /// Версия программы.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public Version AppVersion { get; } 
    
    /// <summary>
    /// Дата сборки программы.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public DateTime AppBuildDate { get; }

    /// <summary>
    /// Сервис директорий приложения.
    /// </summary>
    public AppDirService AppDir { get; }

    /// <summary>
    /// Сервис локализации приложения.
    /// </summary>
    public AppLocalizationService AppLocalization { get; }

    /// <summary>
    /// Конструктор.
    /// </summary>
    public AppSettingService(IAppErrorMsgProvider appErrorMsgProvider, AppDirService appDir, AppInfo appInfo)
    {
        AppName = appInfo.Name;
        AppVersion = appInfo.Version;
        AppBuildDate = appInfo.BuildDate;
        AppDir = appDir;
        AppLocalization = new AppLocalizationService(appErrorMsgProvider, GetLangConfigItem, SetOrUpdateLangConfigItem);
        var configurationFileMap = new ExeConfigurationFileMap
        {
            ExeConfigFilename = appDir.SettingFullFilePath
        };
        _config = ConfigurationManager.OpenMappedExeConfiguration(configurationFileMap, ConfigurationUserLevel.None);
    }

    /// <summary>
    /// Получение значения элемента настроек по ключу.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public string? GetConfigItem(string key)
        => _config.AppSettings.Settings[key]?.Value;
    
    /// <summary>
    /// Сохранение элемента настроек по ключу.
    /// </summary>
    public void SetConfigItem(string key, string value)
    {
        if (_config.AppSettings.Settings[key] == null)
            _config.AppSettings.Settings.Add(key, value);
        else
            _config.AppSettings.Settings[key].Value = value;
    }
    
    /// <summary>
    /// Сохранение настроек в файле.
    /// </summary>
    public void SaveConfig()
        => _config.Save();
}