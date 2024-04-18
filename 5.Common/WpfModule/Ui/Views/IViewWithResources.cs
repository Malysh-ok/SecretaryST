using System.Windows;

namespace Common.WpfModule.Ui.Views;

/// <summary>
/// Представление, реализующее ресурсы.
/// </summary>
public interface IViewWithResources
{
    public ResourceDictionary Resources { get; set; }
}