using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Cogs.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
using ProblemDomain.UseCases.Services;

namespace Presentation.ViewModels.Common.Observables;

/// <summary>
/// ObservableObject, инкапсулирующий свойства и методы для работы со <see cref="SportEvent"/>.
/// </summary>
public class SportEventObservable : ObservableValidator
{
    private readonly SportEventService _sportEventService;

    /// <summary>
    /// Словарь текстовых значений для трудностей видов программы.
    /// </summary>
    private readonly  ObservableDictionary<(DisciplineGroupEnm, Difficulty.IdEnm), string> _difficultyMap;

    
    /// <summary>
    /// Конструктор.
    /// </summary>
    public SportEventObservable(
        SportEventService sportEventService,
        ObservableDictionary<(DisciplineGroupEnm, Difficulty.IdEnm), string> difficultyMap,
        SportEvent sportEvent)
    {
        _sportEventService = sportEventService;
        _difficultyMap = difficultyMap;
        SportEvent = sportEvent;
        Discipline = sportEvent.Discipline;
        Difficulty = sportEvent.Difficulty;
    }

    /// <summary>
    /// Текущий вид программы (основное свойство).
    /// </summary>
    public SportEvent SportEvent
    {
        get;
        set => SetProperty(ref field, value);
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
                AvailableDifficulties = new ObservableCollection<KeyValuePair<Difficulty.IdEnm, string>>(
                    _sportEventService.GetAvailableDifficulties(_difficultyMap, Discipline));
                
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
    public Difficulty.IdEnm Difficulty
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
    /// Коллекция допустимых значений трудности для текущей группы дисциплин.
    /// </summary>
    /// <remarks>
    /// Необходимость <see cref="KeyValuePair"/> продиктована удобством биндинга в представлении. 
    /// </remarks>
    public ObservableCollection<KeyValuePair<Difficulty.IdEnm, string>> AvailableDifficulties
    {
        get;
        set => SetProperty(ref field, value);
    } = [];

    /// <summary>
    /// Флаг доступности признака короткой дистанции.
    /// </summary>
    public bool? IsShortAvailable
    {
        get => _sportEventService.IsShortAvailable(Discipline);
    }
}