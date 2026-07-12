using System.Linq.Expressions;
using Common.BaseComponents.Components;
using Common.BaseComponents.Components.Exceptions;
using Microsoft.EntityFrameworkCore;
using ProblemDomain.Entities._Contracts;

namespace ProblemDomain.UseCases._Contracts;

/// <summary>
/// Интерфейс репозитория (класса, для непосредственной работы с БД).
/// </summary>
/// <remarks>
/// <b>Когда использовать стандартные методы (<see cref="AddRangeQuickly{TEntity}"/>,
/// <see cref="RemoveRangeQuickly{TEntity}"/>, <see cref="UpdateRangeQuickly{TEntity}"/>):</b>
/// <list type="bullet">
///     <item>Сущность только что загружена из БД через этот же контекст</item>
///     <item>Сущность создана вручную через <c>new</c> (заведомо новая)</item>
///     <item>Вы точно знаете состояние сущности (новая/существующая)</item>
///     <item>Массовая операция с заведомо новыми сущностями (например, импорт из CSV)</item>
/// </list>
/// <b>Когда использовать "умные" методы с проверками (<see cref="AddRange{TEntity}"/>,
/// <see cref="RemoveRange{TEntity}"/>, <see cref="UpdateRange{TEntity}"/>):</b>
/// <list type="bullet">
///     <item>Сущность пришла извне (другой контекст, API, десериализация, Blazor)</item>
///     <item>Вы НЕ уверены в состоянии сущности (новая или уже существует в БД)</item>
///     <item>Массовая операция с сущностями неизвестного происхождения</item>
///     <item>Долгоживущий репозиторий, где сущности могут быть из разных источников</item>
/// </list>
/// <b>Проще говоря:</b> стандартные методы — когда вы контролируете жизненный цикл сущности;
/// "умные" методы — когда сущность пришла из "внешнего мира".
/// </remarks>
public interface IRepository : IDisposable
{
    /// <summary>
    /// Список ошибок, возникших при работе с репозиторием.
    /// </summary>
    public ExceptionList<BaseException> ExceptionsList { get; }

    #region [---------- Получение ----------]
    
    /// <summary>
    /// Получить количество сущностей в репозитории.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    public Task<Result<int>> CountAsync<TEntity>()
        where TEntity : class, IAbstractEntity;

    /// <summary>
    /// Получить из репозитория сущность по Id.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <param name="id">Id получаемой сущности.</param>
    /// <returns>Получаемая сущность.</returns>
    public Task<Result<TEntity?>> FindAsync<TEntity>(object id) 
        where TEntity : class, IAbstractEntity;

    /// <summary>
    /// Получить сущности из репозитория по Id.
    /// </summary>
    /// <param name="id">Id получаемой сущности.</param>
    /// <returns>Получаемая сущность.</returns>
    /// <remarks>
    /// Отличается от <see cref="FindAsync{TEntity}"/> возможностью задать навигационные свойства,
    /// но работает медленнее.
    /// </remarks>
    /// <inheritdoc cref="GetFirstAsync{TEntity}(string[])"/>
    // ReSharper disable once InvalidXmlDocComment
    public Task<Result<TEntity?>> GetByIdAsync<TEntity>(int id, params string[] navigationProperties) 
        where TEntity : class, IAbstractEntity;

    /// <summary>
    /// Получить из репозитория первую сущность типа <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="navigationProperties">Массив имен навигационных свойств сущности,
    ///     данные которых включаются в результат (т.е. мы получаем связанные сущности);
    ///     имя может быть составным (т.е. вторая часть есть навигационное свойство связанной сущности),
    ///     в этом случае части имени разделяются символом '.'.</param>
    /// <returns>Получаемая сущность.</returns>
    public Task<Result<TEntity?>> GetFirstAsync<TEntity>(params string[] navigationProperties)
        where TEntity : class, IAbstractEntity;

    /// <summary>
    /// Получить из репозитория последнюю сущность типа <typeparamref name="TEntity"/>.
    /// </summary>
    /// <returns>Получаемая сущность.</returns>
    /// <inheritdoc cref="GetFirstAsync{TEntity}(string[])"/>
    public Task<Result<TEntity?>> GetLastAsync<TEntity>(params string[] navigationProperties)
        where TEntity : class, IAbstractEntity;

    /// <summary>
    /// Получить из репозитория сущность типа <typeparamref name="TEntity"/> по условию.
    /// </summary>
    /// <param name="predicate">Условие поиска -
    /// лямбда-выражение, которое будет преобразовано в SQL-условие <c>WHERE</c></param>
    /// <returns>Получаемая сущность.</returns>
    /// <inheritdoc cref="GetFirstAsync{TEntity}(string[])"/>
    public Task<Result<TEntity?>> GetByConditionAsync<TEntity>(Expression<Func<TEntity, bool>> predicate,
            // ReSharper disable once InvalidXmlDocComment
            params string[] navigationProperties)
        where TEntity : class;
    
    /// <summary>
    /// Получение всех сущностей из репозитория.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <param name="filter">Выражение, определяющее условие фильтрации -
    /// лямбда-выражение, которое будет преобразовано в SQL-условие <c>WHERE</c></param>
    /// <returns>Коллекция получаемых сущностей.</returns>
    /// <inheritdoc cref="GetFirstAsync{TEntity}(string[])"/>
    public Task<Result<IList<TEntity>>> GetAllAsync<TEntity>(
        Expression<Func<TEntity, bool>>? filter = null, 
        // ReSharper disable once InvalidXmlDocComment
        params string[] navigationProperties) 
        where TEntity : class;

    /// <summary>
    /// Получение всех нумерованных сущностей из репозитория с возможностью сортировки по номеру.
    /// </summary>
    /// <param name="ascending">Признак сортировки (True - по возрастанию, False - по убыванию).</param>
    /// <inheritdoc cref="GetAllAsync{TEntity}(Expression{Func{TEntity, bool}},string[])"/>
    public Task<Result<IList<TEntity>>> GetNumberedAllAsync<TEntity>(bool ascending = true,
        // ReSharper disable once InvalidXmlDocComment
        Expression<Func<TEntity, bool>>? filter = null, 
        // ReSharper disable once InvalidXmlDocComment
        params string[] navigationProperties)
        where TEntity : class, INumberedEntity;

    /// <summary>
    /// Получение всех нумерованных сущностей из репозитория с заданным номером.
    /// </summary>
    /// <param name="number">Искомый номер.</param>
    /// <inheritdoc cref="GetAllAsync{TEntity}(Expression{Func{TEntity, bool}},string[])"/>
    public Task<Result<IList<TEntity>>> GetAllByNumberAsync<TEntity>(int? number, 
        // ReSharper disable once InvalidXmlDocComment
        Expression<Func<TEntity, bool>>? filter = null, 
        // ReSharper disable once InvalidXmlDocComment
        params string[] navigationProperties)
        where TEntity : class, INumberedEntity;

    #endregion

    #region [---------- Добавление/обновление ----------]

    /// <summary>
    /// Добавление новой сущности в репозиторий с "умной" обработкой состояния.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <param name="entity">Добавляемая сущность.</param>
    /// <remarks>
    /// - Если сущность новая (Id=0) и не отслеживается → добавляется.<br/>
    /// - Если сущность с Id=0 уже отслеживается как Unchanged → меняет состояние на Added.<br/>
    /// - Если сущность с Id>0 пытается добавиться → выбрасывает исключение.<br/>
    /// </remarks>
    public Result<int> Add<TEntity>(TEntity? entity) 
        where TEntity : class, IAbstractEntity;

    /// <summary>
    /// Добавление списка сущностей в репозиторий с "умной" обработкой состояния для каждой.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <param name="entities">Список добавляемых сущностей.</param>
    public Result<int> AddRange<TEntity>(IList<TEntity>? entities) 
        where TEntity : class, IAbstractEntity;

    /// <summary>
    /// Добавление списка сущностей в репозиторий.
    /// </summary>
    /// <inheritdoc cref="AddRange{TEntity}(IList{TEntity})"/>
    public Result<int> AddRangeQuickly<TEntity>(IList<TEntity>? entities)
        where TEntity : class;

    /// <summary>
    /// Обновление сущности в репозитории с "умной" обработкой состояния.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <param name="entity">Добавляемая сущность.</param>
    /// <remarks>
    /// ВНИМАНИЕ! Метод помечает ВСЕ свойства как изменённые.<br/>
    /// - Неотслеживаемая с Id=0 → добавляется.<br/>
    /// - Неотслеживаемая с Id>0 → присоединяется и помечается Modified.<br/>
    /// - Отслеживаемая Unchanged → меняет состояние на Modified.<br/>
    /// </remarks>
    public Result<int> Update<TEntity>(TEntity? entity) 
        where TEntity : class, IAbstractEntity;

    /// <summary>
    /// Обновление списка сущностей в репозитории с умной обработкой состояния для каждой.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <param name="entities">Список изменяемых сущностей.</param>
    public Result<int> UpdateRange<TEntity>(IList<TEntity>? entities) 
        where TEntity : class, IAbstractEntity;

    /// <summary>
    /// Обновление списка сущностей в репозитории.
    /// </summary>
    /// <inheritdoc cref="UpdateRange{TEntity}(IList{TEntity})"/>
    public Result<int> UpdateRangeQuickly<TEntity>(IList<TEntity>? entities)
        where TEntity : class;

    #endregion
    
    #region [---------- Удаление ----------]
    
    /// <summary>
    /// Удаление сущности с "умной" обработкой состояния.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <param name="entity">Удаляемая сущность.</param>
    /// <remarks>
    /// - Новая сущность (Id=0) → просто отключается от отслеживания.<br/>
    /// - Существующая неотслеживаемая → присоединяется и удаляется.<br/>
    /// - Отслеживаемая существующая → помечается на удаление.<br/>
    /// </remarks>
    public Result<int> Remove<TEntity>(TEntity? entity) 
        where TEntity : class, IAbstractEntity;

    /// <summary>
    /// Удаление списка сущностей с "умной" обработкой состояния для каждой.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <param name="entities">Список удаляемых сущностей.</param>
    public Result<int> RemoveRange<TEntity>(IList<TEntity>? entities) 
        where TEntity : class, IAbstractEntity;

    /// <summary>
    /// Удаление списка сущностей.
    /// </summary>
    /// <inheritdoc cref="RemoveRange{TEntity}(IList{TEntity})"/>
    public Result<int> RemoveRangeQuickly<TEntity>(IList<TEntity>? entities)
        where TEntity : class;

    /// <summary>
    /// Удаление всех сущностей.
    /// </summary>
    public Result<int> RemoveAllQuickly<TEntity>()
        where TEntity : class;

    #endregion

    #region [---------- Присоединение/отсоединение ----------]

    /// <summary>
    /// Начинает отслеживание указанной сущности.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <param name="entity">Присоединяемая сущность.</param>
    public Result<int> Attach<TEntity>(TEntity? entity) 
        where TEntity : class;

    /// <summary>
    /// Начинает отслеживание нескольких сущностей.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <param name="entities">Список присоединяемых сущностей.</param>
    public Result<int> AttachRange<TEntity>(IList<TEntity>? entities) 
        where TEntity : class;

    /// <summary>
    /// Сбрасывает отслеживание сущности.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <param name="entity">Присоединяемая сущность.</param>
    public Result<int> Detach<TEntity>(TEntity? entity) 
        where TEntity : class;
    
    /// <summary>
    /// Сбрасывает отслеживание всех сущностей указанного типа.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    public Result<int> DetachAll<TEntity>()
        where TEntity : class;

    #endregion
    
    #region [---------- Состояние и сохранение ----------]

    /// <summary>
    /// Возвращает текущее состояние сущности.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    public Result<EntityState> GetEntityState<TEntity>(TEntity entity) 
        where TEntity : class;

    /// <summary>
    /// Сохранение всех изменений.
    /// </summary>
    /// <remarks>
    /// После вызова временные ID заменяются на реальные.<br/>
    /// При конфликте выбрасывается DbUpdateConcurrencyException (упакованное в Result).
    /// </remarks>
    public Task<Result<int>> SaveChangesAsync();

    /// <summary>
    /// Откат всех несохранённых изменений.
    /// </summary>
    public Task<Result<int>> DiscardChangesAsync();

    #endregion

    #region [---------- Перезагрузки ----------]

    /// <summary>
    /// Перезагружает сущность из БД.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <param name="entity">Перезагружаемая сущность.</param>
    public Task<Result<int>> ReloadAsync<TEntity>(TEntity entity) 
        where TEntity : class;

    /// <summary>
    /// Перезагружает список сущностей.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    /// <param name="entities">Список перезагружаемых сущностей.</param>
    public Task<Result<int>> ReloadRangeAsync<TEntity>(IList<TEntity> entities) 
        where TEntity : class;
    
    /// <summary>
    /// Перезагружает все сущности соответствующего типа.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности.</typeparam>
    public Task<Result<int>> ReloadAllAsync<TEntity>() 
        where TEntity : class;

    #endregion
}
