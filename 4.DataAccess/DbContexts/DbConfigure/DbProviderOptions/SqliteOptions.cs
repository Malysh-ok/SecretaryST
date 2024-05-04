using Common.BaseComponents.Components.IO;
using Common.BaseExtensions;
using DataAccess.DbContexts.DbConfigure._Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccess.DbContexts.DbConfigure.DbProviderOptions;

/// <summary>
/// Опции (настройки) провайдера БД - SQLite.
/// </summary>
public class SqliteOptions : IDbProviderOptions
{
    /// <inheritdoc />
    public DbProviderEnm DbProviderName => DbProviderEnm.Sqlite;

    /// <inheritdoc />
    public bool IsEmbeddedDb => true;

    /// <inheritdoc />
    public string? DateTimeColumnType => null;

    /// <inheritdoc />
    public string? MigrationsAssemblyName { get; private init; }

    /// <summary>
    /// Конструктор, запрещающий создание экземпляра без параметров.
    /// </summary>
    private SqliteOptions()
    {
    }

    /// <inheritdoc />
    public static IDbProviderOptions Create(string? migrationsAssemblyName = null)
    {
        var sqliteOptions = new SqliteOptions
        {
            MigrationsAssemblyName = migrationsAssemblyName
        };
        return sqliteOptions;
    }
    
    /// <inheritdoc />
    public void ClearAutoincrementSequence(DbContext dbContext, 
        string autoincrementColumnName, params string[] tableNames)
    {
        // Формируем из последовательности названий строку для SQL-выражения
        var str = $"'{string.Join("', '", tableNames)}'";
            
        // Удаляем названия таблиц из специальной скрытой таблицы sqlite_sequence,
        // предназначенной для автоинкремента столбцов
        dbContext.Database.ExecuteSqlRaw
            ($"DELETE FROM `sqlite_sequence` WHERE `name` IN ({str});");
    }

    /// <inheritdoc />
    public string FixConnectionString(string connectionString, string? rootPath)
    {
        var arr = connectionString.Split(';')           // получили массив параметров строки подключения
            .Select(s =>
            {
                const string dataSourceKey = "data source=";
                var value = s.GetEnd(dataSourceKey);            // вторая часть фразы параметра
                if (value is null)
                    // выходим, если параметр не "data source="
                    return s;
                    
                // Вторая часть фразы параметра - имя файла БД,
                // добавляем к нему путь к директории БД
                return dataSourceKey + PathEx.Combine(rootPath ?? string.Empty, value);
            }).ToArray();

        return string.Join(";", arr);          // снова получили строку
    }

    /// <inheritdoc />
    public string GetConnectionString(ConfigurationManager configuration)
    {
        return configuration.GetConnectionString("SqliteConnection")?.Trim() ?? string.Empty;
    }
    
    /// <inheritdoc />
    public void UseProvider<TDbContext>(
        DbContextOptionsBuilder optionsBuilder, string connectionString) where TDbContext : DbContext
    {
        if (MigrationsAssemblyName.IsNullOrEmpty())
            optionsBuilder.UseSqlite(connectionString);
        else
            optionsBuilder.UseSqlite(connectionString, 
                b => b.MigrationsAssembly(MigrationsAssemblyName));
    }

    /// <inheritdoc />
    public void ModelBuilderInit(ModelBuilder modelBuilder)
    {
        // Ничего не делаем
    }

}