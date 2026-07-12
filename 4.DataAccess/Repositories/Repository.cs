using System.Linq.Expressions;
using Common.BaseComponents.Components;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions;
using DataAccess.DataAccessAssets.Services;
using DataAccess.DataAccessExceptions;
using DataAccess.DbContexts._Contracts;
using Microsoft.EntityFrameworkCore;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.UseCases._Contracts;

namespace DataAccess.Repositories;

/// <inheritdoc />
/// <summary>
/// Репозиторий (класс, для непосредственной работы с БД).
/// </summary>
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class Repository<TDbContext> : IRepository 
    where TDbContext : AbstractDbContext
{
    /// <summary>
    /// Признак того, что ресурсы освобождены.
    /// </summary>
    // ReSharper disable once RedundantDefaultMemberInitializer
    private bool _disposed = false;
    
    /// <summary>
    /// Автоматическое обнаружение изменений в уже отслеживаемых сущностях.
    /// </summary>
    // ReSharper disable once RedundantDefaultMemberInitializer
    private readonly bool _autoDetectChangesState = false;

    /// <summary>
    /// Провайдер сообщений об ошибках.
    /// </summary>
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly DataAccessErrorMsgProvider _dataAccessErrorMsgProvider;

    /// <summary>
    /// Контекст БД.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    protected TDbContext DbContext;

    /// <summary>
    /// Фабрика создания контекста (по факту - не используется)
    /// </summary>
    protected IDbContextFactory<TDbContext> ContextFactory;
    
    /// <inheritdoc />
    // TODO: Удалить ExceptionsList?
    public ExceptionList<BaseException> ExceptionsList { get; }

    #region [---------- Скрытые методы ----------]
    
    /// <summary>
    /// Добавить связанные сущности в запрос.
    /// </summary>
    /// <param name="queryable">IQueryable-запрос.</param>
    /// <param name="navigationProperties">Добавляемые сущности.</param>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <returns>IQueryable-запрос.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    protected IQueryable<TEntity> AddNavigationProperties<TEntity>(
        IQueryable<TEntity> queryable, IEnumerable<string> navigationProperties)
        where TEntity : class
    {
        // Перебираем все названия навигационных свойств
        foreach (var propName in navigationProperties)
        {
            if (! propName.IsNullOrEmpty())
                queryable = queryable.Include(propName);    // включаем связанные сущности в результат запроса к БД
        }

        return queryable;
    }
    
    #endregion

    /// <summary>
    /// Конструктор, запрещающий создание экземпляра без параметров.
    /// </summary>
    private Repository()
    {
        DbContext = null!;
        ContextFactory = null!;
        _dataAccessErrorMsgProvider = null!;
        ExceptionsList = new ExceptionList<BaseException>();
    }
        
    /// <summary>
    /// Конструктор.
    /// </summary>
    public Repository(TDbContext dbContext, DataAccessErrorMsgProvider dataAccessErrorMsgProvider) : this()
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dataAccessErrorMsgProvider = dataAccessErrorMsgProvider;

        // Отключаем AutoDetectChanges при создании для производительности
        _autoDetectChangesState = DbContext.ChangeTracker.AutoDetectChangesEnabled;
        DbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            
        if (dbContext.IsNullOrEmptyConnectionString || !dbContext.IsPossibleConnect)
        {
            var ex = _dataAccessErrorMsgProvider.CreateException(DataAccessErrorCodes.DbConnectionError);
            ExceptionsList.Add(ex);
        }
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    internal Repository(IDbContextFactory<TDbContext> contextFactory) : this()
    {
        ContextFactory = contextFactory;
    }
    
    #region [---------- Получение ----------]

    /// <inheritdoc />
    public async Task<Result<int>> CountAsync<TEntity>()
        where TEntity : class, IAbstractEntity
    {
        try
        {
            return Result<int>.Done(await DbContext.Set<TEntity>().CountAsync());
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }

    /// <inheritdoc />
    public async Task<Result<TEntity?>> FindAsync<TEntity>(object id) 
        where TEntity : class, IAbstractEntity 
    {
        try
        {
            return Result<TEntity?>.Done(await DbContext.Set<TEntity>().FindAsync(id));
        }
        catch (Exception ex)
        {
            return Result<TEntity?>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<TEntity?>> GetByIdAsync<TEntity>(int id, params string[] navigationProperties)
        where TEntity : class, IAbstractEntity
    {
        try
        {
            var queryable = DbContext.Set<TEntity>().AsTracking(); // Т.к. запрос может быть сложным,
                                                                   // .AsNoTracking() использовать не получается

            return Result<TEntity?>.Done(await AddNavigationProperties(queryable, navigationProperties)
                .FirstOrDefaultAsync(e => e.Id == id));
        }
        catch (Exception ex)
        {
            return Result<TEntity?>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }

    /// <inheritdoc />
    public async Task<Result<TEntity?>> GetFirstAsync<TEntity>(params string[] navigationProperties) 
        where TEntity : class, IAbstractEntity
    {
        try
        {
            var queryable = DbContext.Set<TEntity>().AsTracking();  // Т.к. запрос может быть сложным,
                                                                    // .AsNoTracking() использовать не получается

            return Result<TEntity?>.Done(await AddNavigationProperties(queryable, navigationProperties)
                                               .OrderBy(e => e.Id).FirstOrDefaultAsync());
        }
        catch (Exception ex)
        {
            return Result<TEntity?>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }

    /// <inheritdoc />
    public async Task<Result<TEntity?>> GetLastAsync<TEntity>(params string[] navigationProperties) 
        where TEntity : class, IAbstractEntity
    {
        try
        {
            var queryable = DbContext.Set<TEntity>().AsTracking();  // Т.к. запрос может быть сложным,
                                                                    // .AsNoTracking() использовать не получается

            return Result<TEntity?>.Done(await AddNavigationProperties(queryable, navigationProperties)
                .OrderBy(e => e.Id).LastOrDefaultAsync());
        }
        catch (Exception ex)
        {
            return Result<TEntity?>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<TEntity?>> GetByConditionAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,  
            params string[] navigationProperties)
        where TEntity : class
    {
        try
        {
            var queryable = DbContext.Set<TEntity>().AsTracking();  // Т.к. запрос может быть сложным,
                                                                    // .AsNoTracking() использовать не получается
            
            return Result<TEntity?>.Done(await AddNavigationProperties(queryable, navigationProperties)
                .FirstOrDefaultAsync(predicate));
        }
        catch (Exception ex)
        {
            return Result<TEntity?>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }

    /// <inheritdoc />
    public async Task<Result<IList<TEntity>>> GetAllAsync<TEntity>(
            Expression<Func<TEntity, bool>>? filter = null,
            params string[] navigationProperties)
        where TEntity : class
    {
        try
        {
            var queryable = DbContext.Set<TEntity>().AsTracking();  // Т.к. запрос может быть сложным,
                                                                    // .AsNoTracking() использовать не получается
            if (filter is not null)
                queryable = queryable.Where(filter);
                                                                    
            return Result<IList<TEntity>>.Done(await AddNavigationProperties(queryable, navigationProperties)
                .ToListAsync());
        }
        catch (Exception ex)
        {
            return Result<IList<TEntity>>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<IList<TEntity>>> GetNumberedAllAsync<TEntity>(bool ascending = true,
            Expression<Func<TEntity, bool>>? filter = null,
            params string[] navigationProperties
        )
        where TEntity : class, INumberedEntity
    {
        try
        {
            var queryable = DbContext.Set<TEntity>().AsTracking();  // Т.к. запрос может быть сложным,
                                                                    // .AsNoTracking() использовать не получается
            if (filter is not null)
                queryable = queryable.Where(filter);
            
            queryable = ascending 
                ? queryable.OrderBy(e => e.Number) 
                : queryable.OrderByDescending(e => e.Number);
        
            return Result<IList<TEntity>>.Done(await AddNavigationProperties(queryable, navigationProperties)
                .ToListAsync());
        }
        catch (Exception ex)
        {
            return Result<IList<TEntity>>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<IList<TEntity>>> GetAllByNumberAsync<TEntity>(int? number,
            Expression<Func<TEntity, bool>>? filter = null,
            params string[] navigationProperties
        )
        where TEntity : class, INumberedEntity
    {
        try
        {
            var queryable = DbContext.Set<TEntity>().AsTracking();  // Т.к. запрос может быть сложным,
                                                                    // .AsNoTracking() использовать не получается
            if (filter is not null)
                queryable = queryable.Where(filter);
                                                                    
            return Result<IList<TEntity>>.Done(await AddNavigationProperties(queryable, navigationProperties)
                                               .Where(e => e.Number == number)
                                               .ToListAsync());
        }
        catch (Exception ex)
        {
            return Result<IList<TEntity>>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }

    #endregion

    #region [---------- Добавление/обновление ----------]

    /// <inheritdoc />
    public Result<int> Add<TEntity>(TEntity? entity) where TEntity : class, IAbstractEntity
    {
        try
        {
            if (entity == null)
                return Result<int>.Done(0);
            
            var entry = DbContext.Entry(entity);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (entry.State)
            {
                case EntityState.Detached:
                    DbContext.Set<TEntity>().Add(entity);
                    break;

                case EntityState.Unchanged:
                case EntityState.Modified:
                    // Уже отслеживается как существующий - проверяем ID
                    if (entity.Id == 0)
                    {
                        entry.State = EntityState.Added;
                    }
                    else
                    {
                        // Ошибка
                        // return Result<int>.Fail(DbException.Create(
                        //     $"It is not possible to add an entity of type {typeof(TEntity).Name} with Id = {entity.Id} " +
                        //     "An entity with this Id already exists.",
                        //     null,
                        //     "ru",
                        //     $"Невозможно добавить сущность типа {typeof(TEntity).Name} с Id = {entity.Id}. " +
                        //     "Сущность с таким Id уже существует.")
                        // );
                        return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                            DataAccessErrorCodes.EntityAlreadyExistsError, null, 
                            $"{typeof(TEntity).Name}", $"{entity.Id}"));
                    }

                    break;

                case EntityState.Added:
                    // Уже добавлен, ничего не делаем
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Added;
                    break;
            }

            return Result<int>.Done(1);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// Работает медленно. По возможности не использовать.
    /// </remarks>
    public Result<int> AddRange<TEntity>(IList<TEntity>? entities) where TEntity : class, IAbstractEntity
    {
        try
        {
            if (entities == null || entities.Count == 0)
                return Result<int>.Done(0);

            foreach (var entity in entities)
            {
                Add(entity); // Используем Add с проверками
            }
            
            return Result<int>.Done(entities.Count);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }

    /// <inheritdoc />
    public Result<int> AddRangeQuickly<TEntity>(IList<TEntity>? entities) where TEntity : class
    {
        try
        {
            if (entities == null || entities.Count == 0)
                return Result<int>.Done(0);

            DbContext.Set<TEntity>().AddRange(entities);
            
            return Result<int>.Done(entities.Count);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }

    /// <inheritdoc />
    public Result<int> Update<TEntity>(TEntity? entity) where TEntity : class, IAbstractEntity
    {
        try
        {
            if (entity == null)
                return Result<int>.Done(0);

            var entry = DbContext.Entry(entity);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (entry.State)
            {
                case EntityState.Detached:
                    if (entity.Id == 0)
                    {
                        DbContext.Set<TEntity>().Add(entity);
                    }
                    else
                    {
                        DbContext.Set<TEntity>().Attach(entity);
                        entry.State = EntityState.Modified;
                    }
                    break;

                case EntityState.Unchanged:
                    entry.State = EntityState.Modified;
                    break;

                case EntityState.Modified:
                case EntityState.Added:
                    break;

                case EntityState.Deleted:
                    // Ошибка
                    // return Result<int>.Fail(DbException.Create(
                    //     $"It is not possible to update a deleted entity of type {typeof(TEntity).Name}.",
                    //     null,
                    //     "ru",
                    //     $"Невозможно обновить удалённую сущность типа {typeof(TEntity).Name}.")
                    // );
                    return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                        DataAccessErrorCodes.UpdateDeletedEntityError, null,
                        $"{typeof(TEntity).Name}"));
            }
            
            return Result<int>.Done(1);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// Работает медленно. По возможности не использовать.
    /// </remarks>
    public Result<int> UpdateRange<TEntity>(IList<TEntity>? entities) where TEntity : class, IAbstractEntity
    {
        try
        {
            if (entities == null || entities.Count == 0)
                return Result<int>.Done(0);

            foreach (var entity in entities)
            {
                Update(entity); // Используем Update с проверками
            }
            
            return Result<int>.Done(entities.Count);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }

    /// <inheritdoc />
    public Result<int> UpdateRangeQuickly<TEntity>(IList<TEntity>? entities) where TEntity : class
    {
        try
        {
            if (entities == null || entities.Count == 0)
                return Result<int>.Done(0);

            DbContext.Set<TEntity>().UpdateRange(entities);
        
            return Result<int>.Done(entities.Count);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }

    #endregion

    #region [---------- Удаление ----------]

    /// <inheritdoc />
    public Result<int> Remove<TEntity>(TEntity? entity) where TEntity : class, IAbstractEntity
    {
        try
        {
            if (entity == null)
                return Result<int>.Done(0);
        
            var entry = DbContext.Entry(entity);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (entry.State)
            {
                case EntityState.Detached:
                    if (entity.Id == 0)
                    {
                        // Новая сущность, не сохранённая в БД - просто игнорируем
                        return Result<int>.Done(1);
                    }
                    DbContext.Set<TEntity>().Attach(entity);
                    entry.State = EntityState.Deleted;
                    break;

                case EntityState.Added:
                    // Новая сущность - просто отключаем
                    entry.State = EntityState.Detached;
                    break;

                case EntityState.Unchanged:
                case EntityState.Modified:
                    entry.State = EntityState.Deleted;
                    break;

                case EntityState.Deleted:
                    // Уже удалён
                    break;
            }
        
            return Result<int>.Done(1);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// Работает медленно. По возможности не использовать.
    /// </remarks>
    public Result<int> RemoveRange<TEntity>(IList<TEntity>? entities) where TEntity : class, IAbstractEntity
    {
        try
        {
            if (entities == null || entities.Count == 0)
                return Result<int>.Done(0);

            foreach (var entity in entities)
            {
                Remove(entity); // Используем Remove с проверками
            }
            
            return Result<int>.Done(entities.Count);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }
    
    /// <inheritdoc />
    public Result<int> RemoveRangeQuickly<TEntity>(IList<TEntity>? entities) where TEntity : class
    {
        try
        {
            if (entities == null || entities.Count == 0)
                return Result<int>.Done(0);

            DbContext.Set<TEntity>().RemoveRange(entities);
        
            return Result<int>.Done(entities.Count);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }

    public Result<int> RemoveAllQuickly<TEntity>() where TEntity : class
    {
        try
        {
            var allEntities =  DbContext.Set<TEntity>();
            DbContext.Set<TEntity>().RemoveRange(allEntities);
            
            return Result<int>.Done(allEntities.Count());
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }
    
    #endregion

    #region [---------- Присоединение/отсоединение ----------]

    /// <inheritdoc />
    public Result<int> Attach<TEntity>(TEntity? entity) where TEntity : class
    {
        try
        {
            if (entity == null)
                return Result<int>.Done(0);

            var entry = DbContext.Entry(entity);
            
            if (entry.State == EntityState.Detached)
            {
                DbContext.Set<TEntity>().Attach(entity);
            }
            
            return Result<int>.Done(1);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }

    /// <inheritdoc />
    public Result<int> AttachRange<TEntity>(IList<TEntity>? entities) where TEntity : class
    {
        try
        {
            if (entities == null || entities.Count == 0)
                return Result<int>.Done(0);

            foreach (var entity in entities)
            {
                Attach(entity); // Используем Attach с проверками
            }
            
            return Result<int>.Done(entities.Count);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }
    
    /// <inheritdoc />
    public Result<int> Detach<TEntity>(TEntity? entity) where TEntity : class
    {
        try
        {
            if (entity == null)
                return Result<int>.Done(0);

            var entry = DbContext.Entry(entity);
            entry.State = EntityState.Detached;
        
            return Result<int>.Done(1);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }
    
    /// <inheritdoc />
    public Result<int> DetachAll<TEntity>() where TEntity : class
    {
        try
        {
            var entries = DbContext.ChangeTracker.Entries<TEntity>().ToList();

            // Удаляем из отслеживания все сущности заданного типа
            foreach (var entry in entries)
            {
                entry.State = EntityState.Detached;
            }
        
            return Result<int>.Done(entries.Count);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }
    
    #endregion
    
    #region [---------- Состояние и сохранение ----------]

    /// <inheritdoc />
    public Result<EntityState> GetEntityState<TEntity>(TEntity entity) where TEntity : class
    {
        try
        {
            return Result<EntityState>.Done(DbContext.Entry(entity).State);
        }
        catch (Exception ex)
        {
            return Result<EntityState>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }

    /// <inheritdoc />
    public async Task<Result<int>> SaveChangesAsync()
    {
        try
        {
            // Включаем AutoDetectChanges ТОЛЬКО на время сохранения
            DbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            var result = await DbContext.SaveChangesAsync();

            return Result<int>.Done(result);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Если конфликт - перезагружаем из БД
            foreach (var entry in ex.Entries)
            {
                await entry.ReloadAsync();
            }

            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
        finally
        {
            DbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        }
    }

    /// <inheritdoc />
    public async Task<Result<int>> DiscardChangesAsync()
    {
        try
        {
            var entries = DbContext.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        await entry.ReloadAsync();
                        break;
                }
            }
            
            return Result<int>.Done(entries.Count);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }

    #endregion
    
    #region [---------- Перезагрузки ----------]
    
    /// <inheritdoc />
    public async Task<Result<int>> ReloadAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        try
        {
            var entry = DbContext.Entry(entity);
            if (entry.State == EntityState.Detached) 
                return Result<int>.Done(0);
            
            await entry.ReloadAsync();

            return Result<int>.Done(1);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Работает медленно. По возможности не использовать.
    /// </remarks>
    public async Task<Result<int>> ReloadRangeAsync<TEntity>(IList<TEntity> entities)
        where TEntity : class
    {
        try
        {
            foreach (var entity in entities)
            {
                await ReloadAsync(entity);
            }
            
            return Result<int>.Done(entities.Count);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// Работает медленно. По возможности не использовать.
    /// </remarks>
    public async Task<Result<int>> ReloadAllAsync<TEntity>()
        where TEntity : class
    {
        try
        {
            var entities = await DbContext.Set<TEntity>().ToListAsync();
            foreach (var entity in entities)
            {
                await ReloadAsync(entity);
            }
            
            return Result<int>.Done(entities.Count);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(_dataAccessErrorMsgProvider.CreateException(
                DataAccessErrorCodes.UnknownError, ex));
        }
    }
    
    #endregion

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Восстанавливаем исходное состояние перед Dispose
            DbContext.ChangeTracker.AutoDetectChangesEnabled = _autoDetectChangesState;
            
            DbContext.Dispose();
        }
        _disposed = true;
    }
}