using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccess.DbContexts.DbConfigure._Contracts;

/// <summary>
/// Опции (настройки) провайдера БД.
/// </summary>
public interface IDbProviderOptions
{
    /// <summary>
    /// Тип провайдера.
    /// </summary>
    DbProviderEnm DbProviderName { get; }

    /// <summary>
    /// Признак того, что база данных является встраиваемой.
    /// </summary>
    bool IsEmbeddedDb { get; }
    
    /// <summary>
    /// Тип столбца БД, соответствующий сущности DateTime.
    /// </summary>
    string? DateTimeColumnType { get; }
    
    /// <summary>
    /// Наименование сборки с миграциями.
    /// </summary>
    string? MigrationsAssemblyName { get; }

    /// <summary>
    /// Создать экземпляр <see cref="IDbProviderOptions"/>.
    /// </summary>
    /// <param name="migrationsAssemblyName">Наименование сборки с миграциями.</param>
    static abstract IDbProviderOptions Create(string? migrationsAssemblyName);

    /// <summary>
    /// Сброс начальных значений автоинкремента для столбца <paramref name="autoincrementColumnName"/>
    /// всех таблиц массива <paramref name="tableNames"/>.
    /// </summary>
    /// <param name="dbContext">Контекст БД.</param>
    /// <param name="autoincrementColumnName">столбец, у которого сбрасывается автоинкремент
    /// (необходимость данного параметра определяется провайдером БД).</param>
    /// <param name="tableNames">массив таблиц, в которых сбрасываем автоинкремент.</param>
    void ClearAutoincrementSequence(DbContext dbContext, 
        string autoincrementColumnName, params string[] tableNames);

    /// <summary>
    /// Коррекция строки подключения.
    /// </summary>
    /// <remarks>
    /// Актуально, по всей видимости, только для встраиваемых БД,
    /// где в строке подключения указывается путь к файлу БД, этот путь и подлежит коррекции.
    /// </remarks>
    /// <param name="connectionString">Исходная строка подключения.</param>
    /// <param name="rootPath">Путь к корневой папке приложения.</param>
    string FixConnectionString(string connectionString, string? rootPath = null);

    /// <summary>
    /// Получить строку подключения из конфигурации.
    /// </summary>
    /// <param name="configuration">Конфигурация.</param>
    string GetConnectionString (ConfigurationManager configuration);

    /// <summary>
    /// Настраивает контекст для подключения к БД.
    /// </summary>
    /// <param name="optionsBuilder">Экземпляр <see cref="DbContextOptionsBuilder"/>,
    ///     с помощью которого получаем опции контекста БД.</param>
    /// <param name="connectionString">Строка подключения.</param>
    void UseProvider<TDbContext>(DbContextOptionsBuilder optionsBuilder,
        string connectionString) where TDbContext : DbContext;

    /// <summary>
    /// Различные настройки, применяемые при создании БД.
    /// </summary>
    /// <param name="modelBuilder">Экземпляр <see cref="ModelBuilder"/>.</param>
    void ModelBuilderInit(ModelBuilder modelBuilder);
}