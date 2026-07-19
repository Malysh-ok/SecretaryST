using System.Reflection;
using AppDomain.AppUseCases._Contracts;

namespace AppDomain.AppAssets.Services;

/// <summary>
/// Реализация провайдера, извлекающая ресурсы, встроенные в исполняемую сборку приложения.
/// </summary>
/// <remarks>
/// Использует <see cref="Assembly.GetExecutingAssembly"/> для доступа к ресурсам,
/// добавленным в проект с типом <c>Build Action = Embedded Resource</c>.
/// </remarks>
public class EmbeddedResourceProvider : IEmbeddedResourceProvider
{
    private const string SettingsResourcePath = "AppDomain.AppAssets.Config";

    /// <inheritdoc />
    public void EnsureSettingsFileExists(string resourceName, string outputPath)
    {
        if (File.Exists(outputPath))
            return;

        var fullResourceName = $"{SettingsResourcePath}.{resourceName}";
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(fullResourceName);
        if (stream == null)
            throw new InvalidOperationException($"Ресурс '{fullResourceName}' не найден в сборке.");

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        using var file = File.Create(outputPath);
        stream.CopyTo(file);
    }
}