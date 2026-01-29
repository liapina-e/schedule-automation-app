using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using schedule_automation_app_client.ViewModels;

namespace schedule_automation_app_client;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        // InitializeComponent();
        AvaloniaXamlLoader.Load(this);
        DataContext = new MainViewModel();
    }
}