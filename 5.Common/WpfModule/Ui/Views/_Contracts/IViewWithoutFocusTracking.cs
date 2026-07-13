using Common.WpfModule.Ui.Services._Contracts;

namespace Common.WpfModule.Ui.Views._Contracts;

/// <summary>
/// Маркерный интерфейс для окон, которые НЕ должны участвовать в механизме
/// автоматического возврата фокуса при активации главного окна.
/// </summary>
/// <remarks>
/// Если окно реализует этот интерфейс, <see cref="IViewService"/> не будет
/// отслеживать его активацию и не будет возвращать фокус при активации главного окна.
/// </remarks>
public interface IViewWithoutFocusTracking
{
}