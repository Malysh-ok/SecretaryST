namespace Presentation.ViewModels.Shared.Models._Contracts;

/// <summary>
/// Интерфейс для уведомления об изменениях в <see cref="RefereeObservable"/>.
/// </summary>
public interface IRefereeChangeNotifier
{
    /// <summary>
    /// Вызывается при изменении свойства у <see cref="RefereeObservable"/>.
    /// </summary>
    /// <param name="refereeObservable">Observable-модель судьи, в которой произошло изменение.</param>
    /// <param name="propertyName">Наименование изменённого свойства.</param>
    public void OnRefereeChanged(RefereeObservable? refereeObservable, string? propertyName);
}