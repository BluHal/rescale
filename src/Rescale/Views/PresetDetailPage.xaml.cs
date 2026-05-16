using System.Windows.Controls;
using System.Windows.Input;
using Rescale.ViewModels;

namespace Rescale.Views;

/// <summary>Page for editing a single preset: name, icon, hotkey, and per-monitor settings.</summary>
public partial class PresetDetailPage : Page
{
    public PresetDetailPage(PresetDetailViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    private void IconTile_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is PresetDetailViewModel vm)
            vm.ToggleIconPickerCommand.Execute(null);
    }
}
