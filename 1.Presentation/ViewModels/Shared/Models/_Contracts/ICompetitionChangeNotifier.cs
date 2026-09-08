namespace Presentation.ViewModels.Shared.Models._Contracts;

/// <summary>
/// Интерфейс для уведомления об изменениях в <see cref="CompetitionObservable"/>.
/// </summary>
public interface ICompetitionChangeNotifier
{
    /// <summary>
    /// Вызывается при изменении свойства у <see cref="CompetitionObservable"/>.
    /// </summary>
    /// <param name="competitionObservable">Observable-модель соревнования, в которой произошло изменение.</param>
    /// <param name="propertyName">Наименование изменённого свойства.</param>
    public void OnCompetitionChanged(CompetitionObservable competitionObservable , string propertyName);
}