using Common.WpfModule.Components.Models;

namespace Presentation.ViewModels._Contracts;

/// <summary>
/// Интерфейс, предоставляющий данные для статус-бара.
/// </summary>
public interface IStatusBarDataProvider
{
    /// <summary>
    /// Данные для статус-бара.
    /// </summary>
    StatusBarData StatusBarData { get; }
}