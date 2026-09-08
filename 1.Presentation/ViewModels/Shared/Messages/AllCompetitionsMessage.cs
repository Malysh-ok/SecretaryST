using System.Collections.ObjectModel;
using Common.WpfModule.Components.Collections;
using Presentation.ViewModels.Shared.Models;
using ProblemDomain.Entities.CommonEntities;

namespace Presentation.ViewModels.Shared.Messages;

/// <summary>
/// Сообщение, рассылаемое представлениям (окнам) при изменении списка всех соревнований.
/// </summary>
public class AllCompetitionsMessage
{
    /// <summary>
    /// Коллекция данных о соревнованиях.
    /// </summary>
    // ReSharper disable once MemberInitializerValueIgnored
    public ObservableCollectionEx<CompetitionObservable> Competitions { get; set; } = [];

    /// <summary>
    /// Данные о текущем соревновании.
    /// </summary>
    public CompetitionObservable? CurrentCompetition { get; set; }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="competitions">Коллекция данных о соревнованиях.</param>
    /// <param name="currentCompetition">Данные о текущем соревновании.</param>
    public AllCompetitionsMessage(
        ObservableCollectionEx<CompetitionObservable> competitions, 
        CompetitionObservable? currentCompetition)
    {
        Competitions = competitions;
        CurrentCompetition = currentCompetition;
    }
}