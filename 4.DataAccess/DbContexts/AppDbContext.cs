using Common.BaseExtensions.Collections;
using DataAccess.DbContexts._Contracts;
using DataAccess.DbContexts.DbConfigure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProblemDomain.Entities;

namespace DataAccess.DbContexts;

/// <summary>
/// Основной контекст приложения.
/// </summary>
public sealed partial class AppDbContext : AbstractDbContext
{
    /// <summary>
    /// Конфигуратор БД.
    /// </summary>
    private readonly DbConfigurator _dbConfigurator;

#if DEBUG
    /// <summary>
    /// Свойство, позволяющее задействовать логирование от Microsoft.
    /// </summary>
    private static ILoggerFactory ConsoleLoggerFactory =>
        // new LoggerFactory(new[]
        // {
        //     new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider()
        // });
        LoggerFactory.Create(builder => { builder.AddConsole(); });
#endif

    /// <summary>
    /// Конструктор.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options, DbConfigurator dbConfigurator)
        : base(options)
    {
        // ConnectionString = Database.GetConnectionString() ?? string.Empty;
        _dbConfigurator = dbConfigurator;
            
        ConnectionString = Database.GetConnectionString();
    }

    /// <summary>
    /// Конфигурация контекста БД.
    /// </summary>
    /// <remarks>
    /// Этот метод вызывается для каждого созданного экземпляра контекста.
    /// </remarks>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if DEBUG
        // TODO: Использование логирования в AppDbContext
        // optionsBuilder.UseLoggerFactory(ConsoleLoggerFactory);
#endif

        if (optionsBuilder.IsConfigured) return;

        _dbConfigurator.UseProvider<AppDbContext>(optionsBuilder);
        base.OnConfiguring(optionsBuilder);
    }
        
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _dbConfigurator.ModelBuilderInit(modelBuilder);

        #region [---------- КОНФИГУРАЦИЯ ----------]
        
        // Создание статусов соревнований. 
        CreateModel_CompetitionsStatuses(modelBuilder);
            
        // Создание статусов и наименований спортивных соревнований.
        CreateModel_DetailedCompetitionStatuses(modelBuilder);
                    
        // Создание групп дисциплин.
        CreateModel_DisciplineGroups(modelBuilder);

        // Создание подгрупп дисциплин.
        CreateModel_DisciplineSubGroups(modelBuilder);

        // Создание дисциплин.
        CreateModel_Disciplines(modelBuilder);

        // Создание судейских категорий
        CreateModel_RefereeLevels(modelBuilder);
                
        // Создание судейских должностей. 
        CreateModel_RefereeingPositions(modelBuilder);
                
        // Создание вариантов пола.
        CreateModel_Sexes(modelBuilder);
                
        // Создание типов спортивных юнитов.
        CreateModel_SportUnitTypes(modelBuilder);
        
        #endregion

        #region [---------- ОБЩИЕ ДАННЫЕ ----------]
        // Создание спортсменов.
        CreateModel_Athletes(modelBuilder);
                
        // Создание делегаций.
        CreateModel_Delegations(modelBuilder);

        // Создание судей.
        CreateModel_Referees(modelBuilder);
                
        // Создание представителей.
        CreateModel_Representatives(modelBuilder);
                    
        // Создание данных о соревновании.
        CreateModel_CompetitionData(modelBuilder);

        #endregion

        
        #region [---------- ДИСТАНЦИИ ----------]
        
        // Создание спортивных дисциплин.
        CreateModel_SportEvents(modelBuilder);

        // Создание спортивных юнитов.
        CreateModel_SportUnits(modelBuilder);
                
        #endregion
    }

    /// <inheritdoc />
    public override void ClearAutoincrementSequence(string autoincrementColumnName, params Type[] entityTypes)
    {
        // Получаем последовательность названий таблиц БД, соответствующих типам сущностей аргумента функции
        var tableNames = Model.GetEntityTypes()
            .Where(et => entityTypes.Contains(et.ClrType))
            .Select(et => et.GetTableName()).RemoveEmptyStr().ToArray();

        // Сбрасываем начальные значения
        _dbConfigurator.ProviderOptions.ClearAutoincrementSequence(this,
            autoincrementColumnName, tableNames);
    }
}