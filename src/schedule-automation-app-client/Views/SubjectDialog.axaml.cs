using Avalonia.Controls;
using schedule_automation_app_client.ViewModels;
 
namespace schedule_automation_app_client.Views;
 
public partial class SubjectDialog : Window
{
    public SubjectDialog()
    {
        InitializeComponent();
    }
 
    public SubjectDialog(SubjectDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
 
    private void Close(bool saved)
    {
        Close();
    }
}