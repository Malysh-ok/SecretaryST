using Microsoft.EntityFrameworkCore;
using ProblemDomain.Entities;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;

namespace DataAccess.DbContexts._Contracts.DbContextPartitions;

/// <summary>
/// Интерфейс контекста БД для Общих сущностей.
/// </summary>
public interface ICommonDbContext
{
    /// <summary>
    /// Спортсмены.
    /// </summary>
    public DbSet<Athlete> Athletes { get; set; }

    /// <summary>
    /// Делегации.
    /// </summary>
    public DbSet<Delegation> Delegations { get; set; }

    /// <summary>
    /// Судьи.
    /// </summary>
    public DbSet<Referee> Referees  { get; set; }
            
    /// <summary>
    /// Представители.
    /// </summary>
    public DbSet<Representative> Representatives { get; set; }

    /// <summary>
    /// Данные о соревновании.
    /// </summary>
    public DbSet<CompetitionData> CompetitionData { get; set; }    }