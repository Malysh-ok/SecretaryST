using System.Diagnostics.CodeAnalysis;
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
    /// Соревнования.
    /// </summary>
    // TODO: Разобраться, зачем нам нужны DbSet<>.
    public DbSet<Competition> Competitions { get; set; }

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
    /// Создание Соревнования (главной сущности).
    /// </summary>
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    private void CreateModel_Competitions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Competition>(entity =>
        {
            entity.ToTable($"Competitions",
                t => t.HasComment("Соревнования"));

            entity.Property(с => с.Id).ValueGeneratedOnAdd();
                
            entity.Property(c => c.Name).IsRequired().HasMaxLength(500);
                
            entity.Property(c => c.ConductingOrganizations).IsRequired().HasMaxLength(500);
                
            entity.Property(c => c.Date)
                .IsRequired()
                .HasColumnType(_dbConfigurator.ProviderOptions.DateTimeColumnType);

            entity.Property(c => c.DayCount).IsRequired();

            entity.Property(c => c.Venue).IsRequired().HasMaxLength(300);
                
            entity.Property(c => c.Description).HasMaxLength(300);

            entity.HasKey(e => e.Id)
                .HasName("PK_Competitions");

            // Вторичный ключ - Статус соревнований
            entity.HasOne(c => c.CompetitionsStatus)
                .WithMany(cs => cs.Competitions)
                .HasForeignKey(c => c.CompetitionsStatusId)
                .HasConstraintName("FK_SportEvents_CompetitionsStatusId");
                
            // Вторичный ключ - Статус и наименования соревнования
            entity.HasOne(c => c.DetailedCompetitionStatus)
                .WithMany(dcs => dcs.Competitions)
                .HasForeignKey(c => c.DetailedCompetitionStatusId)
                .HasConstraintName("FK_SportEvents_DetailedCompetitionStatusId");
                
            // Вторичный ключ (многие-ко-многим) - связь с судьями
            entity.HasMany(c => c.Secretaries)
                .WithMany(r => r.Competitions)
                .UsingEntity(etb => etb.ToTable("_CompetitionsReferees"));
                
            // Вторичный ключ (один-к-одному) - Главный судья
            entity.HasOne(c => c.ChiefReferee)
                // .WithOne(r => r.CompetitionChiefReferee)
                .WithOne()
                .HasForeignKey<Competition>(с => с.ChiefRefereeId)
                .HasConstraintName("FK_Competitions_ChiefRefereeId")
                .OnDelete(DeleteBehavior.Restrict);
                
            // Вторичный ключ (один-к-одному) - Главный секретарь
            entity.HasOne(c => c.ChiefSecretary)
                // .WithOne(r => r.CompetitionChiefSecretary)
                .WithOne()
                .HasForeignKey<Competition>(c => c.ChiefSecretaryId)
                .HasConstraintName("FK_Competitions_ChiefSecretaryId")
                .OnDelete(DeleteBehavior.Restrict);

            // Вторичный ключ (один-к-одному) - Председатель комиссии по допуску
            entity.HasOne(c => c.MandateChairman)
                // .WithOne(r => r.CompetitionMandateChairman)
                .WithOne()
                .HasForeignKey<Competition>(c => c.MandateChairmanId)
                .HasConstraintName("FK_Competitions_MandateChairmanId")
                .OnDelete(DeleteBehavior.Restrict);
        });
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
        #endregion

        #region [---------- ДИСТАНЦИИ ----------]
        // Создание спортивных дисциплин.
        CreateModel_SportEvents(modelBuilder);

        // Создание спортивных юнитов.
        CreateModel_SportUnits(modelBuilder);
                
        #endregion
            
        // Создание соревнований (главной сущности).
        CreateModel_Competitions(modelBuilder);
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