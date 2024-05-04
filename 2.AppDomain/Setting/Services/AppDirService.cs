
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBePrivate.Global

using AppDomain.AppExceptions;
using Common.BaseComponents.Components;
using Common.BaseComponents.Components.IO;

namespace AppDomain.Setting.Services;

/// <summary>
/// Сервис директорий приложения.
/// </summary>
public class AppDirService
{
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
            return Result<bool>.Fail(
                AppException.CreateException("The application directories could not be created.", ex, "ru",
                    "Не удалось создать директории приложения."));
        }
    }
}