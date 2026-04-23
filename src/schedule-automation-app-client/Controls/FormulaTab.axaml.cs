using Avalonia.Controls;
using schedule_automation_app_client.ViewModels;

namespace schedule_automation_app_client.Controls;

public partial class FormulaTab : UserControl
{
    public FormulaTab()
    {
        InitializeComponent();
    }

    private void OnCellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.RefreshFormulaStats();
        }
    }
}