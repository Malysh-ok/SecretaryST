using System.Collections.ObjectModel;
using ProblemDomain.Entities.CommonEntities;

namespace Presentation.ViewModels.Common;

/// <summary>
/// Сообщение, рассылаемое представлениям (окнам) при изменении списка всех соревнований.
/// </summary>
public class AllCompetitionsMessage
{
    /// <summary>
    /// Коллекция данных о соревнованиях.
    /// </summary>
    // ReSharper disable once MemberInitializerValueIgnored
    public ObservableCollection<CompetitionData> CompetitionDataCollection { get; set; } = [];

    /// <summary>
    /// Данные о текущем соревновании.
    /// </summary>
    public CompetitionData? CurrentCompetitionData { get; set; }

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
    /// <param name="competitionDataCollection">Коллекция данных о соревнованиях.</param>
    /// <param name="currentCompetitionData">Данные о текущем соревновании.</param>
    public AllCompetitionsMessage(
        ObservableCollection<CompetitionData> competitionDataCollection, 
        CompetitionData? currentCompetitionData)
    {
        CompetitionDataCollection = competitionDataCollection;
        CurrentCompetitionData = currentCompetitionData;
    }
}