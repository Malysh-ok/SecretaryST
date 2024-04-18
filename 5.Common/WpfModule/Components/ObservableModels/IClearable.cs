namespace Common.WpfModule.Components.ObservableModels;

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