using System;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;

namespace Presentation.Shell.Views.UserControls;

public partial class SettingControl : UserControl
{
    public SettingControl()
    {
        InitializeComponent();
        
        // TODO: временно - обновляем таблицу спортивных дисциплин
        WeakReferenceMessenger.Default.Register<EventArgs>(this, (r, e) =>
        {
            SportEventsGrid.Items.Refresh();
        });
    }
}