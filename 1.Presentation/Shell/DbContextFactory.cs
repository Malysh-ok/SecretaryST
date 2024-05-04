using DataAccess.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Presentation.Shell;

/// <summary>
/// Фабрика создания контекста БД (используется EF при создании миграций).
/// </summary>
/// <remarks>
/// Помещается в проекте запуска приложения, чтобы получить правильный корневой
/// каталог приложения (с exe-файлом), от которого высчитываются производные каталоги. 
/// </remarks>
public class DbContextFactory() : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <inheritdoc />
    public AppDbContext CreateDbContext(string[] args)
        => new(
            new DbContextOptionsBuilder<AppDbContext>().Options,
            new StartupItemsFactory().CreateDbConfigurator()
        );
}