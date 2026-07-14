using System.Collections.ObjectModel;
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
    public ObservableCollection<CompetitionData> Competitions { get; set; } = [];

    /// <summary>
    /// Данные о текущем соревновании.
    /// </summary>
    public CompetitionData? CurrentCompetition { get; set; }

    /// <summary>
    /// Конструктор, запрещающий создания экземпляра без параметров.
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private AllCompetitionsMessage()
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="competitions">Коллекция данных о соревнованиях.</param>
    /// <param name="currentCompetition">Данные о текущем соревновании.</param>
    public AllCompetitionsMessage(
        ObservableCollection<CompetitionData> competitions, 
        CompetitionData? currentCompetition)
    {
        Competitions = competitions;
        CurrentCompetition = currentCompetition;
    }
}