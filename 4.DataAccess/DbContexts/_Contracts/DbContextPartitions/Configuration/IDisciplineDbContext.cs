using Microsoft.EntityFrameworkCore;
using ProblemDomain.Entities.LibraryEntities;

namespace DataAccess.DbContexts._Contracts.DbContextPartitions.Configuration;

/// <summary>
/// Интерфейс контекста БД для настроек.
/// </summary>
public interface IDisciplineDbContext
{
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
}