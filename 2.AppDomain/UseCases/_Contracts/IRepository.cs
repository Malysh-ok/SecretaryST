using System.Diagnostics.CodeAnalysis;
using Common.BaseComponents.Components;
using Common.BaseComponents.Components.Exceptions;
using ProblemDomain.Entities._Contracts;
// ReSharper disable InvalidXmlDocComment

namespace AppDomain.UseCases._Contracts;

/// <summary>
/// Интерфейс репозитория.
/// </summary>
public interface IRepository
{
    /// <summary>
    /// Список ошибок, возникших при работе с репозиторием.
    /// </summary>
    public ExceptionList<BaseException> ExceptionsList { get; }

    /// <summary>
    /// Добавить сущность типа <typeparamref name="TEntity"/> в репозиторий.
    /// </summary>
    /// <param name="entity">Добавляемая сущность.</param>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <returns>Количество добавленных сущностей.</returns>
    public Task<Result<int>> AddAsync<TEntity>(TEntity entity) 
        where TEntity : class, IAbstractEntity;

    /// <summary>
    /// Добавить коллекцию сущностей типа <typeparamref name="TEntity"/> в репозиторий.
    /// </summary>
    /// <param name="entities">Добавляемая коллекция сущностей.</param>
    /// <typeparam name="TEntity">Тип сущностей.</typeparam>
    /// <returns>Количество добавленных сущностей.</returns>
    public Task<Result<int>> AddRangeAsync<TEntity>(IEnumerable<TEntity> entities) 
        where TEntity : class, IAbstractEntity;
    
    /// <summary>
    /// Добавить или обновить сущность типа <typeparamref name="TEntity"/> в репозитории.
    /// </summary>
    /// <param name="entity">Добавляемая сущность.</param>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <returns>Количество добавленных сущностей.</returns>
    public Task<Result<int>> AddOrUpdateAsync<TEntity>(TEntity entity)
        where TEntity : class, IAbstractEntity;

    /// <summary>
    /// Добавить или обновить коллекцию сущностей типа <typeparamref name="TEntity"/> в репозитории.
    /// </summary>
    /// <param name="entities">Добавляемая коллекция сущностей.</param>
    /// <typeparam name="TEntity">Тип сущностей.</typeparam>
    /// <returns>Количество добавленных сущностей.</returns>
    public Task<Result<int>> AddOrUpdateRangeAsync<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class, IAbstractEntity;

    /// <summary>
    /// Заменить сущности с номером в репозитории.
    /// </summary>
    /// <remarks>
    /// Поиск по номеру и замена сущностей в репозитории.
    /// Если сущности не найдены, то они добавляются. 
    /// </remarks>
    /// <param name="replaceableEntities">Заменяемые сущности.</param>
    /// <typeparam name="TEntity">Тип сущностей.</typeparam>
    /// <returns>Заменённые сущности.</returns>
    public Task<Result<List<TEntity>>> ReplaceNumberedRangeAsync<TEntity>(IList<TEntity> replaceableEntities)
        where TEntity : AbstractEntity, INumberedEntity, ICopyEntity;

    /// <summary>
    /// Заменить сущности с именем в репозитории.
    /// </summary>
    /// <remarks>
    /// Поиск по имени и замена сущностей в репозитории.
    /// Если сущности не найдены, то они добавляются. 
    /// </remarks>
    /// <param name="replaceableEntities">Заменяемые сущности.</param>
    /// <typeparam name="TEntity">Тип сущностей.</typeparam>
    /// <returns>Заменённые сущности.</returns>
    public Task<Result<List<TEntity>>> ReplaceNamedRangeAsync<TEntity>(IEnumerable<TEntity> replaceableEntities)
        where TEntity : AbstractEntity, INamedEntity, ICopyEntity;
    
    /// <summary>
    /// Получить количество сущностей в репозитории.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    public Task<int> CountAsync<TEntity>()
        where TEntity : class, IAbstractEntity;

    /// <summary>
    /// Получить из репозитория сущность типа <typeparamref name="TEntity"/> по Id.
    /// </summary>
    /// <param name="id">Id получаемой сущности.</param>
    public Task<TEntity?> FindAsync<TEntity>(object id) 
        where TEntity : class, IAbstractEntity;
    
    /// <summary>
    /// Получить из репозитория все сущности типа <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="navigationProperties">Массив имен навигационных свойств сущности,
    ///     данные которых включаются в результат (т.е. мы получаем связанные сущности);
    ///     имя может быть составным (т.е. вторая часть есть навигационное свойство связанной сущности),
    ///     в этом случае части имени разделяются символом '.'.</param>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <returns>Коллекция получаемых сущностей.</returns>
    public Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(params string[] navigationProperties)
        where TEntity : AbstractEntity;

    /// <param name="personality">Наименование получаемых сущностей.</param>
    /// <inheritdoc cref="GetAllAsync{TEntity}(string[])"/>
    public Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(IPersonalityEntity personality, params string[] navigationProperties)
        where TEntity : AbstractEntity, IPersonalityEntity;
        
    /// <summary>
    /// Получить из репозитория все сущности типа <typeparamref name="TEntity"/> с наименованием <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Наименование получаемых сущностей.</param>
    /// <param name="isUseLike">Использовать оператор 'like' при поиске сущностей по имени.</param>
    /// <inheritdoc cref="GetAllAsync{TEntity}(string[])"/>
    public Task<IEnumerable<TEntity>> GetAllFromNameAsync<TEntity>(string? name, bool isUseLike = false,
        params string[] navigationProperties)
        where TEntity : AbstractEntity, INamedEntity;

    /// <summary>
    /// Получить из репозитория все нумерованные сущности типа <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="navigationProperties">Массив имен навигационных свойств сущности,
    ///     данные которых включаются в результат (т.е. мы получаем связанные сущности);
    ///     имя может быть составным (т.е. вторая часть есть навигационное свойство связанной сущности),
    ///     в этом случае части имени разделяются символом '.'.</param>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <returns>Коллекция получаемых сущностей.</returns>
    public Task<IEnumerable<TEntity>> GetNumberedAllAsync<TEntity>(params string[] navigationProperties)
        where TEntity : AbstractEntity, INumberedEntity;
    
    /// <summary>
    /// Получить из репозитория все сущности типа <typeparamref name="TEntity"/> с номером <paramref name="number"/>.
    /// </summary>
    /// <param name="number">Номер получаемых сущностей.</param>
    /// <inheritdoc cref="GetAllAsync{TEntity}(string[])"/>
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    public Task<IEnumerable<TEntity>> GetAllFromNumberAsync<TEntity>(int? number, params string[] navigationProperties)
        where TEntity : AbstractEntity, INumberedEntity;

    /// <summary>
    /// Получить из репозитория первую сущность типа <typeparamref name="TEntity"/>.
    /// </summary>
    /// <returns>Получаемая сущность.</returns>
    /// <inheritdoc cref="GetAllAsync{TEntity}(string[])"/>
    public Task<TEntity?> GetFirstAsync<TEntity>(params string[] navigationProperties)
        where TEntity : AbstractEntity;

    /// <summary>
    /// Получить из репозитория сущность типа <typeparamref name="TEntity"/> по Id.
    /// </summary>
    /// <remarks>
    /// Отличается от <see cref="FindAsync{TEntity}"/> возможностью задать навигационные свойства,
    /// но работает медленее.
    /// </remarks>
    /// <param name="id">Id получаемой сущности.</param>
    /// <returns>Получаемая сущность.</returns>
    /// <inheritdoc cref="GetAllAsync{TEntity}(string[])"/>
    public Task<TEntity?> GetFromIdAsync<TEntity>(int id, params string[] navigationProperties)
        where TEntity : AbstractEntity;

    /// <summary>
    /// Получить из репозитория последнюю сущность типа <typeparamref name="TEntity"/>.
    /// </summary>
    /// <returns>Получаемая сущность.</returns>
    /// <inheritdoc cref="GetAllAsync{TEntity}(string[])"/>
    public Task<TEntity?> GetLastAsync<TEntity>(params string[] navigationProperties)
        where TEntity : AbstractEntity;

    /// <summary>
    /// Обновить сущность из репозитория.
    /// </summary>
    /// <param name="entity">Обновляемая сущность.</param>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    public Task ReloadAsync<TEntity>(TEntity? entity)
        where TEntity : AbstractEntity;

    /// <summary>
    /// Обновить коллекцию сущностей из репозитория.
    /// </summary>
    /// <param name="entities">Обновляемая коллекция сущностей.</param>
    /// <typeparam name="TEntity">Тип сущностей.</typeparam>
    /// <returns>True, если данные коллекции изменены, иначе - false.</returns>
    public Task<IEnumerable<TEntity>> ReloadRangeAsync<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : AbstractEntity;
    
    /// <summary>
    /// Признак существования сущности.
    /// </summary>
    /// <param name="entity">Проверяемая сущность.</param>
    /// <param name="cancellationToken">Объект для отмены операции.</param>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    public Task<bool> IsExistingAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) 
        where TEntity : AbstractEntity;

    /// <summary>
    /// Признак существования коллекции сущностей.
    /// </summary>
    /// <param name="entities">Проверяемая коллекции сущностей.</param>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    public Task<bool> IsExistingAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        where TEntity : AbstractEntity;

    /// <summary>
    /// Удалить сущность типа <typeparamref name="TEntity"/> из репозитория.
    /// </summary>
    /// <param name="entity">Удаляемая сущность.</param>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <returns>Количество удаленных сущностей.</returns>
    public Task<Result<int>> RemoveAsync<TEntity>(TEntity entity)
        where TEntity : AbstractEntity;

    /// <summary>
    /// Удалить коллекцию сущностей типа <typeparamref name="TEntity"/> из репозитория.
    /// </summary>
    /// <param name="entities">Удаляемая коллекция сущностей.</param>
    /// <typeparam name="TEntity">Тип сущностей.</typeparam>
    /// <returns>Количество удаленных сущностей.</returns>
    public Task<Result<int>> RemoveRangeAsync<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : AbstractEntity;

    /// <summary>
    /// Удалить все сущности типа <typeparamref name="TEntity"/> из репозитория.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущностей.</typeparam>
    /// <returns>Количество удаленных сущностей.</returns>
    public Task<Result<int>> RemoveAllAsync<TEntity>(bool isSaveChanges = false)
        where TEntity : AbstractEntity;
}