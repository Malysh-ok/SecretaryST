using System.Diagnostics;
using DataAccess.DbContexts;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Presentation.Shell;
using ProblemDomain.Entities.CommonEntities;

/*
 * Два теста. Один передает в конструктор Репозитория контекст БД, второй - фабрику по созданию контекста БД.
 * Вариант с передачей фабрики предпочтительнее, - при вызове метода Репозитория создается контекст БД для выполнения
 * одной единицы работы; кроме того, снимаются проблемы, присущие варианту с передачей контекста БД,
 * связанные с отслеживанием EF сущностей.
 *
 * ВЫВОД: Второй тест (с передачей фабрики контекста БД) работает почти в 5 раз медленнее,
 * что будем считать приемлемым.
 *
 * UPD: Вариант с передачей фабрики контекста БД так же имеет проблемы.
 * Вернулись к варианту с передачей контекста БД.
 */

namespace DataAccess.Tests;

public class RepositorySpeedTests
{
    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    public async Task UseDbContextTest()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var dbContext = new DbContextFactory().CreateDbContext([]);
        var repository = new Repository<AppDbContext>(dbContext);

        for (var i = 0; i < 10000; i++)
        {
            var referees = await repository.GetAllAsync<Referee>();
            // Console.WriteLine(referees.Count());
        }
        
        stopwatch.Stop();
        Console.WriteLine(@"	Тест выполнен за " + stopwatch.ElapsedMilliseconds + @"мс.");
        
        // Резюме: Тест при количестве равным 10000 выполняется за 1.390 сек.
    }


    private class AppDbContextFactory : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext()
        {
            return new DbContextFactory().CreateDbContext([]);
        }
    }
    
    [Test]
    public async Task UseDbContextFactoryTest()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var repository = new Repository<AppDbContext>(new AppDbContextFactory());

        for (var i = 0; i < 10000; i++)
        {
            var referees = await repository.GetAllAsync<Referee>();
            // Console.WriteLine(referees.Count());
        }
        
        stopwatch.Stop();
        Console.WriteLine(@"	Тест выполнен за " + stopwatch.ElapsedMilliseconds + @"мс.");
        
        // Резюме: Тест при количестве равным 10000 выполняется за 5.150 сек.
    }
}