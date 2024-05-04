using Common.BaseComponents.Components;
using Common.BaseExtensions;
using DataAccess.DbContexts.DbConfigure._Contracts;
using DataAccess.DbContexts.DbConfigure.DbProviderOptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccess.DbContexts.DbConfigure;

/// <summary>
/// Кофигуратор БД.
/// </summary>
/// <remarks>
/// Различные настройки, связанные с подключением и работой БД.
/// </remarks>
public class DbConfigurator
{
    /// <summary>
    /// Строка подключения к БД.
    /// </summary>
    public string ProcessedConnectionString { get; private set; }
        
    /// <summary>
    /// Опции текущего провайдера БД.
    /// </summary>
    public IDbProviderOptions ProviderOptions { get; private set; }

    /// <summary>
    /// Признак того, что строка подключения к БД зашифрована.
    /// </summary>
    public bool IsEncryptedConnectionString { get; set; }
        
    /// <summary>
    /// Наименование сборки с миграциями
    /// (по всей видимости, необходимо только фабрике контекста БД,
    /// которая используется в Entity Framework Core).
    /// </summary>
    public string? MigrationsAssemblyName
    {
        get => ProviderOptions.MigrationsAssemblyName;
        set => ProviderOptions = GetProviderOptions(value!);
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    public DbConfigurator(ConfigurationManager configuration, string databasePath)
    {
        ProviderOptions = GetProviderOptions();
        IsEncryptedConnectionString = false;

        // Получаем строку подключения из конфигурации, обрабатываем и сохраняем
        var connectionStringPre = 
            ProviderOptions.GetConnectionString(configuration);
        ProcessedConnectionString = 
            GetProcessedConnectionString(connectionStringPre, databasePath);
    }
        
    /// <summary>
    /// Получить опции текущего провайдера БД (паттерн "Стратегия").
    /// </summary>
    /// <remarks>
    /// В нашем случае провайдер - SQLite.
    /// </remarks>
    private static IDbProviderOptions GetProviderOptions(string? migrationsAssemblyName = null) =>
        SqliteOptions.Create(migrationsAssemblyName);

    /// <summary>
    /// Получить обработанную строку подключения к БД.
    /// </summary>
    /// <param name="connectionStringPre">Строка подключения, полученная из конфигурации.</param>
    /// <param name="rootPath">Путь к корневой папке приложения.</param>
    private string GetProcessedConnectionString(string connectionStringPre, string? rootPath = null)
    {
        // Дешифруем строку подключения при необходимости
        var connStr = IsEncryptedConnectionString
            ? DecryptText(connectionStringPre)
            : connectionStringPre;
        
        // Зашифрованный вариант строки подключения - для отладки
        // var encryptConnStr = EncryptText(connStr);

        // Возвращаем откорректированную строку подключения 
        return ProviderOptions.FixConnectionString(connStr, rootPath);
    }

    /// <summary>
    /// Данные для шифрации/дешифрации.
    /// </summary>
    private static (byte[], byte[]) GetEncryptData()
    {
        var secretKey = ArrayExtensions.RandomByteArray(1, Crypto.SecretKeySize);
        var iv = ArrayExtensions.RandomByteArray(2, Crypto.IvSize);

        return (secretKey, iv);
    }
    
    /// <summary>
    /// Шифруем текст.
    /// </summary>
    private static string EncryptText(string text)
    {
        var (secretKey, iv) = GetEncryptData();
        
        return Crypto.Encrypt(text, secretKey, iv);
    }
    
    /// <summary>
    /// Дешифруем текст.
    /// </summary>
    private static string DecryptText(string text)
    {
        var (secretKey, iv) = GetEncryptData();
        
        return Crypto.Decrypt(text, secretKey, iv);
    }

        
    /// <summary>
    /// Настраивает контекст для подключения к БД.
    /// </summary>
    /// <param name="optionsBuilder">Экземпляр <see cref="DbContextOptionsBuilder"/>,
    ///     с помощью которого получаем опции контекста БД.</param>
    public void UseProvider<TDbContext>(DbContextOptionsBuilder optionsBuilder)
        where TDbContext: DbContext
    {
        ProviderOptions.UseProvider<TDbContext>(optionsBuilder, ProcessedConnectionString);
    }

    /// <inheritdoc cref="IDbProviderOptions.ModelBuilderInit(ModelBuilder)"/>
    public void ModelBuilderInit(ModelBuilder modelBuilder)
    {
        ProviderOptions.ModelBuilderInit(modelBuilder);
    }
}