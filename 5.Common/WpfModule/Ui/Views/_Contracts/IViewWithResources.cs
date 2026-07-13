using System.Windows;

namespace Common.WpfModule.Ui.Views._Contracts;

/// <summary>
/// Представляет представление, которое предоставляет собственный словарь ресурсов.
/// </summary>
public interface IViewWithResources
{
    /// <summary>
    /// Получает словарь ресурсов, связанный с этим представлением.
    /// </summary>
    public ResourceDictionary Resources { get; }
}