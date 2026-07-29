using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.UseCases.Services;

namespace Presentation.ViewModels.Shared.Models;

/// <summary>
/// ObservableObject, инкапсулирующий свойства и методы для работы со <see cref="SportEvent"/>.
/// </summary>
public class SportEventObservable : ObservableValidator
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
        SportEvent sportEvent)
    {
        _sportEventService = sportEventService;
        _difficulties = difficulties;
        _ageGroups = ageGroups;
        SportEvent = sportEvent;
        Discipline = sportEvent.Discipline;
        Difficulty = sportEvent.Difficulty;
        AgeGroup = sportEvent.AgeGroup;
    }

    /// <summary>
    /// Текущий вид программы (основное свойство).
    /// </summary>
    public SportEvent SportEvent
    {
        get;
        init
        {
            if (SetProperty(ref field, value))
            {
            }
        }
    }

    /// <summary>
    /// Текущая дисциплина.
    /// </summary>
    // TODO: временно [Required(ErrorMessage = "Ошибка: недопустимое значение дисциплины.")]
    [Required(ErrorMessage = "Ошибка: недопустимое значение дисциплины.")]
    public Discipline Discipline
    {
        get;
        set
        {
            if (SetProperty(ref field, value, true))
            {
                SportEvent.Discipline = value;
                
                // Обновляем коллекцию доступных трудностей
                UpdateGetAvailableDifficulties();
                
                // Обновляем коллекцию доступных возрастных групп
                UpdateAvailableAgeGroups();

                // Обновляем при необходимости признак короткой дистанции
                if (_sportEventService.IsShortUpdate(value,  SportEvent))
                    OnPropertyChanged(nameof(SportEvent));
                
                // Дополнительно уведомляем UI об изменении
                OnPropertyChanged(nameof(Difficulty));
                OnPropertyChanged(nameof(IsShortAvailable));
            }
        }
    }

    /// <summary>
    /// Текущая трудность вида программы.
    /// </summary>
    [Required]
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
    [Required]
    public AgeGroup AgeGroup
    {
        get;
        set
        {
            if (SetProperty(ref field, value, true))
            {
                SportEvent.AgeGroup = value;
            }
        }
    }

    /// <summary>
    /// Коллекция допустимых значений трудности для текущей группы дисциплин.
    /// </summary>
    public ObservableCollection<Difficulty> AvailableDifficulties
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];
    
    /// <summary>
    /// Коллекция допустимых значений трудности для текущей группы дисциплин.
    /// </summary>
    public ObservableCollection<AgeGroup> AvailableAgeGroups
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>
    /// Флаг доступности признака короткой дистанции.
    /// </summary>
    public bool? IsShortAvailable => _sportEventService.IsShortAvailable(Discipline);

    /// <summary>
    /// Обновляем коллекцию доступных трудностей.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public void UpdateGetAvailableDifficulties()
    {
        AvailableDifficulties = new ObservableCollection<Difficulty>(
            _sportEventService.GetAvailableDifficulties(_difficulties, Discipline));
    }
    
    /// <summary>
    /// Обновляем коллекцию доступных возрастных групп.
    /// </summary>
    public void UpdateAvailableAgeGroups()
    {
        AvailableAgeGroups = new ObservableCollection<AgeGroup>(
            _sportEventService.GetAvailableAgeGroups(_ageGroups, Discipline, SportEvent.CompetitionData.IsStudentCompetition));
    }
}