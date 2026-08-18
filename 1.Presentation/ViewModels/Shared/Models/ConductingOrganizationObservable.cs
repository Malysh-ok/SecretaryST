using CommunityToolkit.Mvvm.ComponentModel;

namespace Presentation.ViewModels.Shared.Models;

/// <summary>
/// ObservableObject, инкапсулирующий свойства и методы для работы с Проводящей организацией.
/// </summary>
public class ConductingOrganizationObservable(int number, string name) : ObservableObject
{
    /// <summary>
    /// Номер.
    /// </summary>
    public int Number
    {
        get;
        set => SetProperty(ref field, value);
    } = number;

    /// <summary>
    /// Наименование.
    /// </summary>
    public string Name
    {
        get;
        set => SetProperty(ref field, value);
    } = name;
}