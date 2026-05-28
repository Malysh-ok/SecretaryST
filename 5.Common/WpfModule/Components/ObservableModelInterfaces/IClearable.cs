namespace Common.WpfModule.Components.ObservableModelInterfaces;

/// <summary>
/// Очищаемый.
/// </summary>
public interface IClearable
{
    /// <summary>
    /// Очистить ссылки, закрыть соединения и т.п.
    /// </summary>
    void Clear();
}