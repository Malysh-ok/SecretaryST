using System.Globalization;
using AppDomain.UseCases._Contracts;
using Common.BaseComponents.Components;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions;
using Common.BaseExtensions.Collections;
using Common.Phrases;
using DataAccess.DbContexts._Contracts;
using DataAccess.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ProblemDomain.Entities._Contracts;

namespace DataAccess.Repositories;

/// <summary>
/// Репозиторий (класс, для непосредственной работы с БД).
/// </summary>
public partial class Repository<TDbContext> : IRepository, IDisposable where TDbContext : AbstractDbContext
{
    /// <summary>
    /// Признак использования пакетного режима.
    /// </summary>
    private bool _isUseBatch;
    
    /// <summary>
    /// Контекст БД.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    protected TDbContext DbContext;

    /// <summary>
    /// Фабрика создания контекста (по факту - не используется)
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    protected readonly IDbContextFactory<TDbContext> ContextFactory;
        
    /// <inheritdoc />
    public ExceptionList<BaseException> ExceptionsList { get; }

    /// <summary>
    /// Конструктор, запрещающий создание экземпляра без параметров.
    /// </summary>
    private Repository()
    {
        _isUseBatch = false;
        DbContext = null!;
        ContextFactory = null!;
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
    /// Конструктор.
    /// </summary>
    internal Repository(IDbContextFactory<TDbContext> contextFactory) : this()
    {
        ContextFactory = contextFactory;
    }


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
        where TEntity : AbstractEntity
    {
        // Перебираем все названия навигационных свойств
        foreach (var propName in navigationProperties)
        {
            if (!propName.IsNullOrEmpty())
                queryable = queryable.Include(propName);    // включаем связанные сущности в результат запроса к БД
        }

        return queryable;
    }

    /// <summary>
    /// Отмена изменений в сущностях.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущностей.</typeparam>
    // ReSharper disable once MemberCanBePrivate.Global
    protected void CancelingEntitiesChanges<TEntity>()
        where TEntity : class
    {
        var entries = DbContext.ChangeTracker.Entries<TEntity>();
        entries.ForEach(e => e.CurrentValues.SetValues(e.OriginalValues));
    }
    
    /// <summary>
    /// Использование пакетного режима.
    /// </summary>
    /// <param name="isUseBatch">Состояние режима.</param>
    public async Task<int> UseBatch(bool isUseBatch = false)
    {
        _isUseBatch = isUseBatch;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_isUseBatch == false && DbContext is not null)
        {
            // TODO: переделать на Result<int> ???
            await DbContext.SaveChangesAsync();
        }

        return 0;
    }

    /// <summary>
    /// Действия, выполняемые перед основным методом.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    protected async Task BeforeAsync()
    {
        /*
        DbContext ??= await ContextFactory.CreateDbContextAsync();
        */
        
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Действия, выполняемые после основного метода.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    protected async Task AfterAsync()
    {
        /*
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_isUseBatch == false && DbContext is not null)
        {
            await DbContext.DisposeAsync();
            DbContext = null!;
        }
        */
        
        await Task.CompletedTask;
    }


    #region [---------- Добавление/обновление ----------]
    
    /// <inheritdoc />
    public async Task<Result<int>> AddAsync<TEntity>(TEntity entity)
        where TEntity : class, IAbstractEntity
    {
        try
        {
            await BeforeAsync();
            
            await DbContext.Set<TEntity>().AddAsync(entity);
            return Result<int>.Done(await DbContext.SaveChangesAsync());
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(ex);
        }
        finally
        {
            await AfterAsync();
        }
    }
        
    /// <inheritdoc />
    public async Task<Result<int>> AddRangeAsync<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class, IAbstractEntity
    {
        try
        {
            await BeforeAsync();

            await DbContext.Set<TEntity>().AddRangeAsync(entities);
            return Result<int>.Done(await DbContext.SaveChangesAsync());
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(ex);
        }
        finally
        {
            await AfterAsync();
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<int>> AddOrUpdateAsync<TEntity>(TEntity entity)
        where TEntity : class, IAbstractEntity
    {
        try
        {
            await BeforeAsync();
            
            if (entity.Id == 0)
                await DbContext.Set<TEntity>().AddAsync(entity);
            
            return Result<int>.Done(await DbContext.SaveChangesAsync());
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(ex);
        }
        finally
        {
            await AfterAsync();
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<int>> AddOrUpdateRangeAsync<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class, IAbstractEntity
    {
        try
        {
            await BeforeAsync();

            var dbSet =
                DbContext.Set<TEntity>();
            await dbSet.AddRangeAsync(entities.Where(e => e.Id == 0));
            
            return Result<int>.Done(await DbContext.SaveChangesAsync());
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(ex);
        }
        finally
        {
            await AfterAsync();
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<List<TEntity>>> ReplaceNumberedRangeAsync<TEntity>(IList<TEntity> replaceableEntities)
        where TEntity : AbstractEntity, INumberedEntity, ICopyEntity
    {
        // Замененные сущности
        List<TEntity> replacedEntities = [];

        // Получаем словарь всех сущностей, где в качестве ключа - номер
        var repositoryEntityDic = 
            (await GetNumberedAllAsync<TEntity>()).ToDictionary(d => d.Number);
        
        // Перебираем заменяемые сущности
        foreach (var item in replaceableEntities)
        {
            // Если в словаре есть заменяемая сущность - копируем ее данные в сущность словаря,
            // иначе - добавляем ее в словарь
            repositoryEntityDic.TryGetValue(item.Number, out var repositoryEntity);
            if (repositoryEntity is not null)
            {
                item.Copy(repositoryEntity);
                replacedEntities.Add(repositoryEntity);
            }
            else
            {
                repositoryEntityDic[item.Number] = item;
                replacedEntities.Add(item);
            }
        }
        
        var addOrUpdateResult = await AddOrUpdateRangeAsync(repositoryEntityDic.Values);

        return addOrUpdateResult.HasValue
            ? Result<List<TEntity>>.Done(replacedEntities)
            : Result<List<TEntity>>.Fail(addOrUpdateResult.Excptn!);
    }
    
    /// <inheritdoc />
    public async Task<Result<List<TEntity>>> ReplaceNamedRangeAsync<TEntity>(IEnumerable<TEntity> replaceableEntities)
        where TEntity : AbstractEntity, INamedEntity, ICopyEntity
    {
        // Замененные сущности
        List<TEntity> replacedEntities = [];
        
        // Получаем словарь всех сущностей, где в качестве ключа - наименование
        var repositoryEntityDic = 
            (await GetAllAsync<TEntity>()).ToDictionary(d => d.Name);
        
        // Перебираем заменяемые сущности
        foreach (var item in replaceableEntities)
        {
            // Если в словаре есть заменяемая сущность - копируем ее данные в сущность словаря,
            // иначе - добавляем ее в словарь
            repositoryEntityDic.TryGetValue(item.Name, out var repositoryEntity);
            if (repositoryEntity is not null)
            {
                item.Copy(repositoryEntity);
                replacedEntities.Add(repositoryEntity);
            }
            else
            {
                repositoryEntityDic[item.Name] = item;
                replacedEntities.Add(item);
            }
        }
        
        var addOrUpdateResult = await AddOrUpdateRangeAsync(repositoryEntityDic.Values);

        return addOrUpdateResult.HasValue
            ? Result<List<TEntity>>.Done(replacedEntities)
            : Result<List<TEntity>>.Fail(addOrUpdateResult.Excptn!);
    }
    
    #endregion

    
    #region [---------- Получение ----------]
    
    /// <inheritdoc />
    public async Task<int> CountAsync<TEntity>()
        where TEntity : class, IAbstractEntity
    {
        try
        {
            await BeforeAsync();

            return await DbContext.Set<TEntity>().CountAsync();
        }
        finally
        {
            await AfterAsync();
        }
    }

    /// <inheritdoc />
    public async Task<TEntity?> FindAsync<TEntity>(object id) 
        where TEntity : class, IAbstractEntity 
    {
        try
        {
            await BeforeAsync();

            var entity = await DbContext.Set<TEntity>().FindAsync(id);

            return entity;
        }
        finally
        {
            await AfterAsync();
        }
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(params string[] navigationProperties)
        where TEntity : AbstractEntity
    {
        try
        {
            await BeforeAsync();

            // Отмена изменений в сущностях
            CancelingEntitiesChanges<TEntity>();

            // Используем универсальный метод Set
            IQueryable<TEntity> queryable =
                    DbContext.Set<TEntity>()
                /*.AsNoTracking()*/; // Т.к. запрос может быть сложным, .AsNoTracking() использовать не получается

            var entities =
                AddNavigationProperties(queryable, navigationProperties).OrderBy(e => e.Id).ToListAsync();

            return await entities;
        }
        finally
        {
            await AfterAsync();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(IPersonalityEntity personality, params string[] navigationProperties)
        where TEntity : AbstractEntity, IPersonalityEntity
    {
        try
        {
            await BeforeAsync();

            // Отмена изменений в сущностях
            CancelingEntitiesChanges<TEntity>();

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
        finally
        {
            await AfterAsync();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> GetNumberedAllAsync<TEntity>(params string[] navigationProperties)
        where TEntity : AbstractEntity, INumberedEntity
    {
        try
        {
            await BeforeAsync();

            // Отмена изменений в сущностях
            CancelingEntitiesChanges<TEntity>();

            IQueryable<TEntity> queryable =
                DbContext.Set<TEntity>();

            var entities =
                AddNavigationProperties(queryable, navigationProperties).OrderBy(e => e.Number).ToListAsync();

            return await entities;
        }
        finally
        {
            await AfterAsync();
        }
    }
        
    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> GetAllFromNameAsync<TEntity>(string? name, bool isUseLike = false,
        params string[] navigationProperties) 
        where TEntity : AbstractEntity, INamedEntity
    {
        try
        {
            await BeforeAsync();
            
            // Отмена изменений в сущностях
            CancelingEntitiesChanges<TEntity>();

            // Используем универсальный метод Set
            IQueryable<TEntity> queryable =
                DbContext.Set<TEntity>(); // Т.к. запрос может быть сложным, .AsNoTracking() использовать не получается

            var entities = isUseLike
                ? await AddNavigationProperties(queryable, navigationProperties)
                    .OrderBy(e => e.Name)
                    .Where(e
                        // Ищем, используя оператор LIKE
                        => EF.Functions.Like(e.Name, name))
                    .ToListAsync()
                : await AddNavigationProperties(queryable, navigationProperties)
                    .OrderBy(e => e.Name)
                    .Where(e
                        // Ищем точные совпадения
                        => e.Name == name)
                    .ToListAsync();

            return entities;
        }
        finally
        {
            await AfterAsync();
        }
    }
        
    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> GetAllFromNumberAsync<TEntity>(int? number,
        params string[] navigationProperties) 
        where TEntity : AbstractEntity, INumberedEntity
    {
        try
        {
            await BeforeAsync();

            // Отмена изменений в сущностях
            CancelingEntitiesChanges<TEntity>();

            // Используем универсальный метод Set
            IQueryable<TEntity> queryable =
                DbContext.Set<TEntity>(); // Т.к. запрос может быть сложным, .AsNoTracking() использовать не получается

            var entities = await AddNavigationProperties(queryable, navigationProperties)
                .OrderBy(e => e.Number)
                .Where(e => e.Number == number)
                .ToListAsync();

            return entities;
        }
        finally
        {
            await AfterAsync();
        }
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetFirstAsync<TEntity>(params string[] navigationProperties) 
        where TEntity : AbstractEntity
    {
        try
        {
            await BeforeAsync();

            // Отмена изменений в сущностях
            CancelingEntitiesChanges<TEntity>();

            // Используем универсальный метод Set
            IQueryable<TEntity> queryable =
                    DbContext.Set<TEntity>()
                /*.AsNoTracking()*/; // Т.к. запрос может быть сложным, .AsNoTracking() использовать не получается

            var entity = await AddNavigationProperties(queryable, navigationProperties)
                .OrderBy(e => e.Id).FirstOrDefaultAsync();

            return entity;
        }
        finally
        {
            await AfterAsync();
        }
    }
        
    /// <inheritdoc />
    public async Task<TEntity?> GetFromIdAsync<TEntity>(int id, params string[] navigationProperties) 
        where TEntity : AbstractEntity 
    {
        try
        {
            await BeforeAsync();
            
            // Отмена изменений в сущностях
            CancelingEntitiesChanges<TEntity>();

            // Используем универсальный метод Set
            IQueryable<TEntity> queryable =
                DbContext.Set<TEntity>(); // Т.к. запрос может быть сложным, .AsNoTracking() использовать не получается

            var entity = await AddNavigationProperties(queryable, navigationProperties)
                .OrderBy(e => e.Id).FirstOrDefaultAsync(e => e.Id == id);

            return entity;
        }
        finally
        {
            await AfterAsync();
        }
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetLastAsync<TEntity>(params string[] navigationProperties) 
        where TEntity : AbstractEntity
    {
        try
        {
            await BeforeAsync();

            // Отмена изменений в сущностях
            CancelingEntitiesChanges<TEntity>();

            // Используем универсальный метод Set
            IQueryable<TEntity> queryable =
                DbContext.Set<TEntity>(); // Т.к. запрос может быть сложным, .AsNoTracking() использовать не получается

            var entity = await AddNavigationProperties(queryable, navigationProperties)
                .OrderBy(e => e.Id).LastOrDefaultAsync();

            return entity;
        }
        finally
        {
            await AfterAsync();
        }
    }
    
    /// <inheritdoc />
    public async Task ReloadAsync<TEntity>(TEntity? entity)
        where TEntity : AbstractEntity
    {
        try
        {
            await BeforeAsync();

            if (entity != null)
                await DbContext.Entry(entity).ReloadAsync();
        }
        finally
        {
            await AfterAsync();
        }
    }

    /// <inheritdoc />
    // TODO: Возможно нужно удалить.
    public async Task<IEnumerable<TEntity>> ReloadRangeAsync<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : AbstractEntity
    {
        List<EntityEntry<TEntity>> entries = [];
        entries.AddRange(entities.Select(entity =>
        {
            var entry = DbContext.Entry(entity);
            entry.CurrentValues.SetValues(entry.OriginalValues);
            return entry;
        }));

        return entries.Select(ee => ee.Entity);
    }
    
    #endregion

    
    #region [---------- Проверки ----------]
    
    // TODO: Возможно нужно удалить.
    
    /// <inheritdoc />
    public async Task<bool> IsExistingAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : AbstractEntity
    {
        return await DbContext.Set<TEntity>().AsNoTracking()
            .ContainsAsync(entity, cancellationToken); // Стандартный comparer сравнивает только по id!
    }
        
    /// <inheritdoc />
    public async Task<bool> IsExistingAsync<TEntity>(IEnumerable<TEntity> entities,  CancellationToken cancellationToken = default)
        where TEntity : AbstractEntity
    {
        try
        {
            await BeforeAsync();

            var result = true;
            foreach (var item in entities)
            {
                result &= await IsExistingAsync(item, cancellationToken);
            }

            return result;
        }
        finally
        {
            await AfterAsync();
        }
    }

    #endregion

    #region [---------- Удаление ----------]
    
    /// <inheritdoc />
    public async Task<Result<int>> RemoveAsync<TEntity>(TEntity entity)
        where TEntity : AbstractEntity
    {
        try
        {
            await BeforeAsync();

            DbContext.Set<TEntity>().Remove(entity);
            // TODO: переделать удаление
            return Result<int>.Done(0);
            return Result<int>.Done(await DbContext.SaveChangesAsync());
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(ex);
        }
        finally
        {
            await AfterAsync();
        }
    }
        
    /// <inheritdoc />
    public async Task<Result<int>> RemoveRangeAsync<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : AbstractEntity
    {
        try
        {
            await BeforeAsync();

            DbContext.Set<TEntity>().RemoveRange(entities);
            // TODO: переделать удаление
            return Result<int>.Done(0);
            return Result<int>.Done(await DbContext.SaveChangesAsync());
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(ex);
        }
        finally
        {
            await AfterAsync();
        }
    }

    /// <inheritdoc />
    public async Task<Result<int>> RemoveAllAsync<TEntity>(bool isSaveChanges = false)
        where TEntity : AbstractEntity
    {
        try
        {
            await BeforeAsync();

            DbContext.Set<TEntity>().RemoveRange(DbContext.Set<TEntity>());
            return isSaveChanges
                ? Result<int>.Done(await DbContext.SaveChangesAsync())
                : Result<int>.Done(0);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(ex);
        }
        finally
        {
            await AfterAsync();
        }
    }
    
    #endregion
        
        
    /// <inheritdoc />
    public void Dispose() => DbContext.Dispose();
}