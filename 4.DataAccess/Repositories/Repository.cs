using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AppDomain.UseCases._Contracts;
using Common.BaseComponents.Components;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions;
using Common.Phrases;
using DataAccess.DbContexts._Contracts;
using DataAccess.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;
using ProblemDomain.Entities._Contracts;

namespace DataAccess.Repositories
{
    /// <summary>
    /// Репозиторий (класс, для непосредственной работы с БД).
    /// </summary>
    public partial class Repository<TDbContext> : IRepository, IDisposable where TDbContext : AbstractDbContext
    {
        /// <summary>
        /// Контекст БД.
        /// </summary>
        protected readonly TDbContext DbContext;
        
        /// <inheritdoc />
        public ExceptionList<BaseException> ExceptionsList { get; }

        /// <summary>
        /// Конструктор, запрещающий создание экземпляра без параметров.
        /// </summary>
        private Repository()
        {
            DbContext = null!;
            DbPhrases.Culture = CultureInfo.CurrentUICulture;       // устанавливаем языковой стандарт для фраз
            ExceptionsList = new ExceptionList<BaseException>();
        }
        
        /// <summary>
        /// Конструктор.
        /// </summary>
        public Repository(TDbContext dbContext) : this()
        {
            DbContext = dbContext;
            
            if (dbContext.IsNullOrEmptyConnectionString || !dbContext.IsPossibleConnect)
            {
                var ex = DbException.CreateException(DbPhrases.DbСonnectiontError);
                ExceptionsList.Add(ex);
            }
        }

        /// <summary>
        /// Добавить связанные сущности в запрос.
        /// </summary>
        /// <param name="queryable">IQueryable-запрос.</param>
        /// <param name="navigationProperties">Добавляемые сущности.</param>
        /// <typeparam name="TEntity">Тип сущности.</typeparam>
        /// <returns>IQueryable-запрос.</returns>
        protected IQueryable<TEntity> AddNavigationProperties<TEntity>(
            IQueryable<TEntity> queryable, IEnumerable<string> navigationProperties)
            where TEntity : class
        {
            // Перебираем все названия навигационных свойств
            foreach (var propName in navigationProperties)
            {
                if (!propName.IsNullOrEmpty())
                    queryable = queryable.Include(propName);    // включаем связанные сущности в результат запроса к БД
            }

            return queryable;
        }

        /// <inheritdoc />
        public async Task<Result<int>> AddAsync<TEntity>(TEntity entity)
            where TEntity : class, IAbstractEntity
        {
            try
            {
                await DbContext.Set<TEntity>().AddAsync(entity);
                return Result<int>.Done(await DbContext.SaveChangesAsync());
            }
            catch (Exception ex)
            {
                return Result<int>.Fail(ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<int>> AddRangeAsync<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class, IAbstractEntity
        {
            try
            {
                await DbContext.Set<TEntity>().AddRangeAsync(entities);
                return Result<int>.Done(await DbContext.SaveChangesAsync());
            }
            catch (Exception ex)
            {
                return Result<int>.Fail(ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<int> CountAsync<TEntity>()
            where TEntity : class, IAbstractEntity
        {
            return await DbContext.Set<TEntity>().CountAsync();
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
        public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(params string[] navigationProperties)
            where TEntity : class, IAbstractEntity
        {
            /*
            // Здесь мы можем указывать различные настройки контекста,
            // например выводить в отладчик сгенерированный SQL-код (последнее не работает в Net Core)
            _dbContext.Database.Log = 
                (s => System.Diagnostics.Debug.WriteLine(s));
            */

            // Используем универсальный метод Set
            IQueryable<TEntity> queryable = 
                DbContext.Set<TEntity>();  // Т.к. запрос может быть сложным, .AsNoTracking() использовать не получается

            var entities =
                AddNavigationProperties(queryable, navigationProperties).OrderBy(e => e.Id).ToListAsync();

            return await entities;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(IPersonalityEntity personality, params string[] navigationProperties)
            where TEntity : class, IPersonalityEntity
        {
            // Используем универсальный метод Set
            IQueryable<TEntity> queryable =
                DbContext.Set<TEntity>(); // Т.к. запрос может быть сложным, .AsNoTracking() использовать не получается

            var entity = await AddNavigationProperties(queryable, navigationProperties)
                .OrderBy(e => e.Name)
                .Where(e =>
                    e.FirstName == personality.FirstName
                    && e.LastName == personality.LastName
                    && e.Patronymic == personality.Patronymic)
                .ToListAsync();

            return entity;
        }

        
        /// <inheritdoc />
        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
        public async Task<IEnumerable<TEntity>> GetAllFromNameAsync<TEntity>(string? name, bool isUseLike = false,
            params string[] navigationProperties) 
            where TEntity : class, IEntityWithName
        {
            // Используем универсальный метод Set
            IQueryable<TEntity> queryable = 
                DbContext.Set<TEntity>();  // Т.к. запрос может быть сложным, .AsNoTracking() использовать не получается

            var entities = isUseLike
                ? await AddNavigationProperties(queryable, navigationProperties)
                    .OrderBy(e => e.Name)
                    .Where(e
                        // Ищем, используя оператор LIKE
                        => EF.Functions.Like(e.Name!, name))
                    .ToListAsync()
                : await AddNavigationProperties(queryable, navigationProperties)
                    .OrderBy(e => e.Name)
                    .Where(e
                        // Ищем точные совпадения
                        => e.Name == name)
                    .ToListAsync();
            
            return entities;
        }
        
        /// <inheritdoc />
        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
        public async Task<IEnumerable<TEntity>> GetAllFromNumberAsync<TEntity>(int? number,
            params string[] navigationProperties) 
            where TEntity : class, IEntityWithNumber
        {
            // Используем универсальный метод Set
            IQueryable<TEntity> queryable = 
                DbContext.Set<TEntity>();  // Т.к. запрос может быть сложным, .AsNoTracking() использовать не получается

            var entities = await AddNavigationProperties(queryable, navigationProperties)
                .OrderBy(e => e.Number)
                .Where(e => e.Number == number)
                .ToListAsync();
            
            return entities;
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
        public async Task<TEntity?> GetFirstAsync<TEntity>(params string[] navigationProperties) 
            where TEntity : class, IAbstractEntity
        {
            // Используем универсальный метод Set
            IQueryable<TEntity> queryable = 
                DbContext.Set<TEntity>();  // Т.к. запрос может быть сложным, .AsNoTracking() использовать не получается

            var entity = await AddNavigationProperties(queryable, navigationProperties)
                .OrderBy(e => e.Id).FirstOrDefaultAsync();

            return entity;
        }
        
        /// <inheritdoc />
        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
        public async Task<TEntity?> GetFromIdAsync<TEntity>(int id, params string[] navigationProperties) 
            where TEntity : class, IAbstractEntity 
        {
            // Используем универсальный метод Set
            IQueryable<TEntity> queryable = 
                DbContext.Set<TEntity>();  // Т.к. запрос может быть сложным, .AsNoTracking() использовать не получается
            
            var entity = await AddNavigationProperties(queryable, navigationProperties)
                .OrderBy(e => e.Id).FirstOrDefaultAsync(e => e.Id == id);
            
            return entity;
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
        public async Task<TEntity?> GetLastAsync<TEntity>(params string[] navigationProperties) 
            where TEntity : class, IAbstractEntity
        {
            // Используем универсальный метод Set
            IQueryable<TEntity> queryable = 
                DbContext.Set<TEntity>();  // Т.к. запрос может быть сложным, .AsNoTracking() использовать не получается

            var entity = await AddNavigationProperties(queryable, navigationProperties)
                .OrderBy(e => e.Id).LastOrDefaultAsync();

            return entity;
        }

        /// <inheritdoc />
        public async Task<bool> IsExistingAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IAbstractEntity
        {
            return await DbContext.Set<TEntity>().AsNoTracking()
                .ContainsAsync(entity, cancellationToken); // Стандартный comparer сравнивает только по id!
        }
        
        /// <inheritdoc />
        public async Task<bool> IsExistingAsync<TEntity>(IEnumerable<TEntity> entities,  CancellationToken cancellationToken = default)
            where TEntity : class, IAbstractEntity
        {
            var result = true;
            foreach (var item in entities)
            {
                result &= await IsExistingAsync(item, cancellationToken);
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<Result<int>> RemoveAsync<TEntity>(TEntity entity)
            where TEntity : class, IAbstractEntity
        {
            try
            {
                DbContext.Set<TEntity>().Remove(entity);
                return Result<int>.Done(await DbContext.SaveChangesAsync());
            }
            catch (Exception ex)
            {
                return Result<int>.Fail(ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<int>> RemoveRangeAsync<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class, IAbstractEntity
        {
            try
            {
                DbContext.Set<TEntity>().RemoveRange(entities);
                return Result<int>.Done(await DbContext.SaveChangesAsync());
            }
            catch (Exception ex)
            {
                return Result<int>.Fail(ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<int>> UpdateAsync<TEntity>(TEntity entity)
            where TEntity : class, IAbstractEntity
        {
            try
            {
                DbContext.Set<TEntity>().Update(entity);
                return Result<int>.Done(await DbContext.SaveChangesAsync());
            }
            catch (Exception ex)
            {
                return Result<int>.Fail(ex);
            }
        }

        /// <inheritdoc />
        public async Task<Result<int>> UpdateRangeAsync<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class, IAbstractEntity
        {
            try
            {
                DbContext.Set<TEntity>().UpdateRange(entities);
                return Result<int>.Done(await DbContext.SaveChangesAsync());
            }
            catch (Exception ex)
            {
                return Result<int>.Fail(ex);
            }
        }
        
        /// <inheritdoc />
        public void Dispose() => DbContext.Dispose();
    }
}