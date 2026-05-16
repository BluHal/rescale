using System.Windows.Controls;
using Rescale.ViewModels;

namespace Rescale.Views;

/// <summary>Page displaying the list of favorite and library presets.</summary>
public partial class PresetListPage : Page
{
    public PresetListPage(MainViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
