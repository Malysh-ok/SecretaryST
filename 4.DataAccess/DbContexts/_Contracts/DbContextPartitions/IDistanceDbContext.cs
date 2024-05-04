using Microsoft.EntityFrameworkCore;
using ProblemDomain.Entities.DistanceEntities;

namespace DataAccess.DbContexts._Contracts.DbContextPartitions;

/// <summary>
/// Интерфейс контекста БД для сущностей Дистанций.
/// </summary>
public interface IDistanceDbContext
{
    /// <summary>
    /// Виды программ.
    /// </summary>
    public DbSet<SportEvent> SportEvents { get; set; }

    /// <summary>
    /// Спортивные юниты.
    /// </summary>
    public DbSet<SportUnit> SportUnits { get; set; }}