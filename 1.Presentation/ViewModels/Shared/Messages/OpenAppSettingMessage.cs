using CommunityToolkit.Mvvm.Messaging;

namespace Presentation.ViewModels.Shared.Messages;

/// <summary>
/// Сообщение для открытия окна настроек приложения.
/// Отправляется из ViewModel через мессенджер (<see cref="WeakReferenceMessenger"/>).
/// Принимается и обрабатывается в окне (обычно в code-behind).
/// </summary>
/// <remarks>
/// Используется для передачи контекста данных (например, текущей ViewModel или DTO)
/// из места отправки в окно-получатель.
/// </remarks>
public class OpenAppSettingMessage(object? dataContext = null)
{
    /// <summary>
    /// Контекст данных, который должен быть передан в открываемое окно.
    /// Обычно это ViewModel, которая будет установлена как DataContext окна.
    /// Может быть null, если окно использует свой собственный DataContext.
    /// </summary>
    public object? DataContext { get; } = dataContext;
}