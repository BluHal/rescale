using System.Windows.Controls;
using Rescale.ViewModels;

namespace Rescale.Views;

/// <summary>Page for application settings: theme, autostart, and cycle hotkey.</summary>
public partial class SettingsPage : Page
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
