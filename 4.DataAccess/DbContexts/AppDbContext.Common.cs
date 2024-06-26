using Common.BaseExtensions.ValueTypes;
using DataAccess.DbContexts._Contracts.DbContextPartitions;
using Microsoft.EntityFrameworkCore;
using ProblemDomain.Entities;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;

namespace DataAccess.DbContexts;

// Общее.
public sealed partial class AppDbContext : ICommonDbContext
{
    /// <summary>
    /// Наименование схемы.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    private const string COMMON_SCHEMA_NAME = "Common";

    /// <summary>
    /// Префикс наименования таблиц данной схемы.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    private const string COMMON_TABLE_PRE = "Common_";

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
    public DbSet<CompetitionData> CompetitionData { get; set; }

    /// <summary>
    /// Создание спортсменов.
    /// </summary>
    private void CreateModel_Athletes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Athlete>(entity =>
        {
            entity.ToTable($"{COMMON_TABLE_PRE}Athletes", COMMON_SCHEMA_NAME,
                t => t.HasComment("Спортсмены"));

            entity.Property(a => a.Id).ValueGeneratedOnAdd();

            entity.Property(a => a.FirstName).IsRequired().HasMaxLength(100);

            entity.Property(a => a.LastName).IsRequired().HasMaxLength(100);

            entity.Property(a => a.Patronymic).HasMaxLength(100);

            // Вычисляемое поле Name
            entity.Property(a => a.Name)
                .HasComputedColumnSql($"{nameof(Athlete.LastName)} || ' ' || " +
                                      $"{nameof(Athlete.FirstName)} || " +
                                      $"IIF({nameof(Athlete.Patronymic)} IS NULL, '', ' ' || {nameof(Athlete.Patronymic)})"
                    , stored: true);

            entity.Property(a => a.BirthDate)
                .IsRequired()
                .HasColumnType(_dbConfigurator.ProviderOptions.DateTimeColumnType);
            // .HasConversion(
            //     dt => dt.FromNullable(null).ToUnixTimestamp(),
            //     dtUnix => dtUnix.FromUnixTimestamp()
            //     );

            entity.HasKey(a => a.Id)
                .HasName("PK_Athletes");

            // Вторичный ключ - Делегация
            entity.HasOne(a => a.Delegation)
                .WithMany(d => d.Athletes)
                .HasForeignKey(a => a.DelegationId)
                .HasConstraintName("FK_Athletes_DelegationId");
                
            // Вторичный ключ - Спортивный юнит
            entity.HasOne(a => a.SportUnit)
                .WithMany(su => su.Athletes)
                .HasForeignKey(a => a.SportUnitId)
                .HasConstraintName("FK_Athletes_SportUnitId");
                
            // Вторичный ключ - Вариант пола
            entity.HasOne(a => a.Sex)
                .WithMany(s => s.Athletes)
                .HasForeignKey(a => a.SexId)
                .HasConstraintName("FK_Athletes_SexId");
        });
    }

    /// <summary>
    /// Создание делегаций.
    /// </summary>
    private void CreateModel_Delegations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Delegation>(entity =>
        {
            entity.ToTable($"{COMMON_TABLE_PRE}Delegations", COMMON_SCHEMA_NAME,
                t => t.HasComment("Делегации"));

            entity.Property(d => d.Id).ValueGeneratedOnAdd();

            entity.Property(d => d.Number).IsRequired();

            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);

            entity.Property(d => d.Region).IsRequired().HasMaxLength(100);

            entity.Property(d => d.Description).HasMaxLength(300);

            entity.HasKey(d => d.Id)
                .HasName("PK_Delegations");

            // Вторичный ключ - Представители
            entity.HasOne(d => d.Representative)
                .WithMany(r => r.Delegations)
                .HasForeignKey(d => d.RepresentativeId)
                .HasConstraintName("FK_Delegations_RepresentativeId");
        });
    }

    /// <summary>
    /// Создание судей.
    /// </summary>
    private void CreateModel_Referees(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Referee>(entity =>
        {
            entity.ToTable($"{COMMON_TABLE_PRE}Referees", COMMON_SCHEMA_NAME,
                t => t.HasComment("Судьи"));

            entity.Property(r => r.Id).ValueGeneratedNever()
                .HasConversion(
                    enm => enm.ToInt(),
                    i => i.ToEnumWithException<RefereeJobTitleEnm>()
                );

            entity.Property(r => r.FirstName).IsRequired().HasMaxLength(100);

            entity.Property(r => r.LastName).IsRequired().HasMaxLength(100);

            entity.Property(r => r.Patronymic).HasMaxLength(100);
                
            // Вычисляемое поле Name
            entity.Property(a => a.Name)
                .HasComputedColumnSql($"{nameof(Athlete.LastName)} || ' ' || " +
                                      $"{nameof(Athlete.FirstName)} || " +
                                      $"IIF({nameof(Athlete.Patronymic)} IS NULL, '', ' ' || {nameof(Athlete.Patronymic)})"
                    , stored: true);
                
            entity.Property(r => r.Domicile).HasMaxLength(100);

            entity.HasKey(r => r.Id)
                .HasName("PK_Referees");
                
            // Вторичный ключ - Судейская категория
            entity.HasOne(r => r.RefereeLevel)
                .WithMany(rl => rl.Referees)
                .HasForeignKey(r => r.RefereeLevelId)
                .HasConstraintName("FK_Referees_RefereeLevelId");
                
            // Вторичный ключ (один-к-одному) - Судейская должность
            entity.HasOne(r => r.RefereeJobTitle)
                // .WithOne(r => r.CompetitionChiefReferee)
                .WithOne()
                .HasForeignKey<Referee>(с => с.RefereeRefereeJobTitleId)
                .HasConstraintName("FK_Referees_RefereeingPositionId")
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
        
    /// <summary>
    /// Создание представителей.
    /// </summary>
    private void CreateModel_Representatives(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Representative>(entity =>
        {
            entity.ToTable($"{COMMON_TABLE_PRE}Representatives", COMMON_SCHEMA_NAME,
                t => t.HasComment("Представители"));

            entity.Property(r => r.Id).ValueGeneratedOnAdd();

            entity.Property(r => r.FirstName).IsRequired().HasMaxLength(100);

            entity.Property(r => r.LastName).IsRequired().HasMaxLength(100);

            entity.Property(r => r.Patronymic).HasMaxLength(100);
                
            // Вычисляемое поле Name
            entity.Property(a => a.Name)
                .HasComputedColumnSql($"{nameof(Athlete.LastName)} || ' ' || " +
                                      $"{nameof(Athlete.FirstName)} || " +
                                      $"IIF({nameof(Athlete.Patronymic)} IS NULL, '', ' ' || {nameof(Athlete.Patronymic)})"
                    , stored: true);

            entity.Property(r => r.PhoneNumber).HasMaxLength(20);

            entity.Property(r => r.Email).HasMaxLength(100);
                
            entity.HasKey(r => r.Id)
                .HasName("PK_Representatives");
                
            // Вторичный ключ - Варианты пола
            entity.HasOne(r => r.Sex)
                .WithMany(s => s.Representatives)
                .HasForeignKey(r => r.SexId)
                .HasConstraintName("FK_Representatives_SexId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Создание Соревнования (главной сущности).
    /// </summary>
    private void CreateModel_CompetitionData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompetitionData>(entity =>
        {
            entity.ToTable($"{COMMON_TABLE_PRE}CompetitionData", COMMON_SCHEMA_NAME,
                t => t.HasComment("Данные о соревновании"));

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
        });
    }    

}