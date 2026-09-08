using CommunityToolkit.Mvvm.ComponentModel;
using Presentation.ViewModels.Shared.Models._Contracts;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.UseCases.Services;

namespace Presentation.ViewModels.Shared.Models;

/// <summary>
/// ObservableObject, инкапсулирующий свойства и методы для работы с <see cref="ProblemDomain.Entities.CommonEntities.Competition"/>.
/// </summary>
public class CompetitionObservable : ObservableValidator, IEquatable<CompetitionObservable>
{
    private CompetitionService? _competitionService;
    private ICompetitionChangeNotifier? _competitionChangeNotifier;

    /// <summary>
    /// Конструктор неполного экземпляра.
    /// </summary>
    private CompetitionObservable(Competition competition)
    {
        Competition = competition;
        ShortName = competition.ShortName;
    }
    
    /// <summary>
    /// Конструктор полного экземпляра.
    /// </summary>
    private CompetitionObservable(
        CompetitionService competitionService,
        Competition competition,
        ICompetitionChangeNotifier competitionChangeNotifier)
    {
        // Заполняем свойства
        UpdateFrom(competitionService, competition, competitionChangeNotifier);

        // Валидация всех свойств
        ValidateAfterLoad();
    }

    /// <summary>
    /// Текущее соревнование (основное свойство).
    /// </summary>
    public Competition Competition { get; private set; } = null!;

    /// <summary>
    /// Краткое название соревнований.
    /// </summary>
    public string ShortName
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                Competition.ShortName = value;

                // Оповещаем родительскую ViewModel об изменении короткого наименования
                _competitionChangeNotifier?.OnCompetitionChanged(this, nameof(ShortName));
            }
        }
    } = null!;

    /// <summary>
    /// Начальная дата соревнований.
    /// </summary>
    public DateTime? InitialDate
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                Competition.InitialDate = value!.Value;
                
                // Оповещаем родительскую ViewModel об изменении начальной даты соревнования
                _competitionChangeNotifier?.OnCompetitionChanged(this, nameof(InitialDate));
            }
        }
    } = null!;
    
    /// <summary>
    /// Конечная дата соревнований.
    /// </summary>
    public DateTime? EndDate
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                Competition.EndDate = value!.Value;
                
                // Оповещаем родительскую ViewModel об изменении конечной даты соревнования
                _competitionChangeNotifier?.OnCompetitionChanged(this, nameof(EndDate));
            }
        }
    } = null!;

    /// <summary>
    /// Детальный статус соревнования.
    /// </summary>
    public DetailedCompetitionStatus DetailedCompetitionStatus
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                Competition.DetailedCompetitionStatus = value;

                // Оповещаем родительскую ViewModel об изменении детального статуса соревнования
                _competitionChangeNotifier?.OnCompetitionChanged(this, nameof(DetailedCompetitionStatus));
            }
        }
    } = null!;

    /// <summary>
    /// Признак того, что соревнования студенческие.
    /// </summary>
    public bool IsStudentCompetition
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                Competition.IsStudentCompetition = value;
                
                // Оповещаем родительскую ViewModel об изменении признака студенческих соревнований
                _competitionChangeNotifier?.OnCompetitionChanged(this, nameof(IsStudentCompetition));
            }
        }
    }
    
    /// <summary>
    /// Создание <see cref="CompetitionObservable"/> с неполными данными.
    /// </summary>
    /// <param name="competition">Текущее соревнование.</param>
    public static CompetitionObservable CreatePartial(Competition competition)
    {
        return new CompetitionObservable(competition);
    }

    /// <summary>
    /// Создание <see cref="CompetitionObservable"/>.
    /// </summary>
    /// <param name="competitionService">Сервис для работы с соревнованиями.</param>
    /// <param name="competition">Текущее соревнование.</param>
    /// <param name="competitionChangeNotifier">Интерфейс для уведомления об изменениях
    /// в текущем экземпляре <see cref="CompetitionObservable"/>.</param>
    public static CompetitionObservable CreateComplete(
        CompetitionService competitionService,
        Competition competition,
        ICompetitionChangeNotifier competitionChangeNotifier)
    {
        return new CompetitionObservable(competitionService, competition, competitionChangeNotifier);
    }
    
    /// <summary>
    /// Обновляет данные текущего Observable-объекта из переданного объекта <see cref="ProblemDomain.Entities.CommonEntities.Competition"/>.
    /// </summary>
    public void UpdateFrom(
        CompetitionService competitionService,
        Competition competition,
        ICompetitionChangeNotifier competitionChangeNotifier)
    {
        _competitionService = competitionService;
        _competitionChangeNotifier = competitionChangeNotifier;

        Competition = competition;
        ShortName = competition.ShortName;
        InitialDate = competition.InitialDate;
        EndDate = competition.EndDate;
        DetailedCompetitionStatus = competition.DetailedCompetitionStatus;
        IsStudentCompetition = competition.IsStudentCompetition;

        // Уведомляем UI об изменениях всех свойств
        OnPropertyChanged(string.Empty);
    }

    
    /// <summary>
    /// Принудительная валидация <see cref="RefereeObservable"/>.
    /// </summary>
    private void ValidateAfterLoad()
    {
        // Запускаем валидацию всех свойств
        ValidateAllProperties();
    }

    /// <inheritdoc />
    public bool Equals(CompetitionObservable? other)
    {
        if (other is null || GetType() != other.GetType()) return false;

        return Competition.Id == other.Competition.Id;
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
        => Equals(obj as CompetitionObservable);
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return HashCode.Combine(Competition.Id);
    }
}