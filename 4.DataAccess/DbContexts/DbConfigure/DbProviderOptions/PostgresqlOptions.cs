using Common.BaseExtensions;
using DataAccess.DbContexts.DbConfigure._Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccess.DbContexts.DbConfigure.DbProviderOptions;

/// <summary>
/// Опции (настройки) провайдера БД - PostgreSql.
/// </summary>
public class PostgresqlOptions : IDbProviderOptions
{
    /// <inheritdoc />
    public DbProviderEnm DbProviderName => DbProviderEnm.PostgreSql;

    /// <inheritdoc />
    public bool IsEmbeddedDb => false;
    
    /// <inheritdoc />
    public string DateTimeColumnType => "TimestampTz";
    
    /// <inheritdoc />
    public string? MigrationsAssemblyName { get; private init; }
    
    /// <summary>
    /// Конструктор, запрещающий создание экземпляра без параметров.
    /// </summary>
    private PostgresqlOptions()
    {
    }

    /// <inheritdoc />
    public static IDbProviderOptions Create(string? migrationsAssemblyName = null)
    {
        var sqliteOptions = new PostgresqlOptions
        {
            MigrationsAssemblyName = migrationsAssemblyName
        };
        return sqliteOptions;
    }
    
    /// <inheritdoc />
    public void ClearAutoincrementSequence(DbContext dbContext, 
        string autoincrementColumnName, params string[] tableNames)
    {
        // TODO: В PostgreeSQL не тестировалось!
        foreach (var table in tableNames)
        {
            // Получаем название последовательности для столбца с автоинкрементом
            var sequenceName = dbContext.Database.SqlQueryRaw<string>
                ($"SELECT pg_get_serial_sequence('{table}', '{autoincrementColumnName}');").FirstOrDefault();

            // Обнуляем счетчик
            // dbContext.Database.ExecuteSqlRaw
            //     ($"ALTER SEQUENCE {sequenceName} RESTART WITH 1;");
            dbContext.Database.ExecuteSqlRaw
                ($"ALTER SEQUENCE {sequenceName} RESTART WITH 1;");
        }
    }

    /// <inheritdoc />
    public string FixConnectionString(string connectionString, string? rootPath) =>
        // По сути - ничего не делаем: просто пробрасываем connectionString
        connectionString;

    /// <inheritdoc />
    public string GetConnectionString(ConfigurationManager configuration)
    {
        // TODO: Не тестировалось!
        return configuration.GetConnectionString("PostgreSqlConnection")?.Trim() ?? string.Empty;
    }

    /// <inheritdoc />
    public void UseProvider<TDbContext>(
        DbContextOptionsBuilder optionsBuilder, string connectionString) where TDbContext : DbContext
    {
        if (MigrationsAssemblyName.IsNullOrEmpty())
            optionsBuilder.UseNpgsql(connectionString);
        else
            optionsBuilder.UseNpgsql(connectionString, 
                b => b.MigrationsAssembly(MigrationsAssemblyName));
    }

    /// <inheritdoc />
    public void ModelBuilderInit(ModelBuilder modelBuilder)
    {
        modelBuilder.UseIdentityByDefaultColumns(); // стратегия генерации автоувеличения значения столбцов

        modelBuilder.HasPostgresExtension("adminpack")
            .HasAnnotation("Relational:Collation", "Russian_Russia.1251")    // правило сортировки
            .HasAnnotation("Relational:CType", "Russian_Russia.1251");       // набор символов
    }
}