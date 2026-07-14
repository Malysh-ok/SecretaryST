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
    public CompetitionData? CurrentCompetition { get; set; }

    /// <summary>
    /// Конструктор, запрещающий создания экземпляра без параметров.
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private CompetitionMessage()
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="currentCompetition">Данные о текущем соревновании.</param>
    public CompetitionMessage(CompetitionData? currentCompetition)
    {
        CurrentCompetition = currentCompetition;
    }
}