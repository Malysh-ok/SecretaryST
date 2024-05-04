using Common.BaseExtensions.ValueTypes;
using DataAccess.DbContexts._Contracts.DbContextPartitions;
using Microsoft.EntityFrameworkCore;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;

namespace DataAccess.DbContexts;

// Библиотека.
public sealed partial class AppDbContext : IConfigurationDbContext
{
    /// <summary>
    /// Наименование схемы.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    private const string CONFIGURATION_SCHEMA_NAME = "Library";
        
    /// <summary>
    /// Префикс наименования таблиц данной схемы.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    private const string CONFIGURATION_TABLE_PRE = "Lib_";
        
    /// <summary>
    /// Статусы соревнований.
    /// </summary>
    public DbSet<CompetitionsStatus> CompetitionsStatuses { get; set; }
        
    /// <summary>
    /// Статусы и наименования спортивных соревнований.
    /// </summary>
    public DbSet<DetailedCompetitionStatus> DetailedCompetitionStatuses { get; set; }
        
    /// <summary>
    /// Группы дисциплин.
    /// </summary>
    public DbSet<DisciplineGroup>? DisciplineGroups { get; set; }
        
    /// <summary>
    /// Подгруппы дисциплин.
    /// </summary>
    public DbSet<DisciplineSubGroup>? DisciplineSubGroups { get; set; }
        
    /// <summary>
    /// Дисциплины.
    /// </summary>
    public DbSet<Discipline>? Disciplines { get; set; }
        
    /// <summary>
    /// Судейские категории.
    /// </summary>
    public DbSet<RefereeLevel>? RefereeLevels { get; set; }
        
    /// <summary>
    /// Судейские должности.
    /// </summary>
    public DbSet<RefereeJobTitle>? RefereeingPositions { get; set; }
        
    /// <summary>
    /// Варианты пола.
    /// </summary>
    public DbSet<Sex>? Sexes { get; set; }
        
    /// <summary>
    /// Типы спортивных юнитов.
    /// </summary>
    public DbSet<SportUnitType>? SportUnitTypes { get; set; }
        
    /// <summary>
    /// Создание статусов соревнований.
    /// </summary>
    private void CreateModel_CompetitionsStatuses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompetitionsStatus>(entity =>
        {
            entity.ToTable($"{CONFIGURATION_TABLE_PRE}CompetitionsStatuses", CONFIGURATION_SCHEMA_NAME, 
                t => t.HasComment("Статусы соревнований"));

            entity.Property(cs => cs.Id).ValueGeneratedNever()
                .HasConversion(
                    enm => enm.ToInt(),
                    i => i.ToEnumWithException<CompetitionsStatusEnm>()
                );
                
            entity.Property(cs => cs.Name).IsRequired().HasMaxLength(100);
                
            entity.Property(cs => cs.NamePlural).IsRequired().HasMaxLength(100);

            entity.Property(cs => cs.Description).HasMaxLength(300);

            entity.HasKey(cs => cs.Id)
                .HasName("PK_CompetitionsStatuses");
        });
    }
        
    /// <summary>
    /// Создание статусов и наименований спортивных соревнований.
    /// </summary>
    private void CreateModel_DetailedCompetitionStatuses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DetailedCompetitionStatus>(entity =>
        {
            entity.ToTable($"{CONFIGURATION_TABLE_PRE}DetailedCompetitionStatuses", CONFIGURATION_SCHEMA_NAME, 
                t => t.HasComment("Статусы и наименования соревнований"));

            entity.Property(dcs => dcs.Id).ValueGeneratedNever()
                .HasConversion(
                    enm => enm.ToInt(),
                    i => i.ToEnumWithException<DetailedCompetitionStatusEnm>()
                );
                
            entity.Property(dcs => dcs.Name).IsRequired().HasMaxLength(100);

            entity.Property(dcs => dcs.Description).HasMaxLength(300);

            entity.HasKey(dcs => dcs.Id)
                .HasName("PK_DetailedCompetitionStatuses");
        });
    }
        
    /// <summary>
    /// Создание групп дисциплин.
    /// </summary>
    private void CreateModel_DisciplineGroups(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DisciplineGroup>(entity =>
        {
            entity.ToTable($"{CONFIGURATION_TABLE_PRE}DisciplineGroups", CONFIGURATION_SCHEMA_NAME, 
                t => t.HasComment("Группы дисциплин"));

            entity.Property(dg => dg.Id).ValueGeneratedNever()
                .HasConversion(
                    enm => enm.ToInt(),
                    i => i.ToEnumWithException<DisciplineGroupEnm>()
                );
                
            entity.Property(dg => dg.Name).IsRequired().HasMaxLength(100);

            entity.Property(dg => dg.Description).HasMaxLength(300);

            entity.HasKey(dg => dg.Id)
                .HasName("PK_DisciplineGroups");
                
            /*
            // Пример заполнения данных с помощью EF
            entity.HasData(
                new DisciplineGroup { Id = 1, Name = "Маршрут" },
                new DisciplineGroup { Id = 2, Name = "Дистанция" },
                new DisciplineGroup { Id = 3, Name = "Северная ходьба" }
            );
            */
        });
    }

    /// <summary>
    /// Создание подгрупп дисциплин.
    /// </summary>
    private void CreateModel_DisciplineSubGroups(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DisciplineSubGroup>(entity =>
        {
            entity.ToTable($"{CONFIGURATION_TABLE_PRE}DisciplineSubGroups", CONFIGURATION_SCHEMA_NAME,
                t => t.HasComment("Подгруппы дисциплин"));

            entity.Property(dsg => dsg.Id).ValueGeneratedNever()
                .HasConversion(
                    enm => enm.ToInt(),
                    i => i.ToEnumWithException<DisciplineSubGroupEnm>()
                );
                
            entity.Property(dsg => dsg.Name).IsRequired().HasMaxLength(100);
                
            entity.Property(dsg => dsg.Description).HasMaxLength(300);

            entity.HasKey(dsg => dsg.Id)
                .HasName("PK_DisciplineSubGroups");

            // Вторичный ключ - Группа дисциплин
            entity.HasOne(dsg => dsg.DisciplineGroup)
                .WithMany(dg => dg.DisciplineSubGroups)
                .HasForeignKey(dsg => dsg.DisciplineGroupId)
                .HasConstraintName("FK_DisciplineSubGroups_DisciplineGroupId");
        });
    }
        
    /// <summary>
    /// Создание дисциплин.
    /// </summary>
    private void CreateModel_Disciplines(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Discipline>(entity =>
        {
            entity.ToTable($"{CONFIGURATION_TABLE_PRE}Disciplines", CONFIGURATION_SCHEMA_NAME,
                t => t.HasComment("Дисциплины"));

            entity.Property(d => d.Id).ValueGeneratedNever()
                .HasConversion(
                    enm => enm.ToInt(),
                    i => i.ToEnumWithException<DisciplineEnm>()
                );

            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
                
            entity.Property(d => d.Description).HasMaxLength(300);

            entity.HasKey(d => d.Id)
                .HasName("PK_Disciplines");

            // Вторичный ключ - Подгруппа дисциплин
            entity.HasOne(d => d.DisciplineSubGroup)
                .WithMany(dsg => dsg.Disciplines)
                .HasForeignKey(d => d.DisciplineSubGroupId)
                .HasConstraintName("FK_Disciplines_DisciplineSubGroupId");
                
            // Вторичный ключ - Группа дисциплин
            entity.HasOne(d => d.DisciplineGroup)
                .WithMany(dg => dg.Disciplines)
                .HasForeignKey(d => d.DisciplineGroupId)
                .HasConstraintName("FK_Disciplines_DisciplineGroupId");
        });
    }

    /// <summary>
    /// Создание судейских категорий.
    /// </summary>
    private void CreateModel_RefereeLevels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefereeLevel>(entity =>
        {
            entity.ToTable($"{CONFIGURATION_TABLE_PRE}RefereeLevels", CONFIGURATION_SCHEMA_NAME,
                t => t.HasComment("Судейские категории"));

            entity.Property(dg => dg.Id).ValueGeneratedNever()
                .HasConversion(
                    enm => enm.ToInt(),
                    i => i.ToEnumWithException<RefereeLevelEnm>()
                );

            entity.Property(dg => dg.Name).IsRequired().HasMaxLength(100);
                
            entity.Property(dg => dg.LongName).IsRequired().HasMaxLength(100);

            entity.Property(dg => dg.Description).HasMaxLength(300);

            entity.HasKey(dg => dg.Id)
                .HasName("PK_RefereeLevels");
        });
    }
        
    /// <summary>
    /// Создание судейских должностей.
    /// </summary>
    private void CreateModel_RefereeingPositions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefereeJobTitle>(entity =>
        {
            entity.ToTable($"{CONFIGURATION_TABLE_PRE}RefereeJobTitles", CONFIGURATION_SCHEMA_NAME,
                t => t.HasComment("Судейские должности"));

            entity.Property(jt => jt.Id).ValueGeneratedNever()
                .HasConversion(
                    enm => enm.ToInt(),
                    i => i.ToEnumWithException<RefereeJobTitleEnm>()
                );

            entity.Property(dg => dg.Name).IsRequired().HasMaxLength(100);

            entity.Property(dg => dg.Description).HasMaxLength(300);

            entity.HasKey(dg => dg.Id)
                .HasName("PK_RefereeJobTitles");
        });
    }

    /// <summary>
    /// Создание вариантов пола.
    /// </summary>
    private void CreateModel_Sexes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sex>(entity =>
        {
            entity.ToTable($"{CONFIGURATION_TABLE_PRE}Sexes", CONFIGURATION_SCHEMA_NAME, 
                t => t.HasComment("Варианты пола"));

            entity.Property(dg => dg.Id).ValueGeneratedNever()
                .HasConversion(
                    enm => enm.ToInt(),
                    i => i.ToEnumWithException<SexEnm>()
                );
                
            entity.Property(dg => dg.Name).IsRequired().HasMaxLength(100);
                
            entity.Property(dg => dg.PersonalityName).HasMaxLength(100);

            entity.Property(dg => dg.PersonalityNamePlural).HasMaxLength(100);

            entity.Property(dg => dg.TeamName).IsRequired().HasMaxLength(100);

            entity.Property(dg => dg.TeamNamePlural).IsRequired().HasMaxLength(100);

            entity.Property(dg => dg.Description).HasMaxLength(300);

            entity.HasKey(dg => dg.Id)
                .HasName("PK_Sexes");
        });
    }
        
    /// <summary>
    /// Создание типов спортивных юнитов.
    /// </summary>
    private void CreateModel_SportUnitTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SportUnitType>(entity =>
        {
            entity.ToTable($"{CONFIGURATION_TABLE_PRE}SportUnitTypes", CONFIGURATION_SCHEMA_NAME, 
                t => t.HasComment("Типы спортивных юнитов"));

            entity.Property(dg => dg.Id).ValueGeneratedNever()
                .HasConversion(
                    enm => enm.ToInt(),
                    i => i.ToEnumWithException<SportUnitTypeEnm>()
                );
                
            entity.Property(dg => dg.Name).IsRequired().HasMaxLength(100);
                
            entity.Property(dg => dg.AuxName).HasMaxLength(100);

            entity.Property(dg => dg.Description).HasMaxLength(300);

            entity.HasKey(dg => dg.Id)
                .HasName("PK_SportUnitTypes");
        });
    }
}