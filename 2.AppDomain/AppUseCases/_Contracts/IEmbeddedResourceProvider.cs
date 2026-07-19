namespace AppDomain.AppUseCases._Contracts;

/// <summary>
/// Предоставляет доступ к ресурсам, встроенным в исполняемую сборку приложения.
/// </summary>
public interface IEmbeddedResourceProvider
{
    /// <summary>
    /// Гарантирует наличие файла по указанному пути, извлекая его из встроенного ресурса.
    /// </summary>
    /// <param name="resourceName">Имя ресурса.</param>
    /// <param name="outputPath">Полный путь к файлу, который нужно создать.</param>
    void EnsureSettingsFileExists(string resourceName, string outputPath);
}