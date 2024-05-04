using System.Configuration;
using Common.BaseComponents.Components.IO;

namespace AppDomain.Setting.Services;

/// <summary>
/// Сервис настроек приложения.
/// </summary>
public class AppSettingService
{
    #region [---------- НЕ публичные члены ----------]
    
    /// <summary>
    /// Наименование файла конфигурации.
    /// </summary>
    private const string SettingFileName = "Setting.xml";

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
    /// Путь к файлу конфигурации.
    /// </summary>
    public string SettingFilePath
        => PathEx.Combine(AppDir.ConfigPath, SettingFileName);
    
    /// <summary>
    /// Путь к файлу конфигурации.
    /// </summary>
    public string SettingFullFilePath
        => PathEx.Combine(AppDir.AbsoluteConfigPath, SettingFileName);

    /// <summary>
    /// !!!!!!!!!!!!!!!!!!!!!!.
    /// </summary>
    public string SettingJsonFilePath
        => PathEx.Combine(AppDir.ConfigPath, "appsettings.json");

    
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
    public AppSettingService()
    {
        AppDir = new AppDirService();
        
        AppLocalization = new AppLocalizationService(GetLangConfigItem, SetOrUpdateLangConfigItem);

        var configurationFileMap = new ExeConfigurationFileMap
        {
            ExeConfigFilename = SettingFullFilePath
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
    // ReSharper disable once MemberCanBePrivate.Global
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
    // public async Task SaveConfig()
    //     => await Task.Run(() => _config.Save());
    public void SaveConfig()
        => _config.Save();

}