using Microsoft.EntityFrameworkCore;
using ProblemDomain.Entities.LibraryEntities;

namespace DataAccess.DbContexts._Contracts.DbContextPartitions;

/// <summary>
/// Интерфейс контекста БД для Библиотечных сущностей.
/// </summary>
public interface IConfigurationDbContext 
{
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
    public DbSet<SportUnitType>? SportUnitTypes { get; set; }}