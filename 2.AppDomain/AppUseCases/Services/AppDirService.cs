
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using AppDomain.AppExceptions;
using AppDomain.AppUseCases._Contracts;
using Common.BaseComponents.Components;
using Common.BaseComponents.Components.IO;

namespace AppDomain.AppUseCases.Services;

/// <summary>
/// Сервис директорий приложения.
/// </summary>
public class AppDirService
{
    /// <summary>
    /// Провайдер сообщений об ошибках предметной области приложения.
    /// </summary>
    private readonly IAppErrorMsgProvider _appErrorMsgProvider;

    /// <summary>
    /// Наименование файла конфигурации.
    /// </summary>
    public string SettingFileName => "Setting.xml";
    
    /// <summary>
    /// Путь к корневой папке приложения (там, где exe-файл). 
    /// </summary>
    public string RootPath => System.AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// Путь к папке с конфигурацией приложения.
    /// </summary>
    public string ConfigPath => "Config";
    
    /// <inheritdoc cref="ConfigPath"/>
    public string AbsoluteConfigPath => PathEx.Combine(RootPath, ConfigPath);
    
    /// <summary>
    /// Путь к файлу конфигурации.
    /// </summary>
    public string SettingFilePath
        => PathEx.Combine(ConfigPath, SettingFileName);
    
    /// <summary>
    /// Полный путь к файлу конфигурации.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public string SettingFullFilePath
        => PathEx.Combine(AbsoluteConfigPath, SettingFileName);

    /// <summary>
    /// Путь к папке с БД.
    /// </summary>
    public string DatabasePath => "Database";
    
    /// <inheritdoc cref="DatabasePath"/>
    public string AbsoluteDatabasePath => PathEx.Combine(RootPath, DatabasePath);
    
    /// <summary>
    /// Путь к папке с логами.
    /// </summary>
    public string LogsPath => "Logs";
    
    /// <inheritdoc cref="LogsPath"/>
    public string AbsoluteLogsPath => PathEx.Combine(RootPath, LogsPath);

    /// <summary>
    /// Конструктор.
    /// </summary>
    public AppDirService(IAppErrorMsgProvider appErrorMsgProvider)
    {
        _appErrorMsgProvider = appErrorMsgProvider;
    }
    
    /// <summary>
    /// Создание необходимых директорий приложения.
    /// </summary>
    public Result<bool> CreateAppDirs()
    {
        try
        {
            Directory.CreateDirectory(AbsoluteConfigPath);
            Directory.CreateDirectory(AbsoluteDatabasePath);
            Directory.CreateDirectory(AbsoluteLogsPath);

            return Result<bool>.Done(true);
        }
        catch (Exception ex)
        {
            var newEx = _appErrorMsgProvider.CreateException(AppErrorCodes.AppDirsCreateError, ex);
            return Result<bool>.Fail(newEx);
        }
    }
}