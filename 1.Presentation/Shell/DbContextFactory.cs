using DataAccess.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Presentation.Shell.Common;
using Presentation.DesignTime.Services;

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
    {
        var errorMsgProvider = new DesignTimeErrorMsgProvider();
        var appDir = ServiceFactory.CreateAppDirService(errorMsgProvider);
        var dbConfigurator = ServiceFactory.CreateDbConfigurator(appDir);

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        dbConfigurator.UseProvider<AppDbContext>(optionsBuilder);
        return new AppDbContext(optionsBuilder.Options, dbConfigurator);
    }
}