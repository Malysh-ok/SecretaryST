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
    /// Конструктор.
    /// </summary>
    public SportEventObservable(
        SportEventService sportEventService,
        IList<Difficulty> difficulties,
        SportEvent sportEvent)
    {
        _sportEventService = sportEventService;
        _difficulties = difficulties;
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
                AvailableDifficulties = new ObservableCollection<Difficulty>(
                    _sportEventService.GetAvailableDifficulties(_difficulties, Discipline));
                
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
    /// Коллекция допустимых значений трудности для текущей группы дисциплин.
    /// </summary>
    public ObservableCollection<Difficulty> AvailableDifficulties
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