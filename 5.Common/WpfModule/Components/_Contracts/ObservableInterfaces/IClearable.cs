namespace Common.WpfModule.Components._Contracts.ObservableInterfaces;

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