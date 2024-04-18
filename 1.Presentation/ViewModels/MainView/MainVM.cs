using CommunityToolkit.Mvvm.ComponentModel;

// ReSharper disable InconsistentNaming

namespace Presentation.ViewModels.MainView;

/// <summary>
/// ViewModel для основного представления.
/// </summary>
public class MainVM : ObservableRecipient
{
    public BackstageVM BackstageVM { get; }

    public SettingVM SettingVM { get; }

    /// <summary>
    /// Конструктор, запрещающий создания экземпляра без параметров.
    /// </summary>
    private MainVM()
    {
        BackstageVM = null!;
        SettingVM = null!;
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    public MainVM(BackstageVM backstageVm, SettingVM settingVm)
    {
        BackstageVM = backstageVm;
        SettingVM = settingVm;
    }
}