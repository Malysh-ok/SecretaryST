using Common.BaseExtensions.Collections;
using Common.WpfModule.Components.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Presentation.ViewModels.Shared.Infrastructure;
using Presentation.ViewModels.Shared.Models._Contracts;
using Presentation.ViewModels.Shared.Validation;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.UseCases.Services;

namespace Presentation.ViewModels.Shared.Models;

/// <summary>
/// ObservableObject, инкапсулирующий свойства и методы для работы с <see cref="Referee"/>.
/// </summary>
public class RefereeObservable : ObservableValidator, IEquatable<RefereeObservable>
{
    private readonly ViewModelHelper _viewModelHelper;
    private readonly RefereeService _refereeService;                    // сервис для работы с Судьями
    private readonly IList<RefereeCategory> _categories;                // коллекция всех возможных категорий
    private readonly IList<RefereeRole> _roles;                         // коллекция всех возможных должностей
    private readonly IRefereeChangeNotifier _refereeChangeNotifier;     // уведомляет об изменениях в экземпляре данного класса
    private readonly Func<bool> _getIsRefereeDataSelectionRestricted;   // делегат, для получения признака ограничения данных у судей
    private readonly Func<DisciplineGroup?> _getDisciplineGroupFilter;  // делегат, для получения фильтрующей группы дисциплин
    private readonly Func<DateTime?> _getCompetitionDate;               // делегат, для получения актуального значения даты соревнования
    private readonly Func<int> _getSportEventCount;                     // делегат, для получения актуального значения количества видов программ 
    private readonly Func<IList<RefereeRole>> _getOccupiedRefereeRoles; // делегат, для получения коллекции занятых должностей

    /// <summary>
    /// Конструктор.
    /// </summary>
    private RefereeObservable(
        ViewModelHelper viewModelHelper,
        RefereeService refereeService,
        Referee referee,
        IList<RefereeCategory> categories,
        IList<RefereeRole> roles,
        IRefereeChangeNotifier refereeChangeNotifier,
        Func<bool> getIsRefereeDataSelectionRestricted,
        Func<DisciplineGroup?> getDisciplineGroupFilter,
        Func<DateTime?> getCompetitionDate,
        Func<int> getSportEventCount,
        Func<IList<RefereeRole>> getOccupiedRefereeRoles)
    {
        _viewModelHelper = viewModelHelper;
        _refereeService = refereeService;
        _categories = categories;
        _roles = roles;
        _refereeChangeNotifier = refereeChangeNotifier;
        _getIsRefereeDataSelectionRestricted = getIsRefereeDataSelectionRestricted;
        _getDisciplineGroupFilter = getDisciplineGroupFilter;
        _getCompetitionDate = getCompetitionDate;
        _getSportEventCount = getSportEventCount;
        _getOccupiedRefereeRoles = getOccupiedRefereeRoles;
        Referee = referee;
        Number = referee.Number;
        CategoryObservable = new RefereeCategoryObservable(referee.Category);
        CategoryGrantedDate = referee.CategoryGrantedDate;
        RoleObservable = new RefereeRoleObservable(referee.Role);
    }
    
    /// <summary>
    /// Текущий судья (основное свойство).
    /// </summary>
    public Referee Referee
    {
        get;
        private init => SetProperty(ref field, value);
    }

    /// <summary>
    /// Номер судьи.
    /// </summary>
    public int Number
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                Referee.Number = value;
        }
    }
    
    /// <summary>
    /// Текущая судейская категория.
    /// </summary>
    [RefereeCategoryAvailability]
    public RefereeCategoryObservable CategoryObservable
    {
        get;
        set
        {
            if (SetProperty(ref field, value) && Referee.Category != value.Category)
            {
                Referee.Category = value.Category;

                // Ручная валидация категории
                // ReSharper disable once RedundantArgumentDefaultValue
                ValidateProperty(CategoryObservable, nameof(CategoryObservable));

                // Ручная валидация должности (категория влияет на должность)
                ValidateProperty(RoleObservable, nameof(RoleObservable));
                
                // Обновляем должности (асинхронно "запустил и забыл")
                _ = UpdateAvailableRolesAsync();
            }
        }
    }

    /// <summary>
    /// Дата присвоения/подтверждения судейской категории.
    /// </summary>
    public DateTime? CategoryGrantedDate
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                Referee.CategoryGrantedDate =  value;
            }
        }
    }
    
    /// <summary>
    /// Признак просроченности судейской категории.
    /// </summary>
    public bool? IsCategoryExpired => _refereeService.GetIsRefereeCategoryExpiredOn(Referee, _getCompetitionDate());

    /// <summary>
    /// Текущая судейская должность.
    /// </summary>
    [RefereeRoleAvailability]
    public RefereeRoleObservable RoleObservable
    {
        get;
        set
        {
            if (SetProperty(ref field, value) && Referee.Role != value.Role)
            {
                Referee.Role = value.Role;
                
                // Ручная валидация должности
                ValidateProperty(RoleObservable);
                
                // Оповещаем родительскую ViewModel об изменении должности
                _refereeChangeNotifier.OnRefereeChanged(this, nameof(RoleObservable));
            }
        }
    }

    /// <summary>
    /// Коллекция доступных для выбора Observable-судейских категорий. 
    /// </summary>
    public ObservableCollectionEx<RefereeCategoryObservable> AvailableCategories { get; } = [];

    /// <summary>
    /// Коллекция доступных для выбора Observable-судейских должностей. 
    /// </summary>
    public ObservableCollectionEx<RefereeRoleObservable> AvailableRoles { get; } = [];
    
    /// <summary>
    /// Фабричный метод для асинхронного создания <see cref="RefereeObservable"/>.
    /// </summary>
    public static Task<RefereeObservable> CreateAsync(
        ViewModelHelper viewModelHelper,
        RefereeService refereeService,
        Referee referee,
        IList<RefereeCategory> refereeCategories,
        IList<RefereeRole> refereeRoles,
        IRefereeChangeNotifier notifier,
        Func<bool> isRefereeDataSelectionRestricted,
        Func<DisciplineGroup?> getDisciplineGroupFilter,
        Func<DateTime?> getCompetitionInitialDate,
        Func<int> getSportEventCount,
        Func<IList<RefereeRole>> getOccupiedRefereeRoles)
    {
        var observable = new RefereeObservable(
            viewModelHelper,
            refereeService,
            referee,
            refereeCategories,
            refereeRoles,
            notifier,
            isRefereeDataSelectionRestricted,
            getDisciplineGroupFilter,
            getCompetitionInitialDate,
            getSportEventCount,
            getOccupiedRefereeRoles);
        
        // Создаем коллекцию допустимых категорий
        refereeCategories.ForEach(rc => observable.AvailableCategories.Add(new RefereeCategoryObservable(rc)));
        
        // Создаем коллекцию допустимых должностей
        refereeRoles.ForEach(rr => observable.AvailableRoles.Add(new RefereeRoleObservable(rr)));
        
        // REMARK: Здесь можем выполнять асинхронные действия
        
        return Task.FromResult(observable);
    }
    
    /// <summary>
    /// Асинхронное обновление зависимых коллекций.
    /// </summary>
    public async Task  UpdateDependentCollectionsAsync()
    {
        UpdateAvailableCategories();
        await UpdateAvailableRolesAsync();
    }
    
    /// <summary>
    /// Обновление коллекции доступных судейских категорий.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public void UpdateAvailableCategories()
    {
        var availabilityDict = 
            _refereeService.GetAvailableCategories(Referee, _categories, _getIsRefereeDataSelectionRestricted());
        
        // Обновляем IsAvailable у Observable объектов
        foreach (var categoryObservable in AvailableCategories)
        {
            categoryObservable.IsAvailable = availabilityDict.TryGetValue(categoryObservable.Id, out var isAvailable) && isAvailable;
        }
        
        // Ручная валидация категории
        ValidateProperty(CategoryObservable, nameof(CategoryObservable));
    }

    /// <summary>
    /// Обновление признака просроченности судейской категории.
    /// </summary>
    public void UpdateCategoryExpiration()
    {
        OnPropertyChanged(nameof(IsCategoryExpired));
    }

    /// <summary>
    /// Обновление коллекции доступных судейских должностей.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public async Task UpdateAvailableRolesAsync()
    {
        var result = await _refereeService.GetAvailableRolesAsync(
            Referee,
            _roles,
            _getIsRefereeDataSelectionRestricted(),
            _getDisciplineGroupFilter(),
            _getOccupiedRefereeRoles(),
            _getSportEventCount()
        );
        if (!result)
        {
            // Пишем только в лог об ошибке
            _viewModelHelper.HandleException(result.Excptn, this.ToString(), nameof(UpdateAvailableRolesAsync), false);
            return;
        }
    
        var availabilityDict = result.Value!;
    
        // Обновляем IsAvailable у Observable объектов
        foreach (var roleObservable in AvailableRoles)
        {
            roleObservable.IsAvailable = availabilityDict.TryGetValue(roleObservable.Id, out var isAvailable) && isAvailable;
        }
        
        // Ручная валидация должности
        ValidateProperty(RoleObservable, nameof(RoleObservable));
    }
    
    /// <summary>
    /// Получение признака валидности текущей должности.
    /// </summary>
    public async Task<bool> GetIsRoleValidAsync()
    {
        var result = await _refereeService.GetIsRoleValidAsync(
            Referee, 
            _getIsRefereeDataSelectionRestricted(),
            _getDisciplineGroupFilter(),
            _getOccupiedRefereeRoles(), 
            _getSportEventCount()
        );
        if (!result)
        {
            // Пишем только в лог об ошибке
            _viewModelHelper.HandleException(result.Excptn, this.ToString(), nameof(UpdateAvailableRolesAsync), false);
            return false;
        }
        
        return result.Value;
    }

    /// <inheritdoc />
    public bool Equals(RefereeObservable? other)
    {
        if (other is null || GetType() != other.GetType()) return false;

        return Referee.Id == other.Referee.Id;
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
        => Equals(obj as RefereeObservable);
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Referee.Id);
    }
}