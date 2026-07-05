using Common.WpfModule.Ui.Services;

namespace Presentation.ViewModels._Contracts;

/// <summary>
/// Интерфейс, предоставляющий данные для статус-бара.
/// </summary>
public interface IStatusBarDataProvider
{
    /// <summary>
    /// Сервис статус-бара.
    /// </summary>
    StatusBarService StatusBarService { get; }
}