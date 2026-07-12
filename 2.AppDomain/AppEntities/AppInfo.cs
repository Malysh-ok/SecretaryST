namespace AppDomain.AppEntities;

/// <summary>
/// Содержит информацию о приложении: имя, версию и дату сборки.
/// </summary>
public class AppInfo(string? name, Version? version, DateTime? buildDate)
{
    /// <summary>
    /// Имя приложения.
    /// </summary>
    public string Name { get; } = name ?? "Unknown";

    /// <summary>
    /// Версия приложения.
    /// </summary>
    public Version Version { get; } = version ?? new Version("1.0.0.0");

    /// <summary>
    /// Дата и время сборки (в локальном времени).
    /// </summary>
    public DateTime BuildDate { get; } = buildDate ?? DateTime.MinValue;
}