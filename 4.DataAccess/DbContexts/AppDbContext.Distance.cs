using Common.BaseExtensions.ValueTypes;
using DataAccess.DbContexts._Contracts.DbContextPartitions;
using Microsoft.EntityFrameworkCore;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities;

namespace DataAccess.DbContexts;

// Дистанции.
public sealed partial class AppDbContext : IDistanceDbContext
{
    /// <summary>
    /// Наименование схемы.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    private const string DISTANCE_SCHEMA_NAME = "Distance";

    /// <summary>
    /// Префикс наименования таблиц данной схемы.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    private const string DISTANCE_TABLE_PRE = "Distance_";

    /// <summary>
    /// Виды программ.
    /// </summary>
    public DbSet<SportEvent> SportEvents { get; set; }

    /// <summary>
    /// Спортивные юниты.
    /// </summary>
    public DbSet<SportUnit> SportUnits { get; set; }
        
    /// <summary>
    /// Создание Видов программ.
    /// </summary>
    private void CreateModel_SportEvents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SportEvent>(entity =>
        {
            entity.ToTable($"{DISTANCE_TABLE_PRE}SportEvents", DISTANCE_SCHEMA_NAME,
                t => t.HasComment("Виды программ"));

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.Property(e => e.Description).HasMaxLength(300);

            entity.Property(e => e.IsShort).IsRequired(false);
                
            entity.Property(e => e.Difficulty)
                .IsRequired()
                .HasConversion(
                    enm => enm.ToInt(),
                    i => i.ToEnumWithException<Difficulty.IdEnm>()
                );

            entity.HasKey(e => e.Id)
                .HasName("PK_SportEvents");

            // Вторичный ключ - Дисциплина
            entity.HasOne(se => se.Discipline)
                .WithMany(d => d.SportEvents)
                .HasForeignKey(se => se.DisciplineId)
                .HasConstraintName("FK_SportEvents_DisciplineId");
        });
    }
        
    /// <summary>
    /// Создание Спортивных юнитов.
    /// </summary>
    private void CreateModel_SportUnits(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SportUnit>(entity =>
        {
            entity.ToTable($"{DISTANCE_TABLE_PRE}SportUnits", DISTANCE_SCHEMA_NAME,
                t => t.HasComment("Спортивные юниты"));

            entity.Property(su => su.Id).ValueGeneratedOnAdd();

            entity.HasKey(su => su.Id)
                .HasName("PK_SportUnits");
                
            entity.Property(su => su.Name).IsRequired().HasMaxLength(100);

            entity.Property(su => su.Description).HasMaxLength(300);

            // Вторичный ключ - Тип спортивного юнита
            entity.HasOne(su => su.SportUnitType)
                .WithMany(sut => sut.SportUnits)
                .HasForeignKey(su => su.SportUnitTypeId)
                .HasConstraintName("FK_SportUnits_SportUnitTypeId");
                
            // Вторичный ключ - Вид программы
            entity.HasOne(su => su.SportEvent)
                .WithMany(se => se.SportUnits)
                .HasForeignKey(su => su.SportEventId)
                .HasConstraintName("FK_SportUnits_SportEventId");
                
            // Вторичный ключ - Вариант пола
            entity.HasOne(su => su.Sex)
                .WithMany(s => s.SportUnits)
                .HasForeignKey(su => su.SexId)
                .HasConstraintName("FK_SportUnits_SexId");
                
            // Вторичный ключ на самого себя
            entity.HasOne(su => su.ParentSportUnit)
                .WithMany(psu => psu.ChildSportUnits)
                .HasForeignKey(su => su.ParentSportUnitId)
                .HasConstraintName("FK_SportUnits_ParentSportUnitId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}