using Presentation.ViewModels.Shared.Models;
using ProblemDomain.Entities.CommonEntities;

namespace Presentation.ViewModels.Shared.Messages;

/// <summary>
/// Сообщение, рассылаемое представлениям (окнам) при изменении соревнования.
/// </summary>
public class CompetitionMessage
{
    /// <summary>
    /// Данные о текущем соревновании.
    /// </summary>
    public CompetitionObservable? CurrentCompetition { get; set; }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="currentCompetition">Данные о текущем соревновании.</param>
    public CompetitionMessage(CompetitionObservable? currentCompetition)
    {
        CurrentCompetition = currentCompetition;
    }
}