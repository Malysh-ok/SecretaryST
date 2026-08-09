using System.Collections.Specialized;
using Common.WpfModule.Components.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Presentation.ViewModels.Shared.Validation;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.UseCases.Services;

namespace Presentation.ViewModels.Shared.Models;

/// <summary>
/// ObservableObject, инкапсулирующий свойства и методы для работы со <see cref="SportEvent"/>.
/// </summary>
public class SportEventObservable : ObservableValidator, IEquatable<SportEventObservable>
{
    private readonly SportEventService _sportEventService;

    /// <summary>
    /// Коллекция трудностей видов программы.
    /// </summary>
    private readonly IList<Difficulty> _difficulties;

    /// <summary>
    /// Коллекция возрастных групп.
    /// </summary>
    private readonly IList<AgeGroup> _ageGroups;

    /// <summary>
    /// Конструктор.
    /// </summary>
    public SportEventObservable(
        SportEventService sportEventService,
        IList<Difficulty> difficulties,
        IList<AgeGroup> ageGroups,
        SportEvent sportEvent,
        ObservableCollectionEx<Discipline> availableDisciplines)
    {
        _sportEventService = sportEventService;
        _difficulties = difficulties;
        _ageGroups = ageGroups;
        SportEvent = sportEvent;
        Discipline = sportEvent.Discipline;
        DisplayDiscipline = Discipline;
        Difficulty = sportEvent.Difficulty;
        AgeGroup = sportEvent.AgeGroup;
        DisplayAgeGroup = AgeGroup;

        // Уставливаем коллекцию дисциплин и подписываемся на событие её изменения
        AvailableDisciplines = availableDisciplines;
        AvailableDisciplines.CollectionChanged += OnAvailableDisciplinesChanged;

        // Подписываемся на событие изменения коллекции возрастных групп
        AvailableAgeGroups.CollectionChanged += OnAvailableAgeGroupsChanged;

        // Валидация всех свойств
        ValidateAfterLoad();
    }

    /// <summary>
    /// Текущий вид программы (основное свойство).
    /// </summary>
    public SportEvent SportEvent
    {
        get;
        init => SetProperty(ref field, value);
    }

    /// <summary>
    /// Текущая дисциплина.
    /// </summary>
    private Discipline Discipline
    {
        get => SportEvent.Discipline;
        set
        {
            // Обновляем SportEvent
            SportEvent.Discipline = value;

            // Обновляем зависимые коллекции
            UpdateAvailableDifficulties();
            UpdateAvailableAgeGroups();

            // Обновляем флаг короткой дистанции
            if (_sportEventService.IsShortUpdate(value, SportEvent))
                OnPropertyChanged(nameof(SportEvent));

            // Уведомляем UI об изменении
            OnPropertyChanged(nameof(Difficulty));
            OnPropertyChanged(nameof(IsShortAvailable));
        }
    }

    /// <summary>
    /// Дисциплина - свойство для привязки к ComboBox
    /// </summary>
    [DisciplineAvailability]
    public Discipline? DisplayDiscipline
    {
        get;
        set {
            if (SetProperty(ref field, value))
            {
                // При выборе из ComboBox обновляем реальную дисциплину
                Discipline = value ?? Discipline;
            }
        }
    }

    /// <summary>
    /// Текущая трудность вида программы.
    /// </summary>
    [DifficultyAvailability]
    public Difficulty Difficulty
    {
        get;
        set
        {
            if (SetProperty(ref field, value, true))
            {
                SportEvent.Difficulty = value;
            }
        }
    }

    /// <summary>
    /// Текущая возрастная группа.
    /// </summary>
    private AgeGroup AgeGroup
    {
        get => SportEvent.AgeGroup;
        set => SportEvent.AgeGroup = value;
    }

    /// <summary>
    /// Возрастная группа - свойство для привязки к ComboBox.
    /// </summary>
    [AgeGroupAvailability]
    public AgeGroup? DisplayAgeGroup
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                AgeGroup = value ?? AgeGroup;
            }
        }
    }

    /// <summary>
    /// Коллекция доступных для выбора дисциплин. 
    /// </summary>
    public ObservableCollectionEx<Discipline> AvailableDisciplines { get; }

    /// <summary>
    /// Коллекция допустимых значений трудности для текущей группы дисциплин.
    /// </summary>
    public ObservableCollectionEx<Difficulty> AvailableDifficulties { get; } = [];

    /// <summary>
    /// Коллекция допустимых значений трудности для текущей группы дисциплин.
    /// </summary>
    public ObservableCollectionEx<AgeGroup> AvailableAgeGroups { get; } = [];

    /// <summary>
    /// Флаг доступности признака короткой дистанции.
    /// </summary>
    public bool? IsShortAvailable => _sportEventService.IsShortAvailable(Discipline);

    /// <summary>
    /// Обновляем коллекцию доступных трудностей.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public void UpdateAvailableDifficulties()
    {
        AvailableDifficulties.ClearAndAddRange(
            _sportEventService.GetAvailableDifficulties(_difficulties, Discipline));
    }

    /// <summary>
    /// Обновляем коллекцию доступных возрастных групп.
    /// </summary>
    public void UpdateAvailableAgeGroups()
    {
        AvailableAgeGroups.ClearAndAddRange(
            _sportEventService.GetAvailableAgeGroups(_ageGroups, Discipline, SportEvent.CompetitionData.IsStudentCompetition));
    }

    /// <summary>
    /// Принудительная валидация <see cref="SportEventObservable"/>.
    /// </summary>
    private void ValidateAfterLoad()
    {
        // Запускаем валидацию всех свойств
        ValidateAllProperties();
    }

    /// <summary>
    /// Обработчик события изменения коллекции доступных дисциплин.
    /// </summary>
    private void OnAvailableDisciplinesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Восстанавливаем отображаемую дисциплину из текущей
        if (DisplayDiscipline != Discipline)
            DisplayDiscipline = Discipline;
        
        ValidateAllProperties();
    }

    /// <summary>
    /// Обработчик события изменения коллекции доступных возрастных групп.
    /// </summary>
    private void OnAvailableAgeGroupsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Восстанавливаем отображаемую возрастную группу из текущей
        if (DisplayAgeGroup != AgeGroup)
            DisplayAgeGroup = AgeGroup;
        
        ValidateAllProperties();
    }

    public bool Equals(SportEventObservable? other)
    {
        if (other is null || GetType() != other.GetType()) return false;

        return SportEvent.Id == other.SportEvent.Id;
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
        => Equals(obj as SportEventObservable);
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(SportEvent.Id);
    }

}