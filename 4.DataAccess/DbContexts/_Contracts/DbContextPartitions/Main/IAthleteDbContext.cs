using Microsoft.EntityFrameworkCore;
using ProblemDomain.Entities.CommonEntities;

namespace DataAccess.DbContexts._Contracts.DbContextPartitions.Main
{
    /// <summary>
    /// Интерфейс контекста БД для спортсменов.
    /// </summary>
    public interface IAthleteDbContext
    {
        /// <summary>
        /// Спортсмены.
        /// </summary>
        public DbSet<Athlete> Athletes { get; set; }
    }
}